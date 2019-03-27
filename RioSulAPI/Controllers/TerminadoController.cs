using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using RioSulAPI.Class;
using RioSulAPI.Models;

namespace RioSulAPI.Controllers
{
    public class TerminadoController : ApiController
    {
        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

        #region Defectos

        /// <summary>
        /// Crea un nuevo defecto de terminado
        /// </summary>
        /// <param name="OT"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/NuevoDefectoTerminado")]
        public ViewModel.RES_DEFECTO_TERMINADO NuevoDefectoTerminado()
        {
            string imageName = null;
            var httpRequest = HttpContext.Current.Request;
            // Imagen cargada
            var postedFile = httpRequest.Files["Imagen"];
            ViewModel.RES_DEFECTO_TERMINADO API = new ViewModel.RES_DEFECTO_TERMINADO();
            try
            {
                // Creando custom filename
                imageName = new String(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(10).ToArray()).Replace(" ", "-");
                imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
                var filePath = HttpContext.Current.Server.MapPath("~/Imagenes/" + imageName);
                postedFile.SaveAs(filePath);
                Models.C_Terminado _Terminado = new Models.C_Terminado()
                {
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    Clave = httpRequest["Clave"],
                    IdUsuario = Int32.Parse(httpRequest["IdUsuario"]),
                    IdSubModulo = 21,
                    Descripcion = httpRequest["Descripcion"],
                    Nombre = httpRequest["Nombre"],
                    Observaciones = httpRequest["Observaciones"],
                    Imagen = imageName
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
        public ViewModel.RES_BUS_DEFECTO_CORTE_TERMINADO ObtieneDefecto(string Clave = "", string Nombre = "", string Activo = "")
        {
            ViewModel.RES_BUS_DEFECTO_CORTE_TERMINADO API = new ViewModel.RES_BUS_DEFECTO_CORTE_TERMINADO();

            var Terminado = db.VST_TERMINADO
                .Where(x => (x.Clave.Contains(Clave) || x.Nombre.Contains(Nombre)) && x.IdSubModulo == 21);
            try
            {
                if (ModelState.IsValid)
                {
                    switch (Activo)
                    {
                        case "True":
                            Terminado = Terminado.Where(x => x.Activo == true);
                            break;
                        case "False":
                            Terminado = Terminado.Where(x => x.Activo == false);
                            break;
                    }

                    API.Vst_Terminado = Terminado.OrderBy(x => x.Nombre).ToList();
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
                    var consulta = db.VST_TERMINADO.Where(x => x.ID == ID).FirstOrDefault();

                    var filePath = HttpContext.Current.Server.MapPath("~/Imagenes/" + consulta.Imagen);
                    if (File.Exists(filePath))
                    {
                        var extension = Path.GetExtension(consulta.Imagen).Remove(0, 1);

                        consulta.Imagen = "data:image/" + extension + ";base64," + Convert.ToBase64String(File.ReadAllBytes(filePath));
                    }

                    //API.Vst_Terminado = db.VST_TERMINADO.Where(x => x.ID == ID).FirstOrDefault();
                    API.Vst_Terminado = consulta;
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
        [HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        public MESSAGE ActivaInactivaDefecto(int ID)
        {
            MESSAGE API = new MESSAGE();
            try
            {
                if (ModelState.IsValid)
                {
                        Models.C_Terminado c_Cort = db.C_Terminado.Where(x => x.ID == ID).FirstOrDefault();
                        c_Cort.Activo = (c_Cort.Activo == false ? true : false);

                        db.Entry(c_Cort).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        API.Hecho = "Registro modificado con éxito";
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
                API.Hecho = "Error interno";
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Activa o inactiva el defecto por IdDefecto
        /// </summary>
        /// <param name="IdDefecto"></param>
        /// <returns></returns>
        [Route("api/Terminado/Defecto")]
        [HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        public MESSAGE EliminaDefecto(int ID)
        {
            MESSAGE API = new MESSAGE();
            try
            {
                if (ModelState.IsValid)
                {

                    Models.Auditoria_Terminado_Detalle AUD = db.Auditoria_Terminado_Detalle.Where(x => x.IdDefecto == ID).FirstOrDefault();

                    if (AUD == null)
                    {
                        Models.C_Terminado c_Cort = db.C_Terminado.Where(x => x.ID == ID).FirstOrDefault();
                        db.C_Terminado.Remove(c_Cort);
                        db.SaveChanges();

                        API.Hecho = "Defecto eliminado con éxito";
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    else
                    {
                        API.Hecho = "Registro relacionado con Auditoria, Validar Registro, Imposible eliminar";
                        API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
                    }
                }
                else
                {
                    API.Hecho = "Formato Inválido";
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                API.Hecho = "Error interno";
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        #endregion

        #region OPERACIONES

        /// <summary>
        /// CREA UNA NUEVA OPERACIÓN TERMINADO
        /// </summary>
        /// <param name="Operacion"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/NuevaOperacionTerminado")]
        public ViewModel.RES_OPERACION_TERMINADO NuevaOperacionTerminado([FromBody] ViewModel.REQ_OPERACION_TERMINADO Operacion)
        {
            ViewModel.RES_OPERACION_TERMINADO API = new ViewModel.RES_OPERACION_TERMINADO();
            try
            {
                if (ModelState.IsValid){

                    Models.C_Operacion_Terminado prueba = db.C_Operacion_Terminado.Where(x => x.Tipo == "Terminado" && (x.Clave == Operacion.Clave || x.Nombre == Operacion.Nombre)).FirstOrDefault();

                    if (prueba == null)
                    {
                        Models.C_Operacion_Terminado _operacion = new Models.C_Operacion_Terminado()
                        {
                            Activo = true,
                            Clave = Operacion.Clave,
                            Nombre = Operacion.Nombre.ToUpper(),
                            Tipo = "Terminado"
                        };
                        db.C_Operacion_Terminado.Add(_operacion);
                        db.SaveChanges();

                        API.Hecho = true;
                        API.Message2 = "Registro realizado con éxito";
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    else
                    {
                        API.Message2 = "Registro existente, favor de validar";
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
        /// ACTUALIZAMOS ALGUN CAMPO DE UNA OPERACIÓN
        /// </summary>
        /// <param name="Operacion"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/ActualizaOperacionTerminado")]
        public ViewModel.RES_OPERACION_TERMINADO ActualizaOperacionTerminado([FromBody] ViewModel.REQ_EDT_OPERACION_TERMINADO Operacion)
        {
            ViewModel.RES_OPERACION_TERMINADO API = new ViewModel.RES_OPERACION_TERMINADO();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Operacion_Terminado con = db.C_Operacion_Terminado.Where(x => x.ID == Operacion.ID).FirstOrDefault();

                    if (con != null)
                    {
                        Models.C_Operacion_Terminado prueba = db.C_Operacion_Terminado.Where(x => x.Tipo == "Terminado" && (x.Clave == Operacion.Clave || x.Nombre == Operacion.Nombre) && x.ID != Operacion.ID).FirstOrDefault();

                        if (prueba == null)
                        {
                            con.Nombre = Operacion.Nombre.ToUpper();
                            con.Clave = Operacion.Clave;

                            db.Entry(con).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                            API.Message2 = "Registro actualizado con éxito";
                            API.Hecho = true;
                        }
                        else
                        {
                            API.Message2 = "Registro existente, favor de validar";
                            API.Hecho = false;
                            API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
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
                API.Hecho = false;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// OBTENEMOS UNA LISTA DEL CATALOGO DE OPERACIONES DEPENDIENDO LA CLAVE
        /// </summary>
        /// <param name="clave"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/ObtieneOperacionTerminados")]
        public ViewModel.RES_BUS_OPERACION_TERMINADO ObtieneOperacionTerminados(string clave = "", string activo = "")
        {
            ViewModel.RES_BUS_OPERACION_TERMINADO API = new ViewModel.RES_BUS_OPERACION_TERMINADO();
            try
            {
                if (ModelState.IsValid)
                {
                    var Operacion = db.C_Operacion_Terminado.Where(x => x.Tipo == "Terminado" && x.Clave.Contains(clave));

                    switch (activo)
                    {
                        case "True":
                            Operacion = Operacion.Where(x => x.Activo == true);
                            break;
                        case "False":
                            Operacion = Operacion.Where(x => x.Activo == false);
                            break;
                    }

                    API.COperacionTerminados = Operacion.OrderBy(x => x.Clave).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.COperacionTerminados = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.COperacionTerminados = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// OBTENEMOS LA INFORMACION DEL CATALOGO DE OPERACIONES POR ID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/ObtieneInfOperacionTerminado")]
        public ViewModel.RES_BUS_ONE_OPERACION_TERMINADO ObtieneInfOperacionTerminado(int ID)
        {
            ViewModel.RES_BUS_ONE_OPERACION_TERMINADO API = new ViewModel.RES_BUS_ONE_OPERACION_TERMINADO();

            try
            {
                if (ModelState.IsValid)
                {
                    API.c_operacion_t = db.C_Operacion_Terminado.Where(x => x.ID == ID).FirstOrDefault();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.c_operacion_t = null;
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
        /// ACTIVAMOS O DESACTIVAMOS LA OPERACION DEPENDIENDO EL CASO
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/ActivaInactivaOperacion")]
        public MESSAGE ActivaInactivaOperacion(int ID)
        {
            MESSAGE API = new MESSAGE();

            try
            {
                if (ModelState.IsValid)
                {

                        Models.C_Operacion_Terminado op = db.C_Operacion_Terminado.Where(x => x.ID == ID).FirstOrDefault();

                        op.Activo = (op.Activo == false ? true : false);

                        db.Entry(op).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        API.Hecho = "Registro modificado con éxito";
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
                API.Hecho = "Error interno";
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// ACTIVAMOS O DESACTIVAMOS LA OPERACION DEPENDIENDO EL CASO
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/Operacion")]
        public MESSAGE EliminaOperacion(int ID)
        {
            MESSAGE API = new MESSAGE();

            try
            {
                if (ModelState.IsValid)
                {
                    Models.Auditoria_Terminado_Detalle AUD = db.Auditoria_Terminado_Detalle.Where(x => x.IdOperacion == ID).FirstOrDefault();

                    if (AUD == null)
                    {
                        Models.C_Operacion_Terminado op = db.C_Operacion_Terminado.Where(x => x.ID == ID).FirstOrDefault();
                        db.C_Operacion_Terminado.Remove(op);
                        db.SaveChanges();

                        API.Hecho = "Registro eliminado con éxito";
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    else
                    {
                        API.Hecho = "Registro relacionado con Auditoria, Validar Registro, Imposible eliminar";
                        API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
                    }
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
                API.Hecho = "Error interno";
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        #endregion

        #region POSICION

        /// <summary>
        /// Creamos un nuevo registro de una Posición/Terminado
        /// </summary>
        /// <param name="Posicion"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/NuevaPosicionT")]
        public ViewModel.RESPUESTA_MENSAJE NuevaPosicionT([FromBody] ViewModel.REQ_POSICION_TERMINADO Posicion)
        {
            ViewModel.RESPUESTA_MENSAJE API = new ViewModel.RESPUESTA_MENSAJE();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Posicion_Terminado _posicion = new Models.C_Posicion_Terminado()
                    {
                        Activo = true,
                        Clave = Posicion.Clave,
                        Nombre = Posicion.Nombre.ToUpper()
                    };
                    db.C_Posicion_Terminado.Add(_posicion);
                    db.SaveChanges();

                    API.Hecho = true;
                    API.Message2 = "Registro realizado con éxito";
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

                if (ex.ToString().Contains("Violation of UNIQUE KEY"))
                {
                    API.Message2 = "Registro existente, favor de validar";
                    API.Hecho = false;
                    API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
                }
                else
                {
                    API.Hecho = false;
                    API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

            }
            return API;
        }

        /// <summary>
        /// Obtenemos todas las posiciones que existan dependiendo el parámetro
        /// </summary>
        /// <param name="clave"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/ObtienePosicionT")]
        public ViewModel.RES_BUS_POSICION_TERMINADO ObtienePosicionT(string clave="", string activo = "")
        {
            ViewModel.RES_BUS_POSICION_TERMINADO API = new ViewModel.RES_BUS_POSICION_TERMINADO();
            try
            {
                if (ModelState.IsValid)
                {
                    var posicion = db.C_Posicion_Terminado.Where(x => x.Clave.Contains(clave));

                    switch (activo)
                    {
                        case "True":
                            posicion = posicion.Where(x => x.Activo == true);
                            break;
                        case "False":
                            posicion = posicion.Where(x => x.Activo == false);
                            break;
                    }
                    API.c_posicion_t = posicion.OrderBy(x => x.Clave).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.c_posicion_t = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.c_posicion_t = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtenemos un registro del catálogo de posicion/terminado dependiendo el id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/ObtieneInfPosicionT")]
        public ViewModel.RES_BUS_ONE_POSICION_TERMINADO ObtieneInfPosicionT(int ID)
        {
            ViewModel.RES_BUS_ONE_POSICION_TERMINADO API = new ViewModel.RES_BUS_ONE_POSICION_TERMINADO();

            try
            {
                if (ModelState.IsValid)
                {
                    API.c_posicion_t = db.C_Posicion_Terminado.Where(x => x.ID == ID).FirstOrDefault();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.c_posicion_t = null;
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
        /// Actualizamos un registro del catálogo de posiciones
        /// </summary>
        /// <param name="Posicion"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/ActualizaPosicionT")]
        public ViewModel.RESPUESTA_MENSAJE ActualizaPosicionT([FromBody] ViewModel.REQ_EDIT_POSICION_TERMINADO Posicion)
        {
            ViewModel.RESPUESTA_MENSAJE API = new ViewModel.RESPUESTA_MENSAJE();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Posicion_Terminado con = db.C_Posicion_Terminado.Where(x => x.ID == Posicion.ID).FirstOrDefault();

                    if (con != null)
                    {
                        con.Nombre = Posicion.Nombre.ToUpper();
                        con.Clave = Posicion.Clave;

                        db.Entry(con).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                        API.Message2 = "Registro actualizado con éxito";
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
                if (ex.ToString().Contains("Violation of UNIQUE KEY"))
                {
                    API.Message2 = "Favor de validar registros Requeridos, Imposible guardar";
                    API.Hecho = false;
                    API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
                }
                else
                {
                    API.Hecho = false;
                    API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
            }
            return API;
        }

        /// <summary>
        /// Activamos o desactivamos el registro de Posición depenediendo el caso
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/ActivaInactivaPosicionT")]
        public MESSAGE ActivaInactivaPosicionT(int ID)
        {
            MESSAGE API = new MESSAGE();
            try
            {
                if (ModelState.IsValid)
                {
                        Models.C_Posicion_Terminado posicion = db.C_Posicion_Terminado.Where(x => x.ID == ID).FirstOrDefault();
                        posicion.Activo = (posicion.Activo == false ? true : false);

                        db.Entry(posicion).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        API.Hecho = "Registro modificado con éxito";
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
                API.Hecho = "Error interno";
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Activamos o desactivamos el registro de Posición depenediendo el caso
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/Posicion")]
        public MESSAGE EliminaPosicion(int ID)
        {
            MESSAGE API = new MESSAGE();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.Auditoria_Terminado_Detalle AUD = db.Auditoria_Terminado_Detalle.Where(x => x.IdPosicion == ID).FirstOrDefault();

                    if (AUD == null)
                    {
                        Models.C_Posicion_Terminado posicion = db.C_Posicion_Terminado.Where(x => x.ID == ID).FirstOrDefault();
                        db.C_Posicion_Terminado.Remove(posicion);
                        db.SaveChanges();

                        API.Hecho = "Registro eliminado con éxito";
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    else
                    {
                        API.Hecho = "Registro relacionado con Auditoria, Validar Registro, Imposible eliminar";
                        API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
                    }
                }
                else
                {
                    API.Hecho = "Formato Inválido";
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                API.Hecho = "Error interno";
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        #endregion

        #region ORIGEN

        /// <summary>
        /// Creamos un nuevo registro de Origen/Terminado
        /// </summary>
        /// <param name="origen"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/NuevoOrigenT")]
        public ViewModel.RESPUESTA_MENSAJE NuevoOrigenT([FromBody] ViewModel.REQ_ORIGEN_TERMINADO origen)
        {
            ViewModel.RESPUESTA_MENSAJE API = new ViewModel.RESPUESTA_MENSAJE();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Origen_Terminado _origen = new Models.C_Origen_Terminado()
                    {
                        Activo = true,
                        Clave = origen.Clave,
                        Nombre = origen.Nombre.ToUpper()
                    };
                    db.C_Origen_Terminado.Add(_origen);
                    db.SaveChanges();

                    API.Hecho = true;
                    API.Message2 = "Registro realizado con éxito";
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

                if (ex.ToString().Contains("Violation of UNIQUE KEY"))
                {
                    API.Message2 = "Registro existente, favor de validar";
                    API.Hecho = false;
                    API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
                }
                else
                {
                    API.Hecho = false;
                    API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

            }
            return API;
        }

        /// <summary>
        /// Obtenemos todos los registros de origen dependiendo la búsqueda
        /// </summary>
        /// <param name="clave"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/ObtieneOrigenT")]
        public ViewModel.RES_BUS_ORIGEN_TERMINADO ObtieneOrigenT(string clave="", string activo = "")
        {
            ViewModel.RES_BUS_ORIGEN_TERMINADO API = new ViewModel.RES_BUS_ORIGEN_TERMINADO();
            try
            {
                if (ModelState.IsValid)
                {
                    var origen = db.C_Origen_Terminado.Where(x => x.Clave.Contains(clave));

                    switch (activo)
                    {
                        case "True":
                            origen = origen.Where(x => x.Activo == true);
                            break;
                        case "False":
                            origen = origen.Where(x => x.Activo == false);
                            break;
                    }

                    API.c_origen_t = origen.OrderBy(x => x.Clave).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.c_origen_t = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.c_origen_t = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Obtenemos un registro del catálogo de origen dependiendo el ID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/ObtieneInfOrigenT")]
        public ViewModel.RES_BUS_ONE_ORIGEN_TERMINADO ObtieneInfOrigenT(int ID)
        {
            ViewModel.RES_BUS_ONE_ORIGEN_TERMINADO API = new ViewModel.RES_BUS_ONE_ORIGEN_TERMINADO();

            try
            {
                if (ModelState.IsValid)
                {
                    API.c_origen_t = db.C_Origen_Terminado.Where(x => x.ID == ID).FirstOrDefault();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.c_origen_t = null;
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
        /// Actualizamos un registro del catálogo de origen
        /// </summary>
        /// <param name="origen"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/ActualizaOrigenT")]
        public ViewModel.RESPUESTA_MENSAJE ActualizaOrigenT([FromBody] ViewModel.REQ_EDIT_ORIGEN_TERMINADO origen)
        {
            ViewModel.RESPUESTA_MENSAJE API = new ViewModel.RESPUESTA_MENSAJE();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Origen_Terminado con = db.C_Origen_Terminado.Where(x => x.ID == origen.ID).FirstOrDefault();

                    if (con != null)
                    {
                        con.Nombre = origen.Nombre.ToUpper();
                        con.Clave = origen.Clave;

                        db.Entry(con).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                        API.Message2 = "Registro actualizado con éxito";
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
                if (ex.ToString().Contains("Violation of UNIQUE KEY"))
                {
                    API.Message2 = "Favor de validar registros Requeridos, Imposible guardar";
                    API.Hecho = false;
                    API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
                }
                else
                {
                    API.Hecho = false;
                    API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
            }
            return API;
        }

        /// <summary>
        /// Activamos o desactivamos el registro de Posición depenediendo el caso
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/ActivaInactivaOrigenT")]
        public MESSAGE ActivaInactivaOrigenT(int ID)
        {
            MESSAGE API = new MESSAGE();
            try
            {
                if (ModelState.IsValid)
                {
                        Models.C_Origen_Terminado origen = db.C_Origen_Terminado.Where(x => x.ID == ID).FirstOrDefault();
                        origen.Activo = (origen.Activo == false ? true : false);

                        db.Entry(origen).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        API.Hecho = "Registro modificado con éxito";
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
                API.Hecho = "Error interno";
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// Activamos o desactivamos el registro de Posición depenediendo el caso
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Terminado/Origen")]
        public MESSAGE EliminaOrigen(int ID)
        {
            MESSAGE API = new MESSAGE();
            try
            {
                if (ModelState.IsValid)
                {
                    Models.Auditoria_Terminado_Detalle AUD = db.Auditoria_Terminado_Detalle.Where(x => x.IdOrigen == ID).FirstOrDefault();

                    if (AUD == null)
                    {
                        Models.C_Origen_Terminado origen = db.C_Origen_Terminado.Where(x => x.ID == ID).FirstOrDefault();
                        db.C_Origen_Terminado.Remove(origen);
                        db.SaveChanges();

                        API.Hecho = "Registro eliminado con éxito";
                        API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    else
                    {
                        API.Hecho = "Registro relacionado con Auditoria, Validar Registro, Imposible eliminar";
                        API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
                    }
                }
                else
                {
                    API.Hecho = "Formato Inválido";
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                API.Hecho = "Error interno";
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        #endregion

        public partial class MESSAGE
        {
            public string Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
        }
    }
}
