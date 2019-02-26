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
    public class TerminadoController : ApiController
    {
        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

        /// <summary>
        /// Crea un nuevo defecto de terminado
        /// </summary>
        /// <param name="OT"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/NuevoDefectoTerminado")]
        public ViewModel.RES_DEFECTO_TERMINADO NuevoDefectoTerminado([FromBody]ViewModel.REQ_DEFECTO_TERMINADO Terminado)
        {
            ViewModel.RES_DEFECTO_TERMINADO API = new ViewModel.RES_DEFECTO_TERMINADO();
            try
            {
                Models.C_Terminado _Terminado = new Models.C_Terminado()
                {
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    Clave = Terminado.Clave,
                    IdUsuario = Terminado.IdUsuario,
                    IdSubModulo = 21,
                    Descripcion = Terminado.Descripcion,
                    Nombre = Terminado.Nombre,
                    Observaciones = Terminado.Observaciones,
                    Imagen = Terminado.Imagen
                };
                db.C_Terminado.Add(_Terminado);
                db.SaveChanges();

                API.Hecho = true;
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
            }
            return API;
        }

        /// <summary>
        /// Obtiene defecto de terminado por Clave y/o Nombre
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Terminado/ObtieneDefecto")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_CORTE_TERMINADO ObtieneDefecto(string Clave = "", string Nombre = "")
        {
            ViewModel.RES_BUS_DEFECTO_CORTE_TERMINADO API = new ViewModel.RES_BUS_DEFECTO_CORTE_TERMINADO();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Terminado = db.VST_TERMINADO.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 21).OrderBy(x => x.Nombre).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Terminado = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_Terminado = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtiene información de un defecto terminnado por Id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [Route("api/Terminado/ObtieneInfoDefectoTerminado")]
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_EDT_DEFECTO_TERMINADO ObtieneInfoDefecto(int ID)
        {
            ViewModel.RES_EDT_DEFECTO_TERMINADO API = new ViewModel.RES_EDT_DEFECTO_TERMINADO();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Terminado = db.VST_TERMINADO.Where(x => x.ID == ID).FirstOrDefault();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Terminado = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Actualiza los datos de un defecto terminado
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Terminado/ActualizaDefecto")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_TERMINADO ActualizaDefecto([FromBody]ViewModel.REQ_EDT_DEFECTO_TERMINADO Defecto)
        {
            ViewModel.RES_DEFECTO_TERMINADO API = new ViewModel.RES_DEFECTO_TERMINADO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Terminado Vst = db.C_Terminado.Where(x => x.ID == Defecto.ID).FirstOrDefault();
                    if (Vst != null)
                    {
                        Vst.IdUsuario = Defecto.IdUsuario;
                        Vst.Nombre = Defecto.Nombre;
                        Vst.Observaciones = Defecto.Observaciones;
                        Vst.Descripcion = Defecto.Descripcion;
                        Vst.Clave = Defecto.Clave;
                        Vst.Imagen = Defecto.Imagen;

                        db.Entry(Vst).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                        API.Hecho = true;
                    }
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
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Hecho = false;
            }
            return API;
        }

        /// <summary>
        /// Activa o inactiva el defecto por IdDefecto
        /// </summary>
        /// <param name="IdDefecto"></param>
        /// <returns></returns>
        [Route("api/Terminado/ActivaInactivaDefecto")]
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_TERMINADO ActivaInactivaDefecto(int IdDefecto)
        {
            ViewModel.RES_DEFECTO_TERMINADO API = new ViewModel.RES_DEFECTO_TERMINADO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Terminado c_Cort = db.C_Terminado.Where(x => x.ID == IdDefecto).FirstOrDefault();
                    c_Cort.Activo = (c_Cort.Activo == false ? true : false);

                    db.Entry(c_Cort).State = System.Data.Entity.EntityState.Modified;
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
                API.Hecho = false;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

    }
}
