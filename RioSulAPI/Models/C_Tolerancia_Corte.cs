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
    
    public partial class C_Tolerancia_Corte
    {
        public int IdTolerancia { get; set; }
        public int Descripcion { get; set; }
        public bool ToleranciaPositiva { get; set; }
        public bool ToleranciaNegativa { get; set; }
        public int Numerador { get; set; }
        public int Denominador { get; set; }
    }
}
