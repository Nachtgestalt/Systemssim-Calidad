using RioSulAPI.Class;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Microsoft.Ajax.Utilities;

namespace RioSulAPI.Controllers
{
    public class ClienteController : ApiController
    {
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();
        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();

        #region CLIENTE

        /// <summary>
        /// Obtiene los clientes, por nombre, nombre corto o clave
        /// </summary>
        /// <param name="BusquedaCliente"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cliente/ObtieneClientesDynamics")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_CLIENTE ObtieneClientesDynamics(string BusquedaCliente)
        {
            ViewModel.RES_CLIENTE API = new ViewModel.RES_CLIENTE();
            try
            {
                using (SqlConnection _Conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["dbRioSulApp"].ToString()))
                {
                    _Conn.Open();
                    string Filtro = " AND (dbo.XCustomer.LongName LIKE '%" + BusquedaCliente + "%' OR dbo.Customer.Name LIKE '%" + BusquedaCliente + "%' OR dbo.Customer.CustId = '" + BusquedaCliente + "')";

                    string Consulta = "SELECT dbo.Customer.CustId AS cve_cliente, dbo.XCustomer.LongName AS nom_cliente, dbo.Customer.Name AS nom_corto, dbo.Customer.User2 AS nUm_rfc, dbo.Customer.Addr1 AS dir_cliente, dbo.Customer.Addr2 AS nom_colonia, dbo.Customer.Zip AS num_cp, dbo.Customer.Phone AS num_telefono, dbo.Customer.Fax AS num_fax, dbo.Customer.Salut AS nom_contacto, dbo.Customer.Status AS nom_estatus, dbo.Country.Descr AS nom_ciudad, dbo.Customer.State AS nom_estado FROM dbo.Customer LEFT OUTER JOIN dbo.XCustomer ON dbo.Customer.CustId = dbo.XCustomer.CustId LEFT OUTER JOIN dbo.Country ON dbo.Customer.Country = dbo.Country.CountryID WHERE (dbo.Customer.Status = 'A')" + Filtro;

                    API.Clientes = new List<ViewModel.ClienteRioSulApp>();

                    SqlCommand _Command = new SqlCommand(Consulta, _Conn);
                    SqlDataReader _reader = _Command.ExecuteReader();
                    while (_reader.Read())
                    {
                        ViewModel.ClienteRioSulApp clienteRioSulApp = new ViewModel.ClienteRioSulApp();
                        clienteRioSulApp.cve_cliente = _reader["cve_cliente"].ToString().Trim();
                        clienteRioSulApp.nom_cliente = _reader["nom_cliente"].ToString().Trim();
                        clienteRioSulApp.nom_corto = _reader["nom_corto"].ToString().Trim();
                        clienteRioSulApp.nUm_rfc = _reader["nUm_rfc"].ToString().Trim();
                        clienteRioSulApp.nom_colonia = _reader["nom_colonia"].ToString().Trim();
                        clienteRioSulApp.num_cp = _reader["num_cp"].ToString().Trim();
                        clienteRioSulApp.num_telefono = _reader["num_telefono"].ToString().Trim();
                        clienteRioSulApp.num_fax = _reader["num_fax"].ToString().Trim();
                        clienteRioSulApp.nom_contacto = _reader["nom_contacto"].ToString().Trim();
                        clienteRioSulApp.nom_estatus = _reader["nom_estatus"].ToString().Trim();
                        clienteRioSulApp.nom_ciudad = _reader["nom_ciudad"].ToString().Trim();
                        clienteRioSulApp.nom_estado = _reader["nom_estado"].ToString().Trim();

                        API.Clientes.Add(clienteRioSulApp);
                    }
                    _reader.Close();
                }
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Clientes = null;
                Utilerias.EscribirLog(ex.ToString());
            }
            return API;
        }

        /// <summary>
        ///  ObtieneClientes
        /// </summary>
        /// <param name="Observacion"></param>
        /// <param name="Descripcion"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cliente/ObtieneClientes")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public List<ViewModel.VST_C_Clientes> ObtieneClientes(string Observacion = "", string Descripcion = "")
        {
            List<ViewModel.VST_C_Clientes> vST_C_Clientes = new List<ViewModel.VST_C_Clientes>();
            try
            {
                if (Observacion != "" && Descripcion == "")
                {
                    vST_C_Clientes = db.C_Clientes.Where(x => x.Observaciones.Contains(Observacion)).Select(x => new ViewModel.VST_C_Clientes()
                    {
                        Descripcion = x.Descripcion,
                        Observaciones = x.Observaciones,
                        IdClienteRef = x.IdClienteRef
                    }).ToList();
                }
                else if (Observacion == "" && Descripcion != "")
                {
                    vST_C_Clientes = db.C_Clientes.Where(x => x.Descripcion.Contains(Descripcion)).Select(x => new ViewModel.VST_C_Clientes()
                    {
                        Descripcion = x.Descripcion,
                        Observaciones = x.Observaciones,
                        IdClienteRef = x.IdClienteRef
                    }).ToList();
                }
                else
                {
                    vST_C_Clientes = db.C_Clientes.Where(x => x.Descripcion.Contains(Descripcion) || x.Observaciones.Contains(Observacion)).Select(x => new ViewModel.VST_C_Clientes()
                    {
                        Descripcion = x.Descripcion,
                        Observaciones = x.Observaciones,
                        IdClienteRef = x.IdClienteRef
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                vST_C_Clientes = null;
            }
            return vST_C_Clientes;
        }

        /// <summary>
        /// Obtiene los clientes en la aplicación, con referencia cruzada
        /// </summary>
        /// <param name="Observaciones"></param>
        /// <param name="Descripcion"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cliente/ObtieneClientesReferenciaCruzada")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_REF_CRUZADA ObtieneClientesReferenciaCruzada(string Observaciones, string Descripcion)
        {
            ViewModel.RES_REF_CRUZADA API = new ViewModel.RES_REF_CRUZADA();
            try
            {
                Models.C_Clientes c_Clientes = db.C_Clientes.Where(x => x.Observaciones.Contains(Observaciones) || x.Descripcion.Contains(Descripcion)).FirstOrDefault();
                if (c_Clientes != null)
                {
                    API.Ref_C_Clientes = new ViewModel.VST_C_Clientes()
                    {
                        Observaciones = c_Clientes.Observaciones,
                        IdClienteRef = c_Clientes.IdClienteRef,
                        Descripcion = c_Clientes.Descripcion
                    };
                    API.Ref_C_ClientesReferencia = new List<ViewModel.VST_C_ClientesReferencia>();
                    if (c_Clientes.C_ClientesReferencia.Count > 0)
                    {
                        foreach (Models.C_ClientesReferencia item in c_Clientes.C_ClientesReferencia)
                        {
                            ViewModel.VST_C_ClientesReferencia vST_C_ClientesReferencia = new ViewModel.VST_C_ClientesReferencia()
                            {
                                Cve_Cliente = item.Cve_Cliente,
                                Dir_Cliente = item.Dir_Cliente,
                                IdCliente = item.IdCliente,
                                IdClienteRef = item.IdClienteRef,
                                Nom_Ciudad = item.Nom_Ciudad,
                                Nom_Cliente = item.Nom_Cliente,
                                Nom_Colonia = item.Nom_Colonia,
                                Nom_Contacto = item.Nom_Contacto,
                                Nom_Corto = item.Nom_Corto,
                                Nom_Estado = item.Nom_Estado,
                                Nom_Estatus = item.Nom_Estatus,
                                Num_Cp = item.Num_Cp,
                                Num_Fax = item.Num_Fax,
                                Num_Rfc = item.Num_Rfc,
                                Num_Telefono = item.Num_Telefono
                            };
                            API.Ref_C_ClientesReferencia.Add(vST_C_ClientesReferencia);
                        }
                    }
                    API.Message = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    API.Message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    API.Ref_C_Clientes = null; API.Ref_C_ClientesReferencia = null;
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                API.Ref_C_Clientes = null; API.Ref_C_ClientesReferencia = null;
            }
            return API;
        }

        /// <summary>
        /// Guarda un nuevo registro referente a clientes Dynamics 
        /// </summary>
        /// <param name="rEQ_ClienteRef"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cliente/GuardaReferenciaCruzada")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public HttpResponseMessage GuardaReferenciaCruzada([FromBody]ViewModel.REQ_ClienteRef rEQ_ClienteRef)
        {
            HttpResponseMessage Result;
            try
            {
                if (ModelState.IsValid)
                {
                    Models.C_Clientes c_Clientes = new Models.C_Clientes()
                    {
                        Descripcion = rEQ_ClienteRef.Descripcion,
                        Observaciones = rEQ_ClienteRef.Observaciones
                    };
                    db.C_Clientes.Add(c_Clientes);
                    db.SaveChanges();

                    int IdClienteRef = c_Clientes.IdClienteRef;


                    List<ViewModel.VST_C_ClientesRef> Clientes = _objSerializer.Deserialize<List<ViewModel.VST_C_ClientesRef>>(rEQ_ClienteRef.CLI_REF);
                    foreach (ViewModel.VST_C_ClientesRef item in Clientes)
                    {
                        Models.C_ClientesReferencia c_ClientesReferencia = new Models.C_ClientesReferencia()
                        {
                            IdClienteRef = IdClienteRef,
                            Cve_Cliente = item.Cve_Cliente,
                            Dir_Cliente = "",
                            Nom_Ciudad = "",
                            Nom_Cliente = item.Nom_Cliente,
                            Nom_Colonia = "",
                            Nom_Contacto = item.Nom_Contacto,
                            Nom_Corto = item.Nom_Corto,
                            Nom_Estado = "",
                            Nom_Estatus = item.Nom_Estatus,
                            Num_Cp = item.Num_Cp,
                            Num_Fax = "",
                            Num_Rfc = item.Num_Rfc,
                            Num_Telefono = ""
                        };
                        db.C_ClientesReferencia.Add(c_ClientesReferencia);
                    }
                    db.SaveChanges();
                    Result = new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    Result = new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                Result = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return Result;
        }

        /// <summary>
        /// Obtiene las referencias cruzadas por cliente
        /// </summary>
        /// <param name="IdCliente"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/Cliente/ObtieneReferenciasPorCliente")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ViewModel.RES_CLIENTE ObtieneReferenciasPorCliente(int IdCliente)
        {
            ViewModel.RES_CLIENTE API = new ViewModel.RES_CLIENTE();
            try
            {
                API.HEAD_CLIENTE = db.C_Clientes.Where(x => x.IdClienteRef == IdCliente).Select(x => x.Descripcion).FirstOrDefault();
                API.Clientes = db.C_ClientesReferencia.Where(x => x.IdClienteRef == IdCliente).Select(x => new ViewModel.ClienteRioSulApp()
                {
                    cve_cliente = x.Cve_Cliente,
                    nom_cliente = x.Nom_Cliente,
                    nom_corto = x.Nom_Corto,
                    nom_ciudad = x.Nom_Ciudad,
                    nom_colonia = x.Nom_Colonia,
                    dir_cliente = x.Dir_Cliente,
                    nom_contacto = x.Nom_Contacto,
                    nom_estado = x.Nom_Estado,
                    num_cp = x.Num_Cp,
                    num_fax = x.Num_Fax,
                    nom_estatus = x.Nom_Estatus,
                    nUm_rfc = x.Num_Rfc,
                    num_telefono = x.Num_Telefono
                }).ToList();
                API.Message = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Utilerias.EscribirLog(ex.ToString());
                API.Clientes = null;
                API.Message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            return API;
        }

        #endregion

        #region CONSULTA

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cliente/GetMarca")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public MARCAS GetMarcas([FromBody] C_CLIENTE Filtro)
        {
            MARCAS API = new MARCAS();
            API.Marcas = new List<string>();
            List<string> consulta = new List<string>();
            int idClienteRef;

            try
            {
                switch (Filtro.Auditoria)
                {
                    case "Calidad":
                        if (Filtro.IdCliente == null)
                        {
                            API.Marcas = db.VST_AUDITORIA.Where(x => x.Calidad == true).DistinctBy(x => x.Marca).Select(x => x.Marca).ToList();
                            API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        else
                        {
                            foreach (var item in Filtro.IdCliente)
                            {
                                idClienteRef = Convert.ToInt16(item);
                                consulta = db.VST_AUDITORIA.Where(x => x.Calidad == true && x.IdClienteRef == idClienteRef).DistinctBy(x => x.Marca).Select(x => x.Marca).ToList();
                                foreach (var item2 in consulta)
                                {
                                    API.Marcas.Add(item2);
                                }
                            }
                            API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                API.Marcas = null;
                API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            return API;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cliente/GetPO")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public PO GetPo([FromBody] C_CLIENTE_MARCA Filtro)
        {
            PO API = new PO();
            API.PoList = new List<string>();
            List<string> consulta = new List<string>();
            int idClienteRef;

            try
            {
                switch (Filtro.Auditoria)
                {
                    case "Calidad":
                        if (Filtro.IdCliente == null && Filtro.Marca == null)
                        {
                            API.PoList = db.VST_AUDITORIA.Where(x => x.Calidad == true).DistinctBy(x => x.PO).Select(x => x.PO).ToList();
                            API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        }

                        if (Filtro.IdCliente != null && Filtro.Marca == null)
                        {
                            foreach (var item in Filtro.IdCliente)
                            {
                                idClienteRef = Convert.ToInt16(item);
                                consulta = db.VST_AUDITORIA.Where(x => x.Calidad == true && x.IdClienteRef == idClienteRef).DistinctBy(x => x.PO).Select(x => x.PO).ToList();
                                foreach (var item2 in consulta)
                                {
                                    API.PoList.Add(item2);
                                }
                            }
                            API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        }

                        if (Filtro.IdCliente == null && Filtro.Marca != null)
                        {
                            foreach (var item in Filtro.Marca)
                            {
                                consulta = db.VST_AUDITORIA.Where(x => x.Calidad == true && x.Marca == item).DistinctBy(x => x.PO).Select(x => x.PO).ToList();
                                foreach (var item2 in consulta)
                                {
                                    API.PoList.Add(item2);
                                }
                            }
                            API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        }

                        if (Filtro.IdCliente != null && Filtro.Marca != null)
                        {
                            foreach (var item in Filtro.IdCliente)
                            {
                                idClienteRef = Convert.ToInt16(item);
                                foreach (var item2 in Filtro.Marca)
                                {
                                    consulta = db.VST_AUDITORIA.Where(x => x.Calidad == true && x.Marca == item2 && x.IdClienteRef == idClienteRef).DistinctBy(x => x.PO).Select(x => x.PO).ToList();
                                    foreach (var item3 in consulta)
                                    {
                                        API.PoList.Add(item3);
                                    }
                                }
                            }
                            API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                API.PoList = null;
                API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            return API;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cliente/GetCorte")]
        [System.Web.Http.HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        public CORTE GetCorte([FromBody] C_CLIENTE_CORTE Filtro)
        {
            CORTE API = new CORTE();
            API.CorteList = new List<string>();
            List<string> consulta = new List<string>();
            int idClienteRef;

            try
            {
                switch (Filtro.Auditoria)
                {
                    case "Calidad":
                        if (Filtro.IdCliente == null && Filtro.Marca == null && Filtro.PO == null)
                        {
                            API.CorteList = db.VST_AUDITORIA.Where(x => x.Calidad == true).DistinctBy(x => x.NumCortada).Select(x => x.NumCortada).ToList();
                            API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        }

                        if (Filtro.IdCliente != null && Filtro.Marca == null && Filtro.PO == null)
                        {
                            foreach (var item in Filtro.IdCliente)
                            {
                                idClienteRef = Convert.ToInt16(item);
                                consulta = db.VST_AUDITORIA.Where(x => x.Calidad == true && x.IdClienteRef == idClienteRef).DistinctBy(x => x.NumCortada).Select(x => x.NumCortada).ToList();
                                foreach (var item2 in consulta)
                                {
                                    API.CorteList.Add(item2);
                                }
                            }
                            API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        }

                        if (Filtro.IdCliente == null && Filtro.Marca != null && Filtro.PO == null)
                        {
                            foreach (var item in Filtro.Marca)
                            {
                                consulta = db.VST_AUDITORIA.Where(x => x.Calidad == true && x.Marca == item).DistinctBy(x => x.PO).Select(x => x.PO).ToList();
                                foreach (var item2 in consulta)
                                {
                                    API.CorteList.Add(item2);
                                }
                            }
                            API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        }

                        if (Filtro.IdCliente != null && Filtro.Marca != null && Filtro.PO == null)
                        {
                            foreach (var item in Filtro.IdCliente)
                            {
                                idClienteRef = Convert.ToInt16(item);
                                foreach (var item2 in Filtro.Marca)
                                {
                                    consulta = db.VST_AUDITORIA.Where(x => x.Calidad == true && x.Marca == item2 && x.IdClienteRef == idClienteRef).DistinctBy(x => x.PO).Select(x => x.PO).ToList();
                                    foreach (var item3 in consulta)
                                    {
                                        API.CorteList.Add(item3);
                                    }
                                }
                            }
                            API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        }

                        if (Filtro.IdCliente == null && Filtro.Marca == null && Filtro.PO != null)
                        {
                            foreach (var item in Filtro.PO)
                            {
                                consulta = db.VST_AUDITORIA.Where(x => x.Calidad == true && x.PO == item).DistinctBy(x => x.NumCortada).Select(x => x.NumCortada).ToList();
                                foreach (var item2 in consulta)
                                {
                                    API.CorteList.Add(item2);
                                }
                            }

                            API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        }

                        if (Filtro.IdCliente != null && Filtro.Marca == null && Filtro.PO != null)
                        {
                            foreach (var item in Filtro.IdCliente)
                            {
                                idClienteRef = Convert.ToInt16(item);

                                foreach (var item2 in Filtro.PO)
                                {
                                    consulta = db.VST_AUDITORIA.Where(x => x.Calidad == true && x.IdClienteRef == idClienteRef && x.PO == item2).DistinctBy(x => x.NumCortada).Select(x => x.NumCortada).ToList();
                                    foreach (var item3 in consulta)
                                    {
                                        API.CorteList.Add(item3);
                                    }
                                }
                            }
                            API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        }

                        if (Filtro.IdCliente == null && Filtro.Marca != null && Filtro.PO != null)
                        {
                            foreach (var item in Filtro.Marca)
                            {
                                foreach (var item2 in Filtro.PO)
                                {
                                    consulta = db.VST_AUDITORIA.Where(x => x.Calidad == true && x.Marca == item && x.PO == item2).DistinctBy(x => x.PO).Select(x => x.PO).ToList();
                                    foreach (var item3 in consulta)
                                    {
                                        API.CorteList.Add(item3);
                                    }
                                }
                            }
                            API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        }

                        if (Filtro.IdCliente != null && Filtro.Marca != null && Filtro.PO != null)
                        {
                            foreach (var item in Filtro.IdCliente)
                            {
                                idClienteRef = Convert.ToInt16(item);
                                foreach (var item2 in Filtro.Marca)
                                {
                                    foreach (var item3 in Filtro.PO)
                                    {
                                        consulta = db.VST_AUDITORIA.Where(x => x.Calidad == true && x.Marca == item2 && x.IdClienteRef == idClienteRef && x.PO == item3).DistinctBy(x => x.PO).Select(x => x.PO).ToList();
                                        foreach (var item4 in consulta)
                                        {
                                            API.CorteList.Add(item4);
                                        }
                                    }
                                }
                            }
                            API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                API.CorteList = null;
                API.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            return API;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cliente/GetPlanta")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public PLANTAS GetPlanta(string planta = "")
        {
            PLANTAS API = new PLANTAS();
            API.P = new List<PLANTA>();

            try
            {
                using (SqlConnection _Conn = new SqlConnection(System.Configuration.ConfigurationManager
                    .ConnectionStrings["dbRioSulApp"].ToString()))
                {
                    _Conn.Open();
                    string Consulta = " select Planta, Descr from RsTb_Plantas where Planta like '%"+ planta +"%' ";
                    SqlCommand Command = new SqlCommand(Consulta, _Conn);
                    SqlDataReader reader = Command.ExecuteReader();
                    while (reader.Read())
                    {
                        PLANTA p = new PLANTA();

                        p.Planta = reader[0].ToString().Trim();
                        p.Descripcion = reader[1].ToString().Trim();

                        API.P.Add(p);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                API.P = null;
            }

            return API;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        [System.Web.Http.Route("api/Cliente/GetEstilo")]
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = false)]
        public ESTILOS GetEstilo(string estilo = "")
        {
            ESTILOS API = new ESTILOS();
            API.E = new List<ESTILO>();

            try
            {
                using (SqlConnection _Conn = new SqlConnection(System.Configuration.ConfigurationManager
                    .ConnectionStrings["dbRioSulApp"].ToString()))
                {
                    _Conn.Open();
                    string Consulta = " select distinct left(invtid,26) as Estilo, Descr from inventory where MaterialType='PROD. TERM' and left(invtid,26) like '%"+ estilo +"%' ";
                    SqlCommand Command = new SqlCommand(Consulta, _Conn);
                    SqlDataReader reader = Command.ExecuteReader();
                    while (reader.Read())
                    {
                        ESTILO p = new ESTILO();

                        p.Estilo = reader[0].ToString().Trim();
                        p.Descripcion = reader[1].ToString().Trim();

                        API.E.Add(p);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                API.E = null;
            }

            return API;
        }

        #endregion

        public partial class PLANTA
        {
            public string Planta { get; set; }
            public string Descripcion { get; set; }
        }

        public partial class ESTILO
        {
            public string Estilo { get; set; }
            public string Descripcion { get; set; }
        }

        public partial class MARCAS
        {
            public List<string> Marcas { get; set; }
            public HttpResponseMessage HttpResponseMessage { get; set; }
        }

        public partial class PO
        {
            public List<string> PoList { get; set; }
            public HttpResponseMessage HttpResponseMessage { get; set; }
        }

        public partial class CORTE
        {
            public List<string> CorteList { get; set; }
            public HttpResponseMessage HttpResponseMessage { get; set; }
        }

        public partial class PLANTAS
        {
            public List<PLANTA> P { get; set; }
        }

        public partial class ESTILOS
        {
            public List<ESTILO> E { get; set; }
        }

        public partial class C_CLIENTE
        {
            public string[] IdCliente { get; set; }

            [Required]
            public string Auditoria { get; set; }
        }

        public partial class C_CLIENTE_MARCA
        {
            public string[] IdCliente { get; set; }
            public string[] Marca { get; set; }
            [Required]
            public string Auditoria { get; set; }
        }

        public partial class C_CLIENTE_CORTE
        {
            public string[] IdCliente { get; set; }
            public string[] Marca { get; set; }
            public string[] PO { get; set; }
            [Required]
            public string Auditoria { get; set; }
        }
    }
}