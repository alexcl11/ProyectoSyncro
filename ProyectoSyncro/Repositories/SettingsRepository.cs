using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProyectoSyncro.Data;
using ProyectoSyncro.Helpers;
using ProyectoSyncro.Models;
using System.Data.Common;

namespace ProyectoSyncro.Repositories
{
    public class SettingsRepository
    {
        private ApplicationDbContext context;
        public SettingsRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<Usuario>> GetUsuariosEmpresaAsync(int idEmpresa)
        {
            return await this.context.Usuarios
                .Where(u => u.IdEmpresa == idEmpresa)
                .ToListAsync();
        }
        public async Task<Empresa> GetEmpresaAsync(int idEmpresa)
        {
            return await this.context.Empresas
                .Where(e => e.IdEmpresa == idEmpresa)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateEmpresaAsync(int idEmpresa, string cif, string nombre, bool activa)
        {
            
            string sql = "SP_UPDATE_EMPRESA @IdEmpresa, @CIF, @Nombre, @Activo";
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            SqlParameter paramCif = new SqlParameter("@CIF", cif);
            SqlParameter paramNombre = new SqlParameter("@Nombre", nombre);
            SqlParameter paramActivo = new SqlParameter("@Activo", activa);
            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.Text;
                com.CommandText = sql;
                com.Parameters.Add(paramIdEmpresa);
                com.Parameters.Add(paramCif );
                com.Parameters.Add(paramNombre );
                com.Parameters.Add(paramActivo );

                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
            }
        }

        public async Task UpdateUserAsync(int idUsuario, int idEmpresa, string nombre, 
           string email, bool esAdmin)
        {
            string sql = "SP_UPDATE_USER @IdUsuario, @IdEmpresa, @nombre, @email, @esAdmin";

            SqlParameter paramIdUsuario = new SqlParameter("@IdUsuario", idUsuario);
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            SqlParameter paramNombre = new SqlParameter("@nombre", nombre);
            SqlParameter paramEmail = new SqlParameter("@email", email);
            SqlParameter paramEsAdmin = new SqlParameter("@esAdmin", esAdmin);

            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.Text;
                com.CommandText = sql;
                com.Parameters.Add(paramIdUsuario);
                com.Parameters.Add(paramIdEmpresa);
                com.Parameters.Add(paramNombre);
                com.Parameters.Add(paramEmail);
                com.Parameters.Add(paramEsAdmin);

                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
            }
        }

        public async Task CreateUserAsync(int idEmpresa, string nombre, string email, bool esAdmin, string password)
        {
            string salt = HelperTools.GenerateSalt();
            byte[] passwordHash = HelperCryptography.EncryptPassword(password, salt);

            string sql = "SP_CREATE_USER";
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            SqlParameter paramNombre = new SqlParameter("@nombre", nombre);
            SqlParameter paramEmail = new SqlParameter("@email", email);
            SqlParameter paramPassword = new SqlParameter("@password", password);
            SqlParameter paramEsAdmin = new SqlParameter("@esAdmin", esAdmin);
            SqlParameter paramSalt = new SqlParameter("@salt", salt);
            SqlParameter paramPasswordHash = new SqlParameter("@passwordHash", passwordHash);

            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;

                com.Parameters.Add(paramIdEmpresa);
                com.Parameters.Add(paramNombre);
                com.Parameters.Add(paramEmail);
                com.Parameters.Add(paramPassword);
                com.Parameters.Add(paramEsAdmin);
                com.Parameters.Add(paramSalt);
                com.Parameters.Add(paramPasswordHash);

                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
            }
        }

        public async Task DeleteUserAsync(int idUsuario, int idEmpresa) 
        {
            string sql = "SP_DELETE_USER";
            SqlParameter paramIdUsuario = new SqlParameter("@IdUsuario", idUsuario);
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);

            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;

                com.Parameters.Add(paramIdUsuario);
                com.Parameters.Add(paramIdEmpresa);

                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
            }
        }

        public async Task DeleteEmpresaAsync(int idEmpresa)
        {
            string sql = "SP_DELETE_EMPRESA";
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);

            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;

                com.Parameters.Add(paramIdEmpresa);
                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
            }
        }
    }
}
