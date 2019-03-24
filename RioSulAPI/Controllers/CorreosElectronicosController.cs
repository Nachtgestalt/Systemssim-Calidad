using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using RioSulAPI.Class;

namespace RioSulAPI.Controllers
{
    public class CorreosElectronicosController : ApiController
    {
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

        /// <summary>
        /// OBTIENE LOS CORREOS ELECTRONICOS A LAOS QUE SE LES ENVIARA UN CIERRE DE AUDITORIA
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="CorreoE"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/CorreosElectronicos/ObtieneUsuariosCierreAuditoria")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_CORREO ObtieneUsuariosCierreAuditoria(string Key, string CorreoE = "")
        {
            ViewModel.RES_CORREO API = new ViewModel.RES_CORREO();
            try
            {
                if (ModelState.IsValid)
                {
                    if (!string.IsNullOrEmpty(Key) && System.Configuration.ConfigurationManager.AppSettings["PasswordKey"].ToString() == Key)
                    {
                        API.CorreosA = db.VST_CORREOS_AUDITORIA.Where(x => x.Email.Contains(CorreoE)).OrderBy(x => x.Email).ToList();
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    else
                    {
                        API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                        API.CorreosA = null;
                    }
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.CorreosA = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.CorreosA = null;
            }
            return API;
        }

        /// <summary>
        /// INGRESA UN NUEVO USUARIO AL CUAL SE LE ENVIARA CORREO ELECTRONICO, INFORMANDO EL CIERRE DE AUDITORIA
        /// </summary>
        /// <param name="REQ"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/CorreosElectronicos/NuevoCorreoCierreAuditoria")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_NVO_CORREO NuevoCorreoCierreAuditoria([FromBody]ViewModel.REQ_NVO_CORREO REQ)
        {
            ViewModel.RES_NVO_CORREO API = new ViewModel.RES_NVO_CORREO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_CorreosElectronicos c_CorreosElectronicos = new Models.C_CorreosElectronicos()
                    {
                        UsuarioId = REQ.USU_ID,
                        Confeccion = REQ.Confeccion,
                        Corte = REQ.Corte,
                        Lavandería = REQ.Lavanderia,
                        ProcesosEspeciales = REQ.ProcesosEspeciales,
                        Terminado = REQ.Terminado,
                        Calidad = REQ.Calidad
                    };

                    db.C_CorreosElectronicos.Add(c_CorreosElectronicos);
                    db.SaveChanges();

                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                    API.Hecho = true;
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Hecho = false;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Hecho = false;
            }
            return API;
        }

        /// <summary>
        /// EDITA LAS OPCIONES A LAS QUE SE MANDARA CORREO AL CIERRE DE AUDITORIA
        /// </summary>
        /// <param name="REQ"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/CorreosElectronicos/EditaCierreAuditoria")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_NVO_CORREO EditaCierreAuditoria([FromBody]ViewModel.REQ_NVO_CORREO REQ)
        {
            ViewModel.RES_NVO_CORREO API = new ViewModel.RES_NVO_CORREO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_CorreosElectronicos c_CorreosElectronicos = db.C_CorreosElectronicos.Where(x => x.UsuarioId == REQ.USU_ID).FirstOrDefault();
                    c_CorreosElectronicos.Corte = REQ.Corte;
                    c_CorreosElectronicos.Confeccion = REQ.Confeccion;
                    c_CorreosElectronicos.Lavandería = REQ.Lavanderia;
                    c_CorreosElectronicos.ProcesosEspeciales = REQ.ProcesosEspeciales;
                    c_CorreosElectronicos.Terminado = REQ.Terminado;

                    db.Entry(c_CorreosElectronicos).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    API.Hecho = true;
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Hecho = false;
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Hecho = false;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// ELIMINA A LOS USUARIOS A LOS QUE NO SE DESEA SE ENVÍE CORREO CON EL CIERRE DE AUDITORÍA
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="USU_ID"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/CorreosElectronicos/EliminaUsuarioAuditoriaCorreo")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public bool EliminaUsuarioAuditoriaCorreo(string Key, int USU_ID)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!string.IsNullOrEmpty(Key) && System.Configuration.ConfigurationManager.AppSettings["PasswordKey"].ToString() == Key)
                    {
                        Models.C_CorreosElectronicos c_CorreosElectronicos = db.C_CorreosElectronicos.Where(x => x.UsuarioId == USU_ID).FirstOrDefault();

                        db.C_CorreosElectronicos.Remove(c_CorreosElectronicos);
                        db.SaveChanges();

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// OBTIENE LOS USUARIOS NO REGISTRADOS EN LOS AVISOS DE CIERRE DE AUDITORRA
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/CorreosElectronicos/UsuariosNoRegistrados")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_GET_USU UsuariosNoRegistrados(string Key)
        {
            ViewModel.RES_GET_USU API = new ViewModel.RES_GET_USU();
            try
            {
                if (!string.IsNullOrEmpty(Key) && System.Configuration.ConfigurationManager.AppSettings["PasswordKey"].ToString() == Key)
                {
                    API.Usuarios = new List<ViewModel.USU>();
                    API.Usuarios = db.Database.SqlQuery<ViewModel.USU>("SELECT * FROM C_Sg_Usuarios WHERE ID NOT IN(SELECT UsuarioId FROM VST_CORREOS_AUDITORIA)").ToList();
                    API.Hecho = true;
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Hecho = false;
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Usuarios = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Hecho = false;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Usuarios = null;
            }
            return API;
        }


        [System.Web.Http.Route("api/CorreosElectronicos/EditaAlertasPorUsuario")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_EDT_CORREO EditaAlertasPorUsuario(int USU_ID)
        {
            ViewModel.RES_EDT_CORREO API = new ViewModel.RES_EDT_CORREO();
            try
            {
                Models.VST_CORREOS_AUDITORIA vST_CORREOS_AUDITORIA = db.VST_CORREOS_AUDITORIA.Where(x => x.UsuarioId == USU_ID).FirstOrDefault();
                if (vST_CORREOS_AUDITORIA != null)
                {
                    API.Corte = vST_CORREOS_AUDITORIA.Corte;
                    API.Confeccion = vST_CORREOS_AUDITORIA.Confeccion;
                    API.Lavanderia = vST_CORREOS_AUDITORIA.Lavandería;
                    API.ProcesosEspeciales = vST_CORREOS_AUDITORIA.ProcesosEspeciales;
                    API.Terminado = vST_CORREOS_AUDITORIA.Terminado;

                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }
    }
}
