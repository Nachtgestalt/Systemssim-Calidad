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
    
    public partial class VST_CORREOS_AUDITORIA
    {
        public int CorreoId { get; set; }
        public int UsuarioId { get; set; }
        public string Email { get; set; }
        public bool Corte { get; set; }
        public bool Confeccion { get; set; }
        public bool ProcesosEspeciales { get; set; }
        public bool Lavandería { get; set; }
        public bool Terminado { get; set; }
    }
}
