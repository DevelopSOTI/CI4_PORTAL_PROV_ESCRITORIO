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
    /// Implementación del DAO de almacenes contra el Firebird de cada empresa.
    /// Replica el comportamiento de <c>ACTUALIZA_ALMACENES</c> del Delphi
    /// histórico, pero parametrizado, async y con disposición correcta.
    ///
    /// La conexión a la BD de la empresa pasa por <see cref="ConexionMicrosip"/>
    /// (clase compartida en Core), que arma la cadena de conexión usando los
    /// registros HKLM y el nombre corto de la empresa como nombre de archivo .FDB.
    /// </summary>
    public sealed class AlmacenesRepository : IAlmacenesRepository
    {
        // Mismos campos que el Delphi pide. COALESCE(ULT_MODIF, CREACION) en
        // el SELECT asegura que la "fecha efectiva" que viaja al portal
        // siempre tenga valor (evita el loop infinito donde el portal nunca
        // avanza su checkpoint porque guarda NULL — ver ProveedoresRepository).
        private const string SqlAlmacenesBase =
            "SELECT ALMACEN_ID, NOMBRE, NOMBRE_ABREV, " +
            "       COALESCE(FECHA_HORA_ULT_MODIF, FECHA_HORA_CREACION) AS FECHA_HORA_ULT_MODIF " +
            "FROM ALMACENES";

        public async Task<IReadOnlyList<AlmacenMicrosip>> ListarAsync(string nombreEmpresa, DateTime? desde, CancellationToken ct)
        {
            var lista = new List<AlmacenMicrosip>();

            var con = new ConexionMicrosip();
            if (!con.ConectarMicrosip(nombreEmpresa))
            {
                EventoLog.Warning("AlmacenesRepository: no se pudo abrir BD de '" + nombreEmpresa + "'.");
                return lista;
            }

            try
            {
                string sql;
                if (desde.HasValue)
                {
                    // COALESCE en el WHERE: una fila con ULT_MODIF NULL pero
                    // CREACION reciente igual califica como "modificada".
                    sql = SqlAlmacenesBase +
                          " WHERE COALESCE(FECHA_HORA_ULT_MODIF, FECHA_HORA_CREACION) > @desde" +
                          " ORDER BY ALMACEN_ID";
                }
                else
                {
                    sql = SqlAlmacenesBase + " ORDER BY ALMACEN_ID";
                }

                using (var cmd = new FbCommand(sql, con.FBC))
                {
                    if (desde.HasValue)
                        cmd.Parameters.Add("@desde", FbDbType.TimeStamp).Value = desde.Value;

                    using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                    {
                        while (await rd.ReadAsync(ct).ConfigureAwait(false))
                        {
                            var fechaUltModif = rd.IsDBNull(3)
                                ? (DateTime?) null
                                : (DateTime?) Convert.ToDateTime(rd[3]);

                            lista.Add(new AlmacenMicrosip
                            {
                                almacen_id           = Convert.ToInt32(rd["ALMACEN_ID"]),
                                nombre               = Convert.ToString(rd["NOMBRE"])       ?? "",
                                nombre_abrev         = Convert.ToString(rd["NOMBRE_ABREV"]) ?? "",
                                fecha_hora_ult_modif = fechaUltModif.HasValue
                                    ? fechaUltModif.Value.ToString("yyyy-MM-dd HH:mm:ss")
                                    : "",
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EventoLog.Error("AlmacenesRepository('" + nombreEmpresa + "'): " + ex.Message);
            }
            finally
            {
                con.Desconectar();
            }

            return lista;
        }
    }
}
