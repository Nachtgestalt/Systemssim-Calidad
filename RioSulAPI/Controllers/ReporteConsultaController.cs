using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CrystalDecisions.CrystalReports.Engine;
using Microsoft.Ajax.Utilities;
using RioSulAPI.Models;
using RioSulAPI.Reportes;

namespace RioSulAPI.Controllers
{
    public class ReporteConsultaController : ApiController
    {
        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

        /// <summary>
        /// Genera el cierre de auditoria de terminado
        /// </summary>
        /// <param name="IdAuditoria"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [System.Web.Http.Route("api/ReporteConsulta/GetConsulta")]
        public A_RES_AUDIOTRIA GetConsulta([FromBody] C_CONSULTA Filtro)
        {
            A_RES_AUDIOTRIA API = new A_RES_AUDIOTRIA();
            API.Auditoria = new List<RES_AUDITORIA>();
            List<VST_AUDITORIA> consulta = new List<VST_AUDITORIA>();
            try
            {
                switch (Filtro.Auditoria)
                {
                    case "Calidad":
                        var aux = db.VST_AUDITORIA.
                            Where(x => x.Calidad == true && x.Activo == true);

                        if (Filtro.Fecha_i != Convert.ToDateTime("01/01/0001 12:00:00 a. m.") && Filtro.Fecha_f != Convert.ToDateTime("01/01/0001 12:00:00 a. m."))
                        {
                            aux = aux.Where(x => DbFunctions.TruncateTime(x.FechaRegistro) >= Filtro.Fecha_i
                                                 && DbFunctions.TruncateTime(x.FechaRegistro) <= Filtro.Fecha_f);
                        }
                        
                        if (Filtro.IdCliente != null)
                        {
                            int id = Convert.ToInt16(Filtro.IdCliente);
                            aux = aux.Where(x=> x.IdClienteRef == id);
                        }

                        if (Filtro.Marca != null)
                        {
                            aux = aux.Where(x => x.Marca == Filtro.Marca);
                        }

                        if (Filtro.PO != null)
                        {
                            aux = aux.Where(x => x.PO == Filtro.PO);
                        }

                        if (Filtro.Corte != null)
                        {
                            aux = aux.Where(x => x.NumCortada == Filtro.Corte);
                        }

                        if (Filtro.Planta != null)
                        {
                            aux = aux.Where(x => x.Planta == Filtro.Planta);
                        }

                        if (Filtro.Estilo != null)
                        {
                            aux = aux.Where(x => x.Estilo == Filtro.Estilo);
                        }

                        consulta = aux.OrderByDescending(x=> x.FechaRegistro).ToList();
                        foreach (var itemAuditoria in consulta)
                        {
                            RES_AUDITORIA A = new RES_AUDITORIA();
                            Models.Auditoria_Calidad_Detalle ACD = db.Auditoria_Calidad_Detalle
                                .Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).FirstOrDefault();

                            if (ACD != null)
                            {
                                A.pzas_r = db.Auditoria_Calidad_Detalle.Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).Select(x => x.Recup).DefaultIfEmpty(0).Sum();
                                A.pzas_c = db.Auditoria_Calidad_Detalle.Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).Select(x => x.Criterio).DefaultIfEmpty(0).Sum();
                                A.pzas_2 = db.Auditoria_Calidad_Detalle.Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).Select(x => x.Fin).DefaultIfEmpty(0).Sum();
                                A.total = A.pzas_2 + A.pzas_c + A.pzas_r;
                            }
                            else
                            {
                                A.pzas_r = 0;
                                A.pzas_c = 0;
                                A.pzas_2 = 0;
                                A.total = 0;
                            }

                           
                            Models.C_Clientes Cliente;
                            Cliente = db.C_Clientes.Where(x => x.IdClienteRef == itemAuditoria.IdClienteRef).FirstOrDefault();
                            A.Cliente = Cliente.Descripcion;

                            A.Marca = itemAuditoria.Marca;
                            A.IdAuditoria = itemAuditoria.IdAuditoria;
                            A.PO = itemAuditoria.PO;
                            A.Corte = itemAuditoria.NumCortada;
                            A.Planta = itemAuditoria.Planta;
                            A.Estilo = itemAuditoria.Estilo;
                            A.Fecha_i = itemAuditoria.FechaRegistro;
                            A.Fecha_f = itemAuditoria.FechaRegistroFin.GetValueOrDefault();
                            A.OT = itemAuditoria.OrdenTrabajo;

                            if (itemAuditoria.FechaRegistroFin == null)
                            {
                                A.status = "ACTIVA";
                            }
                            else
                            {
                                A.status = "CERRADA";
                            }

                            API.Auditoria.Add(A);
                        }
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                        break;

                    case "Terminado":
                        var aux2 = db.VST_AUDITORIA.
                            Where(x => x.Terminado == true && x.Activo == true);

                        if (Filtro.Fecha_i != Convert.ToDateTime("01/01/0001 12:00:00 a. m.") && Filtro.Fecha_f != Convert.ToDateTime("01/01/0001 12:00:00 a. m."))
                        {
                            aux2 = aux2.Where(x => DbFunctions.TruncateTime(x.FechaRegistro) >= Filtro.Fecha_i
                                                 && DbFunctions.TruncateTime(x.FechaRegistro) <= Filtro.Fecha_f);
                        }

                        if (Filtro.IdCliente != null)
                        {
                            int id = Convert.ToInt16(Filtro.IdCliente);
                            aux2 = aux2.Where(x => x.IdClienteRef == id);
                        }

                        if (Filtro.Marca != null)
                        {
                            aux2 = aux2.Where(x => x.Marca == Filtro.Marca);
                        }

                        if (Filtro.PO != null)
                        {
                            aux2 = aux2.Where(x => x.PO == Filtro.PO);
                        }

                        if (Filtro.Corte != null)
                        {
                            aux2 = aux2.Where(x => x.NumCortada == Filtro.Corte);
                        }

                        if (Filtro.Planta != null)
                        {
                            aux2 = aux2.Where(x => x.Planta == Filtro.Planta);
                        }

                        if (Filtro.Estilo != null)
                        {
                            aux2 = aux2.Where(x => x.Estilo == Filtro.Estilo);
                        }

                        consulta = aux2.OrderByDescending(x => x.FechaRegistro).ToList();
                        foreach (var itemAuditoria in consulta)
                        {
                            RES_AUDITORIA A = new RES_AUDITORIA();
                            Models.Auditoria_Terminado_Detalle ACD = db.Auditoria_Terminado_Detalle
                                .Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).FirstOrDefault();

                            if (ACD != null)
                            {
                                A.pzas_c = db.Auditoria_Terminado_Detalle.Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria && x.Compostura == true).Select(x => x.Cantidad).DefaultIfEmpty(0).Sum();
                                A.pzas_2 = db.Auditoria_Terminado_Detalle.Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria && x.Compostura == false).Select(x => x.Cantidad).DefaultIfEmpty(0).Sum();

                                A.total = A.pzas_c + A.pzas_2;
                            }
                            else
                            {
                                A.pzas_r = 0;
                                A.total = 0;
                            }


                            Models.C_Clientes Cliente;
                            Cliente = db.C_Clientes.Where(x => x.IdClienteRef == itemAuditoria.IdClienteRef).FirstOrDefault();
                            A.Cliente = Cliente.Descripcion;

                            A.Marca = itemAuditoria.Marca;
                            A.IdAuditoria = itemAuditoria.IdAuditoria;
                            A.PO = itemAuditoria.PO;
                            A.Corte = itemAuditoria.NumCortada;
                            A.Planta = itemAuditoria.Planta;
                            A.Estilo = itemAuditoria.Estilo;
                            A.Fecha_i = itemAuditoria.FechaRegistro;
                            A.Fecha_f = itemAuditoria.FechaRegistroFin.GetValueOrDefault();
                            A.OT = itemAuditoria.OrdenTrabajo;

                            if (itemAuditoria.FechaRegistroFin == null)
                            {
                                A.status = "ACTIVA";
                            }
                            else
                            {
                                A.status = "CERRADA";
                            }

                            API.Auditoria.Add(A);
                        }
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                        break;

                    case "Lavanderia":
                        var aux3 = db.VST_AUDITORIA.
                            Where(x => x.Lavanderia == true && x.Activo == true);

                        if (Filtro.Fecha_i != Convert.ToDateTime("01/01/0001 12:00:00 a. m.") && Filtro.Fecha_f != Convert.ToDateTime("01/01/0001 12:00:00 a. m."))
                        {
                            aux3 = aux3.Where(x => DbFunctions.TruncateTime(x.FechaRegistro) >= Filtro.Fecha_i
                                                 && DbFunctions.TruncateTime(x.FechaRegistro) <= Filtro.Fecha_f);
                        }

                        if (Filtro.IdCliente != null)
                        {
                            int id = Convert.ToInt16(Filtro.IdCliente);
                            aux3 = aux3.Where(x => x.IdClienteRef == id);
                        }

                        if (Filtro.Marca != null)
                        {
                            aux3 = aux3.Where(x => x.Marca == Filtro.Marca);
                        }

                        if (Filtro.PO != null)
                        {
                            aux3 = aux3.Where(x => x.PO == Filtro.PO);
                        }

                        if (Filtro.Corte != null)
                        {
                            aux3 = aux3.Where(x => x.NumCortada == Filtro.Corte);
                        }

                        if (Filtro.Planta != null)
                        {
                            aux3 = aux3.Where(x => x.Planta == Filtro.Planta);
                        }

                        if (Filtro.Estilo != null)
                        {
                            aux3 = aux3.Where(x => x.Estilo == Filtro.Estilo);
                        }

                        consulta = aux3.OrderByDescending(x => x.FechaRegistro).ToList();
                        foreach (var itemAuditoria in consulta)
                        {
                            RES_AUDITORIA A = new RES_AUDITORIA();
                            Models.Auditoria_Lavanderia_Detalle ACD = db.Auditoria_Lavanderia_Detalle
                                .Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).FirstOrDefault();

                            if (ACD != null)
                            {
                                A.pzas_r = db.Auditoria_Lavanderia_Detalle.Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).Select(x => x.Cantidad).DefaultIfEmpty(0).Sum();
                                A.total = A.pzas_r;
                            }
                            else
                            {
                                A.pzas_r = 0;
                                A.total = 0;
                            }


                            Models.C_Clientes Cliente;
                            Cliente = db.C_Clientes.Where(x => x.IdClienteRef == itemAuditoria.IdClienteRef).FirstOrDefault();
                            A.Cliente = Cliente.Descripcion;

                            A.Marca = itemAuditoria.Marca;
                            A.IdAuditoria = itemAuditoria.IdAuditoria;
                            A.PO = itemAuditoria.PO;
                            A.Corte = itemAuditoria.NumCortada;
                            A.Planta = itemAuditoria.Planta;
                            A.Estilo = itemAuditoria.Estilo;
                            A.Fecha_i = itemAuditoria.FechaRegistro;
                            A.Fecha_f = itemAuditoria.FechaRegistroFin.GetValueOrDefault();
                            A.OT = itemAuditoria.OrdenTrabajo;

                            if (itemAuditoria.FechaRegistroFin == null)
                            {
                                A.status = "ACTIVA";
                            }
                            else
                            {
                                A.status = "CERRADA";
                            }

                            API.Auditoria.Add(A);
                        }
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                        break;
                    case "ProcesosEspeciales":
                        var aux4 = db.VST_AUDITORIA.
                            Where(x => x.ProcesosEspeciales == true && x.Activo == true);

                        if (Filtro.Fecha_i != Convert.ToDateTime("01/01/0001 12:00:00 a. m.") && Filtro.Fecha_f != Convert.ToDateTime("01/01/0001 12:00:00 a. m."))
                        {
                            aux4 = aux4.Where(x => DbFunctions.TruncateTime(x.FechaRegistro) >= Filtro.Fecha_i
                                                 && DbFunctions.TruncateTime(x.FechaRegistro) <= Filtro.Fecha_f);
                        }

                        if (Filtro.IdCliente != null)
                        {
                            int id = Convert.ToInt16(Filtro.IdCliente);
                            aux4 = aux4.Where(x => x.IdClienteRef == id);
                        }

                        if (Filtro.Marca != null)
                        {
                            aux4 = aux4.Where(x => x.Marca == Filtro.Marca);
                        }

                        if (Filtro.PO != null)
                        {
                            aux4 = aux4.Where(x => x.PO == Filtro.PO);
                        }

                        if (Filtro.Corte != null)
                        {
                            aux4 = aux4.Where(x => x.NumCortada == Filtro.Corte);
                        }

                        if (Filtro.Planta != null)
                        {
                            aux4 = aux4.Where(x => x.Planta == Filtro.Planta);
                        }

                        if (Filtro.Estilo != null)
                        {
                            aux4 = aux4.Where(x => x.Estilo == Filtro.Estilo);
                        }

                        consulta = aux4.OrderByDescending(x => x.FechaRegistro).ToList();
                        foreach (var itemAuditoria in consulta)
                        {
                            RES_AUDITORIA A = new RES_AUDITORIA();
                            Models.Auditoria_Proc_Esp_Detalle ACD = db.Auditoria_Proc_Esp_Detalle
                                .Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).FirstOrDefault();

                            if (ACD != null)
                            {
                                A.pzas_r = db.Auditoria_Proc_Esp_Detalle.Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).Select(x => x.Cantidad).DefaultIfEmpty(0).Sum();
                                A.total = A.pzas_r;
                            }
                            else
                            {
                                A.pzas_r = 0;
                                A.total = 0;
                            }


                            Models.C_Clientes Cliente;
                            Cliente = db.C_Clientes.Where(x => x.IdClienteRef == itemAuditoria.IdClienteRef).FirstOrDefault();
                            A.Cliente = Cliente.Descripcion;

                            A.Marca = itemAuditoria.Marca;
                            A.IdAuditoria = itemAuditoria.IdAuditoria;
                            A.PO = itemAuditoria.PO;
                            A.Corte = itemAuditoria.NumCortada;
                            A.Planta = itemAuditoria.Planta;
                            A.Estilo = itemAuditoria.Estilo;
                            A.Fecha_i = itemAuditoria.FechaRegistro;
                            A.Fecha_f = itemAuditoria.FechaRegistroFin.GetValueOrDefault();
                            A.OT = itemAuditoria.OrdenTrabajo;

                            if (itemAuditoria.FechaRegistroFin == null)
                            {
                                A.status = "ACTIVA";
                            }
                            else
                            {
                                A.status = "CERRADA";
                            }

                            API.Auditoria.Add(A);
                        }
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);

                        break;
                }
            }
            catch (Exception e)
            {
                API.Auditoria = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }


            return API;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idAuditoria"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [System.Web.Http.Route("api/ReporteConsulta/GetConsulta")]
        public HttpResponseMessage createPdf(int idAuditoria, string auditoria = "", string tipo = "")
        {
            dsConsulta ds = new dsConsulta();
            ReportDocument cr = new ReportDocument();

            Models.Auditoria auditoria_gen = new Auditoria();
            List<Models.VST_AUDITORIA_TERMINADO_DETALLE> auditoria_t = new List<VST_AUDITORIA_TERMINADO_DETALLE>();
            List<Models.VST_AUDITORIA_CALIDAD_DETALLE> auditoria_c = new List<VST_AUDITORIA_CALIDAD_DETALLE>();
            List<Models.VST_AUDITORIA_LAVANDERIA_DETALLE> auditoria_l = new List<VST_AUDITORIA_LAVANDERIA_DETALLE>();
            List<Models.VST_AUDITORIA_PROC_ESP_DETALLE> auditoria_pe = new List<VST_AUDITORIA_PROC_ESP_DETALLE>();

            Models.C_Clientes clientes = new C_Clientes();

            switch (auditoria)
            {
                case "Terminado":
                    auditoria_gen = db.Auditorias.Where(x => x.IdAuditoria == idAuditoria).FirstOrDefault();
                    clientes = db.C_Clientes.Where(x => x.IdClienteRef == auditoria_gen.IdClienteRef).FirstOrDefault();

                    if (tipo == "Compostura")
                    {
                        auditoria_t = db.VST_AUDITORIA_TERMINADO_DETALLE
                            .Where(x => x.IdAuditoria == idAuditoria && x.Compostura == true).ToList();
                    }
                    else
                    {
                        auditoria_t = db.VST_AUDITORIA_TERMINADO_DETALLE
                            .Where(x => x.IdAuditoria == idAuditoria && x.Compostura == false).ToList();
                    }

                    foreach (VST_AUDITORIA_TERMINADO_DETALLE item in auditoria_t)
                    {
                        ds.dtDetalle.AdddtDetalleRow(item.Defecto, item.Operacion, item.Posicion, item.Origen,
                            item.Cantidad, item.Nota);
                    }

                    cr.Load(HostingEnvironment.MapPath("~/Reportes/crConsulta.rpt"));

                    break;
                case "Calidad":
                    auditoria_gen = db.Auditorias.Where(x => x.IdAuditoria == idAuditoria).FirstOrDefault();
                    clientes = db.C_Clientes.Where(x => x.IdClienteRef == auditoria_gen.IdClienteRef).FirstOrDefault();

                    auditoria_c = db.VST_AUDITORIA_CALIDAD_DETALLE
                        .Where(x => x.IdAuditoria == idAuditoria).ToList();

                    foreach (VST_AUDITORIA_CALIDAD_DETALLE item in auditoria_c)
                    {
                        ds.dtDetalleC.AdddtDetalleCRow(item.Defecto, item.Operacion, item.Posicion, item.Origen,
                            item.Recup, item.Criterio, item.Fin, item.Nota);
                    }

                    cr.Load(HostingEnvironment.MapPath("~/Reportes/crConsultaCalidad.rpt"));
                    break;
                case "Lavanderia":
                    auditoria_gen = db.Auditorias.Where(x => x.IdAuditoria == idAuditoria).FirstOrDefault();
                    clientes = db.C_Clientes.Where(x => x.IdClienteRef == auditoria_gen.IdClienteRef).FirstOrDefault();

                    auditoria_l = db.VST_AUDITORIA_LAVANDERIA_DETALLE.Where(x => x.IdAuditoria == idAuditoria).ToList();

                    foreach (VST_AUDITORIA_LAVANDERIA_DETALLE item in auditoria_l)
                    {
                        ds.dtDetalle.AdddtDetalleRow(item.NombreDefecto, item.NombreOperacion, item.NombrePosicion, "",
                            item.Cantidad, item.Nota);
                    }

                    cr.Load(HostingEnvironment.MapPath("~/Reportes/crConsultaL.rpt"));

                    break;
                case "ProcesosEspeciales":
                    auditoria_gen = db.Auditorias.Where(x => x.IdAuditoria == idAuditoria).FirstOrDefault();
                    clientes = db.C_Clientes.Where(x => x.IdClienteRef == auditoria_gen.IdClienteRef).FirstOrDefault();

                    auditoria_pe = db.VST_AUDITORIA_PROC_ESP_DETALLE.Where(x => x.IdAuditoria == idAuditoria).ToList();

                    foreach (VST_AUDITORIA_PROC_ESP_DETALLE item in auditoria_pe)
                    {
                        ds.dtDetalle.AdddtDetalleRow(item.NombreDefecto, item.NombreOperacion, item.NombrePosicion, "",
                            item.Cantidad, item.Notas);
                    }

                    cr.Load(HostingEnvironment.MapPath("~/Reportes/crConsultaL.rpt"));
                    break;
            }

            if (auditoria_gen != null)
            {
                ds.dtGeneral.AdddtGeneralRow(auditoria_gen.OrdenTrabajo, clientes.Descripcion, auditoria_gen.Marca,
                    auditoria_gen.Estilo, auditoria_gen.PO,
                    "", auditoria_gen.NumCortada, auditoria_gen.Planta, auditoria_gen.Tela, auditoria_gen.Lavado,
                    auditoria_gen.Ruta, "REPORTE DE " + tipo.ToUpper() + " " + auditoria.ToUpper());                
            }

            cr.SetDataSource(ds);

            
            MemoryStream stream = new MemoryStream();
            cr.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat).CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);

            //FileStream fs = new FileStream(@"C:\Imagen\ejemplo.pdf",FileMode.OpenOrCreate);
            //fs.Write(stream.ToArray(),0,stream.ToArray().Length);
            //fs.Close();

            HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            httpResponseMessage.Content = new StreamContent(stream);
            httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            httpResponseMessage.Content.Headers.ContentDisposition.FileName = "Ejemplo.pdf";
            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            return httpResponseMessage;
        }

        public partial class C_CONSULTA
        {
            [Required] public DateTime Fecha_i { get; set; }
            [Required] public DateTime Fecha_f { get; set; }
            public string IdCliente { get; set; }
            public string Marca { get; set; }
            public string PO { get; set; }
            public string Corte { get; set; }
            public string Planta { get; set; }
            public string Estilo { get; set; }
            public string Auditoria { get; set; }
        }

        public partial class RES_AUDITORIA
        {
            public string Cliente { get; set; }
            public string Marca { get; set; }
            public string PO { get; set; }
            public string Corte { get; set; }
            public string Planta { get; set; }
            public string Estilo { get; set; }
            public DateTime Fecha_i { get; set; }
            public DateTime Fecha_f { get; set; }
            public int pzas_r { get; set; }
            public int pzas_c { get; set; }
            public int pzas_2 { get; set; }
            public int total { get; set; }
            public string status { get; set; }
            public int IdAuditoria { get; set; }
            public string OT { get; set; }
        }

        public partial class A_RES_AUDIOTRIA
        {
            public List<RES_AUDITORIA> Auditoria { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

    }
}
