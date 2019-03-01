using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using RioSulAPI.Class;

namespace RioSulAPI.Controllers
{
	public class AuditoriaTerminadoController : ApiController
	{
		private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();
		private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

		/// <summary>
		/// Registra una nueva auditoría de lavandería
		/// </summary>
		/// <param name="OT"></param>
		/// <returns></returns>
		[HttpPost]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaTerminado/NuevaAuditoriaTerminado")]
		public HttpResponseMessage NuevaAuditoriaTerminado([FromBody]AuditoriaTerminadoController.REQ_NEW_AT OT)
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
							Tendido = false,
							Lavanderia = false,
							Terminado = true,
							Confeccion = false,
							ProcesosEspeciales = false
						};
						db.Auditorias.Add(auditoria);
						db.SaveChanges();

						foreach (DET_AUDITORIA_TERMINADO item in OT.Det)
						{
							Models.Auditoria_Terminado_Detalle auditoria_Terminado = new Models.Auditoria_Terminado_Detalle()
							{
								IdAuditoria = auditoria.IdAuditoria,
								IdOrigen = item.IdOrigen,
								IdPosicion = item.IdPosicion,
								IdOperacion = item.IdOperacion,
								IdDefecto = item.IdDefecto,
								Revisado = item.Revisado,
								Compostura = item.Compostura,
								Cantidad = item.cantidad
							};
							db.Auditoria_Terminado_Detalle.Add(auditoria_Terminado);
						}
						db.SaveChanges();
						return new HttpResponseMessage(HttpStatusCode.OK);
					}
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
		/// Obtiene auditoría de Lavandería
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaTerminado/ObtieneAuditoriaTerminado")]
		public AuditoriaCorteController.RES_AUDITORIA ObtieneAuditoriaTerminado()
		{
			AuditoriaCorteController.RES_AUDITORIA API = new AuditoriaCorteController.RES_AUDITORIA();
			try
			{
				API.RES = db.VST_AUDITORIA.Where(x => x.Terminado == true).ToList();
				API.Message = new HttpResponseMessage(HttpStatusCode.OK);
			}
			catch (Exception ex)
			{
				Utilerias.EscribirLog(ex.ToString());
				API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
				API.RES = null;
			}
			return API;
		}

		/// <summary>
		/// Obtenemos el detalle de la auditoria dependiendo el ID enviado
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaTerminado/ObtieneAuditoriaDet")]
		public RES_AUDITORIA_T_DET ObtieneAuditoriaDet(int id)
		{
			RES_AUDITORIA_T_DET API = new RES_AUDITORIA_T_DET();

			try
			{
				API.RES = db.VST_AUDITORIA.Where(x => x.IdAuditoria == id).FirstOrDefault();
				API.RES_DET = db.VST_AUDITORIA_TERMINADO_DETALLE.Where(x => x.IdAuditoria == id).ToList();
				API.Message = new HttpResponseMessage(HttpStatusCode.OK);
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
		/// Genera el cierre de auditoria de terminado
		/// </summary>
		/// <param name="IdAuditoria"></param>
		/// <returns></returns>
		[HttpGet]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaTerminado/CierreAuditoria")]
		public HttpResponseMessage CierreAuditoria(int IdAuditoria)
		{
			try
			{
				Models.Auditoria API = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria && x.Terminado == true).FirstOrDefault();
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
		/// Obtenemos todas las OT activas
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaTerminado/ObtenemosOT")]
		public RES_OT_GEN ObtenemosOT()
		{
			RES_OT_GEN API = new RES_OT_GEN();
			try
			{
				using (SqlConnection _Conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["dbRioSulApp"].ToString()))
				{
					_Conn.Open();
					string Consulta = " SELECT WONbr FROM WOHeader where Status = 'P' ";
					API.OrdenTrabajo = new List<OT_GEN>();
					SqlCommand Command = new SqlCommand(Consulta, _Conn);
					SqlDataReader sqlData = Command.ExecuteReader();
					while (sqlData.Read())
					{
						OT_GEN OT = new OT_GEN()
						{
							OT = sqlData[0].ToString().Trim()
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
        /// Obtenemos el detalle de la OT
        /// </summary>
        /// <param name="OT"></param>
        /// <returns></returns>
		[HttpGet]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaTerminado/ObtenemosOT_D")]
		public RES_OT_DET ObtenemosOT_D(string OT)
		{
			RES_OT_DET API = new RES_OT_DET();
            API.OT = new OT_DET();
            
			try
			{
				using (SqlConnection _Conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["dbRioSulApp"].ToString()))
				{
					_Conn.Open();
					string Consulta = @" SELECT
								ISNULL(CM.Name, '') AS CLIENTE,
								IV.Size AS LINEA,
								IV.Color AS LAVADO,
								WH.User5 AS PLANTA,
								IADG.Style AS DIVISION,
								ISNULL(WOB.InvtID, '') AS TELA,
								IV.ClassID AS MARCA,
								WH.CustID AS ESTILO,
								WH.InvtID AS TELA_PROV,
								WH.QtyOrig AS NUMERO_CORTADA,
								IV.Descr AS MODELO,
								IV.Descr AS DESCRIPCION,	                            
								WH.User6 AS PO,
								IV.User2 AS RUTA,
								ISNULL(CM.CustId, 0) as CLIENT_ID
								FROM WOHeader AS WH
								LEFT JOIN Customer AS CM
								ON WH.CustID = CM.CustId
								LEFT JOIN Inventory AS IV
								ON WH.InvtID = IV.InvtID
								LEFT JOIN InventoryADG AS IADG
								ON IV.InvtID = IADG.InvtID
								LEFT JOIN WOBuildTo AS WOB
								ON WOB.InvtID = IV.InvtID AND UPPER(IV.ClassID) = 'TEMEZ'
								LEFT JOIN RsTb_Plantas RSP
								ON WH.User5 = RSP.Planta
								LEFT JOIN ItemXRef IXR ON WH.InvtID = IXR.InvtID
								LEFT JOIN rstb_seriesdtl AS RSD ON RSD.WoNbr = WH.WONbr
								where WH.WONbr = '"+ OT +"'; ";
					SqlCommand Command = new SqlCommand(Consulta, _Conn);
					SqlDataReader reader = Command.ExecuteReader();
                    while (reader.Read())
                    {
                        API.OT.Cliente = reader[0].ToString().Trim();
                        API.OT.Linea = reader[1].ToString().Trim();
                        API.OT.Lavado = reader[2].ToString().Trim();
                        API.OT.Planta = reader[3].ToString().Trim();
                        API.OT.Division = reader[4].ToString().Trim();
                        API.OT.Tela_Int = reader[5].ToString().Trim();
                        API.OT.Marca = reader[6].ToString().Trim();
                        API.OT.Estilo = reader[7].ToString().Trim();
                        API.OT.Tela_Prov = reader[8].ToString().Trim();
                        API.OT.No_Cortada = reader[9].ToString().Trim();
                        API.OT.Modelo = reader[10].ToString().Trim();
                        API.OT.Descripcion = reader[11].ToString().Trim();
                        API.OT.PO = reader[12].ToString().Trim();
                        API.OT.Ruta = reader[13].ToString().Trim();
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


		public partial class DET_AUDITORIA_TERMINADO
		{
			[Required]
			public int IdPosicion { get; set; }
			[Required]
			public int IdOperacion { get; set; }
			[Required]
			public int IdOrigen { get; set; }
			[Required]
			public int IdDefecto { get; set; }
			public bool Revisado { get; set; }
			public bool Compostura { get; set; }
			public int cantidad { get; set; }
		}

		public partial class REQ_NEW_AT
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
			public List<DET_AUDITORIA_TERMINADO> Det { get; set; }
		}

		public partial class RES_AUDITORIA_T_DET
		{
			public HttpResponseMessage Message { get; set; }
			public Models.VST_AUDITORIA RES { get; set; }
			public List<Models.VST_AUDITORIA_TERMINADO_DETALLE> RES_DET { get; set; }
		}

		public partial class OT_GEN
		{
			public string OT { get; set; }
		}

		public partial class OT_DET
		{
			public string Cliente { get; set; }
			public string Linea { get; set; }
			public string Lavado { get; set; }
			public string Planta { get; set; }
			public string Division { get; set; }
			public string Tela_Int { get; set; }
			public string Marca { get; set; }
			public string Estilo { get; set; }
			public string Tela_Prov { get; set; }
			public string No_Cortada { get; set; }
			public string Modelo { get; set; }
			public string Descripcion { get; set; }
			public string PO { get; set; }
			public string Ruta { get; set; }
		}

		public partial class RES_OT_DET
		{
            public OT_DET OT { get; set; }
            public HttpResponseMessage Message { get; set; }
		}

		public partial class RES_OT_GEN
		{
			public HttpResponseMessage Message { get; set; }
			public List<OT_GEN> OrdenTrabajo { get; set; }
		}
	}
}
