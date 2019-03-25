﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using RioSulAPI.Class;
using RioSulAPI.Models;

namespace RioSulAPI.Controllers
{
    public class AuditoriaCalidadController : ApiController
    {
        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

        #region AUDITORIA

      
        /// <summary>
        /// Registra una nueva auditoría de Calidad
        /// </summary>
        /// <param name="OT"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCalidad/NuevaAuditoriaCalidad")]
        public HttpResponseMessage NuevaAuditoriaCalidad([FromBody]AuditoriaCalidadController.REQ_NEW_AT OT)
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
                            Confeccion = false,
                            ProcesosEspeciales = false,
                            Calidad = true,
                            Activo = true
                        };
                        db.Auditorias.Add(auditoria);
                        db.SaveChanges();

                        foreach (AuditoriaCalidadController.DET_AUDITORIA_CALIDAD item in OT.Det)
                        {
                            Models.Auditoria_Calidad_Detalle auditoria_calidad = new Models.Auditoria_Calidad_Detalle()
                            {
                                IdAuditoria = auditoria.IdAuditoria,
                                Id_Origen = item.IdOrigen,
                                IdPosicion = item.IdPosicion,
                                IdOperacion = item.IdOperacion,
                                IdDefecto = item.IdDefecto,
                                Recup = item.Recup,
                                Criterio = item.Criterio,
                                Fin = item.Fin,
                                Aud_Imagen = item.Imagen,
                                Archivo = item.Archivo,
                                Nota = item.Nota

                            };
                            db.Auditoria_Calidad_Detalle.Add(auditoria_calidad);
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
        /// Obtiene auditoría de Lavandería
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCalidad/ObtieneAuditoriaCalidad")]
        public AuditoriaCorteController.RES_AUDITORIA ObtieneAuditoriaTerminado()
        {
            AuditoriaCorteController.RES_AUDITORIA API = new AuditoriaCorteController.RES_AUDITORIA();
            try
            {
                API.RES = db.VST_AUDITORIA.Where(x => x.Calidad == true).ToList();
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.RES = null;
            }
            return API;
        }

        /// <summary>
        /// Obtenemos el detalle de la auditoria dependiendo el ID enviado
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCalidad/ObtieneAuditoriaDet")]
        public RES_AUDITORIA_T_DET ObtieneAuditoriaDet(int id)
        {
            RES_AUDITORIA_T_DET API = new RES_AUDITORIA_T_DET();

            try
            {
                API.RES = db.VST_AUDITORIA.Where(x => x.IdAuditoria == id && x.Calidad == true).FirstOrDefault();
                API.RES_DET = db.VST_AUDITORIA_CALIDAD_DETALLE.Where(x => x.IdAuditoria == id).ToList();
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                API.RES = null; API.RES_DET = null;
            }

            return API;
        }

        /// <summary>
        /// Genera el cierre de auditoria de terminado
        /// </summary>
        /// <param name="IdAuditoria"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCalidad/CierreAuditoria")]
        public HttpResponseMessage CierreAuditoria(int IdAuditoria)
        {
            try
            {
                Boolean notas = false;
                Models.Auditoria API = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria && x.Calidad == true).FirstOrDefault();
                API.FechaRegistroFin = DateTime.Now;
                db.Entry(API).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                List<Models.VST_CORREOS_AUDITORIA> correos = db.VST_CORREOS_AUDITORIA.Where(x => x.Calidad == true).ToList();
                List<Models.Auditoria_Calidad_Detalle> auditoria_det = db.Auditoria_Calidad_Detalle.Where(x => x.IdAuditoria == IdAuditoria).ToList();

                MailMessage mensaje = new MailMessage();

                mensaje.From = new MailAddress(System.Configuration.ConfigurationManager.AppSettings["Mail"].ToString());
                var password = System.Configuration.ConfigurationManager.AppSettings["Password"].ToString();

                foreach (VST_CORREOS_AUDITORIA item in correos)
                {
                    mensaje.To.Add(item.Email);
                }
                              
                var sub = "AUDITORÍA OT: " + API.OrdenTrabajo.ToUpper();
                var body = "Se ha cerrado la auditoría de la orden de trabajo con número de corte: "+ API.NumCortada.ToUpper() + " en el área de calidad.";

                foreach (Auditoria_Calidad_Detalle item in auditoria_det)
                {
                    if (item.Nota != "null" || item.Nota != "")
                    {
                        notas = true;
                    }
                }

                if (notas)
                {
                    body = body + " \n La Auditoría contiene NOTAS favor de revisar";
                }

                var smtp = new SmtpClient
                {
                    Host = System.Configuration.ConfigurationManager.AppSettings["Host"].ToString(),
                    Port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Port"].ToString()),
                    EnableSsl = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(mensaje.From.Address, password)
                };
                using (var mess = new MailMessage(mensaje.From.Address, mensaje.To.ToString())
                {
                    Subject = sub,
                    Body = body
                })
                {
                    smtp.Send(mess);
                }


                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// ACTUALIZAMOS EL DETALLE DE LA AUDITORIA
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCalidad/ActualizaAuditoriaDet")]
        public AuditoriaTerminadoController.MESSAGE ActualizaAuditoria([FromBody]ACT_DET_AUDITORIA_C AC)
        {
            AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

            try
            {
                if (ModelState.IsValid)
                {
                    List<Models.Auditoria_Calidad_Detalle> ATD = db.Auditoria_Calidad_Detalle.Where(x => x.IdAuditoria == AC.IdAuditoria).ToList();

                    foreach (Models.Auditoria_Calidad_Detalle item in ATD)
                    {
                        db.Auditoria_Calidad_Detalle.Remove(item);
                        db.SaveChanges();
                    }
                   

                    foreach (DET_AUDITORIA_CALIDAD item in AC.Det)
                    {
                        Models.Auditoria_Calidad_Detalle auditoria_calidad = new Models.Auditoria_Calidad_Detalle()
                        {
                            IdAuditoria = AC.IdAuditoria,
                            Id_Origen = item.IdOrigen,
                            IdPosicion = item.IdPosicion,
                            IdOperacion = item.IdOperacion,
                            IdDefecto = item.IdDefecto,
                            Recup = item.Recup,
                            Criterio = item.Criterio,
                            Fin = item.Fin,
                            Aud_Imagen = item.Imagen,
                            Nota = item.Nota,
                            Archivo = item.Archivo 
                        };
                        db.Auditoria_Calidad_Detalle.Add(auditoria_calidad);
                    }
                    db.SaveChanges();

                    API.Message = "Auditoria modificada correctamente";
                    API.Response = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = "Formato inválido";
                    API.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception e)
            {
                API.Message = e.Message;
                API.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            return API;
        }

        /// <summary>
        /// ELIMINAMOS EL DETALLE DE LA AUDITORIA
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCalidad/ActualizaAuditoriaDet")]
        public AuditoriaTerminadoController.MESSAGE EliminaAuditoriaD(int IdAuditoriaDet)
        {
            AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

            try
            {
                Models.Auditoria_Calidad_Detalle ACD = db.Auditoria_Calidad_Detalle.Where(x => x.IdAuditoria_Calidad_Detalle == IdAuditoriaDet).FirstOrDefault();
                if (ACD != null)
                {
                    db.Auditoria_Calidad_Detalle.Remove(ACD);
                    db.SaveChanges();
                    API.Message = "Elemento eliminado con éxito";
                    API.Response = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = "No se encontro el detalle solicitado";
                    API.Response = new HttpResponseMessage(HttpStatusCode.Conflict);
                }
            }
            catch (Exception e)
            {
                API.Message = e.Message;
                API.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            return API;
        }

        /// <summary>
        /// ACTIVAMOS O DESACTIVAMOS LA AUDITORIA
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCalidad/EliminaAuditoria")]
        public AuditoriaTerminadoController.MESSAGE ActivaAuditoria(int IdAuditoria = 0)
        {
            AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

            try
            {
                Models.Auditoria AUD = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria).FirstOrDefault();
                AUD.Activo = (AUD.Activo == false ? true : false);

                db.Entry(AUD).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                API.Message = "Auditoria modificada con éxito";
                API.Response = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                API.Message = e.Message;
                API.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            return API;
        }

        /// <summary>
        /// ELIMINAMOS LA AUDITORIA
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCalidad/EliminaAuditoria")]
        public AuditoriaTerminadoController.MESSAGE EliminaAuditoria(int IdAuditoria = 0)
        {
            AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

            try
            {
                Models.Auditoria AUD = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria).FirstOrDefault();

                db.Auditorias.Remove(AUD);
                db.SaveChanges();

                List<Models.Auditoria_Calidad_Detalle> AD = db.Auditoria_Calidad_Detalle
                    .Where(x => x.IdAuditoria == IdAuditoria).ToList();

                db.Auditoria_Calidad_Detalle.RemoveRange(AD);
                db.SaveChanges();

                API.Message = "Auditoria eliminada con éxito";
                API.Response = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                API.Message = e.Message;
                API.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            return API;
        }

        #endregion

        public partial class DET_AUDITORIA_CALIDAD
        {
            [Required]
            public int IdPosicion { get; set; }
            [Required]
            public int IdOperacion { get; set; }
            [Required]
            public int IdOrigen { get; set; }
            [Required]
            public int IdDefecto { get; set; }

            public int Recup { get; set; }
            public int Criterio { get; set; }
            public int Fin { get; set; }

            public string Imagen { get; set; }
            public string Archivo { get; set; }
            public string Nota { get; set; }
        }

        public partial class REQ_NEW_AT
        {
            [Required]
            public int IdClienteRef { get; set; }
            [Required]
            public string OrdenTrabajo { get; set; }
            public string PO { get; set; }
            public string Tela { get; set; }
            public string Marca { get; set; }
            public string NumCortada { get; set; }
            public string Lavado { get; set; }
            public string Estilo { get; set; }
            public string Planta { get; set; }
            public string Ruta { get; set; }
            [Required]
            public int IdUsuario { get; set; }
            [Required]
            public List<DET_AUDITORIA_CALIDAD> Det { get; set; }
        }

        public partial class RES_AUDITORIA_T_DET
        {
            public HttpResponseMessage Message { get; set; }
            public Models.VST_AUDITORIA RES { get; set; }
            public List<Models.VST_AUDITORIA_CALIDAD_DETALLE> RES_DET { get; set; }
        }

        public partial class ACT_DET_AUDITORIA_C
        {
            [Required]
            public int IdAuditoria { get; set; }

            [Required]
            public List<DET_AUDITORIA_CALIDAD> Det { get; set; }
        }
    }
}
