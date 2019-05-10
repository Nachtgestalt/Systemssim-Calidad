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

#region COMPOSTURAS X MARCA
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
            ds.dsTitulo.AdddsTituloRow("TOTAL COMPOSTURAS X MARCA");

            foreach(var item in marcas)
            {
                c_cortada = 0;
                ds.dsComp.AdddsCompRow(item);
                tot_cortes = db.Auditorias.Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Marca == item).Select(x=> x.IdAuditoria).Count();

                var NumCort = db.Auditorias.Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Marca == item).Select(x => x.NumCortada).ToList();

                foreach(string item2 in NumCort)
                {
                    c_cortada += Convert.ToDecimal(item2);
                }

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

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Reportes/ComposturasMarcaGrafico")]
        public HttpResponseMessage ReporteComposturasMG(DateTime Fecha_i, DateTime Fecha_f)
        {
            dsReportes ds = new dsReportes();
            crCompMarG cr = new crCompMarG();

            decimal tot_cortes = 0, c_cortada = 0, composturas = 0, p_composturas = 0;
            string fecha = "Periodo: " + Fecha_i.ToString("MMMM") + "-" + Fecha_f.ToString("MMMM-yyyy");

            var marcas = db.Auditorias.Where(x => x.Confeccion == true).Select(x => x.Marca).Distinct().ToList();

            ds.dsPosiciones.AdddsPosicionesRow(fecha);
            ds.dsTitulo.AdddsTituloRow("PORCENTAJE DE COMPOSTURAS POR MARCA");

            foreach (var item in marcas)
            {
                c_cortada = 0;
                ds.dsComp.AdddsCompRow(item);
                tot_cortes = db.Auditorias.Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Marca == item).Select(x => x.IdAuditoria).Count();

                var NumCort = db.Auditorias.Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Marca == item).Select(x => x.NumCortada).ToList();

                foreach (string item2 in NumCort)
                {
                    c_cortada += Convert.ToDecimal(item2);
                }

                composturas = db.Auditorias.
                        Join(db.Auditoria_Confeccion_Detalle, x => x.IdAuditoria, y => y.IdAuditoria,
                        (x, y) => new { x.Marca, x.Activo, x.Confeccion, x.FechaRegistro, y.IdOperacion, y.Cantidad }).
                        Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Marca == item).Select(x => x.Cantidad)
                        .DefaultIfEmpty(0).Sum();

                p_composturas = (composturas * 100) / c_cortada;

                ds.dsCompGen.AdddsCompGenRow(item, tot_cortes, c_cortada, composturas, p_composturas);
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
        [Route("api/Reportes/ComposturasMarca2")]
        public HttpResponseMessage ReporteComposturasM2(DateTime Fecha_i, DateTime Fecha_f)
        {
            dsReportes ds = new dsReportes();
            crMarcaPlanta cr = new crMarcaPlanta();

            decimal cantidad = 0, tot_cortes = 0, c_cortada = 0, composturas = 0, p_composturas = 0;
            string fecha = "Periodo: " + Fecha_i.ToString("MMMM") + "-" + Fecha_f.ToString("MMMM-yyyy");

            ds.dsPosiciones.AdddsPosicionesRow(fecha);
            ds.dsTitulo.AdddsTituloRow("TOTAL COMPOSTURAS X MARCA");

            var marcas = db.Auditorias.Where(x => x.Confeccion == true).Select(x => x.Marca).Distinct().ToList();
            var operaciones = db.C_Conf_Confeccion.
                Join(db.Auditoria_Confeccion_Detalle, x => x.ID, y => y.IdOperacion, (x, y) => new { x.ID, x.Clave, x.Nombre, x.Activo, x.IdSubModulo }).
                Where(x => x.Activo == true && x.IdSubModulo == 9).Distinct().ToList();
            var defectos = db.C_Conf_Confeccion.
                Join(db.Auditoria_Confeccion_Detalle, x => x.ID, y => y.IdDefecto, (x, y) => new { x.ID, x.Clave, x.Nombre, x.Activo, x.IdSubModulo }).
                Where(x => x.Activo == true && x.IdSubModulo == 8).Distinct().ToList();

            foreach (var marca in marcas)
            {
                ds.dsComp.AdddsCompRow(marca);

                c_cortada = 0;
                tot_cortes = db.Auditorias.Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Marca == marca).Select(x => x.IdAuditoria).Count();

                var NumCort = db.Auditorias.Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Marca == marca).Select(x => x.NumCortada).ToList();

                foreach (string item2 in NumCort)
                {
                    c_cortada += Convert.ToDecimal(item2);
                }

                composturas = db.Auditorias.
                        Join(db.Auditoria_Confeccion_Detalle, x => x.IdAuditoria, y => y.IdAuditoria,
                        (x, y) => new { x.Marca, x.Activo, x.Confeccion, x.FechaRegistro, y.IdOperacion, y.Cantidad }).
                        Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Marca == marca).Select(x => x.Cantidad)
                        .DefaultIfEmpty(0).Sum();

                p_composturas = (composturas * 100) / c_cortada;

                ds.dsCompGen.AdddsCompGenRow(marca, tot_cortes, c_cortada, composturas, p_composturas);

                foreach (var operacion in operaciones)
                {
                    ds.dsOperacion.AdddsOperacionRow(marca, operacion.Clave);
                    foreach (var defecto in defectos)
                    {
                        cantidad = db.Auditorias.
                        Join(db.Auditoria_Confeccion_Detalle, x => x.IdAuditoria, y => y.IdAuditoria,
                        (x, y) => new { x.Marca, x.Activo, x.Confeccion, x.FechaRegistro, y.IdOperacion, y.Cantidad, y.IdDefecto }).
                        Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Marca == marca && x.IdOperacion == operacion.ID
                        && x.IdDefecto == defecto.ID).Select(x => x.Cantidad).DefaultIfEmpty(0).Sum();

                        ds.dsDefectos.AdddsDefectosRow(operacion.Clave, defecto.Clave, cantidad, marca);
                    }
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
        #endregion

#region COMPOSTURAS X PLANTA
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Reportes/ComposturasPlanta")]
        public HttpResponseMessage ReporteComposturasP(DateTime Fecha_i, DateTime Fecha_f)
        {
            dsReportes ds = new dsReportes();
            crCompMar cr = new crCompMar();

            decimal cantidad = 0, tot_cortes = 0, c_cortada = 0, composturas = 0, p_composturas = 0;
            string fecha = "Periodo: " + Fecha_i.ToString("MMMM") + "-" + Fecha_f.ToString("MMMM-yyyy");

            var planta = db.Auditorias.Where(x => x.Confeccion == true).Select(x => x.Planta).Distinct().ToList();
            var operacion = db.C_Conf_Confeccion.
                Join(db.Auditoria_Confeccion_Detalle, x => x.ID, y => y.IdOperacion, (x, y) => new { x.ID, x.Clave, x.Nombre, x.Activo, x.IdSubModulo }).
                Where(x => x.Activo == true && x.IdSubModulo == 9).Distinct().ToList();

            ds.dsPosiciones.AdddsPosicionesRow(fecha);
            ds.dsTitulo.AdddsTituloRow("TOTAL COMPOSTURAS X PLANTA");

            foreach (var item in planta)
            {
                c_cortada = 0;
                ds.dsComp.AdddsCompRow(item);
                tot_cortes = db.Auditorias.Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Planta == item).Select(x => x.IdAuditoria).Count();

                var NumCort = db.Auditorias.Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Planta == item).Select(x => x.NumCortada).ToList();

                foreach (string item2 in NumCort)
                {
                    c_cortada += Convert.ToDecimal(item2);
                }

                composturas = db.Auditorias.
                        Join(db.Auditoria_Confeccion_Detalle, x => x.IdAuditoria, y => y.IdAuditoria,
                        (x, y) => new { x.Planta, x.Activo, x.Confeccion, x.FechaRegistro, y.IdOperacion, y.Cantidad }).
                        Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Planta == item).Select(x => x.Cantidad)
                        .DefaultIfEmpty(0).Sum();

                p_composturas = (composturas * 100) / c_cortada;

                ds.dsCompGen.AdddsCompGenRow(item, tot_cortes, c_cortada, composturas, p_composturas);

                foreach (var item2 in operacion)
                {
                    cantidad = db.Auditorias.
                        Join(db.Auditoria_Confeccion_Detalle, x => x.IdAuditoria, y => y.IdAuditoria,
                        (x, y) => new { x.Planta, x.Activo, x.Confeccion, x.FechaRegistro, y.IdOperacion, y.Cantidad }).
                        Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Planta == item && x.IdOperacion == item2.ID).Select(x => x.Cantidad)
                        .DefaultIfEmpty(0).Sum();

                    ds.dsCompOp.AdddsCompOpRow(item, item2.Clave, cantidad);
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

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Reportes/ComposturasPlantaGrafico")]
        public HttpResponseMessage ReporteComposturasPG(DateTime Fecha_i, DateTime Fecha_f)
        {
            dsReportes ds = new dsReportes();
            crCompMarG cr = new crCompMarG();

            decimal tot_cortes = 0, c_cortada = 0, composturas = 0, p_composturas = 0;
            string fecha = "Periodo: " + Fecha_i.ToString("MMMM") + "-" + Fecha_f.ToString("MMMM-yyyy");

            var plantas = db.Auditorias.Where(x => x.Confeccion == true).Select(x => x.Planta).Distinct().ToList();

            ds.dsPosiciones.AdddsPosicionesRow(fecha);
            ds.dsTitulo.AdddsTituloRow("PORCENTAJE DE COMPOSTURAS POR PLANTA");
            foreach (var item in plantas)
            {
                c_cortada = 0;
                ds.dsComp.AdddsCompRow(item);
                tot_cortes = db.Auditorias.Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Planta == item).Select(x => x.IdAuditoria).Count();

                var NumCort = db.Auditorias.Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Planta == item).Select(x => x.NumCortada).ToList();

                foreach (string item2 in NumCort)
                {
                    c_cortada += Convert.ToDecimal(item2);
                }

                composturas = db.Auditorias.
                        Join(db.Auditoria_Confeccion_Detalle, x => x.IdAuditoria, y => y.IdAuditoria,
                        (x, y) => new { x.Planta, x.Activo, x.Confeccion, x.FechaRegistro, y.IdOperacion, y.Cantidad }).
                        Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Planta == item).Select(x => x.Cantidad)
                        .DefaultIfEmpty(0).Sum();

                p_composturas = (composturas * 100) / c_cortada;

                ds.dsCompGen.AdddsCompGenRow(item, tot_cortes, c_cortada, composturas, p_composturas);
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
        [Route("api/Reportes/ComposturasPlanta2")]
        public HttpResponseMessage ReporteComposturasP2(DateTime Fecha_i, DateTime Fecha_f)
        {
            dsReportes ds = new dsReportes();
            crMarcaPlanta cr = new crMarcaPlanta();

            decimal cantidad = 0, tot_cortes = 0, c_cortada = 0, composturas = 0, p_composturas = 0;
            string fecha = "Periodo: " + Fecha_i.ToString("MMMM") + "-" + Fecha_f.ToString("MMMM-yyyy");

            ds.dsPosiciones.AdddsPosicionesRow(fecha);
            ds.dsTitulo.AdddsTituloRow("TOTAL COMPOSTURAS X PLANTA");

            var plantas = db.Auditorias.Where(x => x.Confeccion == true).Select(x => x.Planta).Distinct().ToList();
            var operaciones = db.C_Conf_Confeccion.
                Join(db.Auditoria_Confeccion_Detalle, x => x.ID, y => y.IdOperacion, (x, y) => new { x.ID, x.Clave, x.Nombre, x.Activo, x.IdSubModulo }).
                Where(x => x.Activo == true && x.IdSubModulo == 9).Distinct().ToList();
            var defectos = db.C_Conf_Confeccion.
                Join(db.Auditoria_Confeccion_Detalle, x => x.ID, y => y.IdDefecto, (x, y) => new { x.ID, x.Clave, x.Nombre, x.Activo, x.IdSubModulo }).
                Where(x => x.Activo == true && x.IdSubModulo == 8).Distinct().ToList();

            foreach (var planta in plantas)
            {
                ds.dsComp.AdddsCompRow(planta);

                c_cortada = 0;
                tot_cortes = db.Auditorias.Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Planta == planta).Select(x => x.IdAuditoria).Count();

                var NumCort = db.Auditorias.Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Planta == planta).Select(x => x.NumCortada).ToList();

                foreach (string item2 in NumCort)
                {
                    c_cortada += Convert.ToDecimal(item2);
                }

                composturas = db.Auditorias.
                        Join(db.Auditoria_Confeccion_Detalle, x => x.IdAuditoria, y => y.IdAuditoria,
                        (x, y) => new { x.Planta, x.Activo, x.Confeccion, x.FechaRegistro, y.IdOperacion, y.Cantidad }).
                        Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Planta == planta).Select(x => x.Cantidad)
                        .DefaultIfEmpty(0).Sum();

                p_composturas = (composturas * 100) / c_cortada;

                ds.dsCompGen.AdddsCompGenRow(planta, tot_cortes, c_cortada, composturas, p_composturas);

                foreach (var operacion in operaciones)
                {
                    ds.dsOperacion.AdddsOperacionRow(planta, operacion.Clave);
                    foreach (var defecto in defectos)
                    {
                        cantidad = db.Auditorias.
                        Join(db.Auditoria_Confeccion_Detalle, x => x.IdAuditoria, y => y.IdAuditoria,
                        (x, y) => new { x.Planta, x.Activo, x.Confeccion, x.FechaRegistro, y.IdOperacion, y.Cantidad, y.IdDefecto }).
                        Where(x => x.Confeccion == true && x.Activo == true && DbFunctions.TruncateTime(x.FechaRegistro) >= Fecha_i
                        && DbFunctions.TruncateTime(x.FechaRegistro) <= Fecha_f && x.Planta == planta && x.IdOperacion == operacion.ID
                        && x.IdDefecto == defecto.ID).Select(x => x.Cantidad).DefaultIfEmpty(0).Sum();

                        ds.dsDefectos.AdddsDefectosRow(operacion.Clave, defecto.Clave, cantidad, planta);
                    }
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
        #endregion

        #region CORTE X PULGADAS

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Reportes/Corte")]
        public HttpResponseMessage ReporteCorte(DateTime Fecha_i, DateTime Fecha_f)
        {
            dsReportes ds = new dsReportes();
            crComposturas cr = new crComposturas();
            List<TOLERANCIA> tolerancias = new List<TOLERANCIA>();

            var posiciones = db.C_Cort_Cortadores.
                Join(db.Auditoria_Tendido_Detalle, x => x.ID, y => y.IdPosicion, (x, y) => new { x.ID, x.Clave, x.Nombre, x.Activo, x.IdSubModulo }).
                Where(x => x.Activo == true && x.IdSubModulo == 7).Distinct().ToList();

            //TOLERANCIA NEGATIVA
            var tolerancia_aux = db.C_Tolerancia_Corte.Where(x => x.ToleranciaNegativa == true && x.ToleranciaPositiva == false).
                OrderBy(x=> x.Denominador).ToList();

            foreach(var item in tolerancia_aux)
            {
                TOLERANCIA tol = new TOLERANCIA()
                {
                    ID = item.IdTolerancia,
                    Tolerancia = "-" + item.Numerador + "/" + item.Denominador
                };
                tolerancias.Add(tol);
            }

            //TOLERANCIA NEGATIVA Y POSITIVA
            var tolerancia_aux2 = db.C_Tolerancia_Corte.Where(x => x.ToleranciaNegativa == true && x.ToleranciaPositiva == true).
                OrderBy(x => x.Denominador).ToList();

            foreach (var item in tolerancia_aux2)
            {
                TOLERANCIA tol = new TOLERANCIA()
                {
                    ID = item.IdTolerancia,
                    Tolerancia = "+/-" + item.Numerador + "/" + item.Denominador
                };
                tolerancias.Add(tol);
            }

            //TOLERANCIA POSITIVA
            var tolerancia_aux3 = db.C_Tolerancia_Corte.Where(x => x.ToleranciaNegativa == false && x.ToleranciaPositiva == true).
                OrderBy(x => x.Denominador).ToList();

            foreach (var item in tolerancia_aux3)
            {
                TOLERANCIA tol = new TOLERANCIA()
                {
                    ID = item.IdTolerancia,
                    Tolerancia = item.Numerador + "/" + item.Denominador
                };
                tolerancias.Add(tol);
            }

            //RELACION ENTRE TOLERANCIA Y POSICIONES


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

        #endregion

        public partial class RES_REPORTES
        {
            public HttpResponseMessage Message { get; set; }
            public byte[] AUD { get; set; }
        }

        public partial class TOLERANCIA
        {
            public int ID { get; set; }
            public string Tolerancia { get; set; }
        }
    }
}
