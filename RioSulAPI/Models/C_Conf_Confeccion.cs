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
    
    public partial class C_Conf_Confeccion
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public C_Conf_Confeccion()
        {
            this.C_Operacion_Confeccion = new HashSet<C_Operacion_Confeccion>();
            this.C_Operacion_Confeccion1 = new HashSet<C_Operacion_Confeccion>();
            this.C_Plantas_Areas = new HashSet<C_Plantas_Areas>();
        }
    
        public int ID { get; set; }
        public int IdSubModulo { get; set; }
        public int IdUsuario { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Observaciones { get; set; }
        public bool Activo { get; set; }
        public System.DateTime FechaCreacion { get; set; }
        public string Imagen { get; set; }
    
        public virtual C_Sg_Usuarios C_Sg_Usuarios { get; set; }
        public virtual C_SubModulos C_SubModulos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C_Operacion_Confeccion> C_Operacion_Confeccion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C_Operacion_Confeccion> C_Operacion_Confeccion1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C_Plantas_Areas> C_Plantas_Areas { get; set; }
    }
}
