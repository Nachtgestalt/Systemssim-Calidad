using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using System.Web.WebPages;
using RioSulAPI.Class;
using RioSulAPI.Models;

namespace RioSulAPI.Controllers
{
	public class AuditoriaCorteController : ApiController
	{
		private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();
		private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

		#region OTROS_DATOS
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
			int _idCliRef = Convert.ToInt32(IdClienteRef);
			try
			{
				List<Models.C_ClientesReferencia> CliRef;
				string _cli = "";
				//OBTIENE LOS CLIENTES 
				using (Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities())
				{
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

				using (SqlConnection _Conn = new SqlConnection(System.Configuration.ConfigurationManager
					.ConnectionStrings["dbRioSulApp"].ToString()))
				{
					_Conn.Open();
					string Consulta =
						"SELECT WOHeader.ProcStage AS EtapaProceso, LTRIM(RTRIM(WOHeader.User9)) AS OrdenVenta, SOHeader.CustID AS Cve_Cliente, Customer.Name AS NombreCorto, XCustomer.LongName AS NombreLargo, WOHeader.InvtID AS NoArticulo, Inventory.Color AS Lavado, WOHeader.User5 AS Planta, RsTb_Plantas.Descr AS Planta_Descripcion, Inventory.ClassID AS Marca, ISNULL(ItemXRef.AlternateID, '') AS No_Alterno, WOHeader.QtyOrig AS Fab_Original, WOHeader.User6 AS PO FROM WOHeader INNER JOIN SOHeader ON WoHeader.User9 = SoHeader.OrdNbr INNER JOIN Customer ON SOHeader.CustID = Customer.CustId INNER JOIN XCustomer ON Customer.CustId = XCustomer.CustId INNER JOIN Inventory ON WOHeader.InvtID = Inventory.InvtID INNER JOIN RsTb_Plantas ON WOHeader.User5 = RsTb_Plantas.Planta LEFT OUTER JOIN ItemXRef ON WOHeader.InvtID = ItemXRef.InvtID WHERE (1 = 1) AND (SOHeader.CustID IN(" +
						_idCliRef + ")) AND (LTRIM(RTRIM(WOHeader.User9)) = '" + OrdenT +
						"') GROUP BY WOHeader.User9, SOHeader.CustID, WOHeader.ProcStage, Customer.Name, XCustomer.LongName, WOHeader.InvtID, Inventory.Color, WOHeader.User5, RsTb_Plantas.Descr, Inventory.ClassID, ItemXRef.AlternateID, WOHeader.QtyOrig, WOHeader.User6 ORDER BY 1 ";
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
		/// Obtiene los datos generales de la orden de trabajo
		/// </summary>
		/// <param name="OrdenTrabajo"></param>
		/// <param name="CLI_ID"></param>
		/// <returns></returns>
		[HttpGet]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaCorte/ObtieneDatosGeneralesOrdenTrabajo")]
		public RES_OT_DET ObtieneDatosGeneralesOrdenTrabajo(string OrdenTrabajo)
		{
			RES_OT_DET API = new RES_OT_DET();
			API.OT = new OT_DET();

			try
			{
				using (SqlConnection _Conn = new SqlConnection(System.Configuration.ConfigurationManager
					.ConnectionStrings["dbRioSulApp"].ToString()))
				{
					_Conn.Open();
					//string Consulta = "SELECT WOHeader.ProcStage AS EtapaProceso, LTRIM(RTRIM(WOHeader.User9)) AS OrdenVenta, SOHeader.CustID AS Cve_Cliente, Customer.Name AS NombreCorto, XCustomer.LongName AS NombreLargo, WOHeader.InvtID AS NoArticulo, Inventory.Color AS Lavado, WOHeader.User5 AS Planta, RsTb_Plantas.Descr AS Planta_Descripcion, Inventory.ClassID AS Marca, ISNULL(ItemXRef.AlternateID, '') AS No_Alterno, WOHeader.QtyOrig AS Fab_Original, WOHeader.User6 AS PO FROM WOHeader INNER JOIN SOHeader ON WoHeader.User9 = SoHeader.OrdNbr INNER JOIN Customer ON SOHeader.CustID = Customer.CustId INNER JOIN XCustomer ON Customer.CustId = XCustomer.CustId INNER JOIN Inventory ON WOHeader.InvtID = Inventory.InvtID INNER JOIN RsTb_Plantas ON WOHeader.User5 = RsTb_Plantas.Planta LEFT OUTER JOIN ItemXRef ON WOHeader.InvtID = ItemXRef.InvtID WHERE (1 = 1)  AND (SOHeader.CustID IN(" + _cli + ")) AND (LTRIM(RTRIM(WOHeader.User9)) = '" + OrdenTrabajo + "') GROUP BY WOHeader.User9, SOHeader.CustID, WOHeader.ProcStage, Customer.Name, XCustomer.LongName, WOHeader.InvtID, Inventory.Color, WOHeader.User5, RsTb_Plantas.Descr, Inventory.ClassID, ItemXRef.AlternateID, WOHeader.QtyOrig, WOHeader.User6 ORDER BY 1";
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
						WHERE (WH.Status = 'A') AND (WH.ProcStage = 'R') and WH.WONbr = '" + OrdenTrabajo + "'; ";

					SqlCommand _Command = new SqlCommand(Consulta, _Conn);
					SqlDataReader reader = _Command.ExecuteReader();
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
		/// Obtiene los bultos de Dynamics para auditoría
		/// </summary>
		[HttpGet]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaCorte/ObtieneBultosAuditoria")]
		public RES_BULTO ObtieneBultosAuditoria(string OT = "", string Series = "")
		{
			RES_BULTO API = new RES_BULTO();
			API.Bulto = new List<BULTO>();
			try
			{
				using (SqlConnection _Conn = new SqlConnection(System.Configuration.ConfigurationManager
					.ConnectionStrings["dbRioSulApp"].ToString()))
				{
					_Conn.Open();
					string Consulta = "SELECT distinct(Series),WoNbr,Qty,User4 From RsTb_SeriesDtl where Qty >0 and WoNbr = '" + OT + "' and Series = '" + Series + "' ORDER BY Series, Qty;";
					SqlCommand _Command = new SqlCommand(Consulta, _Conn);
					SqlDataReader _reader = _Command.ExecuteReader();
					while (_reader.Read())
					{
						BULTO B = new BULTO()
						{
							Bulto = Convert.ToInt32(_reader[2].ToString()) / Convert.ToInt32(_reader[3].ToString())
						};
						API.Bulto.Add(B);
					}

					_reader.Close();
					API.Message = new HttpResponseMessage(HttpStatusCode.OK);

				}
			}
			catch (Exception ex)
			{
				Utilerias.EscribirLog(ex.ToString());
			}

			return API;
		}

		/// <summary>
		/// Obtiene las series de Dynamics para auditoría
		/// </summary>
		[HttpGet]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaCorte/ObtieneSerieAuditoria")]
		public RES_SERIE ObtieneSerieAuditoria(string OT = "")
		{
			RES_SERIE API = new RES_SERIE();
			API.Serie = new List<SERIE>();
			try
			{
				using (SqlConnection _Conn = new SqlConnection(System.Configuration.ConfigurationManager
					.ConnectionStrings["dbRioSulApp"].ToString()))
				{
					_Conn.Open();
					string Consulta = "SELECT distinct(Series) From RsTb_SeriesDtl where Qty >0 and WoNbr = '" + OT + "' ORDER BY Series;";
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
				API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
				API.Serie = null;
			}

			return API;
		}
		#endregion

		#region AUDITORIA_CORTE
		/// <summary>
		/// Ingresa una nueva auditoria de corte
		/// </summary>
		/// <param name="OT"></param>
		/// <returns></returns>
		[HttpPost]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaCorte/NuevaAuditoriaCorte")]
		public HttpResponseMessage NuevaAuditoriaCorte([FromBody] REQ_NEW_OT_T OT)
		{
			string image_name = "";
			string pdf = "";
			int num_detalle = 0;
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
							ProcesosEspeciales = false,
							Calidad = false,
                            Activo = true
						};
						db.Auditorias.Add(auditoria);
						db.SaveChanges();

						foreach (DET_AUDITORIA_TENDIDO item in OT.Det)
						{
							num_detalle = num_detalle + 1;
							image_name = "";
							pdf = "";
							if (item.Imagen != null && !item.Imagen.IsEmpty())
							{
								string base64 = item.Imagen.Substring(item.Imagen.IndexOf(',') + 1);
								byte[] data = Convert.FromBase64String(base64);

								image_name = "Auditoria_Corte_" + auditoria.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

								using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
								{
									image_file.Write(data, 0, data.Length);
									image_file.Flush();
								}
							}
							if (item.Archivo != null && !item.Archivo.IsEmpty())
							{
								string base64 = item.Archivo.Substring(item.Archivo.IndexOf(',') + 1);
								byte[] data = Convert.FromBase64String(base64);

								pdf = "Auditoria_Corte_" + auditoria.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

								using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Archivos/") + pdf + ".pdf", FileMode.Create))
								{
									image_file.Write(data, 0, data.Length);
									image_file.Flush();
								}
							}
							Models.Auditoria_Corte_Detalle corte_Detalle = new Models.Auditoria_Corte_Detalle()
							{
								IdAuditoriaCorte = auditoria.IdAuditoria,
								Serie = item.Serie,
								Bulto = item.Bulto,
								IdTendido = item.IdTendido,
								IdMesa = item.IdMesa,   
								IdPosicion = item.IdPosicion,
								IdDefecto = item.IdDefecto,
								Cantidad = item.Cantidad,
								Aud_Imagen = image_name,
								Nota =  item.Nota,
								Archivo = pdf,
                                Segundas = item.Segundas
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
			API.RES_DET = new List<Models.VST_AUDITORIA_CORTE_DETALLE>();
			List<Models.VST_AUDITORIA_CORTE_DETALLE> corte = new List<Models.VST_AUDITORIA_CORTE_DETALLE>();
			string file_path = "";

			try
			{
				if (ModelState.IsValid)
				{
					API.RES = db.VST_AUDITORIA.Where(x => x.IdAuditoria == IdAuditoria).FirstOrDefault();
					corte = db.VST_AUDITORIA_CORTE_DETALLE.Where(x => x.IdAuditoriaCorte == IdAuditoria).ToList();

					foreach (Models.VST_AUDITORIA_CORTE_DETALLE item in corte)
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

						file_path = HttpContext.Current.Server.MapPath("~/Archivos/");
						file_path = file_path + item.Archivo + ".pdf";
						if (File.Exists(file_path))
						{
							item.Archivo = "data:application/" + "pdf" + ";base64," + Convert.ToBase64String(File.ReadAllBytes(file_path));
						}
						else
						{
							item.Archivo = "";
						}
						API.RES_DET.Add(item);
					}

					API.Message = new HttpResponseMessage(HttpStatusCode.OK);
				}
				else
				{
					API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
					API.RES = null;
					API.RES_DET = null;
				}
			}
			catch (Exception ex)
			{
				Utilerias.EscribirLog(ex.ToString());
				API.Message = new HttpResponseMessage(HttpStatusCode.OK);
				API.RES = null;
				API.RES_DET = null;
			}

			return API;
		}

		/// <summary>
		/// Genera el cierre de auditoria de corte
		/// </summary>
		/// <param name="IdAuditoria"></param>
		/// <returns></returns>
		[HttpPut]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaCorte/CierreAuditoria")]
		public HttpResponseMessage CierreAuditoria(int IdAuditoria)
		{
			try
			{
				Boolean notas = false;
				Models.Auditoria API = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria && x.Corte == true)
					.FirstOrDefault();
				API.FechaRegistroFin = DateTime.Now;
				db.Entry(API).State = System.Data.Entity.EntityState.Modified;
				db.SaveChanges();

				List<Models.VST_CORREOS_AUDITORIA> correos = db.VST_CORREOS_AUDITORIA.Where(x => x.Corte == true).ToList();
				List<Models.Auditoria_Corte_Detalle> auditoria_det = db.Auditoria_Corte_Detalle.Where(x => x.IdAuditoriaCorte == IdAuditoria).ToList();

				if (correos.Count > 0)
				{
					MailMessage mensaje = new MailMessage();

					mensaje.From = new MailAddress(System.Configuration.ConfigurationManager.AppSettings["Mail"].ToString());
					var password = System.Configuration.ConfigurationManager.AppSettings["Password"].ToString();

					foreach (VST_CORREOS_AUDITORIA item in correos)
					{
						mensaje.To.Add(item.Email);
					}

					var sub = "AUDITORÍA OT: " + API.OrdenTrabajo.ToUpper();
					var body = "Se ha cerrado la auditoría de la orden de trabajo con número de corte: " + API.NumCortada.ToUpper() + " en el área de corte.";

					foreach (Auditoria_Corte_Detalle item in auditoria_det)
					{
						if (item.Nota != "null" && !item.Nota.IsEmpty())
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
				}

				return new HttpResponseMessage(HttpStatusCode.OK);
			}
			catch (Exception ex)
			{
				Utilerias.EscribirLog(ex.ToString());
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}
		}

		[HttpPut]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaCorte/EliminaAuditoria")]
		public AuditoriaTerminadoController.MESSAGE ActivaAuditoria(int ID)
		{
			AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

			try
			{
				Models.Auditoria AUD = db.Auditorias.Where(x => x.IdAuditoria == ID).FirstOrDefault();
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

		[HttpDelete]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaCorte/EliminaAuditoria")]
		public AuditoriaTerminadoController.MESSAGE EliminaAuditoria(int ID)
		{
			AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

			try
			{
				List<Models.Auditoria_Corte_Detalle> AD = db.Auditoria_Corte_Detalle
					.Where(x => x.IdAuditoriaCorte == ID).ToList();

				db.Auditoria_Corte_Detalle.RemoveRange(AD);
				db.SaveChanges();


				Models.Auditoria AUD = db.Auditorias.Where(x => x.IdAuditoria == ID).FirstOrDefault();

				db.Auditorias.Remove(AUD);
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

		/// <summary>
		/// ACTUALIZAMOS EL DETALLE DE LA AUDITORIA
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpPut]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaCorte/AuditoriaCorte")]
		public HttpResponseMessage ActualizaAuditoria([FromBody]EDT_AUDITORIA OT)
		{
			string image_name = "";
			string pdf = "";
			int num_detalle = 0;
			try
			{
				if (ModelState.IsValid)
				{
					List<Models.Auditoria_Corte_Detalle> ATD = db.Auditoria_Corte_Detalle.Where(x => x.IdAuditoriaCorte == OT.IdAuditoria).ToList();

					foreach (Models.Auditoria_Corte_Detalle item in ATD)
					{
						db.Auditoria_Corte_Detalle.Remove(item);
						db.SaveChanges();
					}

					foreach (DET_AUDITORIA_TENDIDO item in OT.Det)
					{
						num_detalle = num_detalle + 1;
						image_name = "";
						pdf = "";

						if (item.Imagen != null && !item.Imagen.IsEmpty())
						{
							string base64 = item.Imagen.Substring(item.Imagen.IndexOf(',') + 1);
							byte[] data = Convert.FromBase64String(base64);

							image_name = "Auditoria_Corte_" + OT.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

							using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
							{
								image_file.Write(data, 0, data.Length);
								image_file.Flush();
							}
						}

						if (item.Archivo != null && !item.Archivo.IsEmpty())
						{
							string base64 = item.Archivo.Substring(item.Archivo.IndexOf(',') + 1);
							byte[] data = Convert.FromBase64String(base64);

							pdf = "Auditoria_Corte_" + OT.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

							using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Archivos/") + pdf + ".pdf", FileMode.Create))
							{
								image_file.Write(data, 0, data.Length);
								image_file.Flush();
							}
						}

						Models.Auditoria_Corte_Detalle auditoria_corte = new Models.Auditoria_Corte_Detalle()
						{
							IdAuditoriaCorte = OT.IdAuditoria,
							Serie = item.Serie,
							Bulto = item.Bulto,
							IdTendido = item.IdTendido,
							IdMesa = item.IdMesa,
							IdPosicion = item.IdPosicion,
							IdDefecto = item.IdDefecto,
							Cantidad = item.Cantidad,
							Aud_Imagen = image_name,
							Nota = item.Nota,
							Archivo = pdf,
                            Segundas = item.Segundas
						};
						db.Auditoria_Corte_Detalle.Add(auditoria_corte);
					}
					db.SaveChanges();

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
		#endregion

		#region AUDITORIA_TENDIDO
		/// <summary>
		/// Ingresa una nueva auditoria de corte tendido
		/// </summary>
		/// <param name="OT"></param>
		/// <returns></returns>
		[HttpPost]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaCorte/NuevaAuditoriaCorteTendido")]
		public HttpResponseMessage NuevaAuditoriaCorteTendido([FromBody] REQ_NEW_OT OT)
		{
            string image_name = "";
            string pdf = "";
            int num_detalle = 0;
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
							ProcesosEspeciales = false,
							Calidad = false,
							Activo = true
						};
						db.Auditorias.Add(auditoria);
						db.SaveChanges();

						foreach (DET_AUDITORIA item in OT.Det)
						{
                            num_detalle = num_detalle + 1;
                            image_name = "";
                            pdf = "";
                            if (item.Imagen != null && !item.Imagen.IsEmpty())
                            {
                                string base64 = item.Imagen.Substring(item.Imagen.IndexOf(',') + 1);
                                byte[] data = Convert.FromBase64String(base64);

                                image_name = "Auditoria_Tendido_" + auditoria.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

                                using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                                {
                                    image_file.Write(data, 0, data.Length);
                                    image_file.Flush();
                                }
                            }
                            if (item.Archivo != null && !item.Archivo.IsEmpty())
                            {
                                string base64 = item.Archivo.Substring(item.Archivo.IndexOf(',') + 1);
                                byte[] data = Convert.FromBase64String(base64);

                                pdf = "Auditoria_Tendido_" + auditoria.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

                                using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Archivos/") + pdf + ".pdf", FileMode.Create))
                                {
                                    image_file.Write(data, 0, data.Length);
                                    image_file.Flush();
                                }
                            }
                            Models.Auditoria_Tendido_Detalle tendido_Detalle = new Models.Auditoria_Tendido_Detalle()
							{
								IdAuditoriaCorte = auditoria.IdAuditoria,
								Serie = item.Serie,
								Bulto = item.Bulto,
                                IdCortado = item.IdCortado,
                                IdCortador = item.IdCortador,
								IdPosicion = item.IdPosicion,
								IdDefecto = item.IdDefecto,
								Cantidad = item.Cantidad,
								Aud_Imagen = image_name,
                                Archivo = pdf,
                                Nota = item.Nota,
                                Segundas = item.Segundas
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
        [Route("api/AuditoriaCorte/ObtieneAuditoriaTendido")]
        public RES_AUDITORIA_DET_TENDIDO ObtieneAuditoriaTendidoPorId(int ID)
        {
            RES_AUDITORIA_DET_TENDIDO API = new RES_AUDITORIA_DET_TENDIDO();
            API.RES_DET = new List<Models.VST_AUDITORIA_TENDIDO_DETALLE>();
            List<Models.VST_AUDITORIA_TENDIDO_DETALLE> corte = new List<Models.VST_AUDITORIA_TENDIDO_DETALLE>();
            string file_path = "";

            try
            {
                if (ModelState.IsValid)
                {
                    API.RES = db.VST_AUDITORIA.Where(x => x.IdAuditoria == ID).FirstOrDefault();
                    corte = db.VST_AUDITORIA_TENDIDO_DETALLE.Where(x => x.IdAuditoriaCorte == ID).ToList();

                    foreach (Models.VST_AUDITORIA_TENDIDO_DETALLE item in corte)
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

                        file_path = HttpContext.Current.Server.MapPath("~/Archivos/");
                        file_path = file_path + item.Archivo + ".pdf";
                        if (File.Exists(file_path))
                        {
                            item.Archivo = "data:application/" + "pdf" + ";base64," + Convert.ToBase64String(File.ReadAllBytes(file_path));
                        }
                        else
                        {
                            item.Archivo = "";
                        }
                        API.RES_DET.Add(item);
                    }

                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.RES = null;
                    API.RES_DET = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                API.RES = null;
                API.RES_DET = null;
            }

            return API;
        }

        /// <summary>
        /// Genera el cierre de auditoria de corte tendido
        /// </summary>
        /// <param name="IdAuditoria"></param>
        /// <returns></returns>
        [HttpPut]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaCorte/CierreAuditoriaTendido")]
		public HttpResponseMessage CierreAuditoriaTendido(int ID)
		{
			try
			{
                Boolean notas = false;
                Models.Auditoria API = db.Auditorias.Where(x => x.IdAuditoria == ID && x.Tendido == true)
					.FirstOrDefault();
				API.FechaRegistroFin = DateTime.Now;
				db.Entry(API).State = System.Data.Entity.EntityState.Modified;
				db.SaveChanges();

                List<Models.VST_CORREOS_AUDITORIA> correos = db.VST_CORREOS_AUDITORIA.Where(x => x.ProcesosEspeciales == true).ToList();
                List<Models.Auditoria_Tendido_Detalle> auditoria_det = db.Auditoria_Tendido_Detalle.Where(x => x.IdAuditoriaCorte == ID).ToList();

                if (correos.Count > 0)
                {
                    MailMessage mensaje = new MailMessage();

                    mensaje.From = new MailAddress(System.Configuration.ConfigurationManager.AppSettings["Mail"].ToString());
                    var password = System.Configuration.ConfigurationManager.AppSettings["Password"].ToString();

                    foreach (VST_CORREOS_AUDITORIA item in correos)
                    {
                        mensaje.To.Add(item.Email);
                    }

                    var sub = "AUDITORÍA OT: " + API.OrdenTrabajo.ToUpper();
                    var body = "Se ha cerrado la auditoría de la orden de trabajo con número de corte: " + API.NumCortada.ToUpper() + " en el área de tendido.";

                    foreach (Auditoria_Tendido_Detalle item in auditoria_det)
                    {
                        if (item.Nota != "null" && !item.Nota.IsEmpty())
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
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
			}
			catch (Exception ex)
			{
				Utilerias.EscribirLog(ex.ToString());
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}
		}

        [HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCorte/EliminaAuditoriaTendido")]
        public AuditoriaTerminadoController.MESSAGE ActivaAuditoriaTendido(int ID)
        {
            AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

            try
            {
                Models.Auditoria AUD = db.Auditorias.Where(x => x.IdAuditoria == ID).FirstOrDefault();
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

        [HttpDelete]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCorte/EliminaAuditoriaTendido")]
        public AuditoriaTerminadoController.MESSAGE EliminaAuditoriaTendido(int ID)
        {
            AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

            try
            {
                List<Models.Auditoria_Tendido_Detalle> AD = db.Auditoria_Tendido_Detalle
                    .Where(x => x.IdAuditoriaCorte == ID).ToList();

                db.Auditoria_Tendido_Detalle.RemoveRange(AD);
                db.SaveChanges();

                Models.Auditoria AUD = db.Auditorias.Where(x => x.IdAuditoria == ID).FirstOrDefault();

                db.Auditorias.Remove(AUD);
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

        /// <summary>
        /// ACTUALIZAMOS EL DETALLE DE LA AUDITORIA
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/AuditoriaCorte/AuditoriaTendido")]
        public AuditoriaTerminadoController.MESSAGE ActualizaAuditoria([FromBody]EDT_AUDITORIA_T OC)
        {
            AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();
            string image_name = "";
            string pdf = "";
            int num_detalle = 0;

            try
            {
                if (ModelState.IsValid)
                {
                    List<Models.Auditoria_Tendido_Detalle> ATD = db.Auditoria_Tendido_Detalle.Where(x => x.IdAuditoriaCorte == OC.IdAuditoria).ToList();

                    foreach (Models.Auditoria_Tendido_Detalle item in ATD)
                    {
                        db.Auditoria_Tendido_Detalle.Remove(item);
                        db.SaveChanges();
                    }

                    foreach (DET_AUDITORIA item in OC.Det)
                    {
                        num_detalle = num_detalle + 1;
                        image_name = "";
                        pdf = "";

                        if (item.Imagen != null && !item.Imagen.IsEmpty())
                        {
                            string base64 = item.Imagen.Substring(item.Imagen.IndexOf(',') + 1);
                            byte[] data = Convert.FromBase64String(base64);

                            image_name = "Auditoria_Tendido_" + OC.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

                            using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Imagenes/") + image_name + ".jpg", FileMode.Create))
                            {
                                image_file.Write(data, 0, data.Length);
                                image_file.Flush();
                            }
                        }

                        if (item.Archivo != null && !item.Archivo.IsEmpty())
                        {
                            string base64 = item.Archivo.Substring(item.Archivo.IndexOf(',') + 1);
                            byte[] data = Convert.FromBase64String(base64);

                            pdf = "Auditoria_Tendido_" + OC.IdAuditoria + DateTime.Now.ToString("yymmssfff") + num_detalle;

                            using (var image_file = new FileStream(HttpContext.Current.Server.MapPath("~/Archivos/") + pdf + ".pdf", FileMode.Create))
                            {
                                image_file.Write(data, 0, data.Length);
                                image_file.Flush();
                            }
                        }

                        Models.Auditoria_Tendido_Detalle auditoria_calidad = new Models.Auditoria_Tendido_Detalle()
                        {
                            IdAuditoriaCorte = OC.IdAuditoria,
                            Serie = item.Serie,
                            Bulto = item.Bulto,
                            IdCortado = item.IdCortado,
                            IdCortador = item.IdCortador,
                            IdPosicion = item.IdPosicion,
                            IdDefecto = item.IdDefecto,
                            Cantidad = item.Cantidad,
                            Aud_Imagen = image_name,
                            Archivo = pdf,
                            Nota = item.Nota,
                            Segundas = item.Segundas
                        };
                        db.Auditoria_Tendido_Detalle.Add(auditoria_calidad);
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
        #endregion

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

		public partial class OT_DATA
		{
			public string Status { get; set; }
			public string Planta { get; set; }
			public string Cortada { get; set; }
			public string Po { get; set; }
			public string Estilo { get; set; }
			public string Cliente { get; set; }
			public string Linea { get; set; }
			public string Lavado { get; set; }
			public string Division { get; set; }
			public string Marca { get; set; }
			public string Modelo { get; set; }
			public string Ruta { get; set; }
		}

		public partial class RES_OT_DET
		{
			public OT_DET OT { get; set; }
			public string Message2 { get; set; }
			public HttpResponseMessage Message { get; set; }
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
			public int ClientId { get; set; }
			public int Bulto { get; set; }
		}

		public partial class REQ_NEW_OT
		{
			[Required] public int IdClienteRef { get; set; }
			[Required] public string OrdenTrabajo { get; set; }
			public string PO { get; set; }
			public string Tela { get; set; }
			public string Marca { get; set; }
			public string NumCortada { get; set; }
			public string Lavado { get; set; }
			public string Estilo { get; set; }
			public string Planta { get; set; }
			public string Ruta { get; set; }
			[Required] public int IdUsuario { get; set; }
			[Required] public List<DET_AUDITORIA> Det { get; set; }
		}

		public partial class REQ_NEW_OT_T
		{
			[Required] public int IdClienteRef { get; set; }
			[Required] public string OrdenTrabajo { get; set; }
			public string PO { get; set; }
			public string Tela { get; set; }
			public string Marca { get; set; }
			public string NumCortada { get; set; }
			public string Lavado { get; set; }
			public string Estilo { get; set; }
			public string Planta { get; set; }
			public string Ruta { get; set; }
			[Required] public int IdUsuario { get; set; }
			[Required] public List<DET_AUDITORIA_TENDIDO> Det { get; set; }
		}

		public partial class DET_AUDITORIA
		{
			[Required] public int IdCortador { get; set; }
			[Required] public string Serie { get; set; }
			[Required] public string Bulto { get; set; }
			[Required] public int IdPosicion { get; set; }
			[Required] public int IdDefecto { get; set; }
			[Required] public int IdCortado { get; set; }
			[Required] public int Cantidad { get; set; }
            [Required] public int Segundas { get; set; }
            public string Imagen { get; set; }
			public string Nota { get; set; }
			public string Archivo { get; set; }
		}

		public partial class DET_AUDITORIA_TENDIDO
		{
			[Required] public string Serie { get; set; }
			[Required] public string Bulto { get; set; }
			[Required] public int IdTendido { get; set; }
			[Required] public int IdMesa { get; set; }
			[Required] public int IdPosicion { get; set; }
			[Required] public int IdDefecto { get; set; }
			[Required] public int Cantidad { get; set; }
            [Required] public int Segundas { get; set; }
            public string Imagen { get; set; }
			public string Nota { get; set; }
			public string Archivo { get; set; }
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

        public partial class RES_AUDITORIA_DET_TENDIDO
        {
            public HttpResponseMessage Message { get; set; }
            public Models.VST_AUDITORIA RES { get; set; }
            public List<Models.VST_AUDITORIA_TENDIDO_DETALLE> RES_DET { get; set; }
        }

        public partial class RES_BULTO
		{
			public List<BULTO> Bulto { get; set; }
			public HttpResponseMessage Message { get; set; }
		}

		public partial class BULTO
		{
			public int Bulto { get; set; }
		}

		public partial class EDT_AUDITORIA
		{
			[Required]
			public int IdAuditoria { get; set; }

			[Required]
			public List<DET_AUDITORIA_TENDIDO> Det { get; set; }
		}

        public partial class EDT_AUDITORIA_T
        {
            [Required]
            public int IdAuditoria { get; set; }

            [Required]
            public List<DET_AUDITORIA> Det { get; set; }
        }
	}
}