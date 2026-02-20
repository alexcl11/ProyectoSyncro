using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProyectoSyncro.Data;
using System.Data.Common;

namespace ProyectoSyncro.Repositories
{
    public class BaseRepository
    {
        private ApplicationDbContext context;
        public BaseRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<string>> GetTablasEmpresaAsync(int idEmpresa) 
        {
            string sql = "SP_ALL_TABLAS_EMPRESA";
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            using (DbCommand com =
                this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;
                com.Parameters.Add(paramIdEmpresa);
                await com.Connection.OpenAsync();
                DbDataReader reader = await com.ExecuteReaderAsync();
                List<string> tablas = new List<string>();
                while (await reader.ReadAsync())
                {
                    string tabla = reader["Tabla"].ToString();
                    tablas.Add(tabla);
                }
                await reader.CloseAsync();
                await com.Connection.CloseAsync();
                return tablas;
            }
        }

    }
}
