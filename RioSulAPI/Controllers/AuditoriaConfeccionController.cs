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
    public class AuditoriaConfeccionController : ApiController
    {
        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

        /// <summary>
        /// Registra nueva auditoria confección
        /// </summary>
        /// <param name="OT"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaConfeccion/NuevaAuditoriaConfeccion")]
        public HttpResponseMessage NuevaAuditoriaConfeccion([FromBody]AuditoriaCorteController.REQ_NEW_OT OT)
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
                            Terminado = false,
                            Confeccion = true,
                            ProcesosEspeciales = false,
                            Calidad = false,
                            Activo = true
                        };
                        db.Auditorias.Add(auditoria);
                        db.SaveChanges();

                        List<DET_AUDITORIA_CONFECCION> Detalles = _objSerializer.Deserialize<List<DET_AUDITORIA_CONFECCION>>(OT.Det);
                        foreach (DET_AUDITORIA_CONFECCION item in Detalles)
                        {
                            Models.Auditoria_Confeccion_Detalle auditoria_Confeccion = new Models.Auditoria_Confeccion_Detalle()
                            {
                                IdAuditoria = auditoria.IdAuditoria,
                                IdArea = item.IdArea,
                                IdOperacion = item.IdOperacion,
                                IdDefecto = item.IdDefecto,
                                Cantidad = item.Cantidad
                            };
                            db.Auditoria_Confeccion_Detalle.Add(auditoria_Confeccion);
                        }
                        db.SaveChanges();
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Obtiene auditoría de confección
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaConfeccion/ObtieneAuditoriaConfeccion")]
        public AuditoriaCorteController.RES_AUDITORIA ObtieneAuditoriaConfeccion()
        {
            AuditoriaCorteController.RES_AUDITORIA API = new AuditoriaCorteController.RES_AUDITORIA();
            try
            {
                API.RES = db.VST_AUDITORIA.Where(x => x.Confeccion == true).ToList();
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                API.RES = null;
            }
            return API;
        }

        /// <summary>
        /// Genera el cierre de auditoria de corte confeccion
        /// </summary>
        /// <param name="IdAuditoria"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaConfeccion/CierreAuditoria")]
        public HttpResponseMessage CierreAuditoria(int IdAuditoria)
        {
            try
            {
                Models.Auditoria API = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria && x.Confeccion == true).FirstOrDefault();
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

        public partial class DET_AUDITORIA_CONFECCION
        {
            [Required]
            public int IdArea { get; set; }
            [Required]
            public int IdOperacion { get; set; }
            [Required]
            public int IdDefecto { get; set; }
            [Required]
            public int Cantidad { get; set; }
        }
    }
}