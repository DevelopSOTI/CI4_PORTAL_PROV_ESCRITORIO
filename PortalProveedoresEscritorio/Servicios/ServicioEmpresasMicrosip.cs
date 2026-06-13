using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using PortalProveedoresCore.Servicios;

namespace PortalProveedoresEscritorio.Servicios
{
    /// <summary>
    /// Replica <c>C_EMPRESAS.EmpresasUsuario</c> del legacy: dado un usuario
    /// Microsip ya autenticado contra CONFIG.FDB, devuelve la lista de
    /// empresas a las que tiene acceso, intersectada con las que estén
    /// <c>Autorizada</c> en el portal.
    ///
    /// Algoritmo (idéntico al legacy <c>C_EMPRESAS.cs:27-122</c>):
    /// <list type="number">
    ///   <item>Lee USUARIOS.ACCESO_EMPRESAS para saber si tiene 'T' (todas)
    ///         o solo una lista.</item>
    ///   <item>Si 'T' → SELECT * FROM EMPRESAS. Si no → JOIN con
    ///         EMPRESAS_USUARIOS para sacar las autorizadas en Microsip.</item>
    ///   <item>Cruza con el portal (<see cref="IPortalApi.ListarEmpresasAutorizadasAsync"/>):
    ///         deja solo las empresas autorizadas en AMBOS lados.</item>
    /// </list>
    /// </summary>
    public sealed class ServicioEmpresasMicrosip
    {
        private readonly IPortalApi _api;

        public ServicioEmpresasMicrosip(IPortalApi api) { _api = api; }

        public async Task<IReadOnlyList<EmpresaEscritorio>> ObtenerAutorizadasAsync(
            FbConnection configFdb, string usuario, CancellationToken ct)
        {
            // 1) ACCESO_EMPRESAS del usuario.
            string acceso = await LeerAccesoEmpresasAsync(configFdb, usuario, ct).ConfigureAwait(false);

            // 2) Lista de Microsip.
            var enMicrosip = await LeerEmpresasMicrosipAsync(configFdb, usuario, acceso == "T", ct).ConfigureAwait(false);
            if (enMicrosip.Count == 0) return new List<EmpresaEscritorio>(0);

            // 3) Intersección con el portal.
            var autorizadasPortal = await _api.ListarEmpresasAutorizadasAsync(ct).ConfigureAwait(false);
            var idsAutorizadas = new HashSet<int>();
            foreach (var e in autorizadasPortal) idsAutorizadas.Add(e.emp_id_msp);

            var resultado = new List<EmpresaEscritorio>(enMicrosip.Count);
            foreach (var e in enMicrosip)
                if (idsAutorizadas.Contains(e.Id))
                    resultado.Add(e);

            return resultado;
        }

        private static async Task<string> LeerAccesoEmpresasAsync(FbConnection con, string usuario, CancellationToken ct)
        {
            const string sql = "SELECT ACCESO_EMPRESAS FROM USUARIOS WHERE NOMBRE = @user AND ESTATUS = 'A'";
            using (var cmd = new FbCommand(sql, con))
            {
                cmd.Parameters.Add("@user", FbDbType.VarChar).Value = usuario;
                var raw = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                return (raw == null || raw == DBNull.Value) ? "" : (Convert.ToString(raw) ?? "").Trim();
            }
        }

        private static async Task<List<EmpresaEscritorio>> LeerEmpresasMicrosipAsync(
            FbConnection con, string usuario, bool todas, CancellationToken ct)
        {
            string sql;
            if (todas)
            {
                sql = "SELECT EMPRESA_ID, NOMBRE_CORTO FROM EMPRESAS ORDER BY NOMBRE_CORTO";
            }
            else
            {
                sql = "SELECT e.EMPRESA_ID, e.NOMBRE_CORTO " +
                      "  FROM EMPRESAS e " +
                      "  JOIN EMPRESAS_USUARIOS eu ON (eu.EMPRESA_ID = e.EMPRESA_ID) " +
                      "  JOIN USUARIOS u ON (u.USUARIO_ID = eu.USUARIO_ID) " +
                      " WHERE u.NOMBRE = @user AND u.ESTATUS = 'A' " +
                      " ORDER BY e.NOMBRE_CORTO";
            }

            var lista = new List<EmpresaEscritorio>();
            using (var cmd = new FbCommand(sql, con))
            {
                if (!todas) cmd.Parameters.Add("@user", FbDbType.VarChar).Value = usuario;
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    while (await rd.ReadAsync(ct).ConfigureAwait(false))
                    {
                        lista.Add(new EmpresaEscritorio
                        {
                            Id          = Convert.ToInt32(rd["EMPRESA_ID"]),
                            NombreCorto = (Convert.ToString(rd["NOMBRE_CORTO"]) ?? "").Trim(),
                        });
                    }
                }
            }
            return lista;
        }
    }

    /// <summary>
    /// Empresa que pasa el filtro de Microsip (acceso del usuario) Y el
    /// filtro del portal (EMP_ESTATUS='Autorizada'). Es lo que ve el
    /// selector y lo que se usa para construir requests a la API.
    /// </summary>
    public sealed class EmpresaEscritorio
    {
        public int    Id          { get; set; }
        public string NombreCorto { get; set; }

        public override string ToString() => NombreCorto;
    }
}
