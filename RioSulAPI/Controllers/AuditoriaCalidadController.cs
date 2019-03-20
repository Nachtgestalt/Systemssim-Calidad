using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using RioSulAPI.Class;
using RioSulAPI.Models;

namespace RioSulAPI.Controllers
{
	public class AuditoriaCalidadController : ApiController
	{
		private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();
		private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

		#region AUDITORIA

	  
		/// <summary>
		/// Registra una nueva auditoría de Calidad
		/// </summary>
		/// <param name="OT"></param>
		/// <returns></returns>
		[HttpPost]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaCalidad/NuevaAuditoriaCalidad")]
		public HttpResponseMessage NuevaAuditoriaCalidad([FromBody]AuditoriaCalidadController.REQ_NEW_AT OT)
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
							Terminado = false,
							Confeccion = false,
							ProcesosEspeciales = false,
							Calidad = true
						};
						db.Auditorias.Add(auditoria);
						db.SaveChanges();

						foreach (AuditoriaCalidadController.DET_AUDITORIA_CALIDAD item in OT.Det)
						{
							Models.Auditoria_Calidad_Detalle auditoria_calidad = new Models.Auditoria_Calidad_Detalle()
							{
								IdAuditoria = auditoria.IdAuditoria,
								Id_Origen = item.IdOrigen,
								IdPosicion = item.IdPosicion,
								IdOperacion = item.IdOperacion,
								IdDefecto = item.IdDefecto,
								Recup = item.Recup,
								Criterio = item.Criterio,
								Fin = item.Fin,
								Aud_Imagen = item.Imagen,
								Archivo = item.Archivo,
								Nota = item.Nota

							};
							db.Auditoria_Calidad_Detalle.Add(auditoria_calidad);
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
		[Route("api/AuditoriaCalidad/ObtieneAuditoriaCalidad")]
		public AuditoriaCorteController.RES_AUDITORIA ObtieneAuditoriaTerminado()
		{
			AuditoriaCorteController.RES_AUDITORIA API = new AuditoriaCorteController.RES_AUDITORIA();
			try
			{
				API.RES = db.VST_AUDITORIA.Where(x => x.Calidad == true).ToList();
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
		[Route("api/AuditoriaCalidad/ObtieneAuditoriaDet")]
		public RES_AUDITORIA_T_DET ObtieneAuditoriaDet(int id)
		{
			RES_AUDITORIA_T_DET API = new RES_AUDITORIA_T_DET();

			try
			{
				API.RES = db.VST_AUDITORIA.Where(x => x.IdAuditoria == id && x.Calidad == true).FirstOrDefault();
				API.RES_DET = db.VST_AUDITORIA_CALIDAD_DETALLE.Where(x => x.IdAuditoria == id).ToList();
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
		[Route("api/AuditoriaCalidad/CierreAuditoria")]
		public HttpResponseMessage CierreAuditoria(int IdAuditoria)
		{
			try
			{
				Models.Auditoria API = db.Auditorias.Where(x => x.IdAuditoria == IdAuditoria && x.Calidad == true).FirstOrDefault();
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
		/// ACTUALIZAMOS EL DETALLE DE LA AUDITORIA
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpPut]
		[ApiExplorerSettings(IgnoreApi = false)]
		[Route("api/AuditoriaCalidad/ActualizaAuditoriaDet")]
		public AuditoriaTerminadoController.MESSAGE ActualizaAuditoria([FromBody]ACT_DET_AUDITORIA_C AC)
		{
			AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

			try
			{
				if (ModelState.IsValid)
				{
					Models.Auditoria_Calidad_Detalle ATD = new Auditoria_Calidad_Detalle()
					{
						IdAuditoria = AC.IdAuditoria,
						Id_Origen = AC.IdOrigen,
						IdPosicion = AC.IdPosicion,
						IdOperacion = AC.IdOperacion,
						IdDefecto = AC.IdDefecto,
						Recup = AC.Recup,
						Criterio = AC.Criterio,
						Fin = AC.Fin,
						Aud_Imagen = AC.Imagen,
					};
					db.Auditoria_Calidad_Detalle.Add(ATD);
					db.SaveChanges();

					API.Message = "Elemento agregado con éxito";
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
		[Route("api/AuditoriaCalidad/ActualizaAuditoriaDet")]
		public AuditoriaTerminadoController.MESSAGE EliminaAuditoria(int IdAuditoriaDet)
		{
			AuditoriaTerminadoController.MESSAGE API = new AuditoriaTerminadoController.MESSAGE();

			try
			{
				Models.Auditoria_Calidad_Detalle ACD = db.Auditoria_Calidad_Detalle.Where(x => x.IdAuditoria_Calidad_Detalle == IdAuditoriaDet).FirstOrDefault();
				if (ACD != null)
				{
					db.Auditoria_Calidad_Detalle.Remove(ACD);
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

		#endregion

		public partial class DET_AUDITORIA_CALIDAD
		{
			[Required]
			public int IdPosicion { get; set; }
			[Required]
			public int IdOperacion { get; set; }
			[Required]
			public int IdOrigen { get; set; }
			[Required]
			public int IdDefecto { get; set; }

			public int Recup { get; set; }
			public int Criterio { get; set; }
			public int Fin { get; set; }

			public string Imagen { get; set; }
			public string Archivo { get; set; }
			public string Nota { get; set; }
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
			public List<DET_AUDITORIA_CALIDAD> Det { get; set; }
		}

		public partial class RES_AUDITORIA_T_DET
		{
			public HttpResponseMessage Message { get; set; }
			public Models.VST_AUDITORIA RES { get; set; }
			public List<Models.VST_AUDITORIA_CALIDAD_DETALLE> RES_DET { get; set; }
		}

		public partial class ACT_DET_AUDITORIA_C
		{
			[Required]
			public int IdAuditoria { get; set; }
			[Required]
			public int IdPosicion { get; set; }
			[Required]
			public int IdOperacion { get; set; }
			[Required]
			public int IdOrigen { get; set; }
			[Required]
			public int IdDefecto { get; set; }

			public int Recup { get; set; }
			public int Criterio { get; set; }
			public int Fin { get; set; }

			public string Imagen { get; set; }
		}
	}
}
