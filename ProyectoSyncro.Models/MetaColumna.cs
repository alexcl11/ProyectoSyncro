using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProyectoSyncro.Models;

public partial class MetaColumna
{
    public int IdColumna { get; set; }

    public int IdTabla { get; set; }

    public string Nombre { get; set; } = null!;

    public string TipoDato { get; set; } = null!;

    public int? IdTablaRelacionada { get; set; }

    [JsonIgnore]
    public virtual MetaTabla IdTablaNavigation { get; set; } = null!;
    [JsonIgnore]
    public virtual MetaTabla? IdTablaRelacionadaNavigation { get; set; }
    [JsonIgnore]
    public virtual ICollection<MetaOpcione> MetaOpciones { get; set; } = new List<MetaOpcione>();
}
