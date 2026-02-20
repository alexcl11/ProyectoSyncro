using System;
using System.Collections.Generic;

namespace ProyectoSyncro.Models;

public partial class MetaTabla
{
    public int IdTabla { get; set; }

    public int IdEmpresa { get; set; }

    public string Nombre { get; set; } = null!;

    public string NombreInterno { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public virtual Empresa IdEmpresaNavigation { get; set; } = null!;

    public virtual ICollection<MetaColumna> MetaColumnaIdTablaNavigations { get; set; } = new List<MetaColumna>();

    public virtual ICollection<MetaColumna> MetaColumnaIdTablaRelacionadaNavigations { get; set; } = new List<MetaColumna>();
}
