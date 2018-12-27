using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RioSulAPI.Class
{
    public class Utilerias
    {
        public static Logger Log = new Logger();

        public static void EscribirLog(string strMensaje)
        {
            string sFechaArchivo = DateTime.Now.ToString("MMyyyy");
            string strFecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            string strRuta = "~/Logs/Log_" + sFechaArchivo + ".txt";
            HttpContext contexto = null;
            try
            {
                if (!File.Exists((contexto ?? HttpContext.Current).Server.MapPath(strRuta)))
                {
                    File.Create((contexto ?? HttpContext.Current).Server.MapPath(strRuta)).Dispose();
                }
                using (StreamWriter swArchivo = new StreamWriter((contexto ?? HttpContext.Current).Server.MapPath(strRuta), true))
                {
                    swArchivo.WriteLine(strFecha);
                    swArchivo.WriteLine(strMensaje.Replace("<br/>", Environment.NewLine));
                    swArchivo.WriteLine("---------------------------------------------------------------------------------------------");
                    swArchivo.Close();
                }
            }
            catch (Exception)
            {

            }
        }
    }
}