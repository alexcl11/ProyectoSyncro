using System;
using System.Collections.Generic;

namespace ProyectoSyncro.Models;

public partial class MetaOpcione
{
    public int IdOpcion { get; set; }

    public int IdColumna { get; set; }

    public string Valor { get; set; } = null!;

    public string? Color { get; set; }

    public virtual MetaColumna IdColumnaNavigation { get; set; } = null!;
}
