﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using System.Web.WebPages;
using RioSulAPI.Class;

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
        [Route("api/AuditoriaProcesosEspeciales/NuevaAuditoriaProcEsp")]
        public HttpResponseMessage NuevaAuditoriaProcEsp([FromBody]REQ_NEW_OT OT)
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

                            Models.Auditoria_Proc_Esp_Detalle auditoria_Proc_Esp = new Models.Auditoria_Proc_Esp_Detalle()
                            {
                                IdDefecto = item.IdDefecto,
                                IdAuditoria = auditoria.IdAuditoria,
                                IdOperacion = item.IdOperacion,
                                IdPosicion = item.IdPosicion,
                                Cantidad = item.Cantidad,
                                Aud_Imagen = image_name
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
        [Route("api/AuditoriaProcesosEspeciales/ObtieneAuditoriaProcEsp")]
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
        /// Genera el cierre de auditoria de corte
        /// </summary>
        /// <param name="IdAuditoria"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaProcesosEspeciales/CierreAuditoria")]
        public HttpResponseMessage CierreAuditoria(int IdAuditoria)
        {
            try
            {
                Models.Auditoria API = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria && x.ProcesosEspeciales == true).FirstOrDefault();
                API.FechaRegistroFin = DateTime.Now;
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
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
            public string Imagen { get; set; }
        }
    }
}
