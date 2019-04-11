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
using Microsoft.Office.Interop.Excel;
using RioSulAPI.Class;
using RioSulAPI.Models;

namespace RioSulAPI.Controllers
{
    public class AuditoriaProcesosEspecialesController : ApiController
    {
        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

        /// <summary>
        /// Registra una nueva auditoría de procesos especiales
        /// </summary>
        /// <param name="OT"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaProcesosEspeciales/AuditoriaProcEsp")]
        public HttpResponseMessage NuevaAuditoriaProcEsp([FromBody]REQ_NEW_OT OT)
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
                            Confeccion = false,
                            ProcesosEspeciales = true,
                            Calidad = false,
                            Activo = true
                        };
                        db.Auditorias.Add(auditoria);
                        db.SaveChanges();

                        foreach (OT_DET item in OT.Det)
                        {

                            num_detalle = num_detalle + 1;
                            image_name = "";
                            pdf = "";
                            if (item.Imagen != null && !item.Imagen.IsEmpty())
                            {
                                string base64 = item.Imagen.Substring(item.Imagen.IndexOf(',') + 1);
                                byte[] data = Convert.FromBase64String(base64);

                                image_name = "Auditoria_ProcEsp_" + auditoria.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

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

                                pdf = "Auditoria_ProcEsp_" + auditoria.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

                                using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Archivos/") + pdf + ".pdf", FileMode.Create))
                                {
                                    image_file.Write(data, 0, data.Length);
                                    image_file.Flush();
                                }
                            }
                            Models.Auditoria_Proc_Esp_Detalle auditoria_Proc_Esp = new Models.Auditoria_Proc_Esp_Detalle()
                            {
                                IdDefecto = item.IdDefecto,
                                IdAuditoria = auditoria.IdAuditoria,
                                IdOperacion = item.IdOperacion,
                                IdPosicion = item.IdPosicion,
                                Cantidad = item.Cantidad,
                                Aud_Imagen = image_name,
                                Notas = item.Notas,
                                Archivo = pdf
                            };
                            db.Auditoria_Proc_Esp_Detalle.Add(auditoria_Proc_Esp);
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
        /// Obtiene auditoría de Procesos Especiales
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaProcesosEspeciales/AuditoriaProcEsp")]
        public AuditoriaCorteController.RES_AUDITORIA ObtieneAuditoriaProcEsp()
        {
            AuditoriaCorteController.RES_AUDITORIA API = new AuditoriaCorteController.RES_AUDITORIA();
            try
            {
                API.RES = db.VST_AUDITORIA.Where(x => x.ProcesosEspeciales == true).ToList();
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
        /// Obtiene auditoría de Procesos Especiales
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaProcesosEspeciales/AuditoriaProcEsp")]
        public R_AUDITORIA_PROC ObtieneAuditoriaProcEspDet(int ID)
        {
            R_AUDITORIA_PROC API = new R_AUDITORIA_PROC();
            API.RES_DET = new List<VST_AUDITORIA_PROC_ESP_DETALLE>();
            List<Models.VST_AUDITORIA_PROC_ESP_DETALLE> proc_esp = new List<VST_AUDITORIA_PROC_ESP_DETALLE>();
            string file_path = "";
            try
            {
                API.RES = db.VST_AUDITORIA.Where(x => x.ProcesosEspeciales == true).FirstOrDefault();
                proc_esp = db.VST_AUDITORIA_PROC_ESP_DETALLE.Where(x => x.IdAuditoria == ID).ToList();

                foreach (VST_AUDITORIA_PROC_ESP_DETALLE item in proc_esp)
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
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.RES = null;
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
        [Route("api/AuditoriaProcesosEspeciales/CierreAuditoria")]
        public HttpResponseMessage CierreAuditoria(int ID)
        {
            try
            {
                Boolean notas = false;
                Models.Auditoria API = db.Auditorias.Where(x => x.IdAuditoria == ID && x.ProcesosEspeciales == true).FirstOrDefault();
                API.FechaRegistroFin = DateTime.Now;
                db.Entry(API).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                List<Models.VST_CORREOS_AUDITORIA> correos = db.VST_CORREOS_AUDITORIA.Where(x => x.ProcesosEspeciales == true).ToList();
                List<Models.Auditoria_Proc_Esp_Detalle> auditoria_det = db.Auditoria_Proc_Esp_Detalle.Where(x => x.IdAuditoria == ID).ToList();

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
                    var body = "Se ha cerrado la auditoría de la orden de trabajo con número de corte: " + API.NumCortada.ToUpper() + " en el área de calidad.";

                    foreach (Auditoria_Proc_Esp_Detalle item in auditoria_det)
                    {
                        if (item.Notas != "null" && !item.Notas.IsEmpty())
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
        [Route("api/AuditoriaProcesosEspeciales/EliminaAuditoria")]
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
        [Route("api/AuditoriaProcesosEspeciales/EliminaAuditoria")]
        public AuditoriaTerminadoController.MESSAGE EliminaAuditoria(int ID)
        {
            AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

            try
            {
                List<Models.Auditoria_Proc_Esp_Detalle> AD = db.Auditoria_Proc_Esp_Detalle
                    .Where(x => x.IdAuditoria == ID).ToList();

                db.Auditoria_Proc_Esp_Detalle.RemoveRange(AD);
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
        [Route("api/AuditoriaProcesosEspeciales/AuditoriaProcEsp")]
        public AuditoriaTerminadoController.MESSAGE ActualizaAuditoria([FromBody]ACT_AUD_PE OC)
        {
            AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();
            string image_name = "";
            string pdf = "";
            int num_detalle = 0;

            try
            {
                if (ModelState.IsValid)
                {
                    List<Models.Auditoria_Proc_Esp_Detalle> ATD = db.Auditoria_Proc_Esp_Detalle.Where(x => x.IdAuditoria == OC.IdAuditoria).ToList();

                    foreach (Models.Auditoria_Proc_Esp_Detalle item in ATD)
                    {
                        db.Auditoria_Proc_Esp_Detalle.Remove(item);
                        db.SaveChanges();
                    }

                    foreach (OT_DET item in OC.Det)
                    {
                        num_detalle = num_detalle + 1;
                        image_name = "";
                        pdf = "";

                        if (item.Imagen != null && !item.Imagen.IsEmpty())
                        {
                            string base64 = item.Imagen.Substring(item.Imagen.IndexOf(',') + 1);
                            byte[] data = Convert.FromBase64String(base64);

                            image_name = "Auditoria_ProcEsp_" + OC.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

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

                            pdf = "Auditoria_ProcEsp_" + OC.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

                            using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Archivos/") + pdf + ".pdf", FileMode.Create))
                            {
                                image_file.Write(data, 0, data.Length);
                                image_file.Flush();
                            }
                        }

                        Models.Auditoria_Proc_Esp_Detalle auditoria_calidad = new Models.Auditoria_Proc_Esp_Detalle()
                        {
                            IdAuditoria = OC.IdAuditoria,
                            IdPosicion = item.IdPosicion,
                            IdOperacion = item.IdOperacion,
                            IdDefecto = item.IdDefecto,
                            Cantidad = item.Cantidad,
                            Aud_Imagen = image_name,
                            Notas = item.Notas,
                            Archivo = pdf
                        };
                        db.Auditoria_Proc_Esp_Detalle.Add(auditoria_calidad);
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



        public partial class DET_AUDITORIA_PROC_ESP
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
            [Required] public List<OT_DET> Det { get; set; }
        }
        public partial class OT_DET
        {
            [Required] public int IdPosicion { get; set; }
            [Required] public int IdOperacion { get; set; }
            [Required] public int IdDefecto { get; set; }
            [Required] public int Cantidad { get; set; }
            public string Notas { get; set; }
            public string Imagen { get; set; }
            public string Archivo { get; set; }
        }
        public partial class R_AUDITORIA_PROC
        {
            public Models.VST_AUDITORIA RES { get; set; }
            public List<Models.VST_AUDITORIA_PROC_ESP_DETALLE> RES_DET { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class ACT_AUD_PE
        {
            [Required]
            public int IdAuditoria { get; set; }

            [Required]
            public List<OT_DET> Det { get; set; }
        }
    }
}
