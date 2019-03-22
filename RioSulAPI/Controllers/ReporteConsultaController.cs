using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using Microsoft.Ajax.Utilities;
using RioSulAPI.Models;

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
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/ReporteConsulta/GetConsulta")]
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
                            Where(x => x.Calidad == true);

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

                        consulta = aux.ToList();
                        foreach (var itemAuditoria in consulta)
                        {
                            RES_AUDITORIA A = new RES_AUDITORIA();
                            Models.Auditoria_Calidad_Detalle ACD = db.Auditoria_Calidad_Detalle
                                .Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).FirstOrDefault();

                            if (ACD != null)
                            {
                                A.pzas_r = db.Auditoria_Calidad_Detalle.Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).Sum(x => x.Recup);
                                A.pzas_c = db.Auditoria_Calidad_Detalle.Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).Sum(x => x.Criterio);
                                A.pzas_2 = db.Auditoria_Calidad_Detalle.Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).Sum(x => x.Fin);
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
                            Where(x => x.Terminado == true);

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

                        consulta = aux2.ToList();
                        foreach (var itemAuditoria in consulta)
                        {
                            RES_AUDITORIA A = new RES_AUDITORIA();
                            Models.Auditoria_Terminado_Detalle ACD = db.Auditoria_Terminado_Detalle
                                .Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).FirstOrDefault();

                            if (ACD != null)
                            {
                                A.pzas_r = db.Auditoria_Terminado_Detalle.Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).Sum(x => x.Cantidad);
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
                            Where(x => x.Terminado == true);

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

                        consulta = aux3.ToList();
                        foreach (var itemAuditoria in consulta)
                        {
                            RES_AUDITORIA A = new RES_AUDITORIA();
                            Models.Auditoria_Lavanderia_Detalle ACD = db.Auditoria_Lavanderia_Detalle
                                .Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).FirstOrDefault();

                            if (ACD != null)
                            {
                                A.pzas_r = db.Auditoria_Lavanderia_Detalle.Where(x => x.IdAuditoria == itemAuditoria.IdAuditoria).Sum(x => x.Cantidad);
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
        }

        public partial class A_RES_AUDIOTRIA
        {
            public List<RES_AUDITORIA> Auditoria { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

    }
}
