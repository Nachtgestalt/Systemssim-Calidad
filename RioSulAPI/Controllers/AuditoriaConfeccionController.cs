using System;
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
        public HttpResponseMessage NuevaAuditoriaConfeccion([FromBody]REQ_NEW_OT OT)
        {
            string image_name = "";
            string pdf = "";
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
                            Lavanderia = false,
                            Terminado = false,
                            Confeccion = true,
                            ProcesosEspeciales = false,
                            Calidad = false,
                            Activo = true
                        };
                        db.Auditorias.Add(auditoria);
                        db.SaveChanges();

                        foreach (DET_AUDITORIA_CONFECCION item in OT.Det)
                        {
                            num_detalle = num_detalle + 1;
                            image_name = "";
                            pdf = "";
                            if (item.Imagen != null && !item.Imagen.IsEmpty())
                            {
                                string base64 = item.Imagen.Substring(item.Imagen.IndexOf(',') + 1);
                                byte[] data = Convert.FromBase64String(base64);

                                image_name = "Auditoria_Confeccion_" + auditoria.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

                                using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                                {
                                    image_file.Write(data, 0, data.Length);
                                    image_file.Flush();
                                }
                            }
                            if (item.Archivo != null && !item.Archivo.IsEmpty())
                            {
                                string base64 = item.Archivo.Substring(item.Archivo.IndexOf(',') + 1);
                                byte[] data = Convert.FromBase64String(base64);

                                pdf = "Auditoria_Confeccion" + auditoria.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

                                using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Archivos/") + pdf + ".pdf", FileMode.Create))
                                {
                                    image_file.Write(data, 0, data.Length);
                                    image_file.Flush();
                                }
                            }
                            Models.Auditoria_Confeccion_Detalle auditoria_Confeccion = new Models.Auditoria_Confeccion_Detalle()
                            {
                                IdAuditoria = auditoria.IdAuditoria,
                                IdArea = item.IdArea,
                                IdOperacion = item.IdOperacion,
                                IdDefecto = item.IdDefecto,
                                Cantidad = item.Cantidad,
                                Aud_Imagen = image_name,
                                Archivo = pdf,
                                Nota = item.Nota
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
		/// Obtiene auditoría por corte por Id
		/// </summary>
		/// <param name="IdAuditoria"></param>
		/// <returns></returns>
		[HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaConfeccion/AuditoriaConfeccion")]
        public RES_AUDITORIA_DET ObtieneAuditoriaCortePorId(int IdAuditoria)
        {
            RES_AUDITORIA_DET API = new RES_AUDITORIA_DET();
            API.RES_DET = new List<Models.VST_AUDITORIA_CONFECCION_DETALLE>();
            List<Models.VST_AUDITORIA_CONFECCION_DETALLE> confeccion = new List<Models.VST_AUDITORIA_CONFECCION_DETALLE>();
            string file_path = "";

            try
            {
                if (ModelState.IsValid)
                {
                    API.RES = db.VST_AUDITORIA.Where(x => x.IdAuditoria == IdAuditoria).FirstOrDefault();
                    confeccion = db.VST_AUDITORIA_CONFECCION_DETALLE.Where(x => x.IdAuditoria == IdAuditoria).ToList();

                    foreach (Models.VST_AUDITORIA_CONFECCION_DETALLE item in confeccion)
                    {
                        file_path = HttpContext.Current.Server.MapPath("~/Imagenes/");
                        file_path = file_path + item.Imagen + ".jpg";
                        if (File.Exists(file_path))
                        {
                            item.Imagen = "data:image/" + "jpg" + ";base64," + Convert.ToBase64String(File.ReadAllBytes(file_path));
                        }
                        else
                        {
                            item.Imagen = "";
                        }

                        file_path = HttpContext.Current.Server.MapPath("~/Archivos/");
                        file_path = file_path + item.Archivo + ".pdf";
                        if (File.Exists(file_path))
                        {
                            item.Archivo = "data:application/" + "pdf" + ";base64," + Convert.ToBase64String(File.ReadAllBytes(file_path));
                        }
                        else
                        {
                            item.Archivo = "";
                        }
                        API.RES_DET.Add(item);
                    }

                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.RES = null;
                    API.RES_DET = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                API.RES = null;
                API.RES_DET = null;
            }

            return API;
        }

        /// <summary>
        /// Genera el cierre de auditoria de corte confeccion
        /// </summary>
        /// <param name="IdAuditoria"></param>
        /// <returns></returns>
        [HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaConfeccion/CierreAuditoria")]
        public HttpResponseMessage CierreAuditoria(int IdAuditoria)
        {
            try
            {
                Boolean notas = false;
                Models.Auditoria API = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria && x.Confeccion == true).FirstOrDefault();
                API.FechaRegistroFin = DateTime.Now;
                db.Entry(API).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                List<Models.VST_CORREOS_AUDITORIA> correos = db.VST_CORREOS_AUDITORIA.Where(x => x.Corte == true).ToList();
                List<Models.Auditoria_Confeccion_Detalle> auditoria_det = db.Auditoria_Confeccion_Detalle.Where(x => x.IdAuditoria == IdAuditoria).ToList();

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
                    var body = "Se ha cerrado la auditoría de la orden de trabajo con número de corte: " + API.NumCortada.ToUpper() + " en el área de confección" +
                        ".";

                    foreach (Auditoria_Confeccion_Detalle item in auditoria_det)
                    {
                        if (item.Nota != "null" && !item.Nota.IsEmpty())
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

        [HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaConfeccion/EliminaAuditoria")]
        public AuditoriaTerminadoController.MESSAGE ActivaAuditoria(int ID)
        {
            AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

            try
            {
                Models.Auditoria AUD = db.Auditorias.Where(x => x.IdAuditoria == ID).FirstOrDefault();
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

        [HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaConfeccion/EliminaAuditoria")]
        public AuditoriaTerminadoController.MESSAGE EliminaAuditoria(int ID)
        {
            AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

            try
            {
                List<Models.Auditoria_Confeccion_Detalle> AD = db.Auditoria_Confeccion_Detalle
                    .Where(x => x.IdAuditoria == ID).ToList();

                db.Auditoria_Confeccion_Detalle.RemoveRange(AD);
                db.SaveChanges();


                Models.Auditoria AUD = db.Auditorias.Where(x => x.IdAuditoria == ID).FirstOrDefault();

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
        [Route("api/AuditoriaConfeccion/AuditoriaConfeccion")]
        public HttpResponseMessage ActualizaAuditoria([FromBody]EDT_AUDITORIA OT)
        {
            string image_name = "";
            string pdf = "";
            int num_detalle = 0;
            try
            {
                if (ModelState.IsValid)
                {
                    List<Models.Auditoria_Confeccion_Detalle> ATD = db.Auditoria_Confeccion_Detalle.Where(x => x.IdAuditoria == OT.IdAuditoria).ToList();

                    foreach (Models.Auditoria_Confeccion_Detalle item in ATD)
                    {
                        db.Auditoria_Confeccion_Detalle.Remove(item);
                        db.SaveChanges();
                    }

                    foreach (DET_AUDITORIA_CONFECCION item in OT.Det)
                    {
                        num_detalle = num_detalle + 1;
                        image_name = "";
                        pdf = "";

                        if (item.Imagen != null && !item.Imagen.IsEmpty())
                        {
                            string base64 = item.Imagen.Substring(item.Imagen.IndexOf(',') + 1);
                            byte[] data = Convert.FromBase64String(base64);

                            image_name = "Auditoria_Confeccion_" + OT.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

                            using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                            {
                                image_file.Write(data, 0, data.Length);
                                image_file.Flush();
                            }
                        }
                        if (item.Archivo != null && !item.Archivo.IsEmpty())
                        {
                            string base64 = item.Archivo.Substring(item.Archivo.IndexOf(',') + 1);
                            byte[] data = Convert.FromBase64String(base64);

                            pdf = "Auditoria_Confeccion_" + OT.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

                            using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Archivos/") + pdf + ".pdf", FileMode.Create))
                            {
                                image_file.Write(data, 0, data.Length);
                                image_file.Flush();
                            }
                        }

                        Models.Auditoria_Confeccion_Detalle auditoria_corte = new Models.Auditoria_Confeccion_Detalle()
                        {
                            IdAuditoria = OT.IdAuditoria,
                            IdArea = item.IdArea,
                            IdOperacion = item.IdOperacion,
                            IdDefecto = item.IdDefecto,
                            Cantidad = item.Cantidad,
                            Aud_Imagen = image_name,
                            Archivo = pdf,
                            Nota = item.Nota
                        };
                        db.Auditoria_Confeccion_Detalle.Add(auditoria_corte);
                    }
                    db.SaveChanges();

                    return new HttpResponseMessage(HttpStatusCode.OK);
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
            public string Imagen { get; set; }
            public string Archivo { get; set; }
            public string Nota { get; set; }
        }

        public partial class REQ_NEW_OT
        {
            [Required] public int IdClienteRef { get; set; }
            [Required] public string OrdenTrabajo { get; set; }
            public string PO { get; set; }
            public string Tela { get; set; }
            public string Marca { get; set; }
            public string NumCortada { get; set; }
            public string Lavado { get; set; }
            public string Estilo { get; set; }
            public string Planta { get; set; }
            public string Ruta { get; set; }
            [Required] public int IdUsuario { get; set; }
            [Required] public List<DET_AUDITORIA_CONFECCION> Det { get; set; }
        }

        public partial class RES_AUDITORIA_DET
        {
            public HttpResponseMessage Message { get; set; }
            public Models.VST_AUDITORIA RES { get; set; }
            public List<Models.VST_AUDITORIA_CONFECCION_DETALLE> RES_DET { get; set; }
        }

        public partial class EDT_AUDITORIA
        {
            [Required]
            public int IdAuditoria { get; set; }

            [Required]
            public List<DET_AUDITORIA_CONFECCION> Det { get; set; }
        }
    }
}