using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using RioSulAPI.Class;

namespace RioSulAPI.Controllers
{
    public class CatalogoSegundasController : ApiController
    {
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

        /// <summary>
        /// OBTIENE LOS ESTILOS DE SEGUNDAS DE LA BASE DYNAMICS
        /// </summary>
        /// <param name="Consulta"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/CatalogoSegundas/ObtieneSegundasDynamics")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_SEGUNDAS ObtieneSegundasDynamics(string Key)
        {
            ViewModel.RES_SEGUNDAS API = new ViewModel.RES_SEGUNDAS();
            try
            {
                if (!string.IsNullOrEmpty(Key) && System.Configuration.ConfigurationManager.AppSettings["PasswordKey"].ToString() == Key)
                {
                    using (SqlConnection _Conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["dbRioSulApp"].ToString()))
                    {
                        _Conn.Open();
                        SqlCommand _sqlCommand = new SqlCommand("SELECT DISTINCT SUBSTRING(dbo.Inventory.InvtID, 0, 25) AS estilo, dbo.Inventory.Descr AS des_estilo FROM dbo.Inventory WHERE(dbo.Inventory.TranStatusCode = 'AC') AND(dbo.Inventory.InvtType = 'F') ORDER BY 1", _Conn);
                        SqlDataReader sqlData = _sqlCommand.ExecuteReader();
                        API.estilos = new List<ViewModel.Estilos>();
                        while (sqlData.Read())
                        {
                            ViewModel.Estilos estilos = new ViewModel.Estilos();
                            estilos.estilo = sqlData[0].ToString().Trim();
                            estilos.des_estilo = sqlData[1].ToString().Trim();

                            API.estilos.Add(estilos);

                        }
                        sqlData.Close();
                    }
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.estilos = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.estilos = null;
            }
            return API;
        }

        /// <summary>
        /// AGREGA SEGUNDA A LA BASE DE DATOS
        /// </summary>
        /// <param name="SEG"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/CatalogoSegundas/GuardaSegundaPorcentajes")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_NEW_SEGUNDA GuardaSegundaPorcentajes([FromBody]ViewModel.REQ_C_SEGUNDA SEG)
        {
            ViewModel.RES_NEW_SEGUNDA API = new ViewModel.RES_NEW_SEGUNDA();
            try
            {
                Models.C_Segundas c_Segundas = new Models.C_Segundas()
                {
                    Descripcion = SEG.Descripcion,
                    Estilo = SEG.Estilo,
                    Porcentaje_Confeccion = SEG.Porcentaje_Confeccion,
                    Porcentaje_Corte = SEG.Porcentaje_Corte,
                    Porcentaje_Lavanderia = SEG.Porcentaje_Lavanderia,
                    Porcentaje_ProcesosEspeciales = SEG.Porcentaje_ProcesosEspeciales,
                    Porcentaje_Tela = SEG.Porcentaje_Tela,
                    Porcentaje_Terminado = SEG.Porcentaje_Terminado,
                    FechaRegistro = DateTime.Now,
                    Costo_Estilo = SEG.Costo_Estilo,
                    Costo_Segunda = SEG.Costo_Segunda
                };

                db.C_Segundas.Add(c_Segundas);
                db.SaveChanges();

                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        /// <summary>
        /// EDITA LOS PORCENTAJES DE UNA SEGUNDA, DE LA BASE DE DATOS
        /// </summary>
        /// <param name="RES"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/CatalogoSegundas/EditaSegundaPorcentajes")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public bool EditaSegundaPorcentajes([FromBody]ViewModel.RES_C_SEGUNDA RES, string Key)
        {
            try
            {
                if (!string.IsNullOrEmpty(Key) && System.Configuration.ConfigurationManager.AppSettings["PasswordKey"].ToString() == Key)
                {
                    Models.C_Segundas c_ = db.C_Segundas.Where(x => x.IdSegunda == RES.IdSegunda).FirstOrDefault();
                    c_.Porcentaje_Confeccion = RES.Porcentaje_Confeccion;
                    c_.Porcentaje_Corte = RES.Porcentaje_Corte;
                    c_.Porcentaje_Lavanderia = RES.Porcentaje_Lavanderia;
                    c_.Porcentaje_ProcesosEspeciales = RES.Porcentaje_ProcesosEspeciales;
                    c_.Porcentaje_Tela = RES.Porcentaje_Tela;
                    c_.Porcentaje_Terminado = RES.Porcentaje_Terminado;
                    c_.Costo_Estilo = RES.Costo_Estilo;
                    c_.Costo_Segunda = RES.Costo_Segunda;

                    db.Entry(c_).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// ELIMINA EL REGISTRO DEL CATALOGO SEGUNDAS
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="IdSegunda"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/CatalogoSegundas/EliminaSegundaPorcentaje")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public bool ElimianSegundaPorcentaje(string Key, int IdSegunda)
        {
            try
            {
                if (!string.IsNullOrEmpty(Key) && System.Configuration.ConfigurationManager.AppSettings["PasswordKey"].ToString() == Key)
                {
                    Models.C_Segundas _Segundas = db.C_Segundas.Where(x => x.IdSegunda == IdSegunda).FirstOrDefault();
                    db.C_Segundas.Remove(_Segundas);
                    db.SaveChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// OBTIENE LOS ESTILOS YA CON SUS PORCENTAJES DE SEGUNDA
        /// </summary>
        /// <param name="Busqueda"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/CatalogoSegundas/ObtieneEstilosApp")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public List<ViewModel.RES_C_SEGUNDA> ObtieneEstilosApp(string Busqueda = "")
        {
            List<ViewModel.RES_C_SEGUNDA> API = new List<ViewModel.RES_C_SEGUNDA>();
            try
            {
                API = db.C_Segundas.Where(x => x.Descripcion.Contains(Busqueda) || x.Estilo.Contains(Busqueda)).Select(x => new ViewModel.RES_C_SEGUNDA()
                {
                    Descripcion = x.Descripcion,
                    Estilo = x.Estilo,
                    IdSegunda = x.IdSegunda,
                    Porcentaje_Confeccion = x.Porcentaje_Confeccion,
                    Porcentaje_Corte = x.Porcentaje_Corte,
                    Porcentaje_Lavanderia = x.Porcentaje_Lavanderia,
                    Porcentaje_ProcesosEspeciales = x.Porcentaje_ProcesosEspeciales,
                    Porcentaje_Tela = x.Porcentaje_Tela,
                    Porcentaje_Terminado = x.Porcentaje_Terminado,
                    Costo_Estilo = x.Costo_Estilo,
                    Costo_Segunda = x.Costo_Segunda
                }).ToList();
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
            }
            return API;
        }

        /// <summary>
        /// VERIFICA QUE EL ESTILO NO CUENTE CON PORCENTAJES DE SEGUNDAS
        /// </summary>
        /// <param name="Estilo"></param>
        /// <param name="Descripcion"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/CatalogoSegundas/VerificaEstilo")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public bool VerificaEstilo(string Estilo, string Descripcion)
        {
            try
            {
                Models.C_Segundas c_Segundas = db.C_Segundas.Where(x => x.Estilo == Estilo && x.Descripcion == Descripcion).FirstOrDefault();
                if (c_Segundas != null)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// OBTIENE LOS DETALLES DEL IdSegunda, PARA EDICIÓN
        /// </summary>
        /// <param name="IdSegunda"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/CatalogoSegundas/GetSegundaById")] 
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_C_SEGUNDA GetSegundaById(int IdSegunda)
        {
            ViewModel.RES_C_SEGUNDA rES_C_SEGUNDA = new ViewModel.RES_C_SEGUNDA();
            try
            {
                Models.C_Segundas c_Segundas = db.C_Segundas.Where(x => x.IdSegunda == IdSegunda).FirstOrDefault();
                rES_C_SEGUNDA.IdSegunda = c_Segundas.IdSegunda;
                rES_C_SEGUNDA.Porcentaje_Confeccion = c_Segundas.Porcentaje_Confeccion;
                rES_C_SEGUNDA.Porcentaje_Corte = c_Segundas.Porcentaje_Corte;
                rES_C_SEGUNDA.Porcentaje_Lavanderia = c_Segundas.Porcentaje_Lavanderia;
                rES_C_SEGUNDA.Porcentaje_ProcesosEspeciales = c_Segundas.Porcentaje_ProcesosEspeciales;
                rES_C_SEGUNDA.Porcentaje_Tela = c_Segundas.Porcentaje_Tela;
                rES_C_SEGUNDA.Porcentaje_Terminado = c_Segundas.Porcentaje_Terminado;
                rES_C_SEGUNDA.Estilo = c_Segundas.Estilo;
                rES_C_SEGUNDA.Descripcion = c_Segundas.Descripcion;
                rES_C_SEGUNDA.Costo_Segunda = c_Segundas.Costo_Segunda;
                rES_C_SEGUNDA.Costo_Estilo = c_Segundas.Costo_Estilo;

            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
            }
            return rES_C_SEGUNDA;
        }

    }
}
