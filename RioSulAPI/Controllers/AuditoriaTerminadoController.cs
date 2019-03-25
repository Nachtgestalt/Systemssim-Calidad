using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Policy;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using Microsoft.Ajax.Utilities;
using RioSulAPI.Class;
using RioSulAPI.Models;

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
							ProcesosEspeciales = false,
							Calidad = false,
							Activo = true
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
								Cantidad = item.cantidad,
								Aud_Imagen = item.Imagen,
								Nota = item.Nota,
								Archivo = item.Archivo
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
		public RES_AUDITORIA_T_DET ObtieneAuditoriaDet(int id, string tipo = "")
		{
			RES_AUDITORIA_T_DET API = new RES_AUDITORIA_T_DET();

			try
			{
				API.RES = db.VST_AUDITORIA.Where(x => x.IdAuditoria == id).FirstOrDefault();
				var aux = db.VST_AUDITORIA_TERMINADO_DETALLE.Where(x => x.IdAuditoria == id);

				switch (tipo)
				{
					case "Compostura":
						aux = aux.Where(x => x.Compostura == true);
						break;
					case "Segundas":
						aux = aux.Where(x => x.Compostura == false);
						break;
					default:
						break;
				}
				
				API.RES_DET = aux.ToList();
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
				Boolean notas = false;
				Models.Auditoria API = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria && x.Terminado == true).FirstOrDefault();
				API.FechaRegistroFin = DateTime.Now;
				db.Entry(API).State = System.Data.Entity.EntityState.Modified;
				db.SaveChanges();

				List<Models.VST_CORREOS_AUDITORIA> correos = db.VST_CORREOS_AUDITORIA.Where(x => x.Calidad == true).ToList();
				List<Models.Auditoria_Terminado_Detalle> auditoria_det = db.Auditoria_Terminado_Detalle.Where(x => x.IdAuditoria == IdAuditoria).ToList();

				MailMessage mensaje = new MailMessage();

				mensaje.From = new MailAddress(System.Configuration.ConfigurationManager.AppSettings["Mail"].ToString());
				var password = System.Configuration.ConfigurationManager.AppSettings["Password"].ToString();

				foreach (VST_CORREOS_AUDITORIA item in correos)
				{
					mensaje.To.Add(item.Email);
				}

				var sub = "AUDITORÍA OT: " + API.OrdenTrabajo.ToUpper();
				var body = "Se ha cerrado la auditoría de la orden de trabajo con número de corte: " + API.NumCortada.ToUpper() + " en el área del terminado.";

				foreach (Auditoria_Terminado_Detalle item in auditoria_det)
				{

					if (item.Nota != "null" || item.Nota != "")
					{
						notas = true;
					}
				}

				if (notas)
				{
					body = body + " \n La Auditoría contiene NOTAS favor de revisar";
				}

				var smtp = new SmtpClient
				{
					Host = System.Configuration.ConfigurationManager.AppSettings["Host"].ToString(),
					Port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Port"].ToString()),
					EnableSsl = false,
					DeliveryMethod = SmtpDeliveryMethod.Network,
					UseDefaultCredentials = false,
					Credentials = new NetworkCredential(mensaje.From.Address, password)
				};
				using (var mess = new MailMessage(mensaje.From.Address, mensaje.To.ToString())
				{
					Subject = sub,
					Body = body
				})
				{
					smtp.Send(mess);
				}

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
					string Consulta = @"SELECT        distinct(WH.WONbr)
									FROM ItemXRef AS IXR RIGHT OUTER JOIN
									WOHeader AS WH INNER JOIN
									SOHeader INNER JOIN
									RsTb_SeriesDtl AS RSD ON SOHeader.OrdNbr = RSD.User1 INNER JOIN
									Customer AS CM ON SOHeader.CustID = CM.CustId ON WH.WONbr = RSD.WoNbr LEFT OUTER JOIN
									Inventory AS IV ON WH.InvtID = IV.InvtID LEFT OUTER JOIN
									InventoryADG AS IADG ON IV.InvtID = IADG.InvtID LEFT OUTER JOIN
									WOBuildTo AS WOB ON WOB.InvtID = IV.InvtID AND UPPER(IV.ClassID) = 'TEMEZ' LEFT OUTER JOIN
									RsTb_Plantas AS RSP ON WH.User5 = RSP.Planta ON IXR.InvtID = WH.InvtID    
									WHERE (WH.Status = 'A') AND (WH.ProcStage = 'R');";
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
				//DETALLE DE LA OT
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
									ISNULL(CM.CustId, 0) AS CLIENT_ID
FROM            ItemXRef AS IXR RIGHT OUTER JOIN
						 WOHeader AS WH INNER JOIN
						 SOHeader INNER JOIN
						 RsTb_SeriesDtl AS RSD ON SOHeader.OrdNbr = RSD.User1 INNER JOIN
						 Customer AS CM ON SOHeader.CustID = CM.CustId ON WH.WONbr = RSD.WoNbr LEFT OUTER JOIN
						 Inventory AS IV ON WH.InvtID = IV.InvtID LEFT OUTER JOIN
						 InventoryADG AS IADG ON IV.InvtID = IADG.InvtID LEFT OUTER JOIN
						 WOBuildTo AS WOB ON WOB.InvtID = IV.InvtID AND UPPER(IV.ClassID) = 'TEMEZ' LEFT OUTER JOIN
						 RsTb_Plantas AS RSP ON WH.User5 = RSP.Planta ON IXR.InvtID = WH.InvtID
						WHERE (WH.Status = 'A') AND (WH.ProcStage = 'R') and WH.WONbr = '" + OT +"'; ";
					SqlCommand Command = new SqlCommand(Consulta, _Conn);
					SqlDataReader reader = Command.ExecuteReader();
					if (reader.Read())
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
						API.OT.ID_Cliente = reader[14].ToString().Trim();
					}
					reader.Close();
					API.Message = new HttpResponseMessage(HttpStatusCode.OK);
				}
				//OBTENEMOS EL ID REF DEL CLIENTE
				using (Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities())
				{
					Models.C_ClientesReferencia CliRef;
					Models.C_Clientes Cliente;
					CliRef = db.C_ClientesReferencia.Where(x => x.Cve_Cliente == API.OT.ID_Cliente).FirstOrDefault();

					if (CliRef == null)
					{
						API.Message = new HttpResponseMessage(HttpStatusCode.Conflict);
						API.Message2 = "Cliente inexistente en el catálogo";
						API.OT = null;
					}
					else
					{
						Cliente = db.C_Clientes.Where(x => x.IdClienteRef == CliRef.IdClienteRef).FirstOrDefault();
						API.OT.ID_Cliente = CliRef.IdClienteRef.ToString();
						API.OT.Cliente = Cliente.Descripcion;
						API.Message = new HttpResponseMessage(HttpStatusCode.Accepted);
					}

					
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
		/// ACTUALIZAMOS EL DETALLE DE LA AUDITORIA
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpPut]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaTerminado/ActualizaAuditoriaDet")]
		public MESSAGE ActualizaAuditoria([FromBody]ACT_DET_AUDITORIA_T AT)
		{
			MESSAGE API = new MESSAGE();

			try
			{
				if (ModelState.IsValid)
				{
					List<Models.Auditoria_Terminado_Detalle> ATD = db.Auditoria_Terminado_Detalle.Where(x => x.IdAuditoria == AT.IdAuditoria).ToList();

					foreach (Models.Auditoria_Terminado_Detalle item in ATD)
					{
						db.Auditoria_Terminado_Detalle.Remove(item);
						db.SaveChanges();
					}
					
					foreach (DET_AUDITORIA_TERMINADO item in AT.Det)
					{
						Models.Auditoria_Terminado_Detalle auditoria_Terminado = new Models.Auditoria_Terminado_Detalle()
						{
							IdAuditoria = AT.IdAuditoria,
							IdOrigen = item.IdOrigen,
							IdPosicion = item.IdPosicion,
							IdOperacion = item.IdOperacion,
							IdDefecto = item.IdDefecto,
							Revisado = item.Revisado,
							Compostura = item.Compostura,
							Cantidad = item.cantidad,
							Aud_Imagen = item.Imagen,
							Nota = item.Nota,
							Archivo = item.Archivo
						};
						db.Auditoria_Terminado_Detalle.Add(auditoria_Terminado);
					}
					db.SaveChanges();

					API.Message = "Auditoria modificada correctamente";
					API.Response = new HttpResponseMessage(HttpStatusCode.OK);
				}
				else
				{
					API.Message = "Formato inválido";
					API.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);
				}
			}
			catch (Exception e)
			{
				API.Message = e.Message;
				API.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);
			}

			return API;
		}

		/// <summary>
		/// ELIMINAMOS EL DETALLE DE LA AUDITORIA
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpDelete]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaTerminado/ActualizaAuditoriaDet")]
		public MESSAGE EliminaAuditoriaD(int IdAuditoriaDet)
		{
			MESSAGE API = new MESSAGE();

			try
			{
				Models.Auditoria_Terminado_Detalle ATD = db.Auditoria_Terminado_Detalle.Where(x => x.IdAuditoriaTerminadoDetalle == IdAuditoriaDet).FirstOrDefault();
				if (ATD != null)
				{
					db.Auditoria_Terminado_Detalle.Remove(ATD);
					db.SaveChanges();
					API.Message = "Elemento eliminado con éxito";
					API.Response = new HttpResponseMessage(HttpStatusCode.OK);
				}
				else
				{
					API.Message = "No se encontro el detalle solicitado";
					API.Response = new HttpResponseMessage(HttpStatusCode.Conflict);
				}
			}
			catch (Exception e)
			{
				API.Message = e.Message;
				API.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);
			}

			return API;
		}

		/// <summary>
		/// ACTIVAMOS O DESACTIVAMOS LA AUDITORIA
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpPut]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaTerminado/EliminaAuditoria")]
		public AuditoriaTerminadoController.MESSAGE ActivaAuditoria(int IdAuditoria = 0)
		{
			AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

			try
			{
				Models.Auditoria AUD = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria).FirstOrDefault();
				AUD.Activo = (AUD.Activo == false ? true : false);

				db.Entry(AUD).State = System.Data.Entity.EntityState.Modified;
				db.SaveChanges();

				API.Message = "Auditoria modificada con éxito";
				API.Response = new HttpResponseMessage(HttpStatusCode.OK);
			}
			catch (Exception e)
			{
				API.Message = e.Message;
				API.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}

			return API;
		}

		/// <summary>
		/// ELIMINAMOS LA AUDITORIA
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpDelete]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaTerminado/EliminaAuditoria")]
		public AuditoriaTerminadoController.MESSAGE EliminaAuditoria(int IdAuditoria = 0)
		{
			AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

			try
			{
				Models.Auditoria AUD = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria).FirstOrDefault();

				db.Auditorias.Remove(AUD);
				db.SaveChanges();

				List<Models.Auditoria_Terminado_Detalle> AD = db.Auditoria_Terminado_Detalle
					.Where(x => x.IdAuditoria == IdAuditoria).ToList();

				db.Auditoria_Terminado_Detalle.RemoveRange(AD);
				db.SaveChanges();

				API.Message = "Auditoria eliminada con éxito";
				API.Response = new HttpResponseMessage(HttpStatusCode.OK);
			}
			catch (Exception e)
			{
				API.Message = e.Message;
				API.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
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
			public string Imagen { get; set; }
			public string Nota { get; set; }
			public string Archivo { get; set; }
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
			public string ID_Cliente { get; set; }
		}

		public partial class RES_OT_DET
		{
			public OT_DET OT { get; set; }
			public string Message2 { get; set; }
			public HttpResponseMessage Message { get; set; }
		}

		public partial class RES_OT_GEN
		{
			public HttpResponseMessage Message { get; set; }
			public List<OT_GEN> OrdenTrabajo { get; set; }
		}

		public partial class MESSAGE
		{
			public string Message { get; set; }
			public HttpResponseMessage Response { get; set; }
		}

		public partial class ACT_DET_AUDITORIA_T
		{
			[Required]
			public int IdAuditoria { get; set; }

			[Required]
			public List<DET_AUDITORIA_TERMINADO> Det { get; set; }
		}

	}
}
