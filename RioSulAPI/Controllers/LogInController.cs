using RioSulAPI.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Mvc;

namespace RioSulAPI.Controllers
{
    public class LogInController : ApiController
    {
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

        /// POST api/Login/IniciaSesion
        /// <summary>
        /// Valida el inicio de sesion
        /// </summary>
        /// <param name="Login"></param>
        [System.Web.Http.Route("api/LogIn/IniciaSesion")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_LOGIN IniciaSesion([FromBody]ViewModel.REQ_LOGIN Login)
        {
            ViewModel.RES_LOGIN API = new ViewModel.RES_LOGIN();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Sg_Usuarios _Usuario = db.C_Sg_Usuarios.Where(x => x.Usuario == Login.Usuario && x.Contraseña == Login.Contrasena && x.Activo == true).FirstOrDefault();
                    if (_Usuario != null)
                    {
                        API.Usuario = new ViewModel.USU
                        {
                            Usuario = _Usuario.Usuario,
                            Contraseña = _Usuario.Contraseña,
                            Activo = _Usuario.Activo,
                            ID = _Usuario.ID,
                            LastLogin = _Usuario.LastLogin,
                            Nombre = _Usuario.Nombre,
                            Email = _Usuario.Email
                        };
                        List<Models.VST_PERMISOS> _PER = db.VST_PERMISOS.Where(x => x.ID == _Usuario.ID).ToList();

                        API.PER = new List<ViewModel.PERMISOS>();
                        foreach (Models.VST_PERMISOS item in _PER.Where(x => x.SubMenu == null))
                        {
                            ViewModel.PERMISOS _per = new ViewModel.PERMISOS();
                            _per.PerGral = item;
                            _per.Per = _PER.Where(x => x.Pantalla_ID == item.PantallaOperacionID).ToList();

                            API.PER.Add(_per);
                        }
                        
                        API.Valido = true;
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);

                        _Usuario.LastLogin = DateTime.Now;

                        db.Entry(_Usuario).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        API.Valido = false; API.Usuario = null; API.PER = null;
                        API.Message = new HttpResponseMessage(HttpStatusCode.NoContent);
                    }
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Valido = false; API.Usuario = null; API.PER = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.Log.Log(ex.ToString(), "LogIn", "RioSulApi");
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Valido = false; API.Usuario = null; API.PER = null;
            }
            return API;
        }
    }
}