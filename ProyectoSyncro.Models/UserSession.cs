namespace ProyectoSyncro.Models
{
    public class UserSession
    {
        public int IdUsuario { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public bool Admin { get; set; }
        public int IdEmpresa { get; set; }
        public string NombreEmpresa { get; set; }
        public bool IsPremium { get; set; }
    }
}
