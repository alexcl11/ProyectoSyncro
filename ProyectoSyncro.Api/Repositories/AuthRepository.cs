using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProyectoSyncro.Api.Data;
using ProyectoSyncro.Api.Helpers;
using ProyectoSyncro.Models;
using System.Data;

namespace ProyectoSyncro.Api.Repositories
{
    public class AuthRepository
    {
        private ApplicationDbContext context;

        public AuthRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<UserSession> LoginUserAsync(string email, string password)
        {
            Login user = await (from datos in this.context.Login
                           where datos.Email == email 
                           select datos).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }
            else
            {
                // NECESITAMOS EL SALT DEL USUARIO
                string salt = user.Salt;
                // CIFRAMOS EL PASSWORD CON SU SALT A NIVEL BYTE[]
                byte[] temp = HelperCryptography.EncryptPassword(password, salt);
                //RECUPERAMOS LOS BYTES[] DEL PASSWORD DE LA BBDD
                byte[] passBytes = user.PasswordAux;

                bool response = HelperTools.CompareArrays(temp, passBytes);
                if (response)
                {
                    Usuario usuario = await (from datos in this.context.Usuarios
                                       where datos.Email == email
                                       select datos).FirstOrDefaultAsync();
                    Empresa empresa = await (from datos in this.context.Empresas
                                             where datos.IdEmpresa == usuario.IdEmpresa
                                             select datos).FirstOrDefaultAsync();
                    UserSession userSession = new UserSession()
                    {
                        IdUsuario = usuario.IdUsuario,
                        Email = usuario.Email,
                        Nombre = usuario.Nombre,
                        Admin = usuario.EsAdmin,
                        IdEmpresa = empresa.IdEmpresa,
                        NombreEmpresa = empresa.NombreEmpresa,
                        IsPremium = empresa.IsPremium
                    };
                    return userSession;
                }
                else
                {
                    return null;
                }
            }

            }

        public async Task<int> RegisterEmpresaUserAsync(string cif,
            string nombreEmpresa, string nombreUsuario,
            string email, string password)
        {
            string salt = HelperTools.GenerateSalt();
            byte[] passwordHash = HelperCryptography.EncryptPassword(password, salt);

            var resultParam = new SqlParameter("@Result", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            string sql = "EXEC SP_CREATE_EMPRESA @CIFEmpresa, @nombreEmpresa, @nombreUser, @emailUser, @passwordUser, @esAdmin, @salt, @passwordHash, @Result OUTPUT";

            await this.context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@CIFEmpresa", cif),
                new SqlParameter("@nombreEmpresa", nombreEmpresa),
                new SqlParameter("@nombreUser", nombreUsuario),
                new SqlParameter("@emailUser", email),
                new SqlParameter("@passwordUser", password),
                new SqlParameter("@esAdmin", true),
                new SqlParameter("@salt", salt),
                new SqlParameter("@passwordHash", passwordHash),
                resultParam
            );

            return (int)resultParam.Value;
        }


    }
}
