using ProyectoSyncro.Models;

public partial class UsuarioAux
{
    public int IdUsuario { get; set; }

    public string? Salt { get; set; }

    public byte[]? Password { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}