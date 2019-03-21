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
        #region DEFECTOS

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

        #endregion

        //----------------------------------------OPERACIONES------------------------------------------------

        #region OPERACIONES

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

        #endregion

        //----------------------------------------POSICIONES------------------------------------------------

        #region POSICIONES
        /// <summary>
        /// Registra una nueva posición de lavandería
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Lavanderia/PosicionLavanderia")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_POSICION_LAV NuevaPosicionLavanderia([FromBody]ViewModel.N_POSICION_LAV Posicion)
        {
            ViewModel.RES_POSICION_LAV API = new ViewModel.RES_POSICION_LAV();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Lavanderia Verifica = db.C_Lavanderia.Where(x =>
                            x.IdSubModulo == 20 && (x.Clave == Posicion.Clave || x.Nombre == Posicion.Nombre))
                        .FirstOrDefault();

                    if (Verifica == null)
                    {
                        Models.C_Lavanderia c_Lavanderia = new Models.C_Lavanderia()
                        {
                            Activo = true,
                            FechaCreacion = DateTime.Now,
                            Clave = Posicion.Clave,
                            Descripcion = Posicion.Descripcion,
                            IdSubModulo = 20,
                            IdUsuario = Posicion.IdUsuario,
                            Nombre = Posicion.Nombre,
                            Observaciones = Posicion.Observaciones,
                            Imagen = Posicion.Imagen
                        };
                        db.C_Lavanderia.Add(c_Lavanderia);
                        db.SaveChanges();

                        foreach (ViewModel.OPERACION_REF item in Posicion.Operacion)
                        {
                            Models.C_Posicion_Lavanderia c_Posicion_Lavanderia = new Models.C_Posicion_Lavanderia()
                            {
                                IdPosicion = c_Lavanderia.ID,
                                IdCortador = item.IdOperacion
                            };
                            db.C_Posicion_Lavanderia.Add(c_Posicion_Lavanderia);
                        }
                        db.SaveChanges();

                        API.Hecho = "Registro realizado con éxito";
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    else
                    {
                        API.Hecho = "Registro existente, favor de validar";
                        API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
                    }

                    
                }
                else
                {
                    API.Hecho = "Formato inválido";
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Hecho = "Error interno";
            }
            return API;
        }

        /// <summary>
        /// Activa o inactiva la posición de lavandería
        /// </summary>
        /// <param name="IdLavanderia"></param>
        /// <returns></returns>
        [HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Lavanderia/ActivaInactivaPosicionLavanderia")]
        public ViewModel.RES_POSICION_LAV ActivaInactivaPosicionLavanderia(int IdLavanderia)
        {
            ViewModel.RES_POSICION_LAV API = new ViewModel.RES_POSICION_LAV();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Lavanderia c_Lav = db.C_Lavanderia.Where(x => x.ID == IdLavanderia && x.IdSubModulo == 20).FirstOrDefault();
                    c_Lav.Activo = (c_Lav.Activo == false ? true : false);

                    db.Entry(c_Lav).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    API.Hecho = "Posición modificada con éxito";
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Hecho = "Formato Inválido";
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Hecho = "Error Interno";
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtiene Posicion por Clave y/o Nombre
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Lavanderia/PosicionLavanderia")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_LAVANDERIA ObtienePosicionLavanderia(string Clave = "", string Nombre = "")
        {
            ViewModel.RES_BUS_DEFECTO_LAVANDERIA API = new ViewModel.RES_BUS_DEFECTO_LAVANDERIA();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Lavanderia = db.VST_LAVANDERIA.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 20 && x.Activo == true).OrderBy(x => x.Nombre).ToList();
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
        /// Actualiza los datos de la posición de lavandría
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/Lavanderia/PosicionLavanderia")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_POSICION_LAV ActualizaPosicionLav([FromBody]ViewModel.E_POSICION_LAV Posicion)
        {
            ViewModel.RES_POSICION_LAV API = new ViewModel.RES_POSICION_LAV();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Lavanderia Vst = db.C_Lavanderia.Where(x => x.ID == Posicion.ID && x.IdSubModulo == 20).FirstOrDefault();
                    //VERIFICAMOS SI EL ELEMENTO EXISTE EN BASE
                    if (Vst != null)
                    {
                        Vst.IdUsuario = Posicion.IdUsuario;
                        Vst.Nombre = Posicion.Nombre;
                        Vst.Observaciones = Posicion.Observaciones;
                        Vst.Descripcion = Posicion.Descripcion;
                        Vst.Clave = Posicion.Clave;
                        Vst.Imagen = Posicion.Imagen;

                        //VERIFICAMOS SI LA CLAVE O NOMBRE YA EXISTEN EN BASE
                        Models.C_Lavanderia Verifica = db.C_Lavanderia.Where(x => x.IdSubModulo == 20 && x.ID != Posicion.ID &&(x.Clave == Posicion.Clave || x.Nombre == Posicion.Nombre)).FirstOrDefault();

                        if (Verifica == null)
                        {
                            db.Entry(Vst).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            //ELIMNAMOS LAS OPERACIONES RELACIONADAS
                            List<Models.C_Posicion_Lavanderia> PT = db.C_Posicion_Lavanderia.Where(x => x.IdPosicion == Posicion.ID).ToList();

                            foreach (Models.C_Posicion_Lavanderia item in PT)
                            {
                                db.C_Posicion_Lavanderia.Remove(item);
                                db.SaveChanges();
                            }

                            //GUARDAMOS LAS OPERACIONES
                            foreach (ViewModel.OPERACION_REF item in Posicion.Operacion)
                            {
                                Models.C_Posicion_Lavanderia c_Posicion_Lavanderia = new Models.C_Posicion_Lavanderia()
                                {
                                    IdPosicion = Vst.ID,
                                    IdCortador = item.IdOperacion
                                };
                                db.C_Posicion_Lavanderia.Add(c_Posicion_Lavanderia);
                            }
                            db.SaveChanges();

                            API.Hecho = "Posición actualizada con éxito";
                            API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        else
                        {
                            API.Hecho = "Registro existente, favor de validar";
                            API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
                        }
                    }
                    else
                    {
                        API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
                        API.Hecho = "Registro no encontrado";
                    }
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Hecho = "Formato Inválido";
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Hecho = "Error interno";
            }
            return API;
        }

        #endregion
    }
}