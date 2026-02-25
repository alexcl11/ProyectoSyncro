using System;
using System.Collections.Generic;

namespace ProyectoSyncro.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public int IdEmpresa { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public bool EsAdmin { get; set; }

    public virtual Empresa IdEmpresaNavigation { get; set; } = null!;
    public virtual UsuarioAux? UsuarioAux { get; set; }
}
