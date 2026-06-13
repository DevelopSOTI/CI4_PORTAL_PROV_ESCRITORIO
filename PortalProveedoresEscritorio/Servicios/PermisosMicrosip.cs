using System;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using PortalProveedoresCore.Configuracion;

namespace PortalProveedoresEscritorio.Servicios
{
    /// <summary>
    /// Réplica del SOAP <c>C_PERMISOS_MICROSIP.PermisoUsuario</c>
    /// (PortalProveedores\C_PERMISOS_MICROSIP.cs:14-43). Verifica si un
    /// usuario Microsip tiene un permiso (DERECHOS_USUARIOS) específico
    /// contra CONFIG.FDB.
    ///
    /// El SQL es el mismo que el SOAP, palabra por palabra:
    /// <code>
    /// SELECT * FROM derechos_usuarios d
    ///   JOIN USUARIOS u ON u.usuario_id = d.usuario_id
    ///  WHERE u.nombre = ? AND u.estatus = 'A'
    ///    AND d.clave_objeto = ?
    /// </code>
    /// Si devuelve al menos una fila → tiene el permiso.
    ///
    /// Permisos relevantes:
    /// <list type="bullet">
    ///   <item><c>831</c> — crear nueva compra en Microsip
    ///     (gate antes de abrir <c>F_APLICAR_FACTURA</c> en el SOAP).</item>
    /// </list>
    ///
    /// Reconecta cada vez que se llama (mismo patrón que el SOAP, donde no
    /// hay una conexión persistente al CONFIG.FDB después del login). Las
    /// credenciales vienen del operador que está logueado en el Escritorio.
    /// </summary>
    public sealed class PermisosMicrosip
    {
        /// <summary>
        /// Devuelve true si el usuario tiene el permiso indicado activo en
        /// Microsip. Cualquier excepción se trata como "no tiene permiso"
        /// para fallar seguro (igual que el SOAP, que también consume la
        /// excepción y deja <c>band=false</c>).
        /// </summary>
        public async Task<bool> TienePermisoAsync(
            string usuario, string password, string clavePermiso, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(clavePermiso))
                return false;

            try
            {
                var reg = new RegistrosWindows();
                reg.LeerRegistros(false);

                var con = new ConexionMicrosip();
                string mensaje;
                if (!con.ConectarConfigPrueba(reg.MICRO_SERVER, reg.MICRO_ROOT,
                                              usuario, password ?? "", out mensaje))
                    return false;

                try
                {
                    const string sql =
                        "SELECT 1 FROM DERECHOS_USUARIOS d " +
                        "  JOIN USUARIOS u ON u.USUARIO_ID = d.USUARIO_ID " +
                        " WHERE u.NOMBRE = @user AND u.ESTATUS = 'A' " +
                        "   AND d.CLAVE_OBJETO = @clave";
                    using (var cmd = new FbCommand(sql, con.FBC))
                    {
                        cmd.Parameters.Add("@user",  FbDbType.VarChar).Value = usuario;
                        cmd.Parameters.Add("@clave", FbDbType.VarChar).Value = clavePermiso;
                        var raw = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                        return raw != null && raw != DBNull.Value;
                    }
                }
                finally
                {
                    con.Desconectar();
                }
            }
            catch
            {
                // Mismo comportamiento que el SOAP: cualquier fallo → no
                // tiene permiso. El operador verá el MessageBox de "Usted
                // no tiene el permiso..." y podrá reintentar.
                return false;
            }
        }
    }
}
