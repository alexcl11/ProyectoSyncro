using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSyncro.Models
{
    [Table("V_LOGIN")]
    public class Login
    {
        [Key]
        [Column("Email")]
        public string Email { get; set; }
        [Column("Salt")]
        public string Salt { get; set; }
        [Column("Password")]
        public byte[] PasswordAux { get; set; }
    }
}
