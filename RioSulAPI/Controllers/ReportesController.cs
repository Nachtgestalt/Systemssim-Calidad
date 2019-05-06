using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using RioSulAPI.Class;
using RioSulAPI.Reportes;

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

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Reportes/Composturas")]
        public HttpResponseMessage ReporteComposturas(DateTime Fecha_i, DateTime Fecha_f)
        {
            dsReportes ds = new dsReportes();
            crComposturas cr = new crComposturas();

            string fecha = "", posicion_n = "", defecto_n = "";
            decimal total_composturas = 0, p_composturas = 0, cantidad = 0;

            var auditoria = db.VST_AUDITORIA.Where(x => x.Terminado == true && x.Activo == true && x.FechaRegistroFin != null);

            if (Fecha_i != Convert.ToDateTime("01/01/0001 12:00:00 a. m.") && Fecha_f != Convert.ToDateTime("01/01/0001 12:00:00 a. m."))
            {
                auditoria = auditoria.Where(x => DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                                     && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f);
            }

            var posicion = db.C_Posicion_Terminado.
                Join(db.Auditoria_Terminado_Detalle, x => x.ID, y => y.IdPosicion, (x, y) => new { x.ID, x.Clave, x.Nombre, x.Activo }).
                Where(x => x.Activo == true).Distinct().ToList();

            var defectos = db.C_Terminado.
                Join(db.Auditoria_Terminado_Detalle, x => x.ID, y => y.IdDefecto, (x, y) => new { x.ID, x.Clave, x.Nombre, x.Activo }).
                Where(x => x.Activo == true).Distinct().ToList();

            foreach(var item in posicion)
            {
                ds.dsPosiciones.AdddsPosicionesRow(item.Clave);
            }

            foreach(var item in auditoria)
            {
                foreach (var item2 in posicion)
                {
                    foreach (var item3 in defectos)
                    {
                        cantidad = db.Auditoria_Terminado_Detalle.Where(x => x.IdAuditoria == item.IdAuditoria && x.IdPosicion == item2.ID
                                    && x.IdDefecto == item3.ID).Select(x=> x.Cantidad).DefaultIfEmpty(0).Sum();
                        ds.dsTotales.AdddsTotalesRow(item.IdAuditoria, cantidad, item2.Clave, item3.Clave);
                    }
                }

                total_composturas = db.Auditoria_Terminado_Detalle.Where(x => x.IdAuditoria == item.IdAuditoria && x.Compostura == true).
                    Select(x => x.Cantidad).DefaultIfEmpty(0).Sum();
                p_composturas = (total_composturas * 100) / Convert.ToDecimal(item.NumCortada);

                fecha = item.FechaRegistroFin.GetValueOrDefault().ToString("dd/MMMM/yyyy");
                ds.dsGeneralC.AdddsGeneralCRow(fecha,item.OrdenTrabajo,item.Marca,item.Planta,item.NumCortada,"",total_composturas,p_composturas,item.IdAuditoria);
            }

            cr.SetDataSource(ds);
            MemoryStream stream = new MemoryStream();
            cr.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat).CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);

            HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            httpResponseMessage.Content = new StreamContent(stream);
            httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            httpResponseMessage.Content.Headers.ContentDisposition.FileName = "Ejemplo.pdf";
            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            return httpResponseMessage;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Reportes/ComposturasMarca")]
        public HttpResponseMessage ReporteComposturasM(DateTime Fecha_i, DateTime Fecha_f)
        {
            dsReportes ds = new dsReportes();
            crCompMar cr = new crCompMar();

            decimal cantidad = 0, tot_cortes = 0, c_cortada = 0, composturas = 0, p_composturas = 0;
            string fecha = "Periodo: " + Fecha_i.ToString("MMMM") + "-" + Fecha_f.ToString("MMMM-yyyy"); 

            var marcas = db.Auditorias.Where(x => x.Confeccion == true).Select(x => x.Marca).Distinct().ToList();
            var operacion = db.C_Conf_Confeccion.
                Join(db.Auditoria_Confeccion_Detalle, x => x.ID, y => y.IdOperacion, (x, y) => new { x.ID, x.Clave, x.Nombre, x.Activo, x.IdSubModulo }).
                Where(x => x.Activo == true && x.IdSubModulo == 9).Distinct().ToList();

            ds.dsPosiciones.AdddsPosicionesRow(fecha);

            foreach(var item in marcas)
            {
                ds.dsComp.AdddsCompRow(item);
                tot_cortes = 10;

                c_cortada = 20;

                composturas = db.Auditorias.
                        Join(db.Auditoria_Confeccion_Detalle, x => x.IdAuditoria, y => y.IdAuditoria,
                        (x, y) => new { x.Marca, x.Activo, x.Confeccion, x.FechaRegistro, y.IdOperacion, y.Cantidad }).
                        Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Marca == item).Select(x => x.Cantidad)
                        .DefaultIfEmpty(0).Sum();

                p_composturas = (composturas * 100) / c_cortada;

                ds.dsCompGen.AdddsCompGenRow(item, tot_cortes, c_cortada, composturas, p_composturas);

                foreach (var item2 in operacion)
                {
                    cantidad = db.Auditorias.
                        Join(db.Auditoria_Confeccion_Detalle, x=> x.IdAuditoria, y=> y.IdAuditoria, 
                        (x,y) => new {x.Marca, x.Activo, x.Confeccion, x.FechaRegistro ,y.IdOperacion, y.Cantidad }).
                        Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Marca == item && x.IdOperacion == item2.ID).Select(x=> x.Cantidad)
                        .DefaultIfEmpty(0).Sum();

                    ds.dsCompOp.AdddsCompOpRow(item,item2.Clave,cantidad);
                }
            }

            cr.SetDataSource(ds);
            MemoryStream stream = new MemoryStream();
            cr.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat).CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);

            HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            httpResponseMessage.Content = new StreamContent(stream);
            httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            httpResponseMessage.Content.Headers.ContentDisposition.FileName = "Ejemplo.pdf";
            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            return httpResponseMessage;
        }

        public partial class RES_REPORTES
        {
            public HttpResponseMessage Message { get; set; }
            public byte[] AUD { get; set; }
        }
    }
}
