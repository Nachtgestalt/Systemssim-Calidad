using RioSulAPI.Class;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using static RioSulAPI.Controllers.CortadoresController;

namespace RioSulAPI.Controllers
{
    public class ConfeccionController : ApiController
    {
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();
        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();
        //----------------------------------------DEFECTOS------------------------------------------------

        /// <summary>
        /// Registra un nuevo defecto confeccion
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/NuevoDefectoConfeccion")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CONFECCION NuevoDefectoConfeccion([FromBody]ViewModel.REQ_DEFECTO_CONFECCION Defecto)
        {
            ViewModel.RES_DEFECTO_CONFECCION API = new ViewModel.RES_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Conf_Confeccion c_Cort = new Models.C_Conf_Confeccion()
                    {
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        Clave = Defecto.Clave,
                        Descripcion = Defecto.Descripcion,
                        IdSubModulo = 8,
                        IdUsuario = Defecto.IdUsuario,
                        Nombre = Defecto.Nombre,
                        Observaciones = Defecto.Observaciones,
                        Imagen = Defecto.Imagen
                    };
                    db.C_Conf_Confeccion.Add(c_Cort);
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
        /// Valida Nuevo confeccion por SubModulo
        /// </summary>
        /// <param name="SubModulo"></param>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ValidaDefectoConfeccionSubModulo")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CONFECCION ValidaDefectoConfeccionSubModulo(int SubModulo, string Clave, string Nombre)
        {
            ViewModel.RES_DEFECTO_CONFECCION API = new ViewModel.RES_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Conf_Confeccion c_Cort = db.C_Conf_Confeccion.Where(x => x.Clave == Clave && x.Nombre == Nombre && x.IdSubModulo == SubModulo).FirstOrDefault();
                    if (c_Cort != null)
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
        /// Activa o inactiva el defecto por IdDefecto
        /// </summary>
        /// <param name="IdDefecto"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ActivaInactivaDefectoConfeccion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CONFECCION ActivaInactivaDefectoConfeccion(int IdDefecto)
        {
            ViewModel.RES_DEFECTO_CONFECCION API = new ViewModel.RES_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Conf_Confeccion c_Cort = db.C_Conf_Confeccion.Where(x => x.ID == IdDefecto).FirstOrDefault();
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

        /// <summary>
        /// Obtiene defecto por Clave y/o Nombre
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ObtieneDefectoConfeccion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_CONFECCION ObtieneDefectoConfeccion(string Clave = "", string Nombre = "")
        {
            ViewModel.RES_BUS_DEFECTO_CONFECCION API = new ViewModel.RES_BUS_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Confeccion = db.VST_CONFECCION.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 8).OrderBy(x => x.Nombre).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Confeccion = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_Confeccion = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtiene información de un tendido por Id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ObtieneInfoDefectoConfeccion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_EDT_DEFECTO_CONFECCION ObtieneInfoDefectoConfeccion(int ID)
        {
            ViewModel.RES_EDT_DEFECTO_CONFECCION API = new ViewModel.RES_EDT_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Confeccion = db.VST_CONFECCION.Where(x => x.ID == ID).FirstOrDefault();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Confeccion = null;
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
        /// Actualiza los datos del tendido
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ActualizaDefectoConfeccion")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CONFECCION ActualizaDefectoConfeccion([FromBody]ViewModel.REQ_EDT_DEFECTO_CONFECCION Defecto)
        {
            ViewModel.RES_DEFECTO_CONFECCION API = new ViewModel.RES_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Conf_Confeccion Vst = db.C_Conf_Confeccion.Where(x => x.ID == Defecto.ID).FirstOrDefault();
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

        //----------------------------------------OPERACIONES------------------------------------------------

        /// <summary>
        /// Registra un nuevo operación confeccion
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/NuevoOperacionConfeccion")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CONFECCION NuevoOperacionConfeccion([FromBody]ViewModel.REQ_DEFECTO_CONFECCION Defecto)
        {
            ViewModel.RES_DEFECTO_CONFECCION API = new ViewModel.RES_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Conf_Confeccion c_Cort = new Models.C_Conf_Confeccion()
                    {
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        Clave = Defecto.Clave,
                        Descripcion = Defecto.Descripcion,
                        IdSubModulo = 9,
                        IdUsuario = Defecto.IdUsuario,
                        Nombre = Defecto.Nombre,
                        Observaciones = Defecto.Observaciones,
                        Imagen = Defecto.Imagen
                    };
                    db.C_Conf_Confeccion.Add(c_Cort);
                    db.SaveChanges();

                    List<JSON_POS_DEF> POS = _objSerializer.Deserialize<List<JSON_POS_DEF>>(Defecto.Imagen);
                    foreach (JSON_POS_DEF item in POS)
                    {
                        Models.C_Operacion_Confeccion c_Operacion_Confeccion = new Models.C_Operacion_Confeccion()
                        {
                            IdOperacion = c_Cort.ID,
                            IdDefecto = item.IdDefecto
                        };
                        db.C_Operacion_Confeccion.Add(c_Operacion_Confeccion);
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
                API.Hecho = false;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Valida Nuevo operacion por SubModulo
        /// </summary>
        /// <param name="SubModulo"></param>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ValidaOperacionSubModulo")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CONFECCION ValidaOperacionSubModulo(int SubModulo, string Clave, string Nombre = "")
        {
            ViewModel.RES_DEFECTO_CONFECCION API = new ViewModel.RES_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Conf_Confeccion c_Cort = db.C_Conf_Confeccion.Where(x => x.Clave == Clave && x.Nombre == Nombre && x.IdSubModulo == SubModulo).FirstOrDefault();
                    if (c_Cort != null)
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
        /// Activa o inactiva el operacion por IdDefecto
        /// </summary>
        /// <param name="IdOperacion"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ActivaInactivaOperacionConfeccion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CONFECCION ActivaInactivaOperacionConfeccion(int IdOperacion)
        {
            ViewModel.RES_DEFECTO_CONFECCION API = new ViewModel.RES_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Conf_Confeccion c_Cort = db.C_Conf_Confeccion.Where(x => x.ID == IdOperacion).FirstOrDefault();
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

        /// <summary>
        /// Obtiene operación por Clave y/o Nombre
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ObtieneOperacionConfeccion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_CONFECCION ObtieneOperacionConfeccion(string Clave = "", string Nombre = "")
        {
            ViewModel.RES_BUS_DEFECTO_CONFECCION API = new ViewModel.RES_BUS_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Confeccion = db.VST_CONFECCION.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 9).OrderBy(x => x.Nombre).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Confeccion = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_Confeccion = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtiene información de una operacion por Id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ObtieneInfoOperacionConfeccion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_EDT_OPERACION_CONFECCION ObtieneInfoOperacionConfeccion(int ID)
        {
            ViewModel.RES_EDT_OPERACION_CONFECCION API = new ViewModel.RES_EDT_OPERACION_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Confeccion = db.VST_CONFECCION.Where(x => x.ID == ID).FirstOrDefault();
                    API.Vst_Oper_Conf = db.VST_OPERACION_CONFECCION.Where(x => x.IdOperacion == ID).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Confeccion = null;
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
        /// Actualiza los datos del tendido
        /// </summary>
        /// <param name="Operacion"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ActualizaOperacionConfeccion")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CONFECCION ActualizaOperacionConfeccion([FromBody]ViewModel.REQ_EDT_DEFECTO_CONFECCION Operacion)
        {
            ViewModel.RES_DEFECTO_CONFECCION API = new ViewModel.RES_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Conf_Confeccion Vst = db.C_Conf_Confeccion.Where(x => x.ID == Operacion.ID).FirstOrDefault();
                    if (Vst != null)
                    {
                        Vst.IdUsuario = Operacion.IdUsuario;
                        Vst.Nombre = Operacion.Nombre;
                        Vst.Observaciones = Operacion.Observaciones;
                        Vst.Descripcion = Operacion.Descripcion;
                        Vst.Clave = Operacion.Clave;
                        Vst.Imagen = Operacion.Imagen;

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
        /// Obtiene defecto por Clave y/o Nombre
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ObtieneDefectosActivosOperacion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_CONFECCION ObtieneDefectosActivosOperacion()
        {
            ViewModel.RES_BUS_DEFECTO_CONFECCION API = new ViewModel.RES_BUS_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Confeccion = db.VST_CONFECCION.Where(x => x.IdSubModulo == 8 && x.Activo == true).OrderBy(x => x.Nombre).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Confeccion = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_Confeccion = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        //----------------------------------------AREA------------------------------------------------
        /// <summary>
        /// Registra un nuevo area confeccion
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/NuevoAreaConfeccion")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CONFECCION NuevoAreaConfeccion([FromBody]ViewModel.REQ_DEFECTO_CONFECCION Defecto)
        {
            ViewModel.RES_DEFECTO_CONFECCION API = new ViewModel.RES_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Conf_Confeccion c_Cort = new Models.C_Conf_Confeccion()
                    {
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        Clave = Defecto.Clave,
                        Descripcion = Defecto.Descripcion,
                        IdSubModulo = 10,
                        IdUsuario = Defecto.IdUsuario,
                        Nombre = Defecto.Nombre,
                        Observaciones = Defecto.Observaciones,
                        Imagen = Defecto.Imagen
                    };
                    db.C_Conf_Confeccion.Add(c_Cort);
                    db.SaveChanges();

                    List<JSON_POS_DEF> POS = _objSerializer.Deserialize<List<JSON_POS_DEF>>(Defecto.Imagen);
                    foreach (JSON_POS_DEF item in POS)
                    {
                        Models.C_Operacion_Confeccion c_Operacion_Confeccion = new Models.C_Operacion_Confeccion()
                        {
                            IdOperacion = c_Cort.ID,
                            IdDefecto = item.IdDefecto
                        };
                        db.C_Operacion_Confeccion.Add(c_Operacion_Confeccion);
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
                API.Hecho = false;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Valida Nuevo area por SubModulo
        /// </summary>
        /// <param name="SubModulo"></param>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ValidaAreaSubModulo")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CONFECCION ValidaAreaSubModulo(int SubModulo, string Clave, string Nombre = "")
        {
            ViewModel.RES_DEFECTO_CONFECCION API = new ViewModel.RES_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Conf_Confeccion c_Cort = db.C_Conf_Confeccion.Where(x => x.Clave == Clave && x.Nombre == Nombre && x.IdSubModulo == SubModulo).FirstOrDefault();
                    if (c_Cort != null)
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
        /// Activa o inactiva el operacion por area
        /// </summary>
        /// <param name="IdOperacion"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ActivaInactivaAreaConfeccion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CONFECCION ActivaInactivaAreaConfeccion(int IdOperacion)
        {
            ViewModel.RES_DEFECTO_CONFECCION API = new ViewModel.RES_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Conf_Confeccion c_Cort = db.C_Conf_Confeccion.Where(x => x.ID == IdOperacion).FirstOrDefault();
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

        /// <summary>
        /// Obtiene area por Clave y/o Nombre
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ObtieneAreaConfeccion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_CONFECCION ObtieneAreaConfeccion(string Clave = "", string Nombre = "")
        {
            ViewModel.RES_BUS_DEFECTO_CONFECCION API = new ViewModel.RES_BUS_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Confeccion = db.VST_CONFECCION.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 10).OrderBy(x => x.Nombre).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Confeccion = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_Confeccion = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtiene información de una area por Id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ObtieneInfoAreaConfeccion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_EDT_OPERACION_CONFECCION ObtieneInfoAreaConfeccion(int ID)
        {
            ViewModel.RES_EDT_OPERACION_CONFECCION API = new ViewModel.RES_EDT_OPERACION_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Confeccion = db.VST_CONFECCION.Where(x => x.ID == ID).FirstOrDefault();
                    API.Vst_Oper_Conf = db.VST_OPERACION_CONFECCION.Where(x => x.IdOperacion == ID).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Confeccion = null;
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
        /// Actualiza los datos del tendido
        /// </summary>
        /// <param name="Area"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ActualizaAreaConfeccion")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CONFECCION ActualizaAreaConfeccion([FromBody]ViewModel.REQ_EDT_DEFECTO_CONFECCION Area)
        {
            ViewModel.RES_DEFECTO_CONFECCION API = new ViewModel.RES_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Conf_Confeccion Vst = db.C_Conf_Confeccion.Where(x => x.ID == Area.ID).FirstOrDefault();
                    if (Vst != null)
                    {
                        Vst.IdUsuario = Area.IdUsuario;
                        Vst.Nombre = Area.Nombre;
                        Vst.Observaciones = Area.Observaciones;
                        Vst.Descripcion = Area.Descripcion;
                        Vst.Clave = Area.Clave;
                        Vst.Imagen = Area.Imagen;

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
        /// Obtiene defecto por Clave y/o Nombre
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ObtieneOperacionAreas")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_CONFECCION ObtieneOperacionAreas()
        {
            ViewModel.RES_BUS_DEFECTO_CONFECCION API = new ViewModel.RES_BUS_DEFECTO_CONFECCION();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Confeccion = db.VST_CONFECCION.Where(x => x.IdSubModulo == 9 && x.Activo == true).OrderBy(x => x.Nombre).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Confeccion = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_Confeccion = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }


        //----------------------------------------PLANTAS------------------------------------------------

        /// <summary>
        /// Obtiene las plantas dadas de alta en Dynamics
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ObtienePlantasDynamics")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_PLANTAS ObtienePlantasDynamics()
        {
            ViewModel.RES_PLANTAS API = new ViewModel.RES_PLANTAS();
            try
            {
                string Consulta = "SELECT Descr, Planta FROM RsTb_Plantas";
                using (SqlConnection _Conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["dbRioSulApp"].ToString()))
                {
                    _Conn.Open();
                    API.Vst_Plantas = new List<ViewModel.PLANTAS>();
                    SqlCommand _Command = new SqlCommand(Consulta, _Conn);
                    SqlDataReader _Reader = _Command.ExecuteReader();
                    while (_Reader.Read())
                    {
                        ViewModel.PLANTAS pLANTAS = new ViewModel.PLANTAS();
                        pLANTAS.Descripcion = _Reader[0].ToString().Trim();
                        pLANTAS.Planta = _Reader[1].ToString().Trim();

                        API.Vst_Plantas.Add(pLANTAS);
                    }
                    _Reader.Close();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                API.Vst_Plantas = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                Utilerias.EscribirLog(ex.ToString());
            }
            return API;
        }

        /// <summary>
        /// Ingresa una nueva relación entre planta y área
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/NuevaPlantaArea")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_PLANTA NuevaPlantaArea([FromBody]ViewModel.REQ_PLANTA_AREA aREA)
        {
            ViewModel.RES_PLANTA API = new ViewModel.RES_PLANTA();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Plantas_Dynamic Planta = new Models.C_Plantas_Dynamic();
                    Planta.Descripcion = aREA.DescripcionPlanta;
                    Planta.Planta = aREA.Planta;
                    db.C_Plantas_Dynamic.Add(Planta);
                    db.SaveChanges();

                    List<JSON_POS_DEF> POS = _objSerializer.Deserialize<List<JSON_POS_DEF>>(aREA.Areas);
                    foreach (JSON_POS_DEF item in POS)
                    {
                        Models.C_Plantas_Areas c_Plantas_Areas = new Models.C_Plantas_Areas()
                        {
                            IdPlanta = Planta.IdPlanta,
                            IdArea = item.IdDefecto
                        };
                        db.C_Plantas_Areas.Add(c_Plantas_Areas);
                    }
                    db.SaveChanges();

                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                Utilerias.EscribirLog(ex.ToString());
            }
            return API;
        }

        /// <summary>
        /// Verifica que la planta no se encuentre ya registrada alguna relación
        /// </summary>
        /// <param name="IdPlanta"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ValidaExistenciaPlanta")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public bool ValidaExistenciaPlanta(int IdPlanta)
        {
            bool Result = true;
            try
            {
                Models.C_Plantas_Areas c_Plantas_ = db.C_Plantas_Areas.Where(x => x.IdPlanta == IdPlanta).FirstOrDefault();
                if (c_Plantas_ != null)
                {
                    Result = false;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                Result = false;
            }
            return Result;
        }

        /// <summary>
        /// Obtiene las relaciones entre áreas y plantas
        /// </summary>
        /// <param name="Planta"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Confeccion/ObtieneAreasRelPlantas")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_PLANTAS_AREAS_REL ObtieneAreasRelPlantas(string Planta)
        {
            ViewModel.RES_PLANTAS_AREAS_REL API = new ViewModel.RES_PLANTAS_AREAS_REL();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Plantas_Dynamic _Dynamic = db.C_Plantas_Dynamic.Where(x => x.Planta == Planta).FirstOrDefault();
                    if (_Dynamic != null)
                    {
                        API.IdPlanta = _Dynamic.IdPlanta;
                        API.Descripcion = _Dynamic.Descripcion;
                        API.Planta = _Dynamic.Planta;
                        API.Areas = db.VST_PLANTAS_AREAS.Where(x => x.IdPlanta == API.IdPlanta).ToList();
                    }
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


        /// <summary>
        /// Elimina la relación de una planta con área
        /// </summary>
        /// <param name="IdPlantaArea"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/Cnfeccion/EliminaRelacionAreaPlanta")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public bool EliminaRelacionAreaPlanta(int IdPlantaArea)
        {
            bool Result = true;
            try
            {
                Models.C_Plantas_Areas _Plantas_Areas = db.C_Plantas_Areas.Where(x => x.IdPlantaArea == IdPlantaArea).FirstOrDefault();
                if (_Plantas_Areas != null)
                {
                    db.C_Plantas_Areas.Remove(_Plantas_Areas);

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                Result = false;
            }
            return Result;
        }

        /// <summary>
        /// Obtiene las áreas relacionadas a una planta
        /// </summary>
        /// <param name="IdPlanta"></param>
        [System.Web.Http.Route("api/Confeccion/ObtieneInfoAreaPlanta")]
        [System.Web.Http.HttpPost]
        public void ObtieneInfoAreaPlanta(int IdPlanta)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
            }
        }
    }
}
