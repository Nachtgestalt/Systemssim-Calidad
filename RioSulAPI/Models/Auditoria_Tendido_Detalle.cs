
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
    
public partial class Auditoria_Tendido_Detalle
{

    public int IdAuditoriaCorteDetalle { get; set; }

    public int IdAuditoriaCorte { get; set; }

    public int IdCortador { get; set; }

    public string Serie { get; set; }

    public string Bulto { get; set; }

    public int IdPosicion { get; set; }

    public int IdDefecto { get; set; }

    public int IdCortado { get; set; }

    public int Cantidad { get; set; }

    public string Aud_Imagen { get; set; }

    public string Nota { get; set; }

    public string Archivo { get; set; }

    public int Segundas { get; set; }



    public virtual Auditoria Auditoria { get; set; }

}

}
