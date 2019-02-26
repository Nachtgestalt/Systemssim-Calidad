using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RioSulAPI.Class
{
    public class Logger
    {
        public void Log(string text, string Metodo, string NombreArchivo)
        {
            string path = @"E:\Desarrollo\LOG\" + NombreArchivo + "_" + DateTime.Now.ToString("MMyyyy") + ".log";
            if (!File.Exists(path)) File.Create(path).Dispose();
            using (StreamWriter writter = new StreamWriter(path, true))
            {
                writter.WriteLine("----------------" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "----------------------", Environment.NewLine);
                writter.WriteLine("-" + Metodo + "-", Environment.NewLine);
                writter.Write("Message: ", Environment.NewLine);
                writter.WriteLine(text, Environment.NewLine);
                writter.Close();
            }
        }
    }
}