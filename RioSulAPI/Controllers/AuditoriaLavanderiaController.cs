﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using System.Web.WebPages;
using RioSulAPI.Class;
using RioSulAPI.Models;


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
        [Route("api/AuditoriaLavanderia/AuditoriaLavanderia")]
        public HttpResponseMessage NuevaAuditoriaLavanderia([FromBody]REQ_NEW_OT OT)
        {
            string image_name = "";
            int num_detalle = 0;

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
                            Lavanderia = true,
                            Terminado = false,
                            Confeccion = false,
                            ProcesosEspeciales = false,
                            Calidad = false,
                            Activo = true
                        };
                        db.Auditorias.Add(auditoria);
                        db.SaveChanges();

                        foreach (DET_AUDITORIA_LAVANDERIA item in OT.Det)
                        {
                            num_detalle = num_detalle + 1;
                            image_name = "";
                            if (item.Aud_Imagen != null && !item.Aud_Imagen.IsEmpty())
                            {
                                string base64 = item.Aud_Imagen.Substring(item.Aud_Imagen.IndexOf(',') + 1);
                                byte[] data = Convert.FromBase64String(base64);

                                image_name = "Auditoria_Lavanderia_" + auditoria.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

                                using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                                {
                                    image_file.Write(data, 0, data.Length);
                                    image_file.Flush();
                                }
                            }

                            Models.Auditoria_Lavanderia_Detalle auditoria_Lavanderia = new Models.Auditoria_Lavanderia_Detalle()
                            {
                                IdAuditoria = auditoria.IdAuditoria,
                                IdDefecto = item.IdDefecto,
                                IdPosicion = item.IdPosicion,
                                IdOperacion = item.IdOperacion,
                                Cantidad = item.Cantidad,
                                Aud_Imagen = image_name,
                                Archivo = item.Archivo,
                                Nota = item.Nota
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
        [Route("api/AuditoriaLavanderia/AuditoriaLavanderia")]
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
        /// Obtenemos el detalle de la auditoria dependiendo el ID enviado
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaLavanderia/AuditoriaLavanderia")]
        public RES_AUDITORIA_T_DET ObtieneAuditoriaDet(int ID)
        {
            RES_AUDITORIA_T_DET API = new RES_AUDITORIA_T_DET();
            List<VST_AUDITORIA_LAVANDERIA_DETALLE> lavanderia = new List<VST_AUDITORIA_LAVANDERIA_DETALLE>();
            API.RES_DET = new List<VST_AUDITORIA_LAVANDERIA_DETALLE>();
            string file_path = "";
            try
            {
                API.RES = db.VST_AUDITORIA.Where(x => x.IdAuditoria == ID && x.Lavanderia == true).FirstOrDefault();
                lavanderia = db.VST_AUDITORIA_LAVANDERIA_DETALLE.Where(x => x.IdAuditoria == ID).ToList();

                foreach (VST_AUDITORIA_LAVANDERIA_DETALLE item in lavanderia)
                {
                    file_path = HttpContext.Current.Server.MapPath("~/Imagenes/");
                    file_path = file_path + item.Aud_Imagen + ".jpg";
                    if (File.Exists(file_path))
                    {
                        item.Aud_Imagen = "data:image/" + "jpg" + ";base64," + Convert.ToBase64String(File.ReadAllBytes(file_path));
                    }
                    else
                    {
                        item.Aud_Imagen = "";
                    }

                    API.RES_DET.Add(item);
                }

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
        /// Genera el cierre de auditoria de corte
        /// </summary>
        /// <param name="IdAuditoria"></param>
        /// <returns></returns>
        [HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaLavanderia/CierreAuditoria")]
        public HttpResponseMessage CierreAuditoria(int IdAuditoria)
        {
            try
            {
                Boolean notas = false;
                Models.Auditoria API = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria && x.Lavanderia == true).FirstOrDefault();
                API.FechaRegistroFin = DateTime.Now;
                db.Entry(API).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                List<Models.VST_CORREOS_AUDITORIA> correos = db.VST_CORREOS_AUDITORIA.Where(x => x.Lavandería == true).ToList();
                List<Models.Auditoria_Calidad_Detalle> auditoria_det = db.Auditoria_Calidad_Detalle.Where(x => x.IdAuditoria == IdAuditoria).ToList();

                if (correos.Count > 0)
                {
                    MailMessage mensaje = new MailMessage();

                    mensaje.From = new MailAddress(System.Configuration.ConfigurationManager.AppSettings["Mail"].ToString());
                    var password = System.Configuration.ConfigurationManager.AppSettings["Password"].ToString();

                    foreach (VST_CORREOS_AUDITORIA item in correos)
                    {
                        mensaje.To.Add(item.Email);
                    }

                    var sub = "AUDITORÍA OT: " + API.OrdenTrabajo.ToUpper();
                    var body = "Se ha cerrado la auditoría de la orden de trabajo con número de corte: " + API.NumCortada.ToUpper() + " en el área de lavandería.";

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
        /// ACTIVAMOS O DESACTIVAMOS LA AUDITORIA
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaLavanderia/EliminaAuditoria")]
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
        [Route("api/AuditoriaLavanderia/EliminaAuditoria")]
        public AuditoriaTerminadoController.MESSAGE EliminaAuditoria(int IdAuditoria = 0)
        {
            AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

            try
            {

                List<Models.Auditoria_Lavanderia_Detalle> AD = db.Auditoria_Lavanderia_Detalle
                    .Where(x => x.IdAuditoria == IdAuditoria).ToList();

                db.Auditoria_Lavanderia_Detalle.RemoveRange(AD);
                db.SaveChanges();

                Models.Auditoria AUD = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria).FirstOrDefault();

                db.Auditorias.Remove(AUD);
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

        /// <summary>
        /// ACTUALIZAMOS EL DETALLE DE LA AUDITORIA
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaLavanderia/ActualizaAuditoriaDet")]
        public AuditoriaTerminadoController.MESSAGE ActualizaAuditoria([FromBody]ACT_DET_AUDITORIA_L AC)
        {
            AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();
            string image_name = "";
            int num_detalle = 0;

            try
            {
                if (ModelState.IsValid)
                {
                    List<Models.Auditoria_Lavanderia_Detalle> ATD = db.Auditoria_Lavanderia_Detalle.Where(x => x.IdAuditoria == AC.IdAuditoria).ToList();

                    foreach (Models.Auditoria_Lavanderia_Detalle item in ATD)
                    {
                        db.Auditoria_Lavanderia_Detalle.Remove(item);
                        db.SaveChanges();
                    }

                    foreach (DET_AUDITORIA_LAVANDERIA item in AC.Det)
                    {
                        num_detalle = num_detalle + 1;
                        image_name = "";

                        if (item.Aud_Imagen != null && !item.Aud_Imagen.IsEmpty())
                        {
                            string base64 = item.Aud_Imagen.Substring(item.Aud_Imagen.IndexOf(',') + 1);
                            byte[] data = Convert.FromBase64String(base64);

                            image_name = "Auditoria_Lavanderia_" + AC.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

                            using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                            {
                                image_file.Write(data, 0, data.Length);
                                image_file.Flush();
                            }
                        }

                        Models.Auditoria_Lavanderia_Detalle auditoria_calidad = new Models.Auditoria_Lavanderia_Detalle()
                        {
                            IdAuditoria = AC.IdAuditoria,
                            IdPosicion = item.IdPosicion,
                            IdOperacion = item.IdOperacion,
                            IdDefecto = item.IdDefecto,
                            Cantidad = item.Cantidad,
                            Aud_Imagen = image_name,
                            Nota = item.Nota,
                            Archivo = item.Archivo
                        };
                        db.Auditoria_Lavanderia_Detalle.Add(auditoria_calidad);
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
            public string Nota { get; set; }
            public string Aud_Imagen { get; set; }
            public string Archivo { get; set; }
        }

        public partial class REQ_NEW_OT
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
            public List<DET_AUDITORIA_LAVANDERIA> Det { get; set; }
        }

        public partial class RES_AUDITORIA_T_DET
        {
            public Models.VST_AUDITORIA RES { get; set; }
            public List<Models.VST_AUDITORIA_LAVANDERIA_DETALLE> RES_DET { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class ACT_DET_AUDITORIA_L
        {
            [Required]
            public int IdAuditoria { get; set; }

            [Required]
            public List<DET_AUDITORIA_LAVANDERIA> Det { get; set; }
        }
    }
}
