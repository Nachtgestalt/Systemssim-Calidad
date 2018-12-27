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
    public class AuditoriaLavanderiaController : ApiController
    {
        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

        /// <summary>
        /// Registra una nueva auditoría de Lavandería
        /// </summary>
        /// <param name="OT"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaLavanderia/NuevaAuditoriaLavanderia")]
        public HttpResponseMessage NuevaAuditoriaLavanderia([FromBody]AuditoriaCorteController.REQ_NEW_OT OT)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities())
                    {
                        Models.Auditoria auditoria = new Models.Auditoria()
                        {
                            IdClienteRef = OT.IdClienteRef,
                            OrdenTrabajo = OT.OrdenTrabajo,
                            PO = OT.PO,
                            Tela = OT.Tela,
                            Marca = OT.Marca,
                            NumCortada = OT.NumCortada,
                            Lavado = OT.Lavado,
                            Estilo = OT.Estilo,
                            Planta = OT.Planta,
                            Ruta = OT.Ruta,
                            FechaRegistro = DateTime.Now,
                            IdUsuario = OT.IdUsuario,
                            Corte = false,
                            Tendido = false,
                            Lavanderia = false,
                            Terminado = true,
                            Confeccion = false,
                            ProcesosEspeciales = false
                        };
                        db.Auditorias.Add(auditoria);
                        db.SaveChanges();

                        List<DET_AUDITORIA_LAVANDERIA> Detalles = _objSerializer.Deserialize<List<DET_AUDITORIA_LAVANDERIA>>(OT.Det);

                        foreach (DET_AUDITORIA_LAVANDERIA item in Detalles)
                        {
                            Models.Auditoria_Lavanderia_Detalle auditoria_Lavanderia = new Models.Auditoria_Lavanderia_Detalle()
                            {
                                IdAuditoria = auditoria.IdAuditoria,
                                IdDefecto = item.IdDefecto,
                                IdPosicion = item.IdPosicion,
                                Cantidad = item.Cantidad,
                                IdOperacion = item.IdOperacion
                            };
                            db.Auditoria_Lavanderia_Detalle.Add(auditoria_Lavanderia);
                        }
                        db.SaveChanges();
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                }
                else
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Obtienen auditoría de Lavandería
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaLavanderia/ObtieneAuditoriaLavanderia")]
        public AuditoriaCorteController.RES_AUDITORIA ObtieneAuditoriaLavanderia()
        {
            AuditoriaCorteController.RES_AUDITORIA API = new AuditoriaCorteController.RES_AUDITORIA();
            try
            {
                API.RES = db.VST_AUDITORIA.Where(x => x.Lavanderia == true).ToList();
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.RES = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Genera el cierre de auditoria de corte
        /// </summary>
        /// <param name="IdAuditoria"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaLavanderia/CierreAuditoria")]
        public HttpResponseMessage CierreAuditoria(int IdAuditoria)
        {
            try
            {
                Models.Auditoria API = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria && x.Lavanderia == true).FirstOrDefault();
                API.FechaRegistroFin = DateTime.Now;
                db.Entry(API).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public partial class DET_AUDITORIA_LAVANDERIA
        {
            [Required]
            public int IdPosicion { get; set; }
            [Required]
            public int IdOperacion { get; set; }
            [Required]
            public int IdDefecto { get; set; }
            [Required]
            public int Cantidad { get; set; }
        }
    }
}
