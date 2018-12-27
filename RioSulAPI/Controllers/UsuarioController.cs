using RioSulAPI.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace RioSulAPI.Controllers
{
    public class UsuarioController : ApiController
    {
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();
        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();

        /// <summary>
        /// AGREGA UN NUEVO USUARIO RioSulApp
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Usuario/NewUser")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_USU NewUser([FromBody]ViewModel.REQ_USU User)
        {
            ViewModel.RES_USU API = new ViewModel.RES_USU();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Sg_Usuarios Sg_Usuario = db.C_Sg_Usuarios.Where(x => x.Usuario == User.Usuario.Usuario).FirstOrDefault();
                    if (Sg_Usuario == null)
                    {
                        Sg_Usuario = new Models.C_Sg_Usuarios();
                        Sg_Usuario.Activo = true;
                        Sg_Usuario.Contraseña = User.Usuario.Contraseña;
                        Sg_Usuario.Nombre = User.Usuario.Nombre;
                        Sg_Usuario.LastLogin = null;
                        Sg_Usuario.Usuario = User.Usuario.Usuario;
                        Sg_Usuario.Email = User.Usuario.Email;

                        db.C_Sg_Usuarios.Add(Sg_Usuario);
                        db.SaveChanges();

                        int Id_C_Sg_usuarios = Sg_Usuario.ID;

                        List<ViewModel.Menu> Menu = _objSerializer.Deserialize<List<ViewModel.Menu>>(User.PerMenu);

                        if (Menu.Count > 0)
                        {
                            foreach (ViewModel.Menu item in Menu)
                            {
                                Models.C_Sg_PantallasOperaciones Oper;
                                Models.T_Sg_UsuariosOperaciones Usuario;
                                if (item.PER_CONSULTAR)
                                {
                                    Oper = db.C_Sg_PantallasOperaciones.Where(x => x.PantallaID == item.PANTALLA_ID && x.C_Sg_Operaciones.Operacion == "PER_CONSULTAR").FirstOrDefault();

                                    Usuario = new Models.T_Sg_UsuariosOperaciones();
                                    Usuario.Activo = true;
                                    Usuario.PantallaOperacionID = Oper.ID;
                                    Usuario.UsuarioID = Id_C_Sg_usuarios;

                                    db.T_Sg_UsuariosOperaciones.Add(Usuario);
                                }
                                if (item.PER_EDITAR)
                                {
                                    Oper = db.C_Sg_PantallasOperaciones.Where(x => x.PantallaID == item.PANTALLA_ID && x.C_Sg_Operaciones.Operacion == "PER_EDITAR").FirstOrDefault();

                                    Usuario = new Models.T_Sg_UsuariosOperaciones();
                                    Usuario.Activo = true;
                                    Usuario.PantallaOperacionID = Oper.ID;
                                    Usuario.UsuarioID = Id_C_Sg_usuarios;

                                    db.T_Sg_UsuariosOperaciones.Add(Usuario);
                                }
                                if (item.PER_REGISTRAR)
                                {
                                    Oper = db.C_Sg_PantallasOperaciones.Where(x => x.PantallaID == item.PANTALLA_ID && x.C_Sg_Operaciones.Operacion == "PER_REGISTRAR").FirstOrDefault();

                                    Usuario = new Models.T_Sg_UsuariosOperaciones();
                                    Usuario.Activo = true;
                                    Usuario.PantallaOperacionID = Oper.ID;
                                    Usuario.UsuarioID = Id_C_Sg_usuarios;

                                    db.T_Sg_UsuariosOperaciones.Add(Usuario);
                                }
                                if (item.PER_PROCESAR)
                                {
                                    Oper = db.C_Sg_PantallasOperaciones.Where(x => x.PantallaID == item.PANTALLA_ID && x.C_Sg_Operaciones.Operacion == "PER_PROCESAR").FirstOrDefault();

                                    Usuario = new Models.T_Sg_UsuariosOperaciones();
                                    Usuario.Activo = true;
                                    Usuario.PantallaOperacionID = Oper.ID;
                                    Usuario.UsuarioID = Id_C_Sg_usuarios;

                                    db.T_Sg_UsuariosOperaciones.Add(Usuario);
                                }
                                if (item.PER_CANCELAR)
                                {
                                    Oper = db.C_Sg_PantallasOperaciones.Where(x => x.PantallaID == item.PANTALLA_ID && x.C_Sg_Operaciones.Operacion == "PER_CANCELAR").FirstOrDefault();

                                    Usuario = new Models.T_Sg_UsuariosOperaciones();
                                    Usuario.Activo = true;
                                    Usuario.PantallaOperacionID = Oper.ID;
                                    Usuario.UsuarioID = Id_C_Sg_usuarios;

                                    db.T_Sg_UsuariosOperaciones.Add(Usuario);
                                }
                                if (item.PER_ACTIVAR)
                                {
                                    Oper = db.C_Sg_PantallasOperaciones.Where(x => x.PantallaID == item.PANTALLA_ID && x.C_Sg_Operaciones.Operacion == "PER_ACTIVAR").FirstOrDefault();

                                    Usuario = new Models.T_Sg_UsuariosOperaciones();
                                    Usuario.Activo = true;
                                    Usuario.PantallaOperacionID = Oper.ID;
                                    Usuario.UsuarioID = Id_C_Sg_usuarios;

                                    db.T_Sg_UsuariosOperaciones.Add(Usuario);
                                }
                                db.SaveChanges();
                            }
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
                API.Hecho = false;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// INACTIVA UN USUARIO EXISTENTE
        /// </summary>
        /// <param name="ID_USU"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Usuario/DisableUser")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_DIS_USU DisableUser(int ID_USU)
        {
            ViewModel.RES_DIS_USU API = new ViewModel.RES_DIS_USU();
            try
            {
                if (ID_USU != 0)
                {
                    Models.C_Sg_Usuarios USUARIO = db.C_Sg_Usuarios.Where(x => x.ID == ID_USU).FirstOrDefault();
                    if (USUARIO != null)
                    {
                        if (USUARIO.Activo == true)
                            USUARIO.Activo = false;
                        else
                            USUARIO.Activo = true;

                        db.Entry(USUARIO).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        API.Hecho = true;
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    else
                    {
                        API.Hecho = false;
                        API.Message = new HttpResponseMessage(HttpStatusCode.NoContent);
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
        /// MODIFICA LA CONTRASEÑA ACTUAL DEL USUARIO 
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Usuario/ChangePassword")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_CHG_PASS ChangePassword([FromBody]ViewModel.REQ_CHG_PASS User)
        {
            ViewModel.RES_CHG_PASS API = new ViewModel.RES_CHG_PASS();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Sg_Usuarios _Usuario = db.C_Sg_Usuarios.Where(x => x.ID == User.ID_USU && x.Contraseña == User.LastPassword).FirstOrDefault();
                    if (_Usuario != null)
                    {
                        _Usuario.Contraseña = User.NewPassword;
                        db.Entry(_Usuario).State = System.Data.Entity.EntityState.Modified;
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
        /// OBTIENE LOS USUARIOS REGISTRADOS A PARTIR DEL KEY REGISTRADO
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Nombre"></param>
        /// <param name="Usuario"></param>
        /// <param name="Email"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Usuario/GetUsers")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_GET_USU GetUsers(string Key = "", string Nombre = "", string Usuario = "", string Email = "")
        {
            ViewModel.RES_GET_USU API = new ViewModel.RES_GET_USU();
            try
            {
                if (!string.IsNullOrEmpty(Key) && System.Configuration.ConfigurationManager.AppSettings["PasswordKey"].ToString() == Key)
                {
                    string Filtro = "";
                    // FILTRO DE NOMBRE
                    if (Nombre != "")
                        Filtro += " AND (Nombre LIKE '%" + Nombre + "%')";
                    //FILTRO DE USUARIO
                    if (Usuario != "")
                        Filtro += " AND (Usuario LIKE '%" + Usuario + "%')";
                    //FILTRO DE EMAIL
                    if (Email != "")
                        Filtro += " AND (Email LIKE '%" + Email + "%')";

                    API.Usuarios = db.Database.SqlQuery<Models.C_Sg_Usuarios>("SELECT * FROM C_Sg_Usuarios WHERE (1 = 1)" + Filtro).Select(x => new ViewModel.USU
                    {
                        Activo = x.Activo,
                        Usuario = x.Usuario,
                        Contraseña = x.Contraseña,
                        Nombre = x.Nombre,
                        LastLogin = x.LastLogin,
                        Email = x.Email,
                        ID = x.ID
                    }).ToList();
                    API.Hecho = true;
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Hecho = false;
                    API.Usuarios = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Hecho = false;
                API.Usuarios = null;
            }
            return API;
        }

        /// <summary>
        /// OBTIENE LOS MENUS ACTIVOS 
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Route("api/Usuario/GetMenus")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_MENU GetMenus(string Key)
        {
            ViewModel.RES_MENU API = new ViewModel.RES_MENU();
            try
            {
                if (!string.IsNullOrEmpty(Key) && System.Configuration.ConfigurationManager.AppSettings["PasswordKey"].ToString() == Key)
                {
                    API.Menus = db.T_Sg_Pantallas.Where(x => x.Activo == true).Select(x => new ViewModel.t_sg_pantallas
                    {
                        ID = x.ID,
                        Pantalla = x.Pantalla,
                        PantallaId = x.Pantalla_ID,
                        Icon = x.Icon,
                        Ruta = x.Ruta,
                        SubMenu = x.SubMenu,
                        Activo = x.Activo
                    }).ToList();
                    API.Operacion = db.C_Sg_Operaciones.Select(x => new ViewModel.c_sg_operaciones
                    {
                        ID = x.ID,
                        Operacion = x.Operacion,
                        Activo = x.activa
                    }).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Menus = null; API.Operacion = null;
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                API.Menus = null;
                API.Operacion = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                Utilerias.EscribirLog(ex.ToString());
            }
            return API;
        }

        /// <summary>
        ///  OBTIENE EL DETALLE DEL USUARIO
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="USU_ID"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Usuario/GetPermitsByUser")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_USU_UPD GetPermitsByUser(string Key, int USU_ID)
        {
            ViewModel.RES_USU_UPD API = new ViewModel.RES_USU_UPD();
            API.Menu = new List<ViewModel.Menu>();
            try
            {
                if (ModelState.IsValid)
                {
                    if (Key == System.Configuration.ConfigurationManager.AppSettings["PasswordKey"].ToString())
                    {
                        API.Usuario = db.C_Sg_Usuarios.Where(x => x.ID == USU_ID).Select(x => new ViewModel.USU
                        {
                            ID = x.ID,
                            Nombre = x.Nombre,
                            Contraseña = x.Contraseña,
                            Activo = x.Activo,
                            Email = x.Email,
                            LastLogin = x.LastLogin,
                            Usuario = x.Usuario
                        }).FirstOrDefault();
                        List<Models.T_Sg_UsuariosOperaciones> ListUsuOp = db.T_Sg_UsuariosOperaciones.Where(x => x.UsuarioID == USU_ID).ToList();
                        if (ListUsuOp.Count > 0)
                        {
                            foreach (Models.T_Sg_UsuariosOperaciones item in ListUsuOp)
                            {
                                if (API.Menu.Count == 0)
                                {
                                    List<Models.VST_PERMISOS_OPERACIONES> vst_oper = db.VST_PERMISOS_OPERACIONES.Where(x => x.UsuarioID == USU_ID).ToList();
                                    if (vst_oper.Count > 0)
                                    {
                                        ViewModel.Menu _menu = new ViewModel.Menu();
                                        _menu.PANTALLA_ID = Convert.ToInt32(item.C_Sg_PantallasOperaciones.PantallaID);
                                        _menu.PER_ACTIVAR = vst_oper.Where(x => x.PantallaID == _menu.PANTALLA_ID && x.OperacionID == 6).FirstOrDefault() != null ? true : false;
                                        _menu.PER_CANCELAR = vst_oper.Where(x => x.PantallaID == _menu.PANTALLA_ID && x.OperacionID == 5).FirstOrDefault() != null ? true : false;
                                        _menu.PER_CONSULTAR = vst_oper.Where(x => x.PantallaID == _menu.PANTALLA_ID && x.OperacionID == 1).FirstOrDefault() != null ? true : false;
                                        _menu.PER_EDITAR = vst_oper.Where(x => x.PantallaID == _menu.PANTALLA_ID && x.OperacionID == 2).FirstOrDefault() != null ? true : false;
                                        _menu.PER_PROCESAR = vst_oper.Where(x => x.PantallaID == _menu.PANTALLA_ID && x.OperacionID == 4).FirstOrDefault() != null ? true : false;
                                        _menu.PER_REGISTRAR = vst_oper.Where(x => x.PantallaID == _menu.PANTALLA_ID && x.OperacionID == 3).FirstOrDefault() != null ? true : false;

                                        API.Menu.Add(_menu);
                                    }
                                }
                                else
                                {
                                    int IdPantalla = Convert.ToInt32(item.C_Sg_PantallasOperaciones.PantallaID);
                                    ViewModel.Menu _menu_ = API.Menu.Where(x => x.PANTALLA_ID == IdPantalla).FirstOrDefault();
                                    if (_menu_ == null)
                                    {
                                        List<Models.VST_PERMISOS_OPERACIONES> vst_oper = db.VST_PERMISOS_OPERACIONES.Where(x => x.UsuarioID == USU_ID).ToList();
                                        if (vst_oper.Count > 0)
                                        {
                                            ViewModel.Menu _menu = new ViewModel.Menu();
                                            _menu.PANTALLA_ID = Convert.ToInt32(item.C_Sg_PantallasOperaciones.PantallaID);
                                            _menu.PER_ACTIVAR = vst_oper.Where(x => x.PantallaID == _menu.PANTALLA_ID && x.OperacionID == 6).FirstOrDefault() != null ? true : false;
                                            _menu.PER_CANCELAR = vst_oper.Where(x => x.PantallaID == _menu.PANTALLA_ID && x.OperacionID == 5).FirstOrDefault() != null ? true : false;
                                            _menu.PER_CONSULTAR = vst_oper.Where(x => x.PantallaID == _menu.PANTALLA_ID && x.OperacionID == 1).FirstOrDefault() != null ? true : false;
                                            _menu.PER_EDITAR = vst_oper.Where(x => x.PantallaID == _menu.PANTALLA_ID && x.OperacionID == 2).FirstOrDefault() != null ? true : false;
                                            _menu.PER_PROCESAR = vst_oper.Where(x => x.PantallaID == _menu.PANTALLA_ID && x.OperacionID == 4).FirstOrDefault() != null ? true : false;
                                            _menu.PER_REGISTRAR = vst_oper.Where(x => x.PantallaID == _menu.PANTALLA_ID && x.OperacionID == 3).FirstOrDefault() != null ? true : false;

                                            API.Menu.Add(_menu);
                                        }
                                    }
                                }

                            }
                        }
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    else
                    {
                        API.Menu = null;
                        API.Usuario = null;
                        API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    API.Menu = null;
                    API.Usuario = null;
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Menu = null;
                API.Usuario = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// ACTUALIZA LOS USUARIOS Y PERMISOS
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Usuario/UpdatePermitsByUser")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_USU UpdatePermitsByUser([FromBody]ViewModel.REQ_USU_UPD User)
        {
            ViewModel.RES_USU API = new ViewModel.RES_USU();
            try
            {
                if (ModelState.IsValid)
                {
                    if (!string.IsNullOrEmpty(User.Key) && System.Configuration.ConfigurationManager.AppSettings["PasswordKey"].ToString() == User.Key)
                    {
                        //Elimina los registros actuales para ingresar los nuevos
                        List<Models.T_Sg_UsuariosOperaciones> t_Sg_Usuarios = db.T_Sg_UsuariosOperaciones.Where(x => x.UsuarioID == User.Usuario.UsuId).ToList();
                        if (t_Sg_Usuarios.Count > 0)
                        {
                            db.T_Sg_UsuariosOperaciones.RemoveRange(t_Sg_Usuarios);
                            db.SaveChanges();
                        }

                        //Actualiza los datos del usuario
                        Models.C_Sg_Usuarios c_Sg_Usuarios = db.C_Sg_Usuarios.Where(x => x.ID == User.Usuario.UsuId).FirstOrDefault();
                        c_Sg_Usuarios.Nombre = User.Usuario.Nombre;
                        c_Sg_Usuarios.Usuario = User.Usuario.Usuario;
                        c_Sg_Usuarios.Contraseña = User.Usuario.Contraseña;
                        c_Sg_Usuarios.Email = User.Usuario.Email;

                        db.Entry(c_Sg_Usuarios).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        //Ingresa los nuevos permisos del usuario
                        List<ViewModel.Menu> Menu = _objSerializer.Deserialize<List<ViewModel.Menu>>(User.PerMenu);
                        if (Menu.Count > 0)
                        {
                            int Id_C_Sg_usuarios = User.Usuario.UsuId;
                            foreach (ViewModel.Menu item in Menu)
                            {
                                Models.C_Sg_PantallasOperaciones Oper;
                                Models.T_Sg_UsuariosOperaciones Usuario;
                                if (item.PER_CONSULTAR)
                                {
                                    Oper = db.C_Sg_PantallasOperaciones.Where(x => x.PantallaID == item.PANTALLA_ID && x.C_Sg_Operaciones.Operacion == "PER_CONSULTAR").FirstOrDefault();

                                    Usuario = new Models.T_Sg_UsuariosOperaciones();
                                    Usuario.Activo = true;
                                    Usuario.PantallaOperacionID = Oper.ID;
                                    Usuario.UsuarioID = Id_C_Sg_usuarios;

                                    db.T_Sg_UsuariosOperaciones.Add(Usuario);
                                }
                                if (item.PER_EDITAR)
                                {
                                    Oper = db.C_Sg_PantallasOperaciones.Where(x => x.PantallaID == item.PANTALLA_ID && x.C_Sg_Operaciones.Operacion == "PER_EDITAR").FirstOrDefault();

                                    Usuario = new Models.T_Sg_UsuariosOperaciones();
                                    Usuario.Activo = true;
                                    Usuario.PantallaOperacionID = Oper.ID;
                                    Usuario.UsuarioID = Id_C_Sg_usuarios;

                                    db.T_Sg_UsuariosOperaciones.Add(Usuario);
                                }
                                if (item.PER_REGISTRAR)
                                {
                                    Oper = db.C_Sg_PantallasOperaciones.Where(x => x.PantallaID == item.PANTALLA_ID && x.C_Sg_Operaciones.Operacion == "PER_REGISTRAR").FirstOrDefault();

                                    Usuario = new Models.T_Sg_UsuariosOperaciones();
                                    Usuario.Activo = true;
                                    Usuario.PantallaOperacionID = Oper.ID;
                                    Usuario.UsuarioID = Id_C_Sg_usuarios;

                                    db.T_Sg_UsuariosOperaciones.Add(Usuario);
                                }
                                if (item.PER_PROCESAR)
                                {
                                    Oper = db.C_Sg_PantallasOperaciones.Where(x => x.PantallaID == item.PANTALLA_ID && x.C_Sg_Operaciones.Operacion == "PER_PROCESAR").FirstOrDefault();

                                    Usuario = new Models.T_Sg_UsuariosOperaciones();
                                    Usuario.Activo = true;
                                    Usuario.PantallaOperacionID = Oper.ID;
                                    Usuario.UsuarioID = Id_C_Sg_usuarios;

                                    db.T_Sg_UsuariosOperaciones.Add(Usuario);
                                }
                                if (item.PER_CANCELAR)
                                {
                                    Oper = db.C_Sg_PantallasOperaciones.Where(x => x.PantallaID == item.PANTALLA_ID && x.C_Sg_Operaciones.Operacion == "PER_CANCELAR").FirstOrDefault();

                                    Usuario = new Models.T_Sg_UsuariosOperaciones();
                                    Usuario.Activo = true;
                                    Usuario.PantallaOperacionID = Oper.ID;
                                    Usuario.UsuarioID = Id_C_Sg_usuarios;

                                    db.T_Sg_UsuariosOperaciones.Add(Usuario);
                                }
                                if (item.PER_ACTIVAR)
                                {
                                    Oper = db.C_Sg_PantallasOperaciones.Where(x => x.PantallaID == item.PANTALLA_ID && x.C_Sg_Operaciones.Operacion == "PER_ACTIVAR").FirstOrDefault();

                                    Usuario = new Models.T_Sg_UsuariosOperaciones();
                                    Usuario.Activo = true;
                                    Usuario.PantallaOperacionID = Oper.ID;
                                    Usuario.UsuarioID = Id_C_Sg_usuarios;

                                    db.T_Sg_UsuariosOperaciones.Add(Usuario);
                                }
                                db.SaveChanges();
                            }
                        }
                        API.Hecho = true;
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    else
                    {
                        API.Hecho = false;
                        API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
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


    }
}