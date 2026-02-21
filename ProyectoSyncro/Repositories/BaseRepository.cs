using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProyectoSyncro.Data;
using ProyectoSyncro.Models;
using System.Data.Common;
using System.Text.Json;

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

        public async Task<List<Dictionary<string, object>>> GetDatosTablaEmpresaAsync(int idEmpresa, string nombreTabla)
        {
            string sql = "SP_MOTRAR_TABLA_EMPRESA";
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            SqlParameter paramNombreTabla = new SqlParameter("@nombreTabla", nombreTabla);
            var datos = new List<Dictionary<string, object>>();
            using (DbCommand com =
                this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;
                com.Parameters.Add(paramIdEmpresa);
                com.Parameters.Add(paramNombreTabla);
                await com.Connection.OpenAsync();
                DbDataReader reader = await com.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var fila = new Dictionary<string, object>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string nombreColumna = reader.GetName(i);

                        if (nombreColumna.ToLower() != "fechacreacion" || nombreColumna.ToLower() != "id")
                        {
                            object valorDato = reader.IsDBNull(i) ? null : reader.GetValue(i);

                            fila.Add(nombreColumna, valorDato);
                            
                        }
                        
                    }
                    datos.Add(fila);
                }
                await reader.CloseAsync();
                await com.Connection.CloseAsync();
                return datos;
            }
        }

        public async Task CreateTablaEmpresaAsync(int idEmpresa, string nombreTabla)
        {
            string sql = "SP_UPSERT_TABLA_SCHEMA_EMPRESA";
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            SqlParameter paramNombreTablaNew = new SqlParameter("@nombreTablaNew", nombreTabla);
            SqlParameter paramNombreTablaOld = new SqlParameter("@nombreTablaOld", "");
            using (DbCommand com =
                this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;
                com.Parameters.Add(paramIdEmpresa);
                com.Parameters.Add(paramNombreTablaNew);
                com.Parameters.Add(paramNombreTablaOld);

                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
            }
        }

        public async Task<List<MetaColumna>> GetColumnasTablaAsync
            (int idEmpresa, string nombreTabla)
        {
            var consulta= from columna in this.context.MetaColumnas
                        join tabla in this.context.MetaTablas
                        on columna.IdTabla equals tabla.IdTabla
                        where tabla.IdEmpresa == idEmpresa && tabla.Nombre == nombreTabla
                        select columna;

            return await consulta.ToListAsync();
        }

        public async Task InsertRegistroTablaEmpresaAsync
            (int idEmpresa, string nombreTabla, Dictionary<string, string> valores)
        {
            string jsonData = JsonSerializer.Serialize(valores);
            string sql = "SP_INSERT_ROW_DINAMICO";
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            SqlParameter paramNombreTabla = new SqlParameter("@NombreTabla", nombreTabla);
            SqlParameter paramJsonData = new SqlParameter("@JsonData", jsonData);
            using (DbCommand com =
                this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;
                com.Parameters.Add(paramIdEmpresa);
                com.Parameters.Add(paramNombreTabla);
                com.Parameters.Add(paramJsonData);
                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
            }
        }

    }
}
