using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Serialization;
using Microsoft.Ajax.Utilities;

namespace RioSulAPI.Controllers
{
    public class ReporteConsultaController : ApiController
    {
        private JavaScriptSerializer _objSerializer = new JavaScriptSerializer();
        private Models.bd_calidadIIEntities db = new Models.bd_calidadIIEntities();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("api/Consulta/GetMarca")]
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
                            API.Marcas = db.VST_AUDITORIA.Where(x => x.Calidad == true).DistinctBy(x=> x.Marca).Select(x => x.Marca).ToList();
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

        public partial class MARCAS
        {
            public List<string> Marcas { get; set; }
            public HttpResponseMessage HttpResponseMessage { get; set; }
        }

        public partial class C_CLIENTE
        {
            
            public string[] IdCliente { get; set; }

            [Required] public string Auditoria { get; set; }
        }
    }
}
