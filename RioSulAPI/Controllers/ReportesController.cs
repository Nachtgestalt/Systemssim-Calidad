using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using RioSulAPI.Class;

namespace RioSulAPI.Controllers
{
    public class ReportesController : ApiController
    {

        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

        /// <summary>
        /// Filtra la información por submodulos
        /// </summary>
        /// <param name="FechaInicial"></param>
        /// <param name="FechaFinal"></param>
        /// <param name="IdCliente"></param>
        /// <param name="Marca"></param>
        /// <param name="NumPo"></param>
        /// <param name="NoCorte"></param>
        /// <param name="Planta"></param>
        /// <param name="IdSubModulo"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Reportes/WipCorte")]
        public RES_REPORTES WipCorte(string FechaInicial = "", string FechaFinal = "", string IdCliente = "", string Marca = "", string NumPo = "", string NoCorte = "", string Planta = "", int IdSubModulo = 0)
        {
            RES_REPORTES API = new RES_REPORTES();
            string NameXlsx = DateTime.Now.ToString("yyyy_MM_dd_HHmmss");
            try
            {
                List<Models.VST_AUDITORIA> AUD = db.VST_AUDITORIA.ToList();

                System.Globalization.CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es-MX");

                Microsoft.Office.Interop.Excel._Application excel_Terminado = new Microsoft.Office.Interop.Excel.Application();
                excel_Terminado.DisplayAlerts = false;
                Microsoft.Office.Interop.Excel._Worksheet worksheet = null;
                Microsoft.Office.Interop.Excel._Workbook workbook = excel_Terminado.Workbooks.Open(System.Web.HttpContext.Current.Server.MapPath(@"~\App_Data\Templates\WipAuditoria.xls"));

                try
                {
                    foreach (Microsoft.Office.Interop.Excel.Worksheet item in workbook.Worksheets)
                    {
                        if (item.Name == "Auditoria")
                        {
                            worksheet = workbook.Sheets["Auditoria"];
                            worksheet.Activate();
                            int IndexRow = 4;
                            foreach (Models.VST_AUDITORIA itemdb in AUD)
                            {
                                worksheet.Cells[IndexRow, 1] = itemdb.Descripcion;
                                worksheet.Cells[IndexRow, 2] = itemdb.Marca;
                                worksheet.Cells[IndexRow, 3] = itemdb.PO;
                                worksheet.Cells[IndexRow, 4] = itemdb.NumCortada;
                                //worksheet.Cells[IndexRow, 5] = itemdb.
                                worksheet.Cells[IndexRow, 6] = itemdb.FechaRegistro;
                                worksheet.Cells[IndexRow, 7] = itemdb.FechaRegistroFin;
                                worksheet.Cells[IndexRow, 8] = db.VST_AUDITORIA_CORTE_DETALLE.Where(x => x.IdAuditoriaCorte == itemdb.IdAuditoria).Count().ToString();
                                worksheet.Cells[IndexRow, 9] = db.VST_AUDITORIA_CORTE_DETALLE.Where(x => x.IdAuditoriaCorte == itemdb.IdAuditoria).Count().ToString();
                                if (itemdb.FechaRegistroFin == null)
                                    worksheet.Cells[IndexRow, 10] = "ABIERTA";
                                else
                                    worksheet.Cells[IndexRow, 10] = "CERRADA";
                                IndexRow++;
                            }
                        }
                        workbook.SaveAs(@"C:\Desarrollo\Temp\" + NameXlsx);
                    }
                }
                catch (Exception ex)
                {
                    Utilerias.EscribirLog(ex.ToString());
                    API.AUD = null;
                    API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    workbook.Close(false);
                    excel_Terminado.Quit();
                    excel_Terminado = null;
                }
                System.IO.FileStream fs = new System.IO.FileStream(@"C:\Desarrollo\Temp\" + NameXlsx + ".xls", System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite);
                API.AUD = new byte[fs.Length];
                int numBytesToRead = (int)fs.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    int n = fs.Read(API.AUD, numBytesRead, numBytesToRead);
                    if (n == 0)
                        break;
                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                numBytesToRead = API.AUD.Length;
                fs.Close();
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.AUD = null;
            }
            return API;
        }

        public partial class RES_REPORTES
        {
            public HttpResponseMessage Message { get; set; }
            public byte[] AUD { get; set; }
        }
    }
}
