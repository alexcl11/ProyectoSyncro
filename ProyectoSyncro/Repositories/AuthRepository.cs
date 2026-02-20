using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProyectoSyncro.Data;
using ProyectoSyncro.Models;

namespace ProyectoSyncro.Repositories
{
    public class AuthRepository
    {
        private ApplicationDbContext context;

        public AuthRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Usuario> LoginUserAsync(string email, string contra)
        {
            var consulta = from datos in this.context.Usuarios
                           where datos.Email == email && datos.Password == contra
                           select datos;
            return await consulta.FirstOrDefaultAsync();
        }

        public async Task CrearEmpresaAsync(string cif,
            string nombreEmpresa, string nombreUsuario,
            string email, string password)
        {
            string sql = "EXEC SP_CREATE_EMPRESA @CIFEmpresa, @nombreEmpresa, @nombreUser, @emailUser, @passwordUser, @esAdmin";

            await this.context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@CIFEmpresa", cif),
                new SqlParameter("@nombreEmpresa", nombreEmpresa),
                new SqlParameter("@nombreUser", nombreUsuario),
                new SqlParameter("@emailUser", email),
                new SqlParameter("@passwordUser", password),
                new SqlParameter("@esAdmin", true)
            );
        }


    }
}
