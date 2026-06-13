using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresService.Repositorios
{
    /// <summary>
    /// Replica el comportamiento del legacy CALCULA_EMPRESAS() pero parametrizado,
    /// async, con using para disposición correcta y una sola consulta a REGISTRY
    /// por empresa (NOMBRE IN ('Nombre','Rfc')).
    ///
    /// Dos planos de conexión:
    ///   1) BD CONFIG (System\CONFIG.FDB) — lista las empresas instaladas.
    ///   2) BD por empresa ({NOMBRE_CORTO}.FDB) — datos en tabla REGISTRY.
    /// </summary>
    public sealed class EmpresasRepository : IEmpresasRepository
    {
        private const string SqlEmpresas =
            "SELECT EMPRESA_ID, NOMBRE_CORTO, FECHA_HORA_ULT_MODIF " +
            "FROM EMPRESAS " +
            "ORDER BY EMPRESA_ID";

        private const string SqlRegistry =
            "SELECT NOMBRE, VALOR FROM REGISTRY WHERE NOMBRE IN ('Nombre','Rfc')";

        public async Task<IReadOnlyList<EmpresaMicrosip>> ListarAsync(CancellationToken ct)
        {
            var lista = new List<EmpresaMicrosip>();
            int ignoradas = 0;

            var conCfg = new ConexionMicrosip();
            if (!conCfg.ConectarConfigMicrosip())
            {
                EventoLog.Error("EmpresasRepository.ListarAsync: no se pudo conectar a CONFIG.FDB.");
                return lista;
            }

            try
            {
                using (var cmd = new FbCommand(SqlEmpresas, conCfg.FBC))
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    while (await rd.ReadAsync(ct).ConfigureAwait(false))
                    {
                        var emp = new EmpresaMicrosip
                        {
                            empresa_id           = Convert.ToInt32(rd["EMPRESA_ID"]),
                            nombre_corto         = Convert.ToString(rd["NOMBRE_CORTO"]),
                            fecha_hora_ult_modif = Convert.ToString(rd["FECHA_HORA_ULT_MODIF"]),
                        };

                        // Réplica del Delphi Func_Calcula.pas:92-99: si el .FDB de
                        // la empresa no se puede abrir (o falla la consulta a
                        // REGISTRY), 'Agregar := False; Inc(Ignoradas)' — la
                        // empresa se OMITE del lote y conserva sus datos en el
                        // portal. Antes se subía con nombre/rfc vacíos y el
                        // endpoint pisaba los valores buenos con ''.
                        if (EnriquecerConRegistry(emp, ct))
                        {
                            lista.Add(emp);
                        }
                        else
                        {
                            ignoradas++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EventoLog.Error("EmpresasRepository.ListarAsync (CONFIG): " + ex.Message);
            }
            finally
            {
                conCfg.Desconectar();
            }

            if (ignoradas > 0)
            {
                EventoLog.Warning("EmpresasRepository: " + ignoradas
                    + " empresa(s) ignorada(s) por BD inaccesible — conservan sus datos en el portal "
                    + "(réplica Func_Calcula.pas:92-99).");
            }

            return lista;
        }

        /// <summary>
        /// Abre la BD de la empresa por su NOMBRE_CORTO y rellena nombre/rfc.
        /// Devuelve <c>true</c> si el enriquecimiento se logró; <c>false</c>
        /// si la BD de esa empresa no está disponible o la consulta a REGISTRY
        /// falla — en ese caso el caller OMITE la empresa del lote (réplica
        /// del 'Agregar := False' del Delphi en Func_Calcula.pas:92-99) para
        /// no machacar nombre/RFC buenos del portal con valores vacíos.
        /// </summary>
        private bool EnriquecerConRegistry(EmpresaMicrosip emp, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var conEmp = new ConexionMicrosip();
            if (!conEmp.ConectarMicrosip(emp.nombre_corto))
            {
                EventoLog.Warning("EmpresasRepository: no se pudo abrir BD de '" + emp.nombre_corto + "'; la empresa se omite del lote.");
                return false;
            }

            try
            {
                using (var cmd = new FbCommand(SqlRegistry, conEmp.FBC))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var nombre = Convert.ToString(rd["NOMBRE"]);
                        var valor  = Convert.ToString(rd["VALOR"]);

                        if (string.Equals(nombre, "Nombre", StringComparison.OrdinalIgnoreCase))
                            emp.nombre = valor;
                        else if (string.Equals(nombre, "Rfc", StringComparison.OrdinalIgnoreCase))
                            emp.rfc = valor;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                EventoLog.Error("EmpresasRepository.EnriquecerConRegistry('" + emp.nombre_corto + "'): " + ex.Message
                    + " — la empresa se omite del lote.");
                return false;
            }
            finally
            {
                conEmp.Desconectar();
            }
        }
    }
}
