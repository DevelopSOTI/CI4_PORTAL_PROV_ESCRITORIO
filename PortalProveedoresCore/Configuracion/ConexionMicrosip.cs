using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using FirebirdSql.Data.FirebirdClient;

namespace PortalProveedoresCore.Configuracion
{
    public class ConexionMicrosip
    {
        private const string LogSource = "PortalProveedoresService";

        private static void LogError(string mensaje)
        {
            try { EventLog.WriteEntry(LogSource, mensaje, EventLogEntryType.Error); }
            catch { }
        }

        private string conectionString;
        private string user, password, database, root, dataSource;
        private FbConnection fbc;
        private RegistrosWindows reg;

        public ConexionMicrosip ()
        {
            user = password = database = root = dataSource = "";            
        }
        public string USER
        {
            set { user = value; } get { return user; }
        }
        public string PASSWORD
        {
            set { password = value; } get { return password; }
        }
        public string DATABASE
        {
            set { database = value; } get { return database; }
        }
        public string ROOT
        {
            set { root = value; } get { return root; }
        }
        public string DATASOURSE
        {
            set { dataSource = value; } get { return dataSource; }
        }   
        public string CONECTIONSTRING
        {
            set { conectionString = value; } get { return conectionString; }
        }
        public FbConnection FBC
        {
            set { fbc = value; } get { return fbc; }
        }
        public RegistrosWindows REG
        {
            set { reg = value; } get { return reg; }
        }
        public bool ConectarMicrosip (string db)
        {
            bool band = false;
            try
            {
                reg = new RegistrosWindows();
                reg.LeerRegistros("SOFTWARE\\SOTI\\Service Portal");
                conectionString = @"User=" + reg.MICRO_USER + "; Password=" + reg.MICRO_PASS
                        + "; Database=" + RutaEmpresa(reg.MICRO_ROOT, db)
                        + "; Datasource=" + reg.MICRO_SERVER + "; Dialect=3" + "; Charset=ISO8859_1";

                fbc = new FbConnection(conectionString);
                fbc.Open();
                band = true;
            }
            catch(Exception err)
            {
                LogError("ConectarMicrosip(" + db + "): " + err.Message);
            }
            return band;

        }
        public int GenDocto(string BDMicrosip)
        {
            int docto_cm_id=0;
            try
            {
                ConexionMicrosip cn = new ConexionMicrosip();
                
                cn.ConectarMicrosip(BDMicrosip);
                FbConnection con_microsip = new FbConnection(cn.CONECTIONSTRING);
                con_microsip.Open();
                FbCommand gen_docto_cm_id = new FbCommand("GEN_DOCTO_ID");
                gen_docto_cm_id.CommandType = CommandType.StoredProcedure;
                gen_docto_cm_id.Connection = con_microsip;
                docto_cm_id = Convert.ToInt32(gen_docto_cm_id.ExecuteScalar());
                gen_docto_cm_id.Cancel();
                con_microsip.Close();
                cn.Desconectar();
            }
            catch (Exception e)
            {
                LogError("GenDocto(" + BDMicrosip + "): " + e.Message);
            }
            return docto_cm_id;
        }
        public bool ConectarConfigMicrosip()
        {
            bool band = false;
            try
            {
                reg = new RegistrosWindows();
                reg.LeerRegistros("SOFTWARE\\SOTI\\Service Portal");
                conectionString = @"User=" + reg.MICRO_USER + "; Password=" + reg.MICRO_PASS
                        + "; Database=" + RutaConfig(reg.MICRO_ROOT)
                        + "; Datasource=" + reg.MICRO_SERVER + "; Dialect=3" + "; Charset=ISO8859_1";

                fbc = new FbConnection(conectionString);
                fbc.Open();
                band = true;
            }
            catch(Exception err)
            {
                LogError("ConectarConfigMicrosip: " + err.Message);
            }

            return band;

        }

        public bool ConectarFB(string usuario, string pass, string ruta, string servidor, out string mensaje)
        {
            mensaje = "";
            try
            {
                reg = new RegistrosWindows();

                if (reg.LeerRegistros(true))
                {
                    conectionString = @"User= " + usuario + ";";
                    conectionString += "Password=" + pass + ";";
                    conectionString += "Database=" + RutaConfig(ruta) + ";";
                    conectionString += "Datasource=" + servidor + ";";
                    conectionString += "Dialect=3;";
                    conectionString += "Charset=ISO8859_1;";

                    fbc = new FbConnection(conectionString);
                    fbc.Open();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                mensaje = "No fue posible establecer conexión con la empresa  .\n\n" + ex.Message;
                //MessageBox.Show("No fue posible establecer conexión con la empresa '" + empresa + "'.\n\n" + ex.Message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }
        public void Desconectar ()
        {
            if (fbc == null) return;
            try { if (fbc.State != ConnectionState.Closed) fbc.Close(); }
            catch (Exception err) { LogError("Desconectar: " + err.Message); }
        }

        /// <summary>
        /// Variante de <see cref="ConectarConfigMicrosip"/> que NO lee del registro
        /// — usa los valores que le pase el llamador. Pensado para el botón
        /// "Probar conexión" del Configurador, donde el operador puede escribir
        /// credenciales nuevas y verificarlas antes de guardarlas a HKLM.
        ///
        /// Devuelve true si abrió correctamente CONFIG.FDB. En caso de fallo,
        /// el mensaje viaja por el parámetro out — no se loguea (es una
        /// operación interactiva, el operador ve el resultado en pantalla).
        /// </summary>
        public bool ConectarConfigPrueba(string servidor, string root, string usuario, string pass, out string mensaje)
        {
            mensaje = "";
            try
            {
                conectionString = "User=" + usuario + ";"
                                + "Password=" + pass + ";"
                                + "Database=" + RutaConfig(root) + ";"
                                + "Datasource=" + servidor + ";"
                                + "Dialect=3;"
                                + "Charset=ISO8859_1;";

                fbc = new FbConnection(conectionString);
                fbc.Open();
                return true;
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }

        // ================================================================
        //  Construcción de la ruta de la BD Firebird — portable Windows/Linux
        // ----------------------------------------------------------------
        //  El servidor Firebird en Linux distingue mayúsculas/minúsculas y
        //  usa '/' como separador (backslash '\' se toma como carácter
        //  literal y rompe la ruta). Firebird en Windows también acepta '/'.
        //  Por eso normalizamos SIEMPRE a '/' y a extensión .fdb en minúscula.
        //  Formato tomado del SOAP legacy (C_ConexionFirebird.cs), que ya
        //  conectaba a Microsip en Linux:
        //     config  → "<root>/System/Config.fdb"   (carpeta "System",
        //                archivo "Config.fdb": C mayúscula, .fdb minúscula)
        //     empresa → "<root>/<NOMBRE_CORTO>.fdb"
        // ================================================================

        /// <summary>
        /// Normaliza la raíz: pasa los '\' a '/' y quita la '/' final, para
        /// poder concatenar "/..." sin duplicar ni mezclar separadores.
        /// </summary>
        private static string RaizNormalizada(string root)
        {
            return (root ?? "").Replace('\\', '/').TrimEnd('/');
        }

        /// <summary>Ruta de la BD de configuración de Microsip: &lt;root&gt;/System/Config.fdb</summary>
        private static string RutaConfig(string root)
        {
            return RaizNormalizada(root) + "/System/Config.fdb";
        }

        /// <summary>Ruta de la BD de una empresa: &lt;root&gt;/&lt;nombreCorto&gt;.fdb</summary>
        private static string RutaEmpresa(string root, string nombreCorto)
        {
            return RaizNormalizada(root) + "/" + nombreCorto + ".fdb";
        }
    }
}
