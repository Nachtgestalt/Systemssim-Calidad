//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RioSulAPI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class VST_AUDITORIA_PROC_ESP_DETALLE
    {
        public int IdAuditoria { get; set; }
        public int IdPosicion { get; set; }
        public string ClavePosicion { get; set; }
        public string NombrePosicion { get; set; }
        public string DescripcionPosicion { get; set; }
        public int IdOperacion { get; set; }
        public string ClaveOperacion { get; set; }
        public string NombreOperacion { get; set; }
        public string DescripcionOperacion { get; set; }
        public int IdDefecto { get; set; }
        public string ClaveDefecto { get; set; }
        public string NombreDefecto { get; set; }
        public string DescripcionDefecto { get; set; }
        public int Cantidad { get; set; }
        public string Imagen { get; set; }
        public string Notas { get; set; }
        public string Archivo { get; set; }
    }
}
