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
    public class ProcesosEspecialesController : ApiController
    {
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();
        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();

        public partial class JSON_POS_DEF
        {
            public int IdDefecto { get; set; }
        }

        //----------------------------------------DEFECTOS------------------------------------------------

        /// <summary>
        /// Registra un nuevo tendido
        /// </summary>
        /// <param name="Proceso"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/NuevoDefectoProceso")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP NuevoDefecto([FromBody]ViewModel.REQ_DEFECTO_PROCESO_ESP Proceso)
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            try
            {
                    Models.C_Procesos_Especiales _Procesos_Especialess = new Models.C_Procesos_Especiales()
                    {
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        Clave = Proceso.Clave,
                        Descripcion = Proceso.Descripcion,
                        IdSubModulo = 27,
                        IdUsuario = Proceso.IdUsuario,
                        Nombre = Proceso.Nombre,
                        Observaciones = Proceso.Observaciones,
                        Imagen = Proceso.Imagen
                    };
                    db.C_Procesos_Especiales.Add(_Procesos_Especialess);
                    db.SaveChanges();

                    API.Hecho = true;
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
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
        [System.Web.Http.Route("api/ProcesosEspeciales/ValidaProcesoEspecialSubModulo")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP ValidaProcesoEspecialSubModulo(int SubModulo, string Clave, string Nombre)
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales c_ProE = db.C_Procesos_Especiales.Where(x => x.Clave == Clave && x.Nombre == Nombre && x.IdSubModulo == SubModulo).FirstOrDefault();
                    if (c_ProE != null)
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
        /// <param name="IdProcesoEspecial"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/ActivaInactivaDefectoProcesoEsp")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP ActivaInactivaDefectoProcessoEsp(int IdProcesoEspecial)
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales c_Cort = db.C_Procesos_Especiales.Where(x => x.ID == IdProcesoEspecial).FirstOrDefault();
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
        [System.Web.Http.Route("api/ProcesosEspeciales/ObtieneDefectoProseso")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_PROCESO_ESP ObtieneDefectoProseso(string Clave = "", string Nombre = "")
        {
            ViewModel.RES_BUS_DEFECTO_PROCESO_ESP API = new ViewModel.RES_BUS_DEFECTO_PROCESO_ESP();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_ProcesosEspeciales = db.VST_PROCESOS_ESPECIALES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 27).OrderBy(x => x.Nombre).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_ProcesosEspeciales = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_ProcesosEspeciales = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtiene información de un tendido por Id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/ObtieneInfoDefectoProcesoEspeciales")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_EDT_PROCESOS_ESPECIALES ObtieneInfoDefectoProcesoEspeciales(int ID)
        {
            ViewModel.RES_EDT_PROCESOS_ESPECIALES API = new ViewModel.RES_EDT_PROCESOS_ESPECIALES();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_ProcesosEsp = db.VST_PROCESOS_ESPECIALES.Where(x => x.ID == ID).FirstOrDefault();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_ProcesosEsp = null;
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
        [System.Web.Http.Route("api/Cortadores/ActualizaDefectoProcesosEspeciales")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP ActualizaDefectoProcesosEspeciales([FromBody]ViewModel.REQ_EDT_DEFECTO_PROCESO_ESP Defecto)
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales Vst = db.C_Procesos_Especiales.Where(x => x.ID == Defecto.ID).FirstOrDefault();
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
        /// Registra un nuevo operación procesos especiales 
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/NuevoOperacionProcesosEspeciales")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP NuevoOperacionProcesosEspeciales([FromBody]ViewModel.REQ_DEFECTO_PROCESO_ESP Defecto)
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales c_ProcEsp = new Models.C_Procesos_Especiales()
                    {
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        Clave = Defecto.Clave,
                        Descripcion = Defecto.Descripcion,
                        IdSubModulo = 13,
                        IdUsuario = Defecto.IdUsuario,
                        Nombre = Defecto.Nombre,
                        Observaciones = Defecto.Observaciones,
                        Imagen = Defecto.Imagen
                    };
                    db.C_Procesos_Especiales.Add(c_ProcEsp);
                    db.SaveChanges();

                    List<JSON_POS_DEF> POS = _objSerializer.Deserialize<List<JSON_POS_DEF>>(Defecto.Imagen);
                    foreach (JSON_POS_DEF item in POS)
                    {
                        Models.C_Operacion_ProcesosEspeciales c_Procesos_Especiales = new Models.C_Operacion_ProcesosEspeciales()
                        {
                            IdOperacion = c_ProcEsp.ID,
                            IdDefecto = item.IdDefecto
                        };
                        db.C_Operacion_ProcesosEspeciales.Add(c_Procesos_Especiales);
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
        /// Valida Nuevo proceso especial por SubModulo
        /// </summary>
        /// <param name="SubModulo"></param>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/ValidaOperacionSubModuloProcesosEspeciales")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP ValidaOperacionSubModulo(int SubModulo, string Clave, string Nombre = "")
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales c_Cort = db.C_Procesos_Especiales.Where(x => x.Clave == Clave && x.Nombre == Nombre && x.IdSubModulo == SubModulo).FirstOrDefault();
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
        [System.Web.Http.Route("api/ProcesosEspeciales/ActivaInactivaOperacionesProcesosEspeciales")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP ActivaInactivaOperacionesProcesosEspeciales(int IdOperacion)
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales c_Cort = db.C_Procesos_Especiales.Where(x => x.ID == IdOperacion).FirstOrDefault();
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
        /// Obtiene procesos especiales por Clave y/o Nombre
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/ObtieneOperacionProcesosEspeciales")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_PROCESO_ESP ObtieneOperacionProcesosEspeciales(string Clave = "", string Nombre = "")
        {
            ViewModel.RES_BUS_DEFECTO_PROCESO_ESP API = new ViewModel.RES_BUS_DEFECTO_PROCESO_ESP();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_ProcesosEspeciales = db.VST_PROCESOS_ESPECIALES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 13).OrderBy(x => x.Nombre).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_ProcesosEspeciales = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_ProcesosEspeciales = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtiene información de una operacion proceos especiales por Id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/ObtieneInfoOperacionProcesosEspeciales")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_EDT_PROCESOS_ESPECIALES ObtieneInfoOperacionProcesosEspeciales(int ID)
        {
            ViewModel.RES_EDT_PROCESOS_ESPECIALES API = new ViewModel.RES_EDT_PROCESOS_ESPECIALES();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_ProcesosEsp = db.VST_PROCESOS_ESPECIALES.Where(x => x.ID == ID).FirstOrDefault();
                    //API.Vst_Oper_Conf = db.VST_OPERACION_CONFECCION.Where(x => x.IdOperacion == ID).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_ProcesosEsp = null;
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
        [System.Web.Http.Route("api/ProcesosEspeciales/ActualizaOperacionProcesosEspeciales")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP ActualizaOperacionConfeccion([FromBody]ViewModel.REQ_EDT_DEFECTO_PROCESO_ESP Operacion)
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales Vst = db.C_Procesos_Especiales.Where(x => x.ID == Operacion.ID).FirstOrDefault();
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
        /// Obtiene defectos por Procesos especiales por Clave y/o Nombre
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Nombre"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/ObtieneDefectosActivosOperacionProcesosEspeciales")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_PROCESO_ESP ObtieneDefectosActivosOperacion()
        {
            ViewModel.RES_BUS_DEFECTO_PROCESO_ESP API = new ViewModel.RES_BUS_DEFECTO_PROCESO_ESP();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_ProcesosEspeciales = db.VST_PROCESOS_ESPECIALES.Where(x => x.IdSubModulo == 13 && x.Activo == true).OrderBy(x => x.Nombre).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_ProcesosEspeciales = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_ProcesosEspeciales = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        //----------------------------------------POSICION------------------------------------------------

        /// <summary>
        /// Registra un nuevo posición
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/NuevoPosicion")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP NuevoPosicion([FromBody]ViewModel.REQ_POSICION_PROC_ESP Defecto)
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales c_Proc_Esp = new Models.C_Procesos_Especiales()
                    {
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        Clave = Defecto.Clave,
                        Descripcion = Defecto.Descripcion,
                        IdSubModulo = 14,
                        IdUsuario = Defecto.IdUsuario,
                        Nombre = Defecto.Nombre,
                        Observaciones = Defecto.Observaciones
                    };

                    db.C_Procesos_Especiales.Add(c_Proc_Esp);
                    db.SaveChanges();

                    List<JSON_POS_DEF> POS = _objSerializer.Deserialize<List<JSON_POS_DEF>>(Defecto.Posicion);
                    foreach (JSON_POS_DEF item in POS)
                    {
                        Models.C_Posicion_ProcesosEsp c_Posicion = new Models.C_Posicion_ProcesosEsp()
                        {
                            IdCortador = c_Proc_Esp.ID,
                            IdPosicion = item.IdDefecto
                        };
                        db.C_Posicion_ProcesosEsp.Add(c_Posicion);
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
        [System.Web.Http.Route("api/ProcesosEspeciales/ValidaPosicionProcesosEspecialesSubModulo")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CORTE ValidaPosicionSubModulo(int SubModulo, string Clave, string Nombre)
        {
            ViewModel.RES_DEFECTO_CORTE API = new ViewModel.RES_DEFECTO_CORTE();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales c_Cort = db.C_Procesos_Especiales.Where(x => x.Clave == Clave && x.Nombre == Nombre && x.IdSubModulo == SubModulo).FirstOrDefault();
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
        /// Activa o inactiva el defecto por Posicion
        /// </summary>
        /// <param name="IdPosicion"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/ActivaInactivaPosicion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CORTE ActivaInactivaPosicion(int IdPosicion)
        {
            ViewModel.RES_DEFECTO_CORTE API = new ViewModel.RES_DEFECTO_CORTE();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales c_Cort = db.C_Procesos_Especiales.Where(x => x.ID == IdPosicion).FirstOrDefault();
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
        [System.Web.Http.Route("api/ProcesosEspeciales/ObtienePosicion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_PROCESO_ESP ObtienePosicion(string Clave = "", string Nombre = "")
        {
            ViewModel.RES_BUS_DEFECTO_PROCESO_ESP API = new ViewModel.RES_BUS_DEFECTO_PROCESO_ESP();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_ProcesosEspeciales = db.VST_PROCESOS_ESPECIALES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 14).OrderBy(x => x.Nombre).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_ProcesosEspeciales = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Vst_ProcesosEspeciales = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtiene información de una posicion por Id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/ObtieneInfoPosicion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_EDT_PROCESOS_ESPECIALES ObtieneInfoPosicion(int ID)
        {
            ViewModel.RES_EDT_PROCESOS_ESPECIALES API = new ViewModel.RES_EDT_PROCESOS_ESPECIALES();
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_ProcesosEsp = db.VST_PROCESOS_ESPECIALES.Where(x => x.ID == ID).FirstOrDefault();
                    API.Vst_Posicion = db.VST_POSICION_PROCESOS_ESPECIALES.Where(x => x.IdCortador == ID).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_Posicion = null; API.Vst_ProcesosEsp = null;
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
        [System.Web.Http.Route("api/ProcesosEspeciales/ActualizaPosicion")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CORTE ActualizaPosicion([FromBody]ViewModel.REQ_EDT_DEFECTO_CORTE Defecto)
        {
            ViewModel.RES_DEFECTO_CORTE API = new ViewModel.RES_DEFECTO_CORTE();
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
        /// Obtiene los defectos activos para relacionar a la posición
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/ObtieneDefectosActivos")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_CORTE ObtieneDefectosActivos()
        {
            ViewModel.RES_BUS_DEFECTO_CORTE API = new ViewModel.RES_BUS_DEFECTO_CORTE();
            try
            {
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                API.Vst_Cortadores = db.VST_CORTADORES.Where(x => x.IdSubModulo == 6 && x.Activo == true).ToList();
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Vst_Cortadores = null;
            }
            return API;
        }
    }
}
