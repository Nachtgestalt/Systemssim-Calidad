using System;
using System.Collections.Generic;
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
    public class LavanderiaController : ApiController
    {
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();
        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();


        public partial class JSON_POS_DEF
        {
            public int IdDefecto { get; set; }
        }

        //----------------------------------------DEFECTOS------------------------------------------------

        /// <summary>
        /// Registra un nuevo Lavandería
        /// </summary>
        /// <param name="Lavanderia"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Lavanderia/NuevoDefectoLavanderia")]
        public ViewModel.RES_DEFECTO_LAV NuevoDefectoLavanderia([FromBody]ViewModel.REQ_DEFECTO_LAV Lavanderia)
        {
            ViewModel.RES_DEFECTO_LAV API = new ViewModel.RES_DEFECTO_LAV();
            try
            {
                Models.C_Lavanderia _Lavanderia = new Models.C_Lavanderia()
                {
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    Clave = Lavanderia.Clave,
                    IdUsuario = Lavanderia.IdUsuario,
                    IdSubModulo = 17,
                    Descripcion = Lavanderia.Descripcion,
                    Nombre = Lavanderia.Nombre,
                    Observaciones = Lavanderia.Observaciones,
                    Imagen = Lavanderia.Imagen
                };
                db.C_Lavanderia.Add(_Lavanderia);
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
        /// Valido nueva lavanderia submodulo 
        /// </summary>
        /// <param name="SubModulo"></param>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Lavanderia/ValidaLavanderiaSubModulo")]
        public ViewModel.RES_DEFECTO_LAV ValidaLavanderiaSuBmodulo(int SubModulo, string Clave, string Nombre)
        {
            ViewModel.RES_DEFECTO_LAV API = new ViewModel.RES_DEFECTO_LAV();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Lavanderia c_Lavanderia = db.C_Lavanderia.Where(x => x.Clave == Clave && x.Nombre == Nombre && x.IdSubModulo == SubModulo).FirstOrDefault();
                    if (c_Lavanderia != null)
                        API.Hecho = false;
                    else
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
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Hecho = false;
            }
            return API;
        }

        /// <summary>
        /// Activa o inactiva el defecto por IdDefecto
        /// </summary>
        /// <param name="IdLavanderia"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Lavanderia/ActivaInactivaLavanderia")]
        public ViewModel.RES_DEFECTO_LAV ActivaInactivaLavanderia(int IdLavanderia)
        {
            ViewModel.RES_DEFECTO_LAV API = new ViewModel.RES_DEFECTO_LAV();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Lavanderia c_Lav = db.C_Lavanderia.Where(x => x.ID == IdLavanderia).FirstOrDefault();
                    c_Lav.Activo = (c_Lav.Activo == false ? true : false);

                    db.Entry(c_Lav).State = System.Data.Entity.EntityState.Modified;
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
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Hecho = false;
            }
            return API;
        }

        /// <summary>
        /// Obtiene defecto por Clave y/o Nombre
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Lavanderia/ObtieneDefectoLavanderia")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_LAVANDERIA ObtieneDefectoLavanderia(string Clave = "", string Nombre = "")
        {
            ViewModel.RES_BUS_DEFECTO_LAVANDERIA API = new ViewModel.RES_BUS_DEFECTO_LAVANDERIA();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Lavanderia = db.VST_LAVANDERIA.Where(x => x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre) && x.IdSubModulo == 17).OrderBy(x => x.Nombre).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Lavanderia = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Vst_Lavanderia = null;
            }
            return API;
        }

        /// <summary>
        /// Actualiza los datos del defecto de lavandría
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Lavanderia/ActualizaDefectoLavanderia")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_LAV ActualizaDefectoLavanderia([FromBody]ViewModel.REQ_EDT_DEFECTO_LAVANDERIA Defecto)
        {
            ViewModel.RES_DEFECTO_LAV API = new ViewModel.RES_DEFECTO_LAV();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Lavanderia Vst = db.C_Lavanderia.Where(x => x.ID == Defecto.ID).FirstOrDefault();
                    if (Vst != null)
                    {
                        Vst.IdUsuario = Defecto.IdUsuario;
                        Vst.Nombre = Defecto.Nombre;
                        Vst.Observaciones = Defecto.Observaciones;
                        Vst.Descripcion = Defecto.Descripcion;
                        Vst.Clave = Defecto.Clave;
                        Vst.Imagen = Defecto.Imagen;
                    }
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

        //----------------------------------------OPERACIONES------------------------------------------------

        /// <summary>
        /// Registra una nueva operación lavandería
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Lavanderia/NuevaOperacionLavanderia")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_LAV NuevaOperacionLavanderia([FromBody]ViewModel.REQ_DEFECTO_LAV Defecto)
        {
            ViewModel.RES_DEFECTO_LAV API = new ViewModel.RES_DEFECTO_LAV();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Lavanderia c_Lavanderia = new Models.C_Lavanderia()
                    {
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        Clave = Defecto.Clave,
                        Descripcion = Defecto.Descripcion,
                        IdSubModulo = 19,
                        IdUsuario = Defecto.IdUsuario,
                        Nombre = Defecto.Nombre,
                        Observaciones = Defecto.Observaciones,
                        Imagen = Defecto.Imagen
                    };
                    db.C_Lavanderia.Add(c_Lavanderia);
                    db.SaveChanges();

                    List<JSON_POS_DEF> POS = _objSerializer.Deserialize<List<JSON_POS_DEF>>(Defecto.Imagen);

                    foreach (JSON_POS_DEF item in POS)
                    {
                        Models.C_Operacion_Lavanderia c_Operacion_Lavanderia = new Models.C_Operacion_Lavanderia()
                        {
                            IdOperacion = c_Lavanderia.ID,
                            IdDefecto = item.IdDefecto
                        };
                        db.C_Operacion_Lavanderia.Add(c_Operacion_Lavanderia);
                    }
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
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Hecho = false;
            }
            return API;
        }

        /// <summary>
        /// Valida nuevo lavanderia por submodulo
        /// </summary>
        /// <param name="SubModulo"></param>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Lavanderia/ValidaOperacionSubModuloLavanderia")]
        public ViewModel.RES_DEFECTO_LAV ValidaOperacionSubModuloLavanderia(int SubModulo, string Clave, string Nombre)
        {
            ViewModel.RES_DEFECTO_LAV API = new ViewModel.RES_DEFECTO_LAV();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Lavanderia c_Lav = db.C_Lavanderia.Where(x => x.Clave == Clave && x.Nombre == Nombre && x.IdSubModulo == SubModulo).FirstOrDefault();
                    if (c_Lav != null)
                    {
                        API.Hecho = false;
                    }
                    else
                    {
                        API.Hecho = true;
                    }
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
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
                API.Hecho = false;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Activa o inactiva la operación de lavandería
        /// </summary>
        /// <param name="IdLavanderia"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Lavanderia/ActivaInactivaOperacionesLavanderia")]
        public ViewModel.RES_DEFECTO_LAV ActivaInactivaOperacionesLavanderia(int IdLavanderia)
        {
            ViewModel.RES_DEFECTO_LAV API = new ViewModel.RES_DEFECTO_LAV();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Lavanderia c_Lav = db.C_Lavanderia.Where(x => x.ID == IdLavanderia).FirstOrDefault();
                    c_Lav.Activo = (c_Lav.Activo == false ? true : false);

                    db.Entry(c_Lav).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    API.Hecho = true;
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Hecho = false;
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
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
        /// Obtiene lavanderia por clave y/o nombre
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Lavanderia/ObtieneOperacionLavanderia")]
        public ViewModel.RES_BUS_DEFECTO_LAVANDERIA ObtieneOperacionLavanderia(string Clave = "", string Nombre = "")
        {
            ViewModel.RES_BUS_DEFECTO_LAVANDERIA API = new ViewModel.RES_BUS_DEFECTO_LAVANDERIA();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Lavanderia = db.VST_LAVANDERIA.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 17).OrderBy(x => x.Nombre).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Lavanderia = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_Lavanderia = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Actualiza el Id Lavanderia
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Lavanderia/ObtieneInfoOperacionLavanderia")]
        public ViewModel.RES_EDT_LAVANDERIA ObtieneInfoOperacionLavanderia(int ID)
        {
            ViewModel.RES_EDT_LAVANDERIA API = new ViewModel.RES_EDT_LAVANDERIA();
            try
            {
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                API.Vst_Lavanderia = db.VST_LAVANDERIA.Where(x => x.ID == ID).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Vst_Lavanderia = null;
            }
            return API;
        }

    }
}