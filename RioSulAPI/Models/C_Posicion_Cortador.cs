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
    
    public partial class C_Posicion_Cortador
    {
        public int ID { get; set; }
        public int IdPosicion { get; set; }
        public int IdCortador { get; set; }
    
        public virtual C_Cort_Cortadores C_Cort_Cortadores { get; set; }
        public virtual C_Cort_Cortadores C_Cort_Cortadores1 { get; set; }
    }
}
