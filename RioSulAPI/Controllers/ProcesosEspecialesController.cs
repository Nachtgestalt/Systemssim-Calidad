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
using System.Web.UI.WebControls;
using System.Web.WebPages;
using RioSulAPI.Class;
using RioSulAPI.Models;

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
        #region DEFECTOS

        /// <summary>
        /// Registra un nuevo tendido
        /// </summary>
        /// <param name="Proceso"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/Defecto")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP NuevoDefecto([FromBody]ViewModel.REQ_DEFECTO_PROCESO_ESP Proceso)
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            string image_name = "";
            try
            {
                Models.C_Procesos_Especiales procesos_especiales =
                    db.C_Procesos_Especiales.Where(x => x.Clave == Proceso.Clave).FirstOrDefault();

                if (procesos_especiales == null)
                {
                    //VERFICAMOS SI EL CAMPO DE LA IMAGEN ES VACIO
                    if (Proceso.Imagen != null && !Proceso.Imagen.IsEmpty())
                    {
                        string base64 = Proceso.Imagen.Substring(Proceso.Imagen.IndexOf(',') + 1);
                        byte[] data = Convert.FromBase64String(base64);

                        image_name = "ProcesosEsp_Defecto" + "27_" + Proceso.Clave + DateTime.Now.ToString("yymmssfff");

                        using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                        {
                            image_file.Write(data, 0, data.Length);
                            image_file.Flush();
                        }
                    }

                    //GUARDAMOS LOS DATOS EN BASE
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
                        Imagen = image_name
                    };
                    db.C_Procesos_Especiales.Add(_Procesos_Especialess);
                    db.SaveChanges();

                    API.Hecho = true;
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Hecho = false;
                    API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
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
        [System.Web.Http.HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP ActivaInactivaDefectoProcessoEsp(int ID)
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales c_Cort = db.C_Procesos_Especiales.Where(x => x.ID == ID).FirstOrDefault();
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
        [System.Web.Http.Route("api/ProcesosEspeciales/Defecto")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_PROCESO_ESP ObtieneDefectoProseso(string Clave = "", string Nombre = "", string Activo = "")
        {
            ViewModel.RES_BUS_DEFECTO_PROCESO_ESP API = new ViewModel.RES_BUS_DEFECTO_PROCESO_ESP();
            API.Vst_ProcesosEspeciales = new List<VST_PROCESOS_ESPECIALES>();
            string image_name = "";
            string file_path = "";
            try
            {
                if (ModelState.IsValid)
                {
                    List<Models.VST_PROCESOS_ESPECIALES> procesos = new List<VST_PROCESOS_ESPECIALES>();

                    switch (Activo)
                    {
                        case "True":
                            procesos = db.VST_PROCESOS_ESPECIALES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 27 && x.Activo == true).
                                OrderBy(x => x.Nombre).ToList();
                            break;
                        case "False":
                            procesos = db.VST_PROCESOS_ESPECIALES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 27 && x.Activo == false).
                                OrderBy(x => x.Nombre).ToList();
                            break;
                        default:
                            procesos = db.VST_PROCESOS_ESPECIALES.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 27).
                                OrderBy(x => x.Nombre).ToList();
                            break;
                    }

                    foreach (Models.VST_PROCESOS_ESPECIALES item in procesos)
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

                        API.Vst_ProcesosEspeciales.Add(item);
                    }
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
        [System.Web.Http.Route("api/ProcesosEspeciales/Defecto")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_EDT_PROCESOS_ESPECIALES ObtieneInfoDefectoProcesoEspeciales(int ID)
        {
            ViewModel.RES_EDT_PROCESOS_ESPECIALES API = new ViewModel.RES_EDT_PROCESOS_ESPECIALES();
            string image_name = "";
            string file_path = "";
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_ProcesosEsp = db.VST_PROCESOS_ESPECIALES.Where(x => x.ID == ID).FirstOrDefault();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);

                    if (API.Vst_ProcesosEsp != null)
                    {
                        file_path = HttpContext.Current.Server.MapPath("~/Imagenes/");
                        file_path = file_path + API.Vst_ProcesosEsp.Imagen + ".jpg";

                        if (File.Exists(file_path))
                        {
                            API.Vst_ProcesosEsp.Imagen = "data:image/" + "jpg" + ";base64," + Convert.ToBase64String(File.ReadAllBytes(file_path));
                        }
                        else
                        {
                            API.Vst_ProcesosEsp.Imagen = "";
                        }
                    }                   
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
        [System.Web.Http.Route("api/ProcesosEspeciales/Defecto")]
        [System.Web.Http.HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP ActualizaDefectoProcesosEspeciales([FromBody]ViewModel.REQ_EDT_DEFECTO_PROCESO_ESP Defecto)
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            string image_name = "";
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales procesos = db.C_Procesos_Especiales
                        .Where(x => x.Clave == Defecto.Clave && x.IdSubModulo == 27 && x.ID != Defecto.ID)
                        .FirstOrDefault();

                    if (procesos == null)
                    {
                        //VERFICAMOS SI EL CAMPO DE LA IMAGEN ES VACIO
                        if (Defecto.Imagen != null && !Defecto.Imagen.IsEmpty())
                        {
                            string base64 = Defecto.Imagen.Substring(Defecto.Imagen.IndexOf(',') + 1);
                            byte[] data = Convert.FromBase64String(base64);

                            image_name = "ProcesosEsp_Defecto" + "27_" + Defecto.Clave + DateTime.Now.ToString("yymmssfff");

                            using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                            {
                                image_file.Write(data, 0, data.Length);
                                image_file.Flush();
                            }
                        }

                        Models.C_Procesos_Especiales Vst = db.C_Procesos_Especiales.Where(x => x.ID == Defecto.ID).FirstOrDefault();
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
                        API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
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
        /// Actualiza los datos del tendido
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/EliminaProcesosEspeciales")]
        [System.Web.Http.HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP EliminaProcesoEsp(int ID, string tipo = "")
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();

            try
            {
                if (ModelState.IsValid)
                {
                    Models.Auditoria_Proc_Esp_Detalle auditoria = new Auditoria_Proc_Esp_Detalle();

                    switch (tipo)
                    {
                        case "Defecto":
                            auditoria = db.Auditoria_Proc_Esp_Detalle.Where(x => x.IdDefecto == ID).FirstOrDefault();
                            break;
                        case "Operacion":
                            auditoria = db.Auditoria_Proc_Esp_Detalle.Where(x => x.IdOperacion == ID).FirstOrDefault();
                            break;
                        case "Posicion":
                            auditoria = db.Auditoria_Proc_Esp_Detalle.Where(x => x.IdPosicion == ID).FirstOrDefault();
                            break;
                    }

                    if (auditoria == null)
                    {
                        Models.C_Procesos_Especiales procesos =
                            db.C_Procesos_Especiales.Where(x => x.ID == ID).FirstOrDefault();
                        db.C_Procesos_Especiales.Remove(procesos);
                        db.SaveChanges();

                        API.Hecho = true;
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    else
                    {
                        API.Hecho = false;
                        API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
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

        //----------------------------------------OPERACIONES------------------------------------------------

        #region OPERACIONES

        /// <summary>
        /// Registra un nuevo operación procesos especiales 
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/Operacion")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP NuevoOperacionProcesosEspeciales([FromBody]ViewModel.N_OPERACION Operacion)
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            string image_name = "";
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales procesos = db.C_Procesos_Especiales
                        .Where(x => x.Clave == Operacion.Clave && x.IdSubModulo == 13).FirstOrDefault();

                    if (procesos == null)
                    {
                        //VERFICAMOS SI EL CAMPO DE LA IMAGEN ES VACIO
                        if (Operacion.Imagen != null && !Operacion.Imagen.IsEmpty())
                        {
                            string base64 = Operacion.Imagen.Substring(Operacion.Imagen.IndexOf(',') + 1);
                            byte[] data = Convert.FromBase64String(base64);

                            image_name = "ProcesosEsp_Operacion" + "13_" + Operacion.Clave + DateTime.Now.ToString("yymmssfff");

                            using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                            {
                                image_file.Write(data, 0, data.Length);
                                image_file.Flush();
                            }
                        }

                        Models.C_Procesos_Especiales c_ProcEsp = new Models.C_Procesos_Especiales()
                        {
                            Activo = true,
                            FechaCreacion = DateTime.Now,
                            Clave = Operacion.Clave,
                            Descripcion = Operacion.Descripcion,
                            IdSubModulo = 13,
                            IdUsuario = Operacion.IdUsuario,
                            Nombre = Operacion.Nombre,
                            Observaciones = Operacion.Observaciones,
                            Imagen = image_name
                        };
                        db.C_Procesos_Especiales.Add(c_ProcEsp);
                        db.SaveChanges();

                        foreach (ViewModel.R_DEFECTOS item in Operacion.Defectos)
                        {
                            Models.C_Operacion_ProcesosEspeciales OPE = new Models.C_Operacion_ProcesosEspeciales()
                            {
                                IdOperacion = c_ProcEsp.ID,
                                IdDefecto = item.ID
                            };
                            db.C_Operacion_ProcesosEspeciales.Add(OPE);
                        }
                        db.SaveChanges();

                        API.Hecho = true;
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    else
                    {
                        API.Hecho = false;
                        API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
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
        [System.Web.Http.HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP ActivaInactivaOperacionesProcesosEspeciales(int ID)
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales c_Cort = db.C_Procesos_Especiales.Where(x => x.ID == ID).FirstOrDefault();
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
        [System.Web.Http.Route("api/ProcesosEspeciales/Operacion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_PROCESO_ESP ObtieneOperacionProcesosEspeciales(string Clave = "", string Nombre = "", string Activo = "")
        {
            ViewModel.RES_BUS_DEFECTO_PROCESO_ESP API = new ViewModel.RES_BUS_DEFECTO_PROCESO_ESP();
            API.Vst_ProcesosEspeciales = new List<VST_PROCESOS_ESPECIALES>();
            List<Models.VST_PROCESOS_ESPECIALES> procesos = new List<VST_PROCESOS_ESPECIALES>();
            string image_name = "";
            string file_path = "";
            try
            {
                if (ModelState.IsValid)
                {
                    switch (Activo)
                    {
                        case "True":
                            procesos = db.VST_PROCESOS_ESPECIALES.
                                Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 13 && x.Activo == true).
                                OrderBy(x => x.Nombre).ToList();
                            break;
                        case "False":
                            procesos = db.VST_PROCESOS_ESPECIALES.
                                Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 13 && x.Activo == false).
                                OrderBy(x => x.Nombre).ToList();
                            break;
                        default:
                            procesos = db.VST_PROCESOS_ESPECIALES.
                                Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 13).
                                OrderBy(x => x.Nombre).ToList();
                            break;
                    }

                    foreach (Models.VST_PROCESOS_ESPECIALES item in procesos)
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

                        API.Vst_ProcesosEspeciales.Add(item);
                    }
                    
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
        [System.Web.Http.Route("api/ProcesosEspeciales/Operacion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.R_PROCESO_ESP_OP ObtieneInfoOperacionProcesosEspeciales(int ID)
        {
            ViewModel.R_PROCESO_ESP_OP API = new ViewModel.R_PROCESO_ESP_OP();
            string image_name = "";
            string file_path = "";
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_ProcesosEsp = db.VST_PROCESOS_ESPECIALES.Where(x => x.ID == ID).FirstOrDefault();

                    file_path = HttpContext.Current.Server.MapPath("~/Imagenes/");
                    file_path = file_path + API.Vst_ProcesosEsp.Imagen + ".jpg";
                    if (File.Exists(file_path))
                    {
                        API.Vst_ProcesosEsp.Imagen = "data:image/" + "jpg" + ";base64," + Convert.ToBase64String(File.ReadAllBytes(file_path));
                    }
                    else
                    {
                        API.Vst_ProcesosEsp.Imagen = "";
                    }

                    API.Defectos = new List<ViewModel.R_DEFECTOS>();
                    List<Models.C_Operacion_ProcesosEspeciales> procesosD =
                        db.C_Operacion_ProcesosEspeciales.Where(x => x.IdOperacion == ID).ToList();

                    foreach (C_Operacion_ProcesosEspeciales item in procesosD)
                    {
                        Models.C_Procesos_Especiales procesos = db.C_Procesos_Especiales
                            .Where(x => x.ID == item.IdDefecto).FirstOrDefault();
                        if (procesos != null)
                        {
                            ViewModel.R_DEFECTOS DE = new ViewModel.R_DEFECTOS();

                            DE.ID = procesos.ID;
                            DE.Clave = procesos.Clave;
                            DE.Nombre = procesos.Nombre;
                            DE.Descripcion = procesos.Descripcion;

                            API.Defectos.Add(DE);
                        }
                    }

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
        [System.Web.Http.Route("api/ProcesosEspeciales/Operacion")]
        [System.Web.Http.HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP ActualizaOperacionConfeccion([FromBody]ViewModel.N_OPERACION Operacion, int ID)
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            string image_name = "";
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales Cons = db.C_Procesos_Especiales.Where(x => x.ID != ID && x.Clave == Operacion.Clave && x.IdSubModulo == 13).
                        FirstOrDefault();
                    if (Cons == null)
                    {
                        Models.C_Procesos_Especiales Vst = db.C_Procesos_Especiales.Where(x => x.ID == ID).FirstOrDefault();

                        if (Operacion.Imagen != null && !Operacion.Imagen.IsEmpty())
                            {
                                string base64 = Operacion.Imagen.Substring(Operacion.Imagen.IndexOf(',') + 1);
                                byte[] data = Convert.FromBase64String(base64);

                                image_name = "ProcesosEsp_Operacion" + "13_" + Operacion.Clave + DateTime.Now.ToString("yymmssfff");

                                using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                                {
                                    image_file.Write(data, 0, data.Length);
                                    image_file.Flush();
                                }
                            }

                        Vst.IdUsuario = Operacion.IdUsuario;
                        Vst.Nombre = Operacion.Nombre;
                        Vst.Observaciones = Operacion.Observaciones;
                        Vst.Descripcion = Operacion.Descripcion;
                        Vst.Clave = Operacion.Clave;
                        Vst.Imagen = image_name;

                        db.Entry(Vst).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        //ELIMINAMOS LOS DEFECTOS RELACIONADOS
                        List<Models.C_Operacion_ProcesosEspeciales> procesos =
                            db.C_Operacion_ProcesosEspeciales.Where(x => x.IdOperacion == ID).ToList();

                        db.C_Operacion_ProcesosEspeciales.RemoveRange(procesos);
                        db.SaveChanges();

                        foreach (ViewModel.R_DEFECTOS item in Operacion.Defectos)
                        {
                            Models.C_Operacion_ProcesosEspeciales Det = new Models.C_Operacion_ProcesosEspeciales();
                            Det.IdOperacion = Vst.ID;
                            Det.IdDefecto = item.ID;

                            db.C_Operacion_ProcesosEspeciales.Add(Det);
                        }

                        db.SaveChanges();

                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                        API.Hecho = true;
                    }
                    else
                    {
                        API.Hecho = false;
                        API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
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

        #endregion

        //----------------------------------------POSICION------------------------------------------------

        /// <summary>
        /// Registra un nuevo posición
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/ProcesosEspeciales/Posicion")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_PROCESO_ESP NuevoPosicion([FromBody]ViewModel.REQ_POSICION_PROC_ESP Posicion)
        {
            ViewModel.RES_DEFECTO_PROCESO_ESP API = new ViewModel.RES_DEFECTO_PROCESO_ESP();
            string image_name = "";
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales procesos = db.C_Procesos_Especiales
                        .Where(x => x.Clave == Posicion.Clave && x.IdSubModulo == 14).FirstOrDefault();

                    if (procesos == null)
                    {
                        if (Posicion.Imagen != null && !Posicion.Imagen.IsEmpty())
                        {
                            string base64 = Posicion.Imagen.Substring(Posicion.Imagen.IndexOf(',') + 1);
                            byte[] data = Convert.FromBase64String(base64);

                            image_name = "ProcesosEsp_Posicion" + "14_" + Posicion.Clave + DateTime.Now.ToString("yymmssfff");

                            using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                            {
                                image_file.Write(data, 0, data.Length);
                                image_file.Flush();
                            }
                        }

                        Models.C_Procesos_Especiales c_Proc_Esp = new Models.C_Procesos_Especiales()
                        {
                            Activo = true,
                            FechaCreacion = DateTime.Now,
                            Clave = Posicion.Clave,
                            Descripcion = Posicion.Descripcion,
                            IdSubModulo = 14,
                            IdUsuario = Posicion.IdUsuario,
                            Nombre = Posicion.Nombre,
                            Observaciones = Posicion.Observaciones,
                            Imagen  = image_name
                        };

                        db.C_Procesos_Especiales.Add(c_Proc_Esp);
                        db.SaveChanges();

                        foreach (ViewModel.R_OPERACIONES item in Posicion.Operacion)
                        {
                            Models.C_Posicion_ProcesosEsp posicionProcesos = new C_Posicion_ProcesosEsp()
                            {
                                IdPosicion = c_Proc_Esp.ID,
                                IdCortador = item.ID
                            };

                            db.C_Posicion_ProcesosEsp.Add(posicionProcesos);
                        }

                        db.SaveChanges();

                        API.Hecho = true;
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    else
                    {
                        API.Hecho = false;
                        API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
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
        [System.Web.Http.HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CORTE ActivaInactivaPosicion(int ID)
        {
            ViewModel.RES_DEFECTO_CORTE API = new ViewModel.RES_DEFECTO_CORTE();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales c_Cort = db.C_Procesos_Especiales.Where(x => x.ID == ID).FirstOrDefault();
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
        [System.Web.Http.Route("api/ProcesosEspeciales/Posicion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_BUS_DEFECTO_PROCESO_ESP ObtienePosicion(string Clave = "", string Nombre = "", string Activo = "")
        {
            ViewModel.RES_BUS_DEFECTO_PROCESO_ESP API = new ViewModel.RES_BUS_DEFECTO_PROCESO_ESP();
            API.Vst_ProcesosEspeciales = new List<VST_PROCESOS_ESPECIALES>();
            List<Models.VST_PROCESOS_ESPECIALES> posicion = new List<VST_PROCESOS_ESPECIALES>();
            string image_name = "";
            string file_path = "";

            try
            {
                if (ModelState.IsValid)
                {
                    switch (Activo)
                    {
                        case "True":
                            posicion = db.VST_PROCESOS_ESPECIALES.
                                Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 14 && x.Activo == true).
                                OrderBy(x => x.Nombre).ToList();
                            break;
                        case "False":
                            posicion = db.VST_PROCESOS_ESPECIALES.
                                Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 14 && x.Activo == false).
                                OrderBy(x => x.Nombre).ToList();
                            break;
                        default:
                            posicion = db.VST_PROCESOS_ESPECIALES.
                                Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 14).
                                OrderBy(x => x.Nombre).ToList();
                            break;
                    }

                    foreach (Models.VST_PROCESOS_ESPECIALES item in posicion)
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

                        API.Vst_ProcesosEspeciales.Add(item);
                    }
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
        [System.Web.Http.Route("api/ProcesosEspeciales/Posicion")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.R_PROCESO_ESP_PO ObtieneInfoPosicion(int ID)
        {
            ViewModel.R_PROCESO_ESP_PO API = new ViewModel.R_PROCESO_ESP_PO();
            string image_name = "";
            string file_path = "";
            try
            {
                if (ModelState.IsValid)
                {
                    API.Vst_ProcesosEsp = db.VST_PROCESOS_ESPECIALES.Where(x => x.ID == ID).FirstOrDefault();
                    API.Operaciones = new List<ViewModel.R_OPERACIONES>();

                    file_path = HttpContext.Current.Server.MapPath("~/Imagenes/");
                    file_path = file_path + API.Vst_ProcesosEsp.Imagen + ".jpg";
                    if (File.Exists(file_path))
                    {
                        API.Vst_ProcesosEsp.Imagen = "data:image/" + "jpg" + ";base64," + Convert.ToBase64String(File.ReadAllBytes(file_path));
                    }
                    else
                    {
                        API.Vst_ProcesosEsp.Imagen = "";
                    }

                    List<Models.C_Posicion_ProcesosEsp>
                        posicion = db.C_Posicion_ProcesosEsp.Where(x => x.IdPosicion == ID).ToList();

                    foreach (C_Posicion_ProcesosEsp item in posicion)
                    {
                        Models.C_Procesos_Especiales Operacion =
                            db.C_Procesos_Especiales.Where(x => x.ID == item.IdCortador).FirstOrDefault();

                        if (Operacion != null)
                        {
                            ViewModel.R_OPERACIONES RO = new ViewModel.R_OPERACIONES();

                            RO.ID = Operacion.ID;
                            RO.Clave = Operacion.Clave;
                            RO.Descripcion = Operacion.Descripcion;
                            RO.Nombre = Operacion.Nombre;

                            API.Operaciones.Add(RO);
                        }
                    }

                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Vst_ProcesosEsp = null; API.Vst_ProcesosEsp = null;
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
        [System.Web.Http.Route("api/ProcesosEspeciales/Posicion")]
        [System.Web.Http.HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_CORTE ActualizaPosicion([FromBody]ViewModel.REQ_POSICION_PROC_ESP Posicion, int ID)
        {
            ViewModel.RES_DEFECTO_CORTE API = new ViewModel.RES_DEFECTO_CORTE();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Procesos_Especiales Vst = db.C_Procesos_Especiales.Where(x => x.ID == ID).FirstOrDefault();
                    if (Vst != null)
                    {

                        Models.C_Procesos_Especiales verifica = db.C_Procesos_Especiales
                            .Where(x => x.Clave == Posicion.Clave && x.IdSubModulo == 14 && x.ID != ID)
                            .FirstOrDefault();

                        if (verifica == null)
                        {
                            Vst.IdUsuario = Posicion.IdUsuario;
                            Vst.Nombre = Posicion.Nombre;
                            Vst.Observaciones = Posicion.Observaciones;
                            Vst.Descripcion = Posicion.Descripcion;
                            Vst.Clave = Posicion.Clave;

                            db.Entry(Vst).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            List<Models.C_Posicion_ProcesosEsp> posicion =
                                db.C_Posicion_ProcesosEsp.Where(x => x.IdPosicion == ID).ToList();
                            db.C_Posicion_ProcesosEsp.RemoveRange(posicion);
                            db.SaveChanges();

                            foreach (ViewModel.R_OPERACIONES item in Posicion.Operacion)
                            {
                                Models.C_Posicion_ProcesosEsp PO = new C_Posicion_ProcesosEsp()
                                {
                                    IdPosicion = ID,
                                    IdCortador = item.ID
                                };
                                db.C_Posicion_ProcesosEsp.Add(PO);
                            }

                            db.SaveChanges();

                            API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                            API.Hecho = true;
                        }
                        else
                        {
                            API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
                            API.Hecho = false;
                        }
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
