using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Entity;
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
using RioSulAPI.Models;

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
            string image_name = "";
            ViewModel.RES_DEFECTO_LAV API = new ViewModel.RES_DEFECTO_LAV();
            try
            {

                Models.C_Lavanderia lavanderia =
                    db.C_Lavanderia.Where(x => x.Clave == Lavanderia.Clave && x.IdSubModulo == 17).FirstOrDefault();

                if (lavanderia == null)
                {
                    if (Lavanderia.Imagen != null && !Lavanderia.Imagen.IsEmpty())
                    {
                        string base64 = Lavanderia.Imagen.Substring(Lavanderia.Imagen.IndexOf(',') + 1);
                        byte[] data = Convert.FromBase64String(base64);

                        image_name = "Lavanderia_Defecto" + "17_" + Lavanderia.Clave + DateTime.Now.ToString("yymmssfff");

                        using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                        {
                            image_file.Write(data, 0, data.Length);
                            image_file.Flush();
                        }
                    }

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
                        Imagen = image_name
                    };
                    db.C_Lavanderia.Add(_Lavanderia);
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
                        API.Hecho = true;
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
        [HttpPut]
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
        public ViewModel.RES_BUS_DEFECTO_LAVANDERIA ObtieneDefectoLavanderia(string Clave = "", string Nombre = "", string Activo = "")
        {
            ViewModel.RES_BUS_DEFECTO_LAVANDERIA API = new ViewModel.RES_BUS_DEFECTO_LAVANDERIA();
            API.Vst_Lavanderia = new List<VST_LAVANDERIA>();
            string image_name = "";
            string file_path = "";

            try
            {
                if (ModelState.IsValid)
                {
                    List<Models.VST_LAVANDERIA> lavanderias = new List<VST_LAVANDERIA>();
                    

                    switch (Activo)
                    {
                        case "True":
                            lavanderias = db.VST_LAVANDERIA.
                                Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 17 && x.Activo == true).
                                OrderBy(x => x.Nombre).ToList();
                            break;

                        case "False":
                            lavanderias = db.VST_LAVANDERIA.
                                Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 17 && x.Activo == false).
                                OrderBy(x => x.Nombre).ToList();
                            break;
                        default:
                            lavanderias = db.VST_LAVANDERIA.
                                Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 17).
                                OrderBy(x => x.Nombre).ToList();
                            break;
                    }

                   
                    foreach (Models.VST_LAVANDERIA item in lavanderias)
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

                        API.Vst_Lavanderia.Add(item);
                    }

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
        [HttpPut]
        [Route("api/Lavanderia/ActualizaDefectoLavanderia")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_LAV ActualizaDefectoLavanderia([FromBody]ViewModel.REQ_EDT_DEFECTO_LAVANDERIA Defecto)
        {
            string image_name = "";
            ViewModel.RES_DEFECTO_LAV API = new ViewModel.RES_DEFECTO_LAV();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Lavanderia lavanderia = db.C_Lavanderia.Where(x => x.Clave == Defecto.Clave && x.ID != Defecto.ID && x.IdSubModulo == 17).FirstOrDefault();

                    if (lavanderia == null)
                    {
                        if (Defecto.Imagen != null && !Defecto.Imagen.IsEmpty())
                        {
                            string base64 = Defecto.Imagen.Substring(Defecto.Imagen.IndexOf(',') + 1);
                            byte[] data = Convert.FromBase64String(base64);

                            image_name = "Lavanderia_Defecto"+ "17_" + Defecto.Clave + DateTime.Now.ToString("yymmssfff");

                            using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                            {
                                image_file.Write(data, 0, data.Length);
                                image_file.Flush();
                            }
                        }

                        Models.C_Lavanderia Vst = db.C_Lavanderia.Where(x => x.ID == Defecto.ID).FirstOrDefault();
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
                        }

                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                        API.Hecho = true;
                    }
                    else
                    {
                        API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
                        API.Hecho = false;
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

        /// <summary>
        /// Elimina el defecto de la lavandería
        /// </summary>
        /// <param name="IdLavanderia"></param>
        /// <returns></returns>
        [HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Lavanderia/EliminaLavanderia")]
        public ViewModel.RES_DEFECTO_LAV DeleteDefectoLav(int IdLavanderia, string tipo = "")
        {
            ViewModel.RES_DEFECTO_LAV API = new ViewModel.RES_DEFECTO_LAV();
            try
            {
                if (ModelState.IsValid)
                {

                    Models.Auditoria_Lavanderia_Detalle auditoria = new Auditoria_Lavanderia_Detalle();

                    switch (tipo)
                    {
                        case "Defecto":
                            auditoria = db.Auditoria_Lavanderia_Detalle.Where(x => x.IdDefecto == IdLavanderia).FirstOrDefault();
                            break;
                        case "Operacion":
                            auditoria = db.Auditoria_Lavanderia_Detalle.Where(x => x.IdOperacion == IdLavanderia).FirstOrDefault();
                            break;
                        case "Posicion":
                            auditoria = db.Auditoria_Lavanderia_Detalle.Where(x => x.IdPosicion == IdLavanderia).FirstOrDefault();
                            break;
                    }

                    if (auditoria == null)
                    {
                        Models.C_Lavanderia c_Lav = db.C_Lavanderia.Where(x => x.ID == IdLavanderia).FirstOrDefault();
                        db.C_Lavanderia.Remove(c_Lav);
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
        /// Registra una nueva operación lavandería
        /// </summary>
        /// <param name="Defecto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Lavanderia/OperacionLavanderia")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DEFECTO_LAV NuevaOperacionLavanderia([FromBody]ViewModel.REQ_DEFECTO_LAV Defecto)
        {
            ViewModel.RES_DEFECTO_LAV API = new ViewModel.RES_DEFECTO_LAV();
            string image_name = "";
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Lavanderia lavanderia =
                        db.C_Lavanderia.Where(x => x.Clave == Defecto.Clave && x.IdSubModulo == 19).FirstOrDefault();

                    if (lavanderia == null)
                    {
                        if (Defecto.Imagen != null && !Defecto.Imagen.IsEmpty())
                        {
                            string base64 = Defecto.Imagen.Substring(Defecto.Imagen.IndexOf(',') + 1);
                            byte[] data = Convert.FromBase64String(base64);

                            image_name = "Lavanderia_Operacion" + "19_" + Defecto.Clave + DateTime.Now.ToString("yymmssfff");

                            using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                            {
                                image_file.Write(data, 0, data.Length);
                                image_file.Flush();
                            }
                        }

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
                            Imagen = image_name
                        };
                        db.C_Lavanderia.Add(c_Lavanderia);
                        db.SaveChanges();

                        foreach (ViewModel.DEFECTO_REF item in Defecto.Defecto)
                        {
                            Models.C_Operacion_Lavanderia c_Operacion_Lavanderia = new Models.C_Operacion_Lavanderia()
                            {
                                IdOperacion = c_Lavanderia.ID,
                                IdDefecto = item.ID
                            };
                            db.C_Operacion_Lavanderia.Add(c_Operacion_Lavanderia);
                        }
                        db.SaveChanges();

                        API.Hecho = true;
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    else
                    {
                        API.Hecho = true;
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
        [HttpPut]
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
        [Route("api/Lavanderia/OperacionLavanderia")]
        public ViewModel.RES_BUS_DEFECTO_LAVANDERIA ObtieneOperacionLavanderia(string Clave = "", string Nombre = "", string Activo = "")
        {
            string image_name = "";
            string file_path = "";
            ViewModel.RES_BUS_DEFECTO_LAVANDERIA API = new ViewModel.RES_BUS_DEFECTO_LAVANDERIA();
            API.Vst_Lavanderia = new List<VST_LAVANDERIA>();
            List<Models.VST_LAVANDERIA> lavanderia = new List<VST_LAVANDERIA>();
            try
            {
                if (ModelState.IsValid)
                {

                    switch (Activo)
                    {
                        case "True":
                            lavanderia = db.VST_LAVANDERIA.Where(x =>
                                (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 19 && x.Activo == true)
                                .OrderBy(x => x.Nombre).ToList();
                            break;
                        case "False":
                            lavanderia = db.VST_LAVANDERIA.Where(x =>
                                    (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 19 && x.Activo == false)
                                .OrderBy(x => x.Nombre).ToList();
                            break;
                        default:
                            lavanderia = db.VST_LAVANDERIA.Where(x =>
                                    (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 19)
                                .OrderBy(x => x.Nombre).ToList();
                            break;
                    }

                    foreach (Models.VST_LAVANDERIA item in lavanderia)
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

                        API.Vst_Lavanderia.Add(item);
                    }

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
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Lavanderia/OperacionLavanderia")]
        public ViewModel.RES_EDT_LAVANDERIA ObtieneInfoOperacionLavanderia(int ID)
        {
            ViewModel.RES_EDT_LAVANDERIA API = new ViewModel.RES_EDT_LAVANDERIA();
            string image_name = "";
            string file_path = "";
            try
            {
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                API.Vst_Lavanderia = db.VST_LAVANDERIA.Where(x => x.ID == ID).FirstOrDefault();

                file_path = HttpContext.Current.Server.MapPath("~/Imagenes/");
                file_path = file_path + API.Vst_Lavanderia.Imagen + ".jpg";
                if (File.Exists(file_path))
                {
                    API.Vst_Lavanderia.Imagen = "data:image/" + "jpg" + ";base64," + Convert.ToBase64String(File.ReadAllBytes(file_path));
                }
                else
                {
                    API.Vst_Lavanderia.Imagen = "";
                }

                API.Defecto = new List<ViewModel.DEFECTO_OPEREACION>();
                List<Models.C_Operacion_Lavanderia> Defectos = db.C_Operacion_Lavanderia.Where(x => x.IdOperacion == ID).ToList();
                foreach (Models.C_Operacion_Lavanderia item in Defectos)
                {
                    Models.VST_LAVANDERIA Lavanderia = db.VST_LAVANDERIA.Where(x => x.ID == item.IdDefecto).FirstOrDefault();

                    if (Lavanderia != null)
                    {                    
                        ViewModel.DEFECTO_OPEREACION DO = new ViewModel.DEFECTO_OPEREACION();

                        DO.ID = Lavanderia.ID;
                        DO.Clave = Lavanderia.Clave;
                        DO.Descripcion = Lavanderia.Descripcion;
                        DO.Nombre = Lavanderia.Nombre;

                        API.Defecto.Add(DO);
                    }

                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Vst_Lavanderia = null;
                API.Defecto = null;
            }
            return API;
        }

        /// <summary>
        /// Actualiza el Id Lavanderia
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Lavanderia/OperacionLavanderia")]
        public ViewModel.RES_DEFECTO_LAV UpdateLavanderia([FromBody]ViewModel.REQ_DEFECTO_LAV Defecto,int ID)
        {
            ViewModel.RES_DEFECTO_LAV API = new ViewModel.RES_DEFECTO_LAV();
            string image_name = "";

            try
            {
                Models.C_Lavanderia vst = db.C_Lavanderia.Where(x => x.ID == ID && x.IdSubModulo == 19).FirstOrDefault();

                if (vst != null)
                {
                    if (Defecto.Imagen != null && !Defecto.Imagen.IsEmpty())
                    {
                        string base64 = Defecto.Imagen.Substring(Defecto.Imagen.IndexOf(',') + 1);
                        byte[] data = Convert.FromBase64String(base64);

                        image_name = "Lavanderia_Posicion" + "19_" + Defecto.Clave + DateTime.Now.ToString("yymmssfff");

                        using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                        {
                            image_file.Write(data, 0, data.Length);
                            image_file.Flush();
                        }
                    }
                    vst.IdUsuario = Defecto.IdUsuario;
                    vst.Nombre = Defecto.Nombre;
                    vst.Observaciones = Defecto.Observaciones;
                    vst.Descripcion = Defecto.Descripcion;
                    vst.Clave = Defecto.Clave;
                    vst.Imagen = image_name;

                    //VERIFICAMOS SI LA CLAVE O NOMBRE YA EXISTEN EN BASE
                    Models.C_Lavanderia Verifica = db.C_Lavanderia.Where(x => x.IdSubModulo == 20 && x.ID != ID && (x.Clave == Defecto.Clave || x.Nombre == Defecto.Nombre)).FirstOrDefault();

                    if (Verifica == null)
                    {
                        db.Entry(vst).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        //ELIMNAMOS LAS OPERACIONES RELACIONADAS
                        List<Models.C_Operacion_Lavanderia> PT = db.C_Operacion_Lavanderia.Where(x => x.IdOperacion == ID).ToList();

                        foreach (Models.C_Operacion_Lavanderia item in PT)
                        {
                            db.C_Operacion_Lavanderia.Remove(item);
                            db.SaveChanges();
                        }

                        //GUARDAMOS LAS OPERACIONES
                        foreach (ViewModel.DEFECTO_REF item in Defecto.Defecto)
                        {
                            Models.C_Operacion_Lavanderia C_Operacion_Lavanderia = new Models.C_Operacion_Lavanderia()
                            {
                                IdOperacion = vst.ID,
                                IdDefecto = item.ID
                            };
                            db.C_Operacion_Lavanderia.Add(C_Operacion_Lavanderia);
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
                    API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
                    API.Hecho = false;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);

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
            string image_name = "";
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Lavanderia Verifica = db.C_Lavanderia.
                                                    Where(x => x.IdSubModulo == 20 && (x.Clave == Posicion.Clave || x.Nombre == Posicion.Nombre))
                                                    .FirstOrDefault();

                    if (Verifica == null)
                    {
                        if (Posicion.Imagen != null && !Posicion.Imagen.IsEmpty())
                        {
                            string base64 = Posicion.Imagen.Substring(Posicion.Imagen.IndexOf(',') + 1);
                            byte[] data = Convert.FromBase64String(base64);

                            image_name = "Lavanderia_Posicion" + "20_" + Posicion.Clave + DateTime.Now.ToString("yymmssfff");

                            using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                            {
                                image_file.Write(data, 0, data.Length);
                                image_file.Flush();
                            }
                        }


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
                            Imagen = image_name
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
        public ViewModel.RES_BUS_DEFECTO_LAVANDERIA ObtienePosicionLavanderia(string Clave = "", string Nombre = "", string Activo = "")
        {
            string image_name = "";
            string file_path = "";
            ViewModel.RES_BUS_DEFECTO_LAVANDERIA API = new ViewModel.RES_BUS_DEFECTO_LAVANDERIA();
            API.Vst_Lavanderia = new List<VST_LAVANDERIA>();
            List<VST_LAVANDERIA> lavanderia = new List<VST_LAVANDERIA>();
            try
            {
                if (ModelState.IsValid)
                {
                    switch (Activo)
                    {
                        case "True":
                            lavanderia = db.VST_LAVANDERIA.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 20 && x.Activo == true).OrderBy(x => x.Nombre).ToList();
                            break;
                        case "False":
                            lavanderia = db.VST_LAVANDERIA.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 20 && x.Activo == false).OrderBy(x => x.Nombre).ToList();
                            break;
                        default:
                            lavanderia = db.VST_LAVANDERIA.Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 20).OrderBy(x => x.Nombre).ToList();
                            break;
                    }

                   
                    foreach (Models.VST_LAVANDERIA item in lavanderia)
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

                        API.Vst_Lavanderia.Add(item);
                    }

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
        /// Actualiza el Id Lavanderia
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Lavanderia/PosicionLavanderia")]
        public ViewModel.LAVANDERIA_P ObtieneInfoPosicionLavanderia(int ID)
        {
            ViewModel.LAVANDERIA_P API = new ViewModel.LAVANDERIA_P();
            string image_name = "";
            string file_path = "";
            try
            {
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                API.Vst_Lavanderia = db.VST_LAVANDERIA.Where(x => x.ID == ID).FirstOrDefault();
                API.Operacion = new List<ViewModel.DEFECTO_OPEREACION>();

                file_path = HttpContext.Current.Server.MapPath("~/Imagenes/");
                file_path = file_path + API.Vst_Lavanderia.Imagen + ".jpg";
                if (File.Exists(file_path))
                {
                    API.Vst_Lavanderia.Imagen = "data:image/" + "jpg" + ";base64," + Convert.ToBase64String(File.ReadAllBytes(file_path));
                }
                else
                {
                    API.Vst_Lavanderia.Imagen = "";
                }

                List<Models.C_Posicion_Lavanderia> Defectos = db.C_Posicion_Lavanderia.Where(x => x.IdPosicion == ID).ToList();
                foreach (Models.C_Posicion_Lavanderia item in Defectos)
                {
                    Models.VST_LAVANDERIA Lavanderia = db.VST_LAVANDERIA.Where(x => x.ID == item.IdCortador).FirstOrDefault();

                    if (Lavanderia != null)
                    {
                        ViewModel.DEFECTO_OPEREACION DO = new ViewModel.DEFECTO_OPEREACION();

                        DO.ID = Lavanderia.ID;
                        DO.Clave = Lavanderia.Clave;
                        DO.Descripcion = Lavanderia.Descripcion;
                        DO.Nombre = Lavanderia.Nombre;

                        API.Operacion.Add(DO);
                    }                    
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Vst_Lavanderia = null;
                API.Operacion = null;
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
            string image_name = "";
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Lavanderia Vst = db.C_Lavanderia.Where(x => x.ID == Posicion.ID && x.IdSubModulo == 20).FirstOrDefault();
                    //VERIFICAMOS SI EL ELEMENTO EXISTE EN BASE
                    if (Vst != null)
                    {
                        if (Posicion.Imagen != null && !Posicion.Imagen.IsEmpty())
                        {
                            string base64 = Posicion.Imagen.Substring(Posicion.Imagen.IndexOf(',') + 1);
                            byte[] data = Convert.FromBase64String(base64);

                            image_name = "Lavanderia_Posicion" + "20_" + Posicion.Clave + DateTime.Now.ToString("yymmssfff");

                            using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                            {
                                image_file.Write(data, 0, data.Length);
                                image_file.Flush();
                            }
                        }

                        Vst.IdUsuario = Posicion.IdUsuario;
                        Vst.Nombre = Posicion.Nombre;
                        Vst.Observaciones = Posicion.Observaciones;
                        Vst.Descripcion = Posicion.Descripcion;
                        Vst.Clave = Posicion.Clave;
                        Vst.Imagen = image_name;

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