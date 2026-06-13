using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace PortalProveedoresCore.Configuracion
{
    public class RegistrosWindows
    {
        private const string ruta_registros = @"SOFTWARE\SOTI\Service Portal";
        RegistryKey rk1 = Registry.LocalMachine;
        RegistryKey rk2 = Registry.LocalMachine;

        private string mysql_dominio, mysql_usuario, mysql_pass, mysql_puerto, mysql_bd;
        private string sql_usuario, sql_password, sql_servidor, sql_bd, rutaserver;
        private string micro_user, micro_pass, micro_server, micro_root, micro_bd, micro_oc;
        private string dir_archivos, serv_archivos;

        public RegistrosWindows()
        {
            mysql_dominio = mysql_usuario = mysql_pass = mysql_puerto = mysql_bd = "";
            sql_usuario = sql_password = sql_servidor = rutaserver = "";
            micro_user = micro_pass = micro_server = micro_root = micro_bd = micro_oc = "";
            dir_archivos = serv_archivos = "";
        }
        #region Creacion de propiedades
        //Propiedades mysql
        public string MYSQL_SERV
        {
            get { return mysql_dominio; }
            set { mysql_dominio = value; }
        }
        public string MYSQL_USER
        {
            get { return mysql_usuario; }
            set { mysql_usuario = value; }
        }
        public string MYSQL_PASS
        {
            set { mysql_pass = value; }
            get { return mysql_pass; }
        }
        public string MYSQL_PORT
        {
            set { mysql_puerto = value; }
            get { return mysql_puerto; }
        }
        public string MYSQL_DATA
        {
            set { mysql_bd = value; }
            get { return mysql_bd; }
        }

        //Propiedades SQLServer
        public string SQL_USUARIO
        {
            get { return sql_usuario; }
            set { sql_usuario = value; }
        }
        public string SQL_PASSWORD
        {
            set { sql_password = value; }
            get { return sql_password; }
        }
        public string SQL_SERVIDOR
        {
            set { sql_servidor = value; }
            get { return sql_servidor; }
        }
        public string RUTASERVER
        {
            set { rutaserver = value; }
            get { return rutaserver; }
        }
        public string SQL_BD
        {
            set { sql_bd = value; }
            get { return sql_bd; }
        }

        //propiedades Microsip
        public string MICRO_USER
        {
            get { return micro_user; }
            set { micro_user = value; }
        }
        public string MICRO_PASS
        {
            set { micro_pass = value; }
            get { return micro_pass; }
        }
        public string MICRO_SERVER
        {
            set { micro_server = value; }
            get { return micro_server; }
        }
        public string MICRO_ROOT
        {
            set { micro_root = value; }
            get { return micro_root; }
        }
        public string MICRO_BD
        {
            set { micro_bd = value; }
            get { return micro_bd; }
        }
        public string MICRO_OC
        {
            set { micro_oc = value; }
            get { return micro_oc; }
        }

        // Última sesión del Escritorio (no del servicio).
        // El operador puede marcar "Recordar contraseña" en FormLogin;
        // si lo hace, USUARIO + PASS se persisten en HKLM para pre-poblar
        // los campos la próxima vez. NULL o "" = no recordar.
        public string MICRO_USER1 { set; get; }
        public string MICRO_PASS1 { set; get; }

        //Propiedades Archivos Adjuntos
        public string DIR_ARCHIVOS
        {
            set { dir_archivos = value; }
            get { return dir_archivos; }
        }
        public string SERV_ARCHIVOS
        {
            set { serv_archivos = value; }
            get { return serv_archivos; }
        }

        //PROPIEDADES CORREO
        public string CORREO_DESTIN_DEST { set; get; }
        public string CORREO_PUERTO_SMTP { set; get; }
        public string CORREO_SMTP_MAIL { set; get; }
        public string CORREO_DIRECCION { set; get; }
        public string CORREO_CONTRAS { set; get; }
        public string CORREO_DESTIN_DIRECCION { set; get; }
        public string CORREO_DESTIN_GRANELES { set; get; }

        //PROPIEDAD DEL WEB SERVICES (legacy SOAP, hoy desuso)
        public string URL_WEBSERVICES { set; get; }
        public string RUTA_ARCHIVOS { set; get; }

        //PROPIEDADES DEL PORTAL CI4 (APIs REST)
        public string PORTAL_BASE_URL { set; get; }
        public string PORTAL_API_KEY  { set; get; }
        //
        public string MAILS_SEND { set; get; }
        public string MODE_TIMER { set; get; }
        public string SERVICE_NAME { set; get; }

        // Comportamiento del servicio en ESTA máquina (no en MySQL):
        // si "True", el servicio manda correo al proveedor cuando registra
        // una compra a partir de una de sus facturas. Cada cliente decide
        // independiente (por eso es HKLM, no PARAMETROS).
        public string ENVIAR_CORREO_COMPRAS { set; get; }
        #endregion
        public bool SO64bits()
        {
            bool bits;
            if (Environment.Is64BitOperatingSystem == true)
            {
                bits = true;
            }
            else
            {
                bits = false;
            }

            return bits;
        }
        public bool SO32bits()
        {
            bool bits;
            if (Environment.Is64BitOperatingSystem == false)
            {
                bits = true;
            }
            else
            {
                bits = false;
            }
            return bits;
        }
        public bool ExisteRegistro(string ruta_registros)
        {
            try
            {
                // return Key.GetValue(Value) != null;
                RegistryKey rkSubKey =Registry.LocalMachine.OpenSubKey(ruta_registros, false);
                //string aux =Convert.ToString(rkSubKey.GetValue("SERVICE_NAME")).Trim();
                //if (aux == null||aux.Length==0)     
                string aux = (string)rkSubKey.GetValue("SERVICE_NAME");
                if(aux is null )
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }
        public bool LeerRegistros(string ruta_registros)
        {
            string msg_local = "";
            bool _exito = false;
            try
            {
                if (SO64bits() == true)
                    rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                else
                    rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                if (!LeerRegistros(false))
                    CrearLlaveRegistro(ruta_registros, out msg_local);
                if (msg_local.Length == 0)
                {
                    rk2 = rk1.OpenSubKey(ruta_registros, false);

                    //REGISTROS MYSQL
                    MYSQL_DATA = (string)rk2.GetValue("MYSQL_DATA");
                    MYSQL_PASS = (string)rk2.GetValue("MYSQL_PASS");
                    MYSQL_PORT = (string)rk2.GetValue("MYSQL_PORT");
                    MYSQL_SERV = (string)rk2.GetValue("MYSQL_SERV");
                    MYSQL_USER = (string)rk2.GetValue("MYSQL_USER");

                    //REGISTROS MSSQL
                    SQL_USUARIO = (string)rk2.GetValue("USU_SQL");
                    SQL_PASSWORD = (string)rk2.GetValue("PASS_SQL");
                    SQL_SERVIDOR = (string)rk2.GetValue("SERV_SQL");
                    SQL_BD = (string)rk2.GetValue("BD_SQL");
                    //RUTASERVER= (string)rk2.GetValue("SERVIDOR_RUTA");

                    //REGSITROS MICROSIP
                    MICRO_USER = (string)rk2.GetValue("MICRO_USER");
                    MICRO_PASS = (string)rk2.GetValue("MICRO_PASS");
                    MICRO_SERVER = (string)rk2.GetValue("MICRO_SERV");
                    MICRO_ROOT = (string)rk2.GetValue("MICRO_ROOT");
                    MICRO_BD = (string)rk2.GetValue("MICRO_BD");
                    //MICRO_OC = (string)rk2.GetValue("MICRO_OC");
                    MICRO_USER1 = (string)rk2.GetValue("MICRO_USER1");
                    MICRO_PASS1 = (string)rk2.GetValue("MICRO_PASS1");

                    //REGISTROS ARCHIVOS ADJUNTOS
                    DIR_ARCHIVOS = (string)rk2.GetValue("DIR_ARCH_ADJ");
                    SERV_ARCHIVOS = (string)rk2.GetValue("SERV_ARCH_ADJ");

                    //REGISTRO CORREO
                    CORREO_DESTIN_DEST = (string)rk2.GetValue("CORREO_DESTIN_DEST");
                    CORREO_PUERTO_SMTP = (string)rk2.GetValue("CORREO_PUERTO_SMTP");
                    CORREO_SMTP_MAIL = (string)rk2.GetValue("CORREO_SMTP_MAIL");
                    CORREO_DIRECCION = (string)rk2.GetValue("CORREO_DIRECCION");
                    CORREO_CONTRAS = (string)rk2.GetValue("CORREO_CONTRAS");
                    CORREO_DESTIN_DIRECCION = (string)rk2.GetValue("CORREO_DESTIN_DIRECCION");
                    CORREO_DESTIN_GRANELES = (string)rk2.GetValue("CORREO_DESTIN_GRANELES");

                    //REGISTROS DEL WEB SERVICE (legacy SOAP)
                    URL_WEBSERVICES = (string)rk2.GetValue("URL_WEBSERVICES");
                    RUTA_ARCHIVOS = (string)rk2.GetValue("RUTA_ARCHIVOS");

                    //REGISTROS DEL PORTAL CI4
                    PORTAL_BASE_URL = (string)rk2.GetValue("PORTAL_BASE_URL");
                    PORTAL_API_KEY  = (string)rk2.GetValue("PORTAL_API_KEY");
                    //REGISTROS DE LA APLICACION
                    MAILS_SEND = (string)rk2.GetValue("MAILS_SEND");
                    MODE_TIMER = (string)rk2.GetValue("MODE_TIMER");
                    SERVICE_NAME = (string)rk2.GetValue("SERVICE_NAME");
                    ENVIAR_CORREO_COMPRAS = (string)rk2.GetValue("ENVIAR_CORREO_COMPRAS");
                    _exito = true;
                }
                else
                    _exito = false;

            }
            catch(Exception Ex)
            {
                msg_local = Ex.Message;
                _exito = false;
            }
            return _exito;
        }
        #region Escribir en registros especificos
        public void EscribirRegistro_Usuario_bd(string ruta_registro, string usuario_bd)
        {
            if (SO64bits() == true)
            {

                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("USU_SQL", usuario_bd);

            }
            else
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("USU_SQL", usuario_bd);
            }
        }
        public void EscribirRegistro_Usuario_bd_M(string ruta_registro, string usuario_bd)
        {
            if (SO64bits() == true)
            {

                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("USU_MSP", usuario_bd);

            }
            else
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("USU_MSP", usuario_bd);
            }
        }
        public void EscribirRegistro_Pass_bd(string ruta_registro, string pass_bd)
        {

            if (SO64bits() == true)
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("PASS_SQL", pass_bd);
            }
            else
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("PASS_SQL", pass_bd);
            }
        }
        public void EscribirRegistro_Pass_bd_M(string ruta_registro, string pass_bd)
        {

            if (SO64bits() == true)
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("PASS_MSP", pass_bd);
            }
            else
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("PASS_MSP", pass_bd);
            }
        }
        public void EscribirRegistro_Nombre_bd(string ruta_registro, string nombre_bd)
        {
            if (SO64bits() == true)
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("BD_SQL", nombre_bd);
            }
            else
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("BD_SQL", nombre_bd);

            }
        }
        public void EscribirRegistro_Nombre_bd_M(string ruta_registro, string nombre_bd)
        {
            if (SO64bits() == true)
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("BD_MSP", nombre_bd);
            }
            else
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("BD_MSP", nombre_bd);

            }
        }
        public void EscribirRegistro_Ruta_bd(string ruta_registro, string ruta_bd)
        {

            if (SO64bits() == true)
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("SERV_SQL", ruta_bd);
            }
            else
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("SERV_SQL", ruta_bd);
            }
        }
        public void EscribirRegistro_Servidor_bd_M(string ruta_registro, string ruta_bd)
        {

            if (SO64bits() == true)
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("SERV_MSP", ruta_bd);
            }
            else
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("SERV_MSP", ruta_bd);
            }
        }
        public void EscribirRegistro_Ruta_bd_M(string ruta_registro, string ruta_bd)
        {

            if (SO64bits() == true)
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("RUTA_MSP", ruta_bd);
            }
            else
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("RUTA_MSP", ruta_bd);
            }
        }
        public void EscribirRegistro_MYSQL_DATA(string ruta_registro, string mysql_data)
        {
            if (SO64bits() == true)
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("MYSQL_DATA", mysql_data);
            }
            else
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("MYSQL_DATA", mysql_data);
            }
        }
        public void EscribirRegistro_MYSQL_PASS(string ruta_registro, string mysql_pass)
        {
            if (SO64bits() == true)
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("MYSQL_DATA", mysql_pass);
            }
            else
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("MYSQL_DATA", mysql_pass);
            }
        }
        public void EscribirRegistro_MYSQL_PORT(string ruta_registro, string mysql_port)
        {
            if (SO64bits() == true)
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("MYSQL_DATA", mysql_port);
            }
            else
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("MYSQL_DATA", mysql_port);
            }
        }
        public void EscribirRegistro_MYSQL_SERV(string ruta_registro, string mysql_serv)
        {
            if (SO64bits() == true)
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("MYSQL_DATA", mysql_serv);
            }
            else
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("MYSQL_DATA", mysql_serv);
            }
        }
        public void EscribirRegistro_MYSQL_USER(string ruta_registro, string mysql_user)
        {
            if (SO64bits() == true)
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("MYSQL_DATA", mysql_user);
            }
            else
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("MYSQL_DATA", mysql_user);
            }
        }
        public void EscribirRegistro_URL_WEBSERVICES(string ruta_registro, string url_webservices)
        {
            if (SO64bits() == true)
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("MYSQL_DATA", url_webservices);
            }
            else
            {
                rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                rk2 = rk1.OpenSubKey(ruta_registro, true);
                rk2.SetValue("MYSQL_DATA", url_webservices);
            }
        }
        #endregion
        public bool EscribirRegistros(string ruta_registros, string nombre_registro, string valor_registro, out string msg)
        {
            bool _exito = false;
            string msg_local = "";
            try
            {
                if (SO64bits())
                {
                    /*rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    rk2 = rk1.OpenSubKey(ruta_registros, true);
                    rk2.SetValue(nombre_registro, valor_registro);*/
                    using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                    {
                        using (var key = root.OpenSubKey(ruta_registros, true))
                        {
                            //var registeredOwner = key.GetValue("RegisteredOwner");
                            key.SetValue(nombre_registro, valor_registro);
                            _exito = true;
                        };
                    };
                }
                else
                {
                    if (SO32bits())
                    {
                        rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                        rk2 = rk1.OpenSubKey(ruta_registros, true);
                        rk2.SetValue(nombre_registro, valor_registro);
                        _exito = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _exito = false;
                msg_local += "No fue posible escribir en los registros de Windows.\n\n" + ex.Message;
            }
            msg = msg_local;
            return _exito;
        }
        public bool CrearLlaveRegistro(string ruta_registro, out string msg)
        {
            bool _exito = false;
            string msg_local = "";
            try
            {
                // Crea la llave HKLM vacía. Los valores (credenciales, URL del portal,
                // API key, etc.) los escribe el Helper elevado o la UI tras pedírselos al
                // operador; las contraseñas se cifran con DPAPI antes de guardarse.
                // No se persisten defaults inseguros aquí.
                rk2 = rk1.CreateSubKey(ruta_registro);
                Microsoft.Win32.Registry.LocalMachine.CreateSubKey(ruta_registro);
                _exito = true;
            }
            catch (Exception ex)
            {
                msg_local = ex.Message;
                _exito = false;
            }
            msg = msg_local;
            return _exito;

        }
        public bool ExisteValorCadena(RegistryKey Key, string valor_cadena)
        {
            bool _existe = false;
            try
            {
                _existe =  Key.GetValue(valor_cadena) != null;
            }
            catch
            {
                _existe = false;
            }
            return _existe;
        }
        public bool LeerRegistros(bool mostrar_alerta)
        {
            try
            {
                if (SO64bits())
                {
                    rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                }
                else
                {
                    if (SO32bits())
                    {
                        rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                    }
                }

                rk2 = rk1.OpenSubKey(ruta_registros, false);

                if (rk2 != null)
                {
                    //REGISTROS MYSQL
                    MYSQL_DATA = (string)rk2.GetValue("MYSQL_DATA");
                    MYSQL_PASS = (string)rk2.GetValue("MYSQL_PASS");
                    MYSQL_PORT = (string)rk2.GetValue("MYSQL_PORT");
                    MYSQL_SERV = (string)rk2.GetValue("MYSQL_SERV");
                    MYSQL_USER = (string)rk2.GetValue("MYSQL_USER");

                    //REGISTROS MSSQL
                    SQL_USUARIO = (string)rk2.GetValue("USU_SQL");
                    SQL_PASSWORD = (string)rk2.GetValue("PASS_SQL");
                    SQL_SERVIDOR = (string)rk2.GetValue("SERV_SQL");
                    SQL_BD = (string)rk2.GetValue("BD_SQL");
                    //RUTASERVER= (string)rk2.GetValue("SERVIDOR_RUTA");

                    //REGSITROS MICROSIP
                    MICRO_USER = (string)rk2.GetValue("MICRO_USER");
                    MICRO_PASS = (string)rk2.GetValue("MICRO_PASS");
                    MICRO_SERVER = (string)rk2.GetValue("MICRO_SERV");
                    MICRO_ROOT = (string)rk2.GetValue("MICRO_ROOT");
                    MICRO_BD = (string)rk2.GetValue("MICRO_BD");
                    //MICRO_OC = (string)rk2.GetValue("MICRO_OC");
                    MICRO_USER1 = (string)rk2.GetValue("MICRO_USER1");
                    MICRO_PASS1 = (string)rk2.GetValue("MICRO_PASS1");

                    //REGISTROS ARCHIVOS ADJUNTOS
                    DIR_ARCHIVOS = (string)rk2.GetValue("DIR_ARCH_ADJ");
                    SERV_ARCHIVOS = (string)rk2.GetValue("SERV_ARCH_ADJ");

                    //REGISTRO CORREO
                    CORREO_DESTIN_DEST = (string)rk2.GetValue("CORREO_DESTIN_DEST");
                    CORREO_PUERTO_SMTP = (string)rk2.GetValue("CORREO_PUERTO_SMTP");
                    CORREO_SMTP_MAIL = (string)rk2.GetValue("CORREO_SMTP_MAIL");
                    CORREO_DIRECCION = (string)rk2.GetValue("CORREO_DIRECCION");
                    CORREO_CONTRAS = (string)rk2.GetValue("CORREO_CONTRAS");
                    CORREO_DESTIN_DIRECCION = (string)rk2.GetValue("CORREO_DESTIN_DIRECCION");
                    CORREO_DESTIN_GRANELES = (string)rk2.GetValue("CORREO_DESTIN_GRANELES");

                    //REGISTROS DEL WEB SERVICE (legacy SOAP)
                    URL_WEBSERVICES = (string)rk2.GetValue("URL_WEBSERVICES");
                    RUTA_ARCHIVOS = (string)rk2.GetValue("RUTA_ARCHIVOS");

                    //REGISTROS DEL PORTAL CI4
                    PORTAL_BASE_URL = (string)rk2.GetValue("PORTAL_BASE_URL");
                    PORTAL_API_KEY  = (string)rk2.GetValue("PORTAL_API_KEY");
                    //REGISTROS DE LA APLICACION
                    MAILS_SEND = (string)rk2.GetValue("MAILS_SEND");
                    MODE_TIMER = (string)rk2.GetValue("MODE_TIMER");
                    SERVICE_NAME = (string)rk2.GetValue("SERVICE_NAME");
                    ENVIAR_CORREO_COMPRAS = (string)rk2.GetValue("ENVIAR_CORREO_COMPRAS");


                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch //(Exception ex)
            {
                if (mostrar_alerta)
                {
                    // MessageBox.Show("No fue posible leer los registros de Windows.\n\n" + ex.Message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return false;
            }
        }
        public bool CrearRegistros(bool mostrar_alerta)
        {
            try
            {
                //Registry.LocalMachine.OpenSubKey("Software", true);
                // Registry.LocalMachine.CreateSubKey(ruta_registros);
                rk2 = rk1.CreateSubKey(ruta_registros);

                /*if (SO64bits())
                {
                    Registry.LocalMachine.OpenSubKey("Software", true);

                    Registry.LocalMachine.CreateSubKey(ruta_registros);
                }
                else
                {
                    if (SO32bits())
                    {
                        Registry.LocalMachine.OpenSubKey("Software", true);

                        Registry.LocalMachine.CreateSubKey(ruta_registros);
                    }
                }*/

                return true;
            }
            catch //(Exception ex)
            {
                if (mostrar_alerta)
                {
                    // MessageBox.Show("No fue posible crear la nueva clave en los registros de Windows.\n\n" + ex.Message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return false;
            }
        }

        public void EscribirRegistros(string nombre_registro, string valor_registro, bool mostrar_alerta)
        {
            try
            {
                if (SO64bits())
                {
                    //rk1 = Registry.LocalMachine.OpenSubKey("Software", true);
                    rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    rk2 = rk1.OpenSubKey(ruta_registros, true);
                    rk2.SetValue(nombre_registro, valor_registro);
                }
                else
                {
                    if (SO32bits())
                    {
                        //rk1 = Registry.LocalMachine.OpenSubKey("Software", true);
                        rk1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                        rk2 = rk1.OpenSubKey(ruta_registros, true);
                        rk2.SetValue(nombre_registro, valor_registro);
                    }
                }
            }
            catch 
            {
                if (mostrar_alerta)
                {
                    // MessageBox.Show("No fue posible escribir en los registros de Windows.\n\n" + ex.Message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
