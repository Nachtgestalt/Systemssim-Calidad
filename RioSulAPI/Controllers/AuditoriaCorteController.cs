using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class AuditoriaCorteController : ApiController
    {
        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

        /// <summary>
        /// Obtiene las Ordenes de trabajo en Dynamics
        /// </summary>
        /// <param name="IdClienteRef"></param>
        /// <param name="OrdenT"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCorte/ObtieneOrdenesTrabajoDynamics")]
        public RES_OT_CLI ObtieneOrdenesTrabajoDynamics(string IdClienteRef, string OrdenT)
        {
            RES_OT_CLI API = new RES_OT_CLI();
            try
            {
                List<Models.C_ClientesReferencia> CliRef;
                string _cli = "";
                //OBTIENE LOS CLIENTES 
                using (Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities())
                {
                    int _idCliRef = Convert.ToInt32(IdClienteRef);
                    CliRef = db.C_ClientesReferencia.Where(x => x.IdClienteRef == _idCliRef).ToList();
                    if (CliRef.Count > 0)
                    {
                        foreach (Models.C_ClientesReferencia item in CliRef)
                        {
                            if (_cli == "")
                                _cli = "'" + item.Cve_Cliente + "'";
                            else
                                _cli += ",'" + item.Cve_Cliente + "'";
                        }
                    }
                }
                using (SqlConnection _Conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["dbRioSulApp"].ToString()))
                {
                    _Conn.Open();
                    string Consulta = "SELECT WOHeader.ProcStage AS EtapaProceso, LTRIM(RTRIM(WOHeader.User9)) AS OrdenVenta, SOHeader.CustID AS Cve_Cliente, Customer.Name AS NombreCorto, XCustomer.LongName AS NombreLargo, WOHeader.InvtID AS NoArticulo, Inventory.Color AS Lavado, WOHeader.User5 AS Planta, RsTb_Plantas.Descr AS Planta_Descripcion, Inventory.ClassID AS Marca, ISNULL(ItemXRef.AlternateID, '') AS No_Alterno, WOHeader.QtyOrig AS Fab_Original, WOHeader.User6 AS PO FROM WOHeader INNER JOIN SOHeader ON WoHeader.User9 = SoHeader.OrdNbr INNER JOIN Customer ON SOHeader.CustID = Customer.CustId INNER JOIN XCustomer ON Customer.CustId = XCustomer.CustId INNER JOIN Inventory ON WOHeader.InvtID = Inventory.InvtID INNER JOIN RsTb_Plantas ON WOHeader.User5 = RsTb_Plantas.Planta LEFT OUTER JOIN ItemXRef ON WOHeader.InvtID = ItemXRef.InvtID WHERE (1 = 1) AND (SOHeader.CustID IN(" + _cli + ")) AND (LTRIM(RTRIM(WOHeader.User9)) = '" + OrdenT + "') GROUP BY WOHeader.User9, SOHeader.CustID, WOHeader.ProcStage, Customer.Name, XCustomer.LongName, WOHeader.InvtID, Inventory.Color, WOHeader.User5, RsTb_Plantas.Descr, Inventory.ClassID, ItemXRef.AlternateID, WOHeader.QtyOrig, WOHeader.User6 ORDER BY 1 ";
                    API.OrdenTrabajo = new List<OT_CLI>();
                    SqlCommand Command = new SqlCommand(Consulta, _Conn);
                    SqlDataReader sqlData = Command.ExecuteReader();
                    while (sqlData.Read())
                    {
                        OT_CLI OT = new OT_CLI()
                        {
                            EtapaProceso = sqlData[0].ToString().Trim(),
                            OrdenVenta = sqlData[1].ToString().Trim(),
                            Cve_Cliente = sqlData[2].ToString().Trim(),
                            NombreCorto = sqlData[3].ToString().Trim(),
                            NombreLargo = sqlData[4].ToString().Trim()
                        };
                        API.OrdenTrabajo.Add(OT);
                    }
                    sqlData.Close();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.OrdenTrabajo = null;
            }
            return API;
        }

        /// <summary>
        /// Ingresa una nueva auditoria de corte
        /// </summary>
        /// <param name="OT"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCorte/NuevaAuditoriaCorte")]
        public HttpResponseMessage NuevaAuditoriaCorte([FromBody]REQ_NEW_OT OT)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities())
                    {
                        Models.Auditoria auditoria = new Models.Auditoria()
                        {
                            IdClienteRef = OT.IdClienteRef,
                            OrdenTrabajo = OT.OrdenTrabajo,
                            PO = OT.PO,
                            Tela = OT.Tela,
                            Marca = OT.Marca,
                            NumCortada = OT.NumCortada,
                            Lavado = OT.Lavado,
                            Estilo = OT.Estilo,
                            Planta = OT.Planta,
                            Ruta = OT.Ruta,
                            FechaRegistro = DateTime.Now,
                            IdUsuario = OT.IdUsuario,
                            Corte = true,
                            Tendido = false,
                            Confeccion = false,
                            Terminado = false,
                            Lavanderia = false,
                            ProcesosEspeciales = false
                        };
                        db.Auditorias.Add(auditoria);
                        db.SaveChanges();

                        List<DET_AUDITORIA> Detalles = _objSerializer.Deserialize<List<DET_AUDITORIA>>(OT.Det);

                        foreach (DET_AUDITORIA item in Detalles)
                        {
                            Models.Auditoria_Corte_Detalle corte_Detalle = new Models.Auditoria_Corte_Detalle()
                            {
                                IdAuditoriaCorte = auditoria.IdAuditoria,
                                IdCortador = item.IdCortador,
                                Serie = item.Serie,
                                Bulto = item.Bulto,
                                IdPosicion = item.IdPosicion,
                                IdDefecto = item.IdDefecto,
                                IdCortado = item.IdCortado,
                                Cantidad = item.Cantidad,
                                Aud_Imagen = item.Imagen
                            };
                            db.Auditoria_Corte_Detalle.Add(corte_Detalle);
                        }
                        db.SaveChanges();
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Ingresa una nueva auditoria de corte tendido
        /// </summary>
        /// <param name="OT"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCorte/NuevaAuditoriaCorteTendido")]
        public HttpResponseMessage NuevaAuditoriaCorteTendido([FromBody]REQ_NEW_OT OT)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities())
                    {
                        Models.Auditoria auditoria = new Models.Auditoria()
                        {
                            IdClienteRef = OT.IdClienteRef,
                            OrdenTrabajo = OT.OrdenTrabajo,
                            PO = OT.PO,
                            Tela = OT.Tela,
                            Marca = OT.Marca,
                            NumCortada = OT.NumCortada,
                            Lavado = OT.Lavado,
                            Estilo = OT.Estilo,
                            Planta = OT.Planta,
                            Ruta = OT.Ruta,
                            FechaRegistro = DateTime.Now,
                            IdUsuario = OT.IdUsuario,
                            Corte = false,
                            Tendido = true,
                            Lavanderia = false,
                            Terminado = false,
                            Confeccion = false,
                            ProcesosEspeciales = false
                        };
                        db.Auditorias.Add(auditoria);
                        db.SaveChanges();

                        List<DET_AUDITORIA_TENDIDO> Detalles = _objSerializer.Deserialize<List<DET_AUDITORIA_TENDIDO>>(OT.Det);

                        foreach (DET_AUDITORIA_TENDIDO item in Detalles)
                        {
                            Models.Auditoria_Tendido_Detalle tendido_Detalle = new Models.Auditoria_Tendido_Detalle()
                            {
                                IdAuditoriaCorte = auditoria.IdAuditoria,
                                Serie = item.Serie,
                                Bulto = item.Bulto,
                                IdTendido = item.IdTendido,
                                IdTipoTendido = item.IdTipoTendido,
                                IdMesa = item.IdMesa,
                                IdPosicion = item.IdPosicion,
                                IdDefecto = item.IdDefecto,
                                Cantidad = item.Cantidad,
                                Aud_Imagen = item.Imagen
                            };
                            db.Auditoria_Tendido_Detalle.Add(tendido_Detalle);
                        }
                        db.SaveChanges();
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Obtiene los datos generales de la orden de trabajo
        /// </summary>
        /// <param name="OrdenTrabajo"></param>
        /// <param name="CLI_ID"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCorte/ObtieneDatosGeneralesOrdenTrabajo")]
        public RES_OT ObtieneDatosGeneralesOrdenTrabajo(string OrdenTrabajo, string CLI_ID)
        {
            RES_OT API = new RES_OT();
            API.OT = new OT_GRL();
            try
            {
                List<Models.C_ClientesReferencia> CliRef;
                string _cli = "";
                //OBTIENE LOS CLIENTES 
                using (Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities())
                {
                    int _idCliRef = Convert.ToInt32(CLI_ID);
                    CliRef = db.C_ClientesReferencia.Where(x => x.IdClienteRef == _idCliRef).ToList();
                    if (CliRef.Count > 0)
                    {
                        foreach (Models.C_ClientesReferencia item in CliRef)
                        {
                            if (_cli == "")
                                _cli = "'" + item.Cve_Cliente + "'";
                            else
                                _cli += ",'" + item.Cve_Cliente + "'";
                        }
                    }
                }
                using (SqlConnection _Conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["dbRioSulApp"].ToString()))
                {
                    _Conn.Open();
                    string Consulta = "SELECT WOHeader.ProcStage AS EtapaProceso, LTRIM(RTRIM(WOHeader.User9)) AS OrdenVenta, SOHeader.CustID AS Cve_Cliente, Customer.Name AS NombreCorto, XCustomer.LongName AS NombreLargo, WOHeader.InvtID AS NoArticulo, Inventory.Color AS Lavado, WOHeader.User5 AS Planta, RsTb_Plantas.Descr AS Planta_Descripcion, Inventory.ClassID AS Marca, ISNULL(ItemXRef.AlternateID, '') AS No_Alterno, WOHeader.QtyOrig AS Fab_Original, WOHeader.User6 AS PO FROM WOHeader INNER JOIN SOHeader ON WoHeader.User9 = SoHeader.OrdNbr INNER JOIN Customer ON SOHeader.CustID = Customer.CustId INNER JOIN XCustomer ON Customer.CustId = XCustomer.CustId INNER JOIN Inventory ON WOHeader.InvtID = Inventory.InvtID INNER JOIN RsTb_Plantas ON WOHeader.User5 = RsTb_Plantas.Planta LEFT OUTER JOIN ItemXRef ON WOHeader.InvtID = ItemXRef.InvtID WHERE (1 = 1)  AND (SOHeader.CustID IN(" + _cli + ")) AND (LTRIM(RTRIM(WOHeader.User9)) = '" + OrdenTrabajo + "') GROUP BY WOHeader.User9, SOHeader.CustID, WOHeader.ProcStage, Customer.Name, XCustomer.LongName, WOHeader.InvtID, Inventory.Color, WOHeader.User5, RsTb_Plantas.Descr, Inventory.ClassID, ItemXRef.AlternateID, WOHeader.QtyOrig, WOHeader.User6 ORDER BY 1";
                    SqlCommand _Command = new SqlCommand(Consulta, _Conn);
                    SqlDataReader reader = _Command.ExecuteReader();
                    if (reader.Read())
                    {
                        API.OT.EtapaProceso = reader[0].ToString();
                        API.OT.OrdenVenta = reader[1].ToString();
                        API.OT.Cve_Cliente = reader[2].ToString();
                        API.OT.NombreCorto = reader[3].ToString();
                        API.OT.NombreLargo = reader[4].ToString();
                        API.OT.NoArticulo = reader[5].ToString();
                        API.OT.Lavado = reader[6].ToString();
                        API.OT.Planta = reader[7].ToString();
                        API.OT.PlantaDescripcion = reader[8].ToString();
                        API.OT.Marca = reader[9].ToString();
                        API.OT.NoAlterno = reader[10].ToString();
                        API.OT.Fab_Original = reader[11].ToString();
                        API.OT.PO = reader[12].ToString();
                    }
                    reader.Close();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.OT = null;
            }
            return API;
        }

        /// <summary>
        /// Obtiene los bultos de Dynamics para auditoría
        /// </summary>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCorte/ObtieneBultosAuditoria")]
        public void ObtieneBultosAuditoria()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
            }
        }

        /// <summary>
        /// Obtiene las series de Dynamics para auditoría
        /// </summary>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCorte/ObtieneSerieAuditoria")]
        public RES_SERIE ObtieneSerieAuditoria()
        {
            RES_SERIE API = new RES_SERIE();
            API.Serie = new List<SERIE>();
            try
            {
                using (SqlConnection _Conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["dbRioSulApp"].ToString()))
                {
                    _Conn.Open();
                    string Consulta = "SELECT Series FROM RSTB_WO009 GROUP BY Series";
                    SqlCommand _Command = new SqlCommand(Consulta, _Conn);
                    SqlDataReader _reader = _Command.ExecuteReader();
                    while (_reader.Read())
                    {
                        SERIE sERIE = new SERIE()
                        {
                            Series = _reader[0].ToString().Trim()
                        };
                        API.Serie.Add(sERIE);
                    }
                    _reader.Close();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError); API.Serie = null;
            }
            return API;
        }

        /// <summary>
        /// Obtiene auditoría por IdAuditoriaCorte
        /// </summary>
        /// <param name="IdAuditoriaCorte"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCorte/ObtieneAuditoriaCorte")]
        public RES_AUDITORIA ObtieneAuditoriaCorte()
        {
            RES_AUDITORIA API = new RES_AUDITORIA();
            try
            {

                API.RES = db.VST_AUDITORIA.Where(x => x.Corte == true).ToList();
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
        /// Obtiene auditoría de tendido
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCorte/ObtieneAuditoriaTendido")]
        public RES_AUDITORIA ObtieneAuditoriaTendido()
        {
            RES_AUDITORIA API = new RES_AUDITORIA();
            try
            {
                API.RES = db.VST_AUDITORIA.Where(x => x.Tendido == true).ToList();
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
        /// Obtiene auditoría por corte por Id
        /// </summary>
        /// <param name="IdAuditoria"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCorte/ObtieneAuditoriaCortePorId")]
        public RES_AUDITORIA_DET ObtieneAuditoriaCortePorId(int IdAuditoria)
        {
            RES_AUDITORIA_DET API = new RES_AUDITORIA_DET();
            try
            {
                if (ModelState.IsValid)
                {
                    API.RES = db.VST_AUDITORIA.Where(x => x.IdAuditoria == IdAuditoria).FirstOrDefault();
                    API.RES_DET = db.VST_AUDITORIA_CORTE_DETALLE.Where(x => x.IdAuditoriaCorte == IdAuditoria).ToList();
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.RES = null; API.RES_DET = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                API.RES = null; API.RES_DET = null;
            }
            return API;
        }

        /// <summary>
        /// Genera el cierre de auditoria de corte
        /// </summary>
        /// <param name="IdAuditoria"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCorte/CierreAuditoria")]
        public HttpResponseMessage CierreAuditoria(int IdAuditoria)
        {
            try
            {
                Models.Auditoria API = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria && x.Corte == true).FirstOrDefault();
                API.FechaRegistroFin = DateTime.Now;
                db.Entry(API).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Genera el cierre de auditoria de corte tendido
        /// </summary>
        /// <param name="IdAuditoria"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCorte/CierreAuditoriaTendido")]
        public HttpResponseMessage CierreAuditoriaTendido(int IdAuditoria)
        {
            try
            {
                Models.Auditoria API = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria && x.Tendido == true).FirstOrDefault();
                API.FechaRegistroFin = DateTime.Now;
                db.Entry(API).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public partial class RES_OT_CLI
        {
            public List<OT_CLI> OrdenTrabajo { get; set; }
            public HttpResponseMessage Message { get; set; }
        }
        public partial class OT_CLI
        {
            public string EtapaProceso { get; set; }
            public string OrdenVenta { get; set; }
            public string Cve_Cliente { get; set; }
            public string NombreCorto { get; set; }
            public string NombreLargo { get; set; }
        }
        public partial class RES_OT
        {
            public HttpResponseMessage Message { get; set; }
            public OT_GRL OT { get; set; }
        }
        public partial class OT_GRL
        {
            public string EtapaProceso { get; set; }
            public string OrdenVenta { get; set; }
            public string Cve_Cliente { get; set; }
            public string NombreCorto { get; set; }
            public string NombreLargo { get; set; }
            public string NoArticulo { get; set; }
            public string Lavado { get; set; }
            public string Planta { get; set; }
            public string PlantaDescripcion { get; set; }
            public string Marca { get; set; }
            public string NoAlterno { get; set; }
            public string Fab_Original { get; set; }
            public string PO { get; set; }
        }
        public partial class REQ_NEW_OT
        {
            [Required]
            public int IdClienteRef { get; set; }
            [Required]
            public string OrdenTrabajo { get; set; }
            public string PO { get; set; }
            public string Tela { get; set; }
            public string Marca { get; set; }
            public string NumCortada { get; set; }
            public string Lavado { get; set; }
            public string Estilo { get; set; }
            public string Planta { get; set; }
            public string Ruta { get; set; }
            [Required]
            public int IdUsuario { get; set; }
            [Required]
            public string Det { get; set; }
        }
        public partial class DET_AUDITORIA
        {
            [Required]
            public int IdCortador { get; set; }
            [Required]
            public string Serie { get; set; }
            [Required]
            public string Bulto { get; set; }
            [Required]
            public int IdPosicion { get; set; }
            [Required]
            public int IdDefecto { get; set; }
            [Required]
            public int IdCortado { get; set; }
            [Required]
            public int Cantidad { get; set; }
            public string Imagen { get; set; }
        }
        public partial class DET_AUDITORIA_TENDIDO
        {
            [Required]
            public string Serie { get; set; }
            [Required]
            public string Bulto { get; set; }
            [Required]
            public int IdTendido { get; set; }
            [Required]
            public int IdTipoTendido { get; set; }
            [Required]
            public int IdMesa { get; set; }
            [Required]
            public int IdPosicion { get; set; }
            [Required]
            public int IdDefecto { get; set; }
            [Required]
            public int Cantidad { get; set; }
            public string Imagen { get; set; }
        }
        public partial class RES_SERIE
        {
            public HttpResponseMessage Message { get; set; }
            public List<SERIE> Serie { get; set; }
        }
        public partial class SERIE
        {
            public string Series { get; set; }
        }
        public partial class RES_AUDITORIA
        {
            public HttpResponseMessage Message { get; set; }
            public List<Models.VST_AUDITORIA> RES { get; set; }
        }
        public partial class RES_AUDITORIA_DET
        {
            public HttpResponseMessage Message { get; set; }
            public Models.VST_AUDITORIA RES { get; set; }
            public List<Models.VST_AUDITORIA_CORTE_DETALLE> RES_DET { get; set; }
        }
    }
}
