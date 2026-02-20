using System;
using System.Collections.Generic;

namespace ProyectoSyncro.Models;

public partial class MetaColumna
{
    public int IdColumna { get; set; }

    public int IdTabla { get; set; }

    public string Nombre { get; set; } = null!;

    public string TipoDato { get; set; } = null!;

    public int? IdTablaRelacionada { get; set; }

    public virtual MetaTabla IdTablaNavigation { get; set; } = null!;

    public virtual MetaTabla? IdTablaRelacionadaNavigation { get; set; }

    public virtual ICollection<MetaOpcione> MetaOpciones { get; set; } = new List<MetaOpcione>();
}
