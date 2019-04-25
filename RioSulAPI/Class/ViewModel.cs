using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace RioSulAPI.Class
{
    public class ViewModel
    {
        #region LogIn

        public partial class REQ_LOGIN
        {
            [Required] public string Usuario { get; set; }
            [Required] public string Contrasena { get; set; }
        }

        public partial class RES_LOGIN
        {
            public HttpResponseMessage Message { get; set; }
            public USU Usuario { get; set; }
            public List<PERMISOS> PER { get; set; }
            public bool Valido { get; set; }
        }

        public partial class USU
        {
            public int ID { get; set; }
            public string Nombre { get; set; }
            public string Usuario { get; set; }
            public string Contraseña { get; set; }
            public bool? Activo { get; set; }
            public DateTime? LastLogin { get; set; }
            public string Email { get; set; }
        }

        public partial class NEW_USU
        {
            public string Nombre { get; set; }
            public string Usuario { get; set; }
            public string Contraseña { get; set; }
            public string Email { get; set; }
        }

        public partial class UPD_USU
        {
            public int UsuId { get; set; }
            public string Nombre { get; set; }
            public string Usuario { get; set; }
            public string Contraseña { get; set; }
            public string Email { get; set; }
        }

        public partial class EDIT_USU
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
            public string Usuario { get; set; }
            public string Contraseña { get; set; }
            public string Email { get; set; }
        }

        public partial class PERMISOS
        {
            public Models.VST_PERMISOS PerGral { get; set; }
            public List<Models.VST_PERMISOS> Per { get; set; }
        }

        #endregion

        #region Usuario

        public partial class REQ_USU
        {
            [Required] public NEW_USU Usuario { get; set; }
            public string PerMenu { get; set; }
        }

        public partial class REQ_USU_UPD
        {
            [Required] public string Key { get; set; }
            [Required] public UPD_USU Usuario { get; set; }
            public string PerMenu { get; set; }
        }

        public partial class Menu
        {
            public int PANTALLA_ID { get; set; }
            public bool PER_CONSULTAR { get; set; }
            public bool PER_EDITAR { get; set; }
            public bool PER_REGISTRAR { get; set; }
            public bool PER_PROCESAR { get; set; }
            public bool PER_CANCELAR { get; set; }
            public bool PER_ACTIVAR { get; set; }
        }

        public partial class RES_USU
        {
            public bool Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_DIS_USU
        {
            public bool Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class REQ_CHG_PASS
        {
            public int ID_USU { get; set; }
            public string LastPassword { get; set; }
            public string NewPassword { get; set; }
        }

        public partial class RES_CHG_PASS
        {
            public bool Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_GET_USU
        {
            public bool Hecho { get; set; }
            public List<ViewModel.USU> Usuarios { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_USU_UPD
        {
            public USU Usuario { get; set; }
            public List<Menu> Menu { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        #endregion

        #region Menu

        public partial class RES_MENU
        {
            public List<t_sg_pantallas> Menus { get; set; }
            public List<c_sg_operaciones> Operacion { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class t_sg_pantallas
        {
            public int ID { get; set; }
            public int? PantallaId { get; set; }
            public string Pantalla { get; set; }
            public string SubMenu { get; set; }
            public bool? Activo { get; set; }
            public string Ruta { get; set; }
            public string Icon { get; set; }
        }

        public partial class c_sg_operaciones
        {
            public int ID { get; set; }
            public string Operacion { get; set; }
            public bool? Activo { get; set; }
        }

        #endregion

        #region Cliente

        public partial class ClienteRioSulApp
        {
            public string cve_cliente { get; set; }
            public string nom_cliente { get; set; }
            public string nom_corto { get; set; }
            public string nUm_rfc { get; set; }
            public string dir_cliente { get; set; }
            public string nom_colonia { get; set; }
            public string num_cp { get; set; }
            public string num_telefono { get; set; }
            public string num_fax { get; set; }
            public string nom_contacto { get; set; }
            public string nom_estatus { get; set; }
            public string nom_ciudad { get; set; }
            public string nom_estado { get; set; }
        }

        public partial class RES_CLIENTE
        {
            public List<ClienteRioSulApp> Clientes { get; set; }
            public HttpResponseMessage Message { get; set; }
            public string HEAD_CLIENTE { get; set; }
        }

        public partial class RES_REF_CRUZADA
        {
            public VST_C_Clientes Ref_C_Clientes { get; set; }
            public List<VST_C_ClientesReferencia> Ref_C_ClientesReferencia { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public class VST_C_Clientes
        {
            public int IdClienteRef { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
        }

        public class VST_C_ClientesReferencia
        {
            public int IdCliente { get; set; }
            public int IdClienteRef { get; set; }
            public string Cve_Cliente { get; set; }
            public string Nom_Cliente { get; set; }
            public string Nom_Corto { get; set; }
            public string Num_Rfc { get; set; }
            public string Dir_Cliente { get; set; }
            public string Nom_Colonia { get; set; }
            public string Num_Cp { get; set; }
            public string Num_Telefono { get; set; }
            public string Num_Fax { get; set; }
            public string Nom_Contacto { get; set; }
            public string Nom_Estatus { get; set; }
            public string Nom_Ciudad { get; set; }
            public string Nom_Estado { get; set; }
        }

        public class REQ_ClienteRef
        {
            [Required] public string Descripcion { get; set; }
            [Required] public string Observaciones { get; set; }
            [Required] public string CLI_REF { get; set; }
        }

        public class VST_C_ClientesRef
        {
            [Required] public string Cve_Cliente { get; set; }
            [Required] public string Nom_Cliente { get; set; }
            [Required] public string Nom_Corto { get; set; }
            public string Num_Rfc { get; set; }
            public string Dir_Cliente { get; set; }
            public string Nom_Colonia { get; set; }
            public string Num_Cp { get; set; }
            public string Num_Telefono { get; set; }
            public string Num_Fax { get; set; }
            [Required] public string Nom_Contacto { get; set; }
            [Required] public string Nom_Estatus { get; set; }
            public string Nom_Ciudad { get; set; }
            public string Nom_Estado { get; set; }
        }

        #endregion

        #region CorreosElectronicos

        public partial class RES_CORREO
        {
            public HttpResponseMessage Message { get; set; }
            public List<Models.VST_CORREOS_AUDITORIA> CorreosA { get; set; }
        }

        public partial class RES_NVO_CORREO
        {
            public bool Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class REQ_NVO_CORREO
        {
            [Required] public int USU_ID { get; set; }
            public bool Corte { get; set; }
            public bool Confeccion { get; set; }
            public bool ProcesosEspeciales { get; set; }
            public bool Lavanderia { get; set; }
            public bool Terminado { get; set; }
            public bool Calidad { get; set; }
        }

        public partial class RES_EDT_CORREO
        {
            public HttpResponseMessage Message { get; set; }
            public bool Corte { get; set; }
            public bool Confeccion { get; set; }
            public bool ProcesosEspeciales { get; set; }
            public bool Lavanderia { get; set; }
            public bool Terminado { get; set; }
        }

        #endregion

        #region CatalogoSegundas

        public partial class RES_SEGUNDAS
        {
            public HttpResponseMessage Message { get; set; }
            public List<Estilos> estilos { get; set; }
        }

        public partial class Estilos
        {
            public string estilo { get; set; }
            public string des_estilo { get; set; }
        }

        public partial class RES_NEW_SEGUNDA
        {
            public HttpResponseMessage Message { get; set; }
        }

        public partial class REQ_C_SEGUNDA
        {
            public string Estilo { get; set; }
            public string Descripcion { get; set; }
            public decimal Porcentaje_Tela { get; set; }
            public decimal Porcentaje_Corte { get; set; }
            public decimal Porcentaje_Confeccion { get; set; }
            public decimal Porcentaje_Lavanderia { get; set; }
            public decimal Porcentaje_ProcesosEspeciales { get; set; }
            public decimal Porcentaje_Terminado { get; set; }
            public decimal Costo_Estilo { get; set; }
            public decimal Costo_Segunda { get; set; }
        }

        public partial class RES_C_SEGUNDA
        {
            public int IdSegunda { get; set; }
            public string Estilo { get; set; }
            public string Descripcion { get; set; }
            public decimal Porcentaje_Tela { get; set; }
            public decimal Porcentaje_Corte { get; set; }
            public decimal Porcentaje_Confeccion { get; set; }
            public decimal Porcentaje_Lavanderia { get; set; }
            public decimal Porcentaje_ProcesosEspeciales { get; set; }
            public decimal Porcentaje_Terminado { get; set; }
            public decimal Costo_Estilo { get; set; }
            public decimal Costo_Segunda { get; set; }
        }

        #endregion

        #region CatalogoCortadores

        public partial class REQ_CORTADOR
        {
            [Required] public int IdSubModulo { get; set; }
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
        }

        public partial class REQ_EDT_CORTADOR
        {
            [Required] public int ID { get; set; }
            [Required] public int IdUsuario { get; set; }
            public string Clave { get; set; }
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
        }

        public partial class RES_CORTADOR
        {
            public bool Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_BUS_CORTADOR
        {
            public List<Models.VST_CORTADORES> Vst_Cortadores { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_EDT_CORTADOR
        {
            public Models.VST_CORTADORES Vst_Cortador { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_TOLERANCIA
        {
            public HttpResponseMessage Message { get; set; }
            public List<TOLERANCIA> Tolerancias { get; set; }
        }

        public partial class TOLERANCIA
        {
            public int? IdTolerancia { get; set; }
            public int Descripcion { get; set; }
            public bool ToleranciaPositiva { get; set; }
            public bool ToleranciaNegativa { get; set; }
            public int Numerador { get; set; }
            public int Denominador { get; set; }
        }

        #endregion

        #region CatalogoCortadorTendido

        public partial class REQ_TENDIDO
        {
            [Required] public int IdSubModulo { get; set; }
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            [Required] public int TipoTendido { get; set; }
        }

        public partial class REQ_EDT_TENDIDO
        {
            [Required] public int ID { get; set; }
            public int IdUsuario { get; set; }
            public string Clave { get; set; }
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            [Required] public int TipoTendido { get; set; }
        }

        public partial class RES_TENDIDO
        {
            public bool Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_BUS_TENDIDO
        {
            public List<Models.VST_CORTADORES> Vst_Cortadores { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_EDT_TENDIDO
        {
            public Models.VST_CORTADORES Vst_Cortador { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        #endregion

        #region CatalogoCortadorTipoTendido

        public partial class REQ_TIPO_TENDIDO
        {
            [Required] public int IdSubModulo { get; set; }
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
        }

        public partial class REQ_EDT_TIPO_TENDIDO
        {
            [Required] public int ID { get; set; }
            [Required] public int IdUsuario { get; set; }
            public string Clave { get; set; }
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
        }

        public partial class RES_TIPO_TENDIDO
        {
            public bool Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_BUS_TIPO_TENDIDO
        {
            public List<Models.VST_CORTADORES> Vst_Cortadores { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_EDT_TIPO_TENDIDO
        {
            public Models.VST_CORTADORES Vst_Cortador { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        #endregion

        #region PosicionCortador

        public partial class REQ_POSICION_CORTE
        {
            [Required] public int IdSubModulo { get; set; }
            public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public List<C_Posicion> Defecto { get; set; }
        }

        public partial class C_Posicion
        {
            public int IdCortador { get; set; }
        }

        public partial class RES_EDT_POSICION_CORTE
        {
            public Models.VST_CORTADORES Vst_Cortador { get; set; }
            public List<Models.VST_POSICION_CORTADOR> Vst_Posicion { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class EDT_POSICION_CORTE
        {
            [Required] public int ID { get; set; }
            public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public List<C_Posicion> Defecto { get; set; }
        }

        #endregion

        #region DefectosCortador

        public partial class REQ_DEFECTO_CORTE
        {
            [Required] public int IdSubModulo { get; set; }
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public string Imagen { get; set; }
        }

        public partial class REQ_EDT_DEFECTO_CORTE
        {
            [Required] public int ID { get; set; }
            public int IdUsuario { get; set; }
            public string Clave { get; set; }
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public string Imagen { get; set; }
        }

        public partial class RES_DEFECTO_CORTE
        {
            public bool Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_BUS_DEFECTO_CORTE
        {
            public List<Models.VST_CORTADORES> Vst_Cortadores { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_EDT_DEFECTO_CORTE
        {
            public Models.VST_CORTADORES Vst_Cortador { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        #endregion

        #region DefectosConfeccion

        public partial class REQ_DEFECTO_CONFECCION
        {
            [Required] public int IdSubModulo { get; set; }
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public string Imagen { get; set; }
        }

        public partial class REQ_EDT_DEFECTO_CONFECCION
        {
            [Required] public int ID { get; set; }
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public string Imagen { get; set; }
        }

        public partial class RES_DEFECTO_CONFECCION
        {
            public bool Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_BUS_DEFECTO_CONFECCION
        {
            public List<Models.VST_CONFECCION> Vst_Confeccion { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_EDT_DEFECTO_CONFECCION
        {
            public Models.VST_CONFECCION Vst_Confeccion { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_EDT_OPERACION_CONFECCION
        {
            public Models.VST_CONFECCION Vst_Confeccion { get; set; }
            public List<Models.VST_OPERACION_CONFECCION> Vst_Oper_Conf { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_PLANTAS
        {
            public HttpResponseMessage Message { get; set; }
            public List<PLANTAS> Vst_Plantas { get; set; }
        }

        public partial class PLANTAS
        {
            public string Descripcion { get; set; }
            public string Planta { get; set; }
        }

        public partial class RES_PLANTA
        {
            public HttpResponseMessage Message { get; set; }
        }

        public partial class REQ_PLANTA_AREA
        {
            [Required] public string Planta { get; set; }
            [Required] public string Descripcion { get; set; }
            [Required] public List<AREA_REL> Areas { get; set; }
        }

        public partial class AREA_REL
        {
            public int IdArea{get;set;}
        }

        public partial class EDT_PLANTA_AREA
        {
            public int IdPlanta { get; set; }
            public List<AREA_REL> Areas { get; set; }
        }

        public partial class RES_PLANTAS_AREAS_REL
        {
            public HttpResponseMessage Message { get; set; }
            public int IdPlanta { get; set; }
            public string Descripcion { get; set; }
            public string Planta { get; set; }
            public List<Models.VST_PLANTAS_AREAS> Areas { get; set; }
        }

        #endregion

        #region ProcesosEspecialee

        public class REQ_DEFECTO_PROCESO_ESP
        {
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public string Imagen { get; set; }

        }

        public partial class REQ_POSICION_PROC_ESP
        {
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public string Imagen { get; set; }
            public List<R_OPERACIONES> Operacion { get; set; }
        }

        public partial class RES_DEFECTO_PROCESO_ESP
        {
            public bool Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_BUS_DEFECTO_PROCESO_ESP
        {
            public List<Models.VST_PROCESOS_ESPECIALES> Vst_ProcesosEspeciales { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_EDT_PROCESOS_ESPECIALES
        {
            public Models.VST_PROCESOS_ESPECIALES Vst_ProcesosEsp { get; set; }
            public HttpResponseMessage Message { get; set; }
            public List<Models.VST_POSICION_PROCESOS_ESPECIALES> Vst_Posicion { get; set; }
        }

        public partial class REQ_EDT_DEFECTO_PROCESO_ESP
        {
            [Required] public int ID { get; set; }
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public string Imagen { get; set; }
        }

        public partial class N_OPERACION
        {
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public string Imagen { get; set; }
            public List<R_DEFECTOS> Defectos { get; set; }
        }

        public partial class R_DEFECTOS
        {
            [Required] public int ID { get; set; }
            public string Clave { get; set; }
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
        }

        public partial class R_OPERACIONES
        {
            [Required] public int ID { get; set; }
            public string Clave { get; set; }
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
        }

        public partial class R_PROCESO_ESP_OP
        {
            public Models.VST_PROCESOS_ESPECIALES Vst_ProcesosEsp { get; set; }
            public List<R_DEFECTOS> Defectos { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class R_PROCESO_ESP_PO
        {
            public Models.VST_PROCESOS_ESPECIALES Vst_ProcesosEsp { get; set; }
            public List<R_OPERACIONES> Operaciones { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        #endregion

        #region Lavanderia

        public partial class RES_DEFECTO_LAV
        {
            public bool Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_POSICION_LAV
        {
            public string Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public class REQ_DEFECTO_LAV
        {
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public string Imagen { get; set; }
            public List<DEFECTO_REF> Defecto { get; set; }
        }

        public partial class DEFECTO_REF
        {
            public int ID { get; set; }
        }

        public partial class OPERACION_REF
        {
            public int ID { get; set; }
        }

        public partial class N_POSICION_LAV
        {
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public string Imagen { get; set; }
            public List<OPERACION_REF> Operacion { get; set; }
        }

        public partial class E_POSICION_LAV
        {
            [Required] public int ID { get; set; }
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public string Imagen { get; set; }
            public List<OPERACION_REF> Operacion { get; set; }
        }

        public partial class RES_BUS_DEFECTO_LAVANDERIA
        {
            public List<Models.VST_LAVANDERIA> Vst_Lavanderia { get; set; }
            public List<Models.VST_PROCESOS_ESPECIALES> Vst_ProcesosEspeciales{ get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_EDT_LAVANDERIA
        {
            public Models.VST_LAVANDERIA Vst_Lavanderia { get; set; }
            public List<DEFECTO_OPEREACION> Defecto { get; set; }
            public HttpResponseMessage Message { get; set; }            
        }

        public partial class DEFECTO_OPEREACION
        {
            public int ID { get; set; }
            public string Clave { get; set; }
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
        }

        public partial class REQ_EDT_DEFECTO_LAVANDERIA
        {
            [Required] public int ID { get; set; }
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public string Imagen { get; set; }
        }

        public partial class LAVANDERIA_P
        {
            public Models.VST_LAVANDERIA Vst_Lavanderia { get; set; }
            public List<DEFECTO_OPEREACION> Operacion { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        #endregion

        #region Terminado

        #region Defecto/Terminado

        public partial class RES_DEFECTO_TERMINADO
        {
            public string Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public class REQ_DEFECTO_TERMINADO
        {
            [Required] public int IdSubModulo { get; set; }
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public string Imagen { get; set; }

        }

        public partial class REQ_EDT_DEFECTO_TERMINADO
        {
            [Required] public int ID { get; set; }
            [Required] public int IdUsuario { get; set; }
            [Required] public string Clave { get; set; }
            public string Nombre { get; set; }
            [Required] public string Descripcion { get; set; }
            public string Observaciones { get; set; }
            public string Imagen { get; set; }
        }

        public partial class RES_EDT_DEFECTO_TERMINADO
        {
            public Models.VST_TERMINADO Vst_Terminado { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public partial class RES_BUS_DEFECTO_CORTE_TERMINADO
        {
            public List<Models.VST_TERMINADO> Vst_Terminado { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        #endregion


        #region Operaciones/Terminado

        public partial class RES_OPERACION_TERMINADO
        {
            public bool Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
            public string Message2 { get; set; }
        }

        public class REQ_OPERACION_TERMINADO
        {
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }

        }

        public class REQ_EDT_OPERACION_TERMINADO
        {
            [Required] public int ID { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
        }

        public class RES_BUS_OPERACION_TERMINADO
        {
            public List<Models.C_Operacion_Terminado> COperacionTerminados { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public class RES_BUS_ONE_OPERACION_TERMINADO
        {
            public Models.C_Operacion_Terminado c_operacion_t { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        #endregion

        #region Posicion/Terminado

        public partial class RESPUESTA_MENSAJE
        {
            public bool Hecho { get; set; }
            public HttpResponseMessage Message { get; set; }
            public string Message2 { get; set; }
        }

        public class REQ_POSICION_TERMINADO
        {
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
        }

        public class RES_BUS_POSICION_TERMINADO
        {
            public List<Models.C_Posicion_Terminado> c_posicion_t { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public class RES_BUS_ONE_POSICION_TERMINADO
        {
            public Models.C_Posicion_Terminado c_posicion_t { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public class REQ_EDIT_POSICION_TERMINADO
        {
            [Required] public int ID { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
        }

        #endregion

        #region Origen/Terminado

        public class REQ_ORIGEN_TERMINADO
        {
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
        }

        public class RES_BUS_ORIGEN_TERMINADO
        {
            public List<Models.C_Origen_Terminado> c_origen_t { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public class RES_BUS_ONE_ORIGEN_TERMINADO
        {
            public Models.C_Origen_Terminado c_origen_t { get; set; }
            public HttpResponseMessage Message { get; set; }
        }

        public class REQ_EDIT_ORIGEN_TERMINADO
        {
            [Required] public int ID { get; set; }
            [Required] public string Clave { get; set; }
            [Required] public string Nombre { get; set; }
        }

        #endregion


        #endregion
    }
}