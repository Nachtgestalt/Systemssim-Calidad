using System;
using System.Collections.Generic;
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
    public class CortadoresController : ApiController
    {
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();
        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();

        //----------------------------------------CORTADRORES------------------------------------------------
        #region CORTADORES
        
        /// <summary>
        /// Registra un nuevo cortador
        /// </summary>
        /// <param name="cORTADOR"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/NuevoCortador")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_CORTADOR NuevoCortador([FromBody]ViewModel.REQ_CORTADOR cORTADOR)
        {
            ViewModel.RES_CORTADOR API = new ViewModel.RES_CORTADOR();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = new Models.C_Cort_Cortadores()
                    {
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        Clave = cORTADOR.Clave,
                        Descripcion = cORTADOR.Descripcion,
                        IdSubModulo = 1,
                        IdUsuario = cORTADOR.IdUsuario,
                        Nombre = cORTADOR.Nombre,
                        Observaciones = cORTADOR.Observaciones
                    };
                    db.C_Cort_Cortadores.Add(c_Cort);
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
        /// Valida Nuevo cortador por SubModulo
        /// </summary>
        /// <param name="SubModulo"></param>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ValidaCortadorSubModulo")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_CORTADOR ValidaCortadorSubModulo(string Clave = "", string Nombre = "", int ID = 0)
        {
            ViewModel.RES_CORTADOR API = new ViewModel.RES_CORTADOR();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => (x.Clave == Clave || x.Nombre == Nombre) && x.IdSubModulo == 1 && x.ID != ID).FirstOrDefault();
                    if (c_Cort != null)
                    {
                        API.Hecho = true;
                    }
                    else
                    {
                        API.Hecho = false;
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
        /// Activa o inactiva cortador por IdCortador
        /// </summary>
        /// <param name="IdCortador"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ActivaInactivaCortador")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_CORTADOR ActivaInactivaCortador(int IdCortador)
        {
            ViewModel.RES_CORTADOR API = new ViewModel.RES_CORTADOR();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => x.ID == IdCortador).FirstOrDefault();
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
        /// Obtiene cortadores por Clave y/o Nombre
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ObtieneCortadores")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_CORTADOR ObtieneCortadores(string Clave = "", string Nombre = "", string Activo = "")
        {
            ViewModel.RES_BUS_CORTADOR API = new ViewModel.RES_BUS_CORTADOR();
            List<Models.VST_CORTADORES> cortadores = new List<Models.VST_CORTADORES>();
            try
            {
                if (ModelState.IsValid)
                {
                    switch (Activo)
                    {
                        case "True":
                            cortadores = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 1 && x.Activo == true).OrderBy(x => x.Nombre).ToList();
                            break;
                        case "False":
                            cortadores = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 1 && x.Activo == false).OrderBy(x => x.Nombre).ToList();
                            break;
                        default:
                            cortadores = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 1).OrderBy(x => x.Nombre).ToList();
                            break;
                    }

                    API.Vst_Cortadores = cortadores;
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Cortadores = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_Cortadores = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtiene información de un cortador por Id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ObtieneInfoCortador")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_EDT_CORTADOR ObtieneInfoCortador(int ID)
        {
            ViewModel.RES_EDT_CORTADOR API = new ViewModel.RES_EDT_CORTADOR();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Cortador = db.VST_CORTADORES.Where(x => x.ID == ID).FirstOrDefault();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Cortador = null;
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
        /// Actualiza los datos del cortador
        /// </summary>
        /// <param name="cORTADOR"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ActualizaCortador")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_CORTADOR ActualizaCortador([FromBody]ViewModel.REQ_EDT_CORTADOR cORTADOR)
        {
            ViewModel.RES_CORTADOR API = new ViewModel.RES_CORTADOR();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores Vst = db.C_Cort_Cortadores.Where(x => x.ID == cORTADOR.ID).FirstOrDefault();
                    if (Vst != null)
                    {
                        Vst.IdUsuario = cORTADOR.IdUsuario;
                        Vst.Nombre = cORTADOR.Nombre;
                        Vst.Observaciones = cORTADOR.Observaciones;
                        Vst.Descripcion = cORTADOR.Descripcion;
                        Vst.Clave = cORTADOR.Clave;

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
        /// Elimina cortador por IdCortador
        /// </summary>
        /// <param name="IdCortador"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/Cortador")]
        [System.Web.Http.HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_CORTADOR EliminaCortador(int IdCortador)
        {
            ViewModel.RES_CORTADOR API = new ViewModel.RES_CORTADOR();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => x.ID == IdCortador).FirstOrDefault();
                    db.C_Cort_Cortadores.Remove(c_Cort);
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

        #endregion
        //----------------------------------------TENDIDO------------------------------------------------
        #region TENDIDO

        /// <summary>
        /// Registra un nuevo tendido
        /// </summary>
        /// <param name="Tendido"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/NuevoTendido")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_TENDIDO NuevoTendido([FromBody]ViewModel.REQ_TENDIDO Tendido)
        {
            ViewModel.RES_TENDIDO API = new ViewModel.RES_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = new Models.C_Cort_Cortadores()
                    {
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        Clave = Tendido.Clave,
                        Descripcion = Tendido.Descripcion,
                        IdSubModulo = 2,
                        IdUsuario = Tendido.IdUsuario,
                        Nombre = Tendido.Nombre,
                        Observaciones = Tendido.Observaciones,
                        TipoTendido = Tendido.TipoTendido
                    };
                    db.C_Cort_Cortadores.Add(c_Cort);
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
        /// Valida Nuevo tendido por SubModulo
        /// </summary>
        /// <param name="SubModulo"></param>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ValidaTendidoSubModulo")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_TENDIDO ValidaTendidoSubModulo(string Clave = "", string Nombre = "", int ID = 0)
        {
            ViewModel.RES_TENDIDO API = new ViewModel.RES_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => (x.Clave == Clave || x.Nombre == Nombre) && x.IdSubModulo == 2 && x.ID != ID).FirstOrDefault();
                    if (c_Cort != null)
                    {
                        API.Hecho = true;
                    }
                    else
                    {
                        API.Hecho = false;
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
        /// Activa o inactiva tendido por IdTendido
        /// </summary>
        /// <param name="IdTendido"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ActivaInactivaTendido")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_TENDIDO ActivaInactivaTendido(int IdTendido)
        {
            ViewModel.RES_TENDIDO API = new ViewModel.RES_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => x.ID == IdTendido).FirstOrDefault();
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
        /// Obtiene tendido por Clave y/o Nombre
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ObtieneTendido")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_TENDIDO ObtieneTendido(string Clave = "", string Nombre = "", string Activo = "")
        {
            ViewModel.RES_BUS_TENDIDO API = new ViewModel.RES_BUS_TENDIDO();
            List<Models.VST_CORTADORES> tendido = new List<Models.VST_CORTADORES>();
            try
            {
                if (ModelState.IsValid)
                {
                    switch (Activo)
                    {
                        case "True":
                            tendido = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 2 && x.Activo == true).OrderBy(x => x.Nombre).ToList();
                            break;
                        case "False":
                            tendido = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 2 && x.Activo == false).OrderBy(x => x.Nombre).ToList();
                            break;
                        default:
                            tendido = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 2).OrderBy(x => x.Nombre).ToList();
                            break;
                    }
                    API.Vst_Cortadores = tendido;
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Cortadores = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_Cortadores = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtiene información de un tendido por Id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ObtieneInfoTendido")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_EDT_TENDIDO ObtieneInfoTendido(int ID)
        {
            ViewModel.RES_EDT_TENDIDO API = new ViewModel.RES_EDT_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Cortador = db.VST_CORTADORES.Where(x => x.ID == ID).FirstOrDefault();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Cortador = null;
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
        /// <param name="Tendido"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ActualizaTendido")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_TENDIDO ActualizaTendido([FromBody]ViewModel.REQ_EDT_TENDIDO Tendido)
        {
            ViewModel.RES_TENDIDO API = new ViewModel.RES_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores Vst = db.C_Cort_Cortadores.Where(x => x.ID == Tendido.ID).FirstOrDefault();
                    if (Vst != null)
                    {
                        Vst.IdUsuario = Tendido.IdUsuario;
                        Vst.Nombre = Tendido.Nombre;
                        Vst.Observaciones = Tendido.Observaciones;
                        Vst.Descripcion = Tendido.Descripcion;
                        Vst.Clave = Tendido.Clave;
                        Vst.TipoTendido = Tendido.TipoTendido;

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
        /// Elimina cortador por IdCortador
        /// </summary>
        /// <param name="IdCortador"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/Tendido")]
        [System.Web.Http.HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_CORTADOR EliminaTendido(int ID)
        {
            ViewModel.RES_CORTADOR API = new ViewModel.RES_CORTADOR();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => x.ID == ID).FirstOrDefault();
                    db.C_Cort_Cortadores.Remove(c_Cort);
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

        #endregion
        //----------------------------------------TIPO TENDIDO------------------------------------------------
        #region TIPO TENDIDO

        /// <summary>
        /// Registra un nuevo tendido
        /// </summary>
        /// <param name="TipoTendido"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/NuevoTipoTendido")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_TIPO_TENDIDO NuevoTipoTendido([FromBody]ViewModel.REQ_TIPO_TENDIDO TipoTendido)
        {
            ViewModel.RES_TIPO_TENDIDO API = new ViewModel.RES_TIPO_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = new Models.C_Cort_Cortadores()
                    {
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        Clave = TipoTendido.Clave,
                        Descripcion = TipoTendido.Descripcion,
                        IdSubModulo = 3,
                        IdUsuario = TipoTendido.IdUsuario,
                        Nombre = TipoTendido.Nombre,
                        Observaciones = TipoTendido.Observaciones
                    };
                    db.C_Cort_Cortadores.Add(c_Cort);
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
        /// Valida Nuevo tendido por SubModulo
        /// </summary>
        /// <param name="SubModulo"></param>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ValidaTipoTendidoSubModulo")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_TIPO_TENDIDO ValidaTipoTendidoSubModulo(int SubModulo, string Clave, string Nombre)
        {
            ViewModel.RES_TIPO_TENDIDO API = new ViewModel.RES_TIPO_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => x.Clave == Clave && x.Nombre == Nombre && x.IdSubModulo == SubModulo).FirstOrDefault();
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
        /// Activa o inactiva tendido por IdTendido
        /// </summary>
        /// <param name="IdTipoTendido"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ActivaInactivaTipoTendido")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_TENDIDO ActivaInactivaTipoTendido(int IdTipoTendido)
        {
            ViewModel.RES_TENDIDO API = new ViewModel.RES_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => x.ID == IdTipoTendido).FirstOrDefault();
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
        /// Obtiene tendido por Clave y/o Nombre
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ObtieneTipoTendido")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_TENDIDO ObtieneTipoTendido(string Clave = "", string Nombre = "")
        {
            ViewModel.RES_BUS_TENDIDO API = new ViewModel.RES_BUS_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Cortadores = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 3).OrderBy(x => x.Nombre).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Cortadores = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_Cortadores = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtiene información de un tendido por Id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ObtieneInfoTipoTendido")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_EDT_TIPO_TENDIDO ObtieneInfoTipoTendido(int ID)
        {
            ViewModel.RES_EDT_TIPO_TENDIDO API = new ViewModel.RES_EDT_TIPO_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Cortador = db.VST_CORTADORES.Where(x => x.ID == ID).FirstOrDefault();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Cortador = null;
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
        /// <param name="TipoTendido"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ActualizaTipoTendido")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_TIPO_TENDIDO ActualizaTipoTendido([FromBody]ViewModel.REQ_EDT_TIPO_TENDIDO TipoTendido)
        {
            ViewModel.RES_TIPO_TENDIDO API = new ViewModel.RES_TIPO_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores Vst = db.C_Cort_Cortadores.Where(x => x.ID == TipoTendido.ID).FirstOrDefault();
                    if (Vst != null)
                    {
                        Vst.IdUsuario = TipoTendido.IdUsuario;
                        Vst.Nombre = TipoTendido.Nombre;
                        Vst.Observaciones = TipoTendido.Observaciones;
                        Vst.Descripcion = TipoTendido.Descripcion;
                        Vst.Clave = TipoTendido.Clave;

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

        #endregion
        //----------------------------------------MESA------------------------------------------------
        #region MESA
        /// <summary>
        /// Registra un nuevo tendido
        /// </summary>
        /// <param name="TipoTendido"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/NuevoMesa")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_TIPO_TENDIDO NuevoMesa([FromBody]ViewModel.REQ_TIPO_TENDIDO TipoTendido)
        {
            ViewModel.RES_TIPO_TENDIDO API = new ViewModel.RES_TIPO_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = new Models.C_Cort_Cortadores()
                    {
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        Clave = TipoTendido.Clave,
                        Descripcion = TipoTendido.Descripcion,
                        IdSubModulo = 4,
                        IdUsuario = TipoTendido.IdUsuario,
                        Nombre = TipoTendido.Nombre,
                        Observaciones = TipoTendido.Observaciones
                    };
                    db.C_Cort_Cortadores.Add(c_Cort);
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
        /// Valida Nuevo tendido por SubModulo
        /// </summary>
        /// <param name="SubModulo"></param>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ValidaMesaSubModulo")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_TIPO_TENDIDO ValidaMesaSubModulo(string Clave = "", string Nombre = "", int ID = 0)
        {
            ViewModel.RES_TIPO_TENDIDO API = new ViewModel.RES_TIPO_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => (x.Clave == Clave || x.Nombre == Nombre) && x.IdSubModulo == 4 && x.ID != ID).FirstOrDefault();
                    if (c_Cort != null)
                    {
                        API.Hecho = true;
                    }
                    else
                    {
                        API.Hecho = false;
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
        /// Activa o inactiva tendido por IdTendido
        /// </summary>
        /// <param name="IdMesa"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ActivaInactivaMesa")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_TENDIDO ActivaInactivaMesa(int IdMesa)
        {
            ViewModel.RES_TENDIDO API = new ViewModel.RES_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => x.ID == IdMesa).FirstOrDefault();
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
        /// Obtiene tendido por Clave y/o Nombre
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ObtieneMesa")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_TENDIDO ObtieneMesa(string Clave = "", string Nombre = "", string Activo = "")
        {
            ViewModel.RES_BUS_TENDIDO API = new ViewModel.RES_BUS_TENDIDO();
            List<Models.VST_CORTADORES> mesa = new List<Models.VST_CORTADORES>();
            try
            {
                if (ModelState.IsValid)
                {
                    switch (Activo)
                    {
                        case "True":
                            mesa = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 4 && x.Activo ==true).OrderBy(x => x.Nombre).ToList();
                            break;
                        case "False":
                            mesa = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 4 && x.Activo == false).OrderBy(x => x.Nombre).ToList();
                            break;
                        default:
                            mesa = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 4).OrderBy(x => x.Nombre).ToList();
                            break;
                    }

                    API.Vst_Cortadores = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 4).OrderBy(x => x.Nombre).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Cortadores = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_Cortadores = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtiene información de un tendido por Id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ObtieneInfoMesa")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_EDT_TIPO_TENDIDO ObtieneInfoMesa(int ID)
        {
            ViewModel.RES_EDT_TIPO_TENDIDO API = new ViewModel.RES_EDT_TIPO_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Cortador = db.VST_CORTADORES.Where(x => x.ID == ID).FirstOrDefault();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Cortador = null;
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
        /// <param name="TipoTendido"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ActualizaMesa")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_TIPO_TENDIDO ActualizaMesa([FromBody]ViewModel.REQ_EDT_TIPO_TENDIDO TipoTendido)
        {
            ViewModel.RES_TIPO_TENDIDO API = new ViewModel.RES_TIPO_TENDIDO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores Vst = db.C_Cort_Cortadores.Where(x => x.ID == TipoTendido.ID).FirstOrDefault();
                    if (Vst != null)
                    {
                        Vst.IdUsuario = TipoTendido.IdUsuario;
                        Vst.Nombre = TipoTendido.Nombre;
                        Vst.Observaciones = TipoTendido.Observaciones;
                        Vst.Descripcion = TipoTendido.Descripcion;
                        Vst.Clave = TipoTendido.Clave;

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
        /// Elimina cortador por IdCortador
        /// </summary>
        /// <param name="IdCortador"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/Mesa")]
        [System.Web.Http.HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_CORTADOR EliminaMesa(int ID)
        {
            ViewModel.RES_CORTADOR API = new ViewModel.RES_CORTADOR();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => x.ID == ID).FirstOrDefault();
                    db.C_Cort_Cortadores.Remove(c_Cort);
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
        #endregion
        //----------------------------------------DEFECTOS------------------------------------------------
        #region DEFECTOS
        /// <summary>
        /// Registra un nuevo tendido
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/NuevoDefecto")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CORTE NuevoDefecto([FromBody]ViewModel.REQ_DEFECTO_CORTE Defecto)
        {
            string image_name = "";
            ViewModel.RES_DEFECTO_CORTE API = new ViewModel.RES_DEFECTO_CORTE();
            try
            {
                if (ModelState.IsValid)
                {
                    if (Defecto.Imagen != null && !Defecto.Imagen.IsEmpty())
                    {
                        string base64 = Defecto.Imagen.Substring(Defecto.Imagen.IndexOf(',') + 1);
                        byte[] data = Convert.FromBase64String(base64);

                        image_name = "Corte_Defecto" + "6_" + Defecto.Clave + DateTime.Now.ToString("yymmssfff");

                        using (var image_file = new System.IO.FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                        {
                            image_file.Write(data, 0, data.Length);
                            image_file.Flush();
                        }
                    }

                    Models.C_Cort_Cortadores c_Cort = new Models.C_Cort_Cortadores()
                    {
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        Clave = Defecto.Clave,
                        Descripcion = Defecto.Descripcion,
                        IdSubModulo = 6,
                        IdUsuario = Defecto.IdUsuario,
                        Nombre = Defecto.Nombre,
                        Observaciones = Defecto.Observaciones,
                        Imagen = image_name
                    };
                    db.C_Cort_Cortadores.Add(c_Cort);
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
        /// Valida Nuevo tendido por SubModulo
        /// </summary>
        /// <param name="SubModulo"></param>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ValidaDefectoSubModulo")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CORTE ValidaDefectoSubModulo(string Clave = "", string Nombre = "",int ID = 0)
        {
            ViewModel.RES_DEFECTO_CORTE API = new ViewModel.RES_DEFECTO_CORTE();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => (x.Clave == Clave || x.Nombre == Nombre) && x.IdSubModulo == 6 && x.ID != ID).FirstOrDefault();
                    if (c_Cort != null)
                    {
                        API.Hecho = true;
                    }
                    else
                    {
                        API.Hecho = false;
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
        [System.Web.Http.Route("api/Cortadores/ActivaInactivaDefecto")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CORTE ActivaInactivaDefecto(int IdDefecto)
        {
            ViewModel.RES_DEFECTO_CORTE API = new ViewModel.RES_DEFECTO_CORTE();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => x.ID == IdDefecto).FirstOrDefault();
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
        [System.Web.Http.Route("api/Cortadores/ObtieneDefecto")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_CORTE ObtieneDefecto(string Clave = "", string Nombre = "", string Activo = "")
        {
            ViewModel.RES_BUS_DEFECTO_CORTE API = new ViewModel.RES_BUS_DEFECTO_CORTE();
            API.Vst_Cortadores = new List<Models.VST_CORTADORES>();
            List<Models.VST_CORTADORES> defectos = new List<Models.VST_CORTADORES>();
            string image_name = "";
            string file_path = "";
            try
            {
                if (ModelState.IsValid)
                {
                    switch (Activo)
                    {
                        case "True":
                            defectos = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 6 && x.Activo == true).OrderBy(x => x.Nombre).ToList();
                            break;
                        case "False":
                            defectos = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 6 && x.Activo == false).OrderBy(x => x.Nombre).ToList();
                            break;
                        default:
                            defectos = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 6).OrderBy(x => x.Nombre).ToList();
                            break;
                    }

                    foreach(Models.VST_CORTADORES item in defectos)
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

                        API.Vst_Cortadores.Add(item);
                    }
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Cortadores = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_Cortadores = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtiene información de un tendido por Id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ObtieneInfoDefecto")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_EDT_DEFECTO_CORTE ObtieneInfoDefecto(int ID)
        {
            ViewModel.RES_EDT_DEFECTO_CORTE API = new ViewModel.RES_EDT_DEFECTO_CORTE();
            API.Vst_Cortador = new Models.VST_CORTADORES();

            Models.VST_CORTADORES defecto = new Models.VST_CORTADORES();

            string image_name = "";
            string file_path = "";
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Cortador = db.VST_CORTADORES.Where(x => x.ID == ID).FirstOrDefault();

                    file_path = HttpContext.Current.Server.MapPath("~/Imagenes/");
                    file_path = file_path + API.Vst_Cortador.Imagen + ".jpg";
                    if (File.Exists(file_path))
                    {
                        API.Vst_Cortador.Imagen = "data:image/" + "jpg" + ";base64," + Convert.ToBase64String(File.ReadAllBytes(file_path));
                    }
                    else
                    {
                        API.Vst_Cortador.Imagen = "";
                    }

                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Cortador = null;
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
        [System.Web.Http.Route("api/Cortadores/ActualizaDefecto")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CORTE ActualizaDefecto([FromBody]ViewModel.REQ_EDT_DEFECTO_CORTE Defecto)
        {
            ViewModel.RES_DEFECTO_CORTE API = new ViewModel.RES_DEFECTO_CORTE();
            string image_name = "";
            try
            {
                if (ModelState.IsValid)
                {
                    if (Defecto.Imagen != null && !Defecto.Imagen.IsEmpty())
                    {
                        string base64 = Defecto.Imagen.Substring(Defecto.Imagen.IndexOf(',') + 1);
                        byte[] data = Convert.FromBase64String(base64);

                        image_name = "Corte_Defecto" + "6_" + Defecto.Clave + DateTime.Now.ToString("yymmssfff");

                        using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                        {
                            image_file.Write(data, 0, data.Length);
                            image_file.Flush();
                        }
                    }

                    Models.C_Cort_Cortadores Vst = db.C_Cort_Cortadores.Where(x => x.ID == Defecto.ID).FirstOrDefault();
                    if (Vst != null)
                    {
                        Vst.IdUsuario = Defecto.IdUsuario;
                        Vst.Nombre = Defecto.Nombre;
                        Vst.Observaciones = Defecto.Observaciones;
                        Vst.Descripcion = Defecto.Descripcion;
                        Vst.Clave = Defecto.Clave;
                        Vst.Imagen = image_name;

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
        /// Elimina cortador por IdCortador
        /// </summary>
        /// <param name="IdCortador"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/Defecto")]
        [System.Web.Http.HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_CORTADOR EliminaDefecto(int ID)
        {
            ViewModel.RES_CORTADOR API = new ViewModel.RES_CORTADOR();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => x.ID == ID).FirstOrDefault();
                    db.C_Cort_Cortadores.Remove(c_Cort);
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
        #endregion
        //----------------------------------------POSICION------------------------------------------------
        #region POSICION
        /// <summary>
        /// Registra un nuevo posición
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/NuevoPosicion")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CORTE NuevoPosicion([FromBody]ViewModel.REQ_POSICION_CORTE Defecto)
        {
            ViewModel.RES_DEFECTO_CORTE API = new ViewModel.RES_DEFECTO_CORTE();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = new Models.C_Cort_Cortadores()
                    {
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        Clave = Defecto.Clave,
                        Descripcion = Defecto.Descripcion,
                        IdSubModulo = 7,
                        IdUsuario = Defecto.IdUsuario,
                        Nombre = Defecto.Nombre,
                        Observaciones = Defecto.Observaciones
                    };

                    db.C_Cort_Cortadores.Add(c_Cort);
                    db.SaveChanges();

                    foreach (ViewModel.C_Posicion item in Defecto.Defecto)
                    {
                        Models.C_Posicion_Cortador c_Posicion_Cortador = new Models.C_Posicion_Cortador()
                        {
                            IdCortador = item.IdCortador,
                            IdPosicion = c_Cort.ID
                        };
                        db.C_Posicion_Cortador.Add(c_Posicion_Cortador);
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
        /// Valida Nueva posisión por SubModulo
        /// </summary>
        /// <param name="SubModulo"></param>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ValidaPosicionSubModulo")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CORTE ValidaPosicionSubModulo(string Clave = "", string Nombre = "", int ID = 0)
        {
            ViewModel.RES_DEFECTO_CORTE API = new ViewModel.RES_DEFECTO_CORTE();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => (x.Clave == Clave || x.Nombre == Nombre) && x.IdSubModulo == 7 && x.ID != ID).FirstOrDefault();
                    if (c_Cort != null)
                    {
                        API.Hecho = true;
                    }
                    else
                    {
                        API.Hecho = false;
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
        /// Activa o inactiva el defecto por Posicion
        /// </summary>
        /// <param name="IdPosicion"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ActivaInactivaPosicion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CORTE ActivaInactivaPosicion(int IdPosicion)
        {
            ViewModel.RES_DEFECTO_CORTE API = new ViewModel.RES_DEFECTO_CORTE();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => x.ID == IdPosicion).FirstOrDefault();
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
        /// Obtiene posicion por Clave y/o Nombre
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ObtienePosicion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_CORTE ObtienePosicion(string Clave = "", string Nombre = "", string Activo = "")
        {
            ViewModel.RES_BUS_DEFECTO_CORTE API = new ViewModel.RES_BUS_DEFECTO_CORTE();
            List<Models.VST_CORTADORES> posicion = new List<Models.VST_CORTADORES>();
            try
            {
                if (ModelState.IsValid)
                {
                    switch (Activo)
                    {
                        case "True":
                            posicion = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 7 && x.Activo == true).OrderBy(x => x.Nombre).ToList();
                            break;
                        case "False":
                            posicion = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 7 && x.Activo == false).OrderBy(x => x.Nombre).ToList();
                            break;
                        default:
                            posicion = db.VST_CORTADORES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 7).OrderBy(x => x.Nombre).ToList();
                            break;
                    }

                    API.Vst_Cortadores = posicion;
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Cortadores = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_Cortadores = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtiene información de una posicion por Id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ObtieneInfoPosicion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_EDT_POSICION_CORTE ObtieneInfoPosicion(int ID)
        {
            ViewModel.RES_EDT_POSICION_CORTE API = new ViewModel.RES_EDT_POSICION_CORTE();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_Cortador = db.VST_CORTADORES.Where(x => x.ID == ID).FirstOrDefault();
                    API.Vst_Posicion = db.VST_POSICION_CORTADOR.Where(x => x.IdPosicion == ID).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Cortador = null;
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
        /// Actualiza los datos de la posición
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ActualizaPosicion")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CORTE ActualizaPosicion([FromBody]ViewModel.EDT_POSICION_CORTE Defecto)
        {
            ViewModel.RES_DEFECTO_CORTE API = new ViewModel.RES_DEFECTO_CORTE();
            List<Models.C_Posicion_Cortador> posicion = new List<Models.C_Posicion_Cortador>();

            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Cort_Cortadores Vst = db.C_Cort_Cortadores.Where(x => x.ID == Defecto.ID).FirstOrDefault();
                    if (Vst != null)
                    {
                        Vst.IdUsuario = Defecto.IdUsuario;
                        Vst.Nombre = Defecto.Nombre;
                        Vst.Observaciones = Defecto.Observaciones;
                        Vst.Descripcion = Defecto.Descripcion;
                        Vst.Clave = Defecto.Clave;

                        db.Entry(Vst).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        posicion = db.C_Posicion_Cortador.Where(x => x.IdPosicion == Defecto.ID).ToList();
                        db.C_Posicion_Cortador.RemoveRange(posicion);
                        db.SaveChanges();

                        foreach (ViewModel.C_Posicion item in Defecto.Defecto)
                        {
                            Models.C_Posicion_Cortador c_Posicion_Cortador = new Models.C_Posicion_Cortador()
                            {
                                IdCortador = item.IdCortador,
                                IdPosicion = Defecto.ID
                            };
                            db.C_Posicion_Cortador.Add(c_Posicion_Cortador);
                        }
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
        /// Elimina cortador por IdCortador
        /// </summary>
        /// <param name="IdCortador"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/Posicion")]
        [System.Web.Http.HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_CORTADOR EliminaPosicion(int ID)
        {
            ViewModel.RES_CORTADOR API = new ViewModel.RES_CORTADOR();
            try
            {
                if (ModelState.IsValid)
                {
                    List<Models.C_Posicion_Cortador> posicion_Cortador = db.C_Posicion_Cortador.Where(x => x.IdPosicion == ID).ToList();
                    db.C_Posicion_Cortador.RemoveRange(posicion_Cortador);
                    db.SaveChanges();

                    Models.C_Cort_Cortadores c_Cort = db.C_Cort_Cortadores.Where(x => x.ID == ID).FirstOrDefault();
                    db.C_Cort_Cortadores.Remove(c_Cort);
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
        #endregion
        //------------------------TOLERANCIA---------------------------------------------

        /// <summary>
        /// Obtiene los rangos de toleranciaregistrados
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ObtieneTolerancias")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_TOLERANCIA ObtieneTolerancias()
        {
            ViewModel.RES_TOLERANCIA API = new ViewModel.RES_TOLERANCIA();
            try
            {
                API.Tolerancias = db.C_Tolerancia_Corte.Select(x => new ViewModel.TOLERANCIA()
                {
                    IdTolerancia = x.IdTolerancia,
                    Denominador = x.Denominador,
                    Descripcion = x.Descripcion,
                    Numerador = x.Numerador,
                    ToleranciaNegativa = x.ToleranciaNegativa,
                    ToleranciaPositiva = x.ToleranciaPositiva
                }).ToList();
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Tolerancias = null;
            }
            return API;
        }

        /// <summary>
        /// Registra nueva tolerancia de cortador
        /// </summary>
        /// <param name="Tolerancia"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/RegistraNuevaTolerancia")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public HttpResponseMessage RegistraNuevaTolerancia(ViewModel.TOLERANCIA Tolerancia)
        {
            HttpResponseMessage API;
            try
            {
                Models.C_Tolerancia_Corte Tol = new Models.C_Tolerancia_Corte()
                {
                    Denominador = Tolerancia.Denominador,
                    Descripcion = Tolerancia.Descripcion,
                    Numerador = Tolerancia.Numerador,
                    ToleranciaNegativa = Tolerancia.ToleranciaNegativa,
                    ToleranciaPositiva = Tolerancia.ToleranciaPositiva
                };
                db.C_Tolerancia_Corte.Add(Tol);
                db.SaveChanges();
                API = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Valida que la tolerancia a registrar no se encuentre aun registrada
        /// </summary>
        /// <param name="Descripcion"></param>
        /// <param name="Numerador"></param>
        /// <param name="Denominador"></param>
        /// <param name="ToleranciaPositiva"></param>
        /// <param name="ToleranciaNegativa"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ValidaNuevaTolerancia")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public bool ValidaNuevaTolerancia(int Descripcion, int Numerador, int Denominador)
        {
            bool Result = false;
            try
            {
                Models.C_Tolerancia_Corte Tolerancia = db.C_Tolerancia_Corte.Where(x => x.Descripcion == Descripcion && x.Numerador == Numerador && x.Denominador == Denominador).FirstOrDefault();
                if (Tolerancia == null)
                {
                    Result = true;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
            }
            return Result;
        }

        /// <summary>
        /// Obtiene la tolerancia por Id
        /// </summary>
        /// <param name="IdTolerancia"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/ObtieneToleranciaPorId")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.TOLERANCIA ObtieneToleranciaPorId(int IdTolerancia)
        {
            ViewModel.TOLERANCIA API = new ViewModel.TOLERANCIA();
            try
            {
                Models.C_Tolerancia_Corte tolerancia_Corte = db.C_Tolerancia_Corte.Where(x => x.IdTolerancia == IdTolerancia).FirstOrDefault();
                if (tolerancia_Corte != null)
                {
                    API.Denominador = tolerancia_Corte.Denominador;
                    API.Numerador = tolerancia_Corte.Numerador;
                    API.Descripcion = tolerancia_Corte.Descripcion;
                    API.ToleranciaNegativa = tolerancia_Corte.ToleranciaNegativa;
                    API.ToleranciaPositiva = tolerancia_Corte.ToleranciaPositiva;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API = null;
            }
            return API;
        }

        /// <summary>
        /// Elimina cortador por IdCortador
        /// </summary>
        /// <param name="IdCortador"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/Tolerancia")]
        [System.Web.Http.HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_CORTADOR EliminaTolerancia(int ID)
        {
            ViewModel.RES_CORTADOR API = new ViewModel.RES_CORTADOR();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Tolerancia_Corte tolerancia_Corte = db.C_Tolerancia_Corte.Where(x => x.IdTolerancia == ID).FirstOrDefault();

                    db.C_Tolerancia_Corte.Remove(tolerancia_Corte);
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
        /// Registra nueva tolerancia de cortador
        /// </summary>
        /// <param name="Tolerancia"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cortadores/Tolerancia")]
        [System.Web.Http.HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        public HttpResponseMessage updateTolerancia(ViewModel.TOLERANCIA Tolerancia)
        {
            HttpResponseMessage API;
            try
            {
                Models.C_Tolerancia_Corte tolerancia_Corte = db.C_Tolerancia_Corte.Where(x => x.IdTolerancia == Tolerancia.IdTolerancia).FirstOrDefault();
                if(tolerancia_Corte != null)
                {
                    tolerancia_Corte.Denominador = Tolerancia.Denominador;
                    tolerancia_Corte.Descripcion = Tolerancia.Descripcion;
                    tolerancia_Corte.Numerador = Tolerancia.Numerador;
                    tolerancia_Corte.ToleranciaNegativa = Tolerancia.ToleranciaNegativa;
                    tolerancia_Corte.ToleranciaPositiva = Tolerancia.ToleranciaPositiva;
                }

                db.Entry(tolerancia_Corte).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                API = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        public partial class JSON_POS_DEF
        {
            public int IdDefecto { get; set; }
        }
    }
}
