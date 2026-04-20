using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProyectoSyncro.Models;

public partial class Empresa
{
    public int IdEmpresa { get; set; }

    public string? Cifempresa { get; set; }

    public string NombreEmpresa { get; set; } = null!;

    public string NombreSchema { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public bool Activo { get; set; }

    public bool IsPremium { get; set; }

    [JsonIgnore]
    public virtual ICollection<MetaTabla> MetaTablas { get; set; } = new List<MetaTabla>();
    [JsonIgnore]

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
