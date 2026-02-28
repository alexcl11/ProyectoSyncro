using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Options;
using ProyectoSyncro.Data;
using ProyectoSyncro.Models;
using System.Data.Common;
using System.Globalization;
using System.Net.NetworkInformation;
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

        public async Task CreateColumnaTablaAsync
            (int idEmpresa, string nombreTabla, string nombreColumna, 
            string tipoDato, string? nombreTablaRelacionada)
        {
            var idTablaRelacionada = 0;
            if (nombreTablaRelacionada != null)
            {
                idTablaRelacionada = await (from datos in this.context.MetaTablas
                                                where datos.IdEmpresa == idEmpresa &&
                                                datos.Nombre == nombreTablaRelacionada
                                                select datos.IdTabla).FirstOrDefaultAsync();
            }
            string sql = "SP_INSERTAR_COLUMNA_TABLA_SCHEMA_EMPRESA";
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            SqlParameter paramNombreTabla = new SqlParameter("@nombreTabla", nombreTabla);
            SqlParameter paramNombreColumna = new SqlParameter("@nombreColumna", nombreColumna);
            SqlParameter paramTipoDato = new SqlParameter("@tipoDato", tipoDato);
            SqlParameter paramIdTablaRelacion;
            if (idTablaRelacionada == 0)
            {
                paramIdTablaRelacion = new SqlParameter("@idTablaRelacionada", null);
            }
            else
            {
                paramIdTablaRelacion = new SqlParameter("@idTablaRelacionada", idTablaRelacionada);
            }


                using (DbCommand com =
                    this.context.Database.GetDbConnection().CreateCommand())
                {
                    com.CommandType = System.Data.CommandType.StoredProcedure;
                    com.CommandText = sql;
                    com.Parameters.Add(paramIdEmpresa);
                    com.Parameters.Add(paramNombreTabla);
                    com.Parameters.Add(paramNombreColumna);
                    com.Parameters.Add(paramTipoDato);
                    com.Parameters.Add(paramIdTablaRelacion);

                    await com.Connection.OpenAsync();
                    await com.ExecuteNonQueryAsync();
                    await com.Connection.CloseAsync();
                }
        }

        public async Task InsertarOpcionColumnaAsync(int idEmpresa, string nombreTabla, string nombreColumna, string valor, string color)
        {

            string sql = "SP_INSERTAR_OPCION_COLUMNA";
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            SqlParameter paramNombreTabla = new SqlParameter("@NombreTabla", nombreTabla);
            SqlParameter paramNombreColumna = new SqlParameter("@NombreColumna", nombreColumna);
            SqlParameter paramValor= new SqlParameter("@Valor", valor);
            SqlParameter paramColor = new SqlParameter("@Color", color);

            using (var com = this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;

                com.Parameters.Add(paramIdEmpresa);
                com.Parameters.Add(paramNombreTabla);
                com.Parameters.Add(paramNombreColumna);
                com.Parameters.Add(paramValor);
                com.Parameters.Add(paramColor); 

                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
            }
        }

        public async Task<List<MetaColumna>> GetColumnasTablaAsync
            (int idEmpresa, string nombreTabla)
        {
            var consulta= (from columna in this.context.MetaColumnas
                        join tabla in this.context.MetaTablas
                        on columna.IdTabla equals tabla.IdTabla
                        where tabla.IdEmpresa == idEmpresa && tabla.Nombre == nombreTabla
                        select columna).OrderBy(x => x.IdColumna);

            return await consulta.ToListAsync();
        }

        public async Task InsertRegistroTablaEmpresaAsync
            (int idEmpresa, string nombreTabla, Dictionary<string, string> valores)
        {
            var valoresLimpios = new Dictionary<string, string>();

            foreach (var valor in valores)
            {
                // Solo procesamos los que no vengan vacíos
                if (!string.IsNullOrEmpty(valor.Value))
                {
                    // Intentamos ver si es una fecha que viene del input HTML ("yyyy-MM-dd")
                    if (DateTime.TryParseExact
                        (valor.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaConvertida))
                    {
                        // Si es fecha, la guardamos en el formato universal irrompible para SQL
                        valoresLimpios.Add(valor.Key, fechaConvertida.ToString("yyyy-MM-ddTHH:mm:ss"));
                    }
                    else if (valor.Value == "on" || valor.Value == "off")
                    {
                        if (valor.Value == "on")
                        {
                            valoresLimpios.Add(valor.Key, "1");
                        }
                        else
                        {
                            valoresLimpios.Add(valor.Key, "0");
                        }
                    }
                    else
                    {
                        // Si es un texto o número normal, lo guardamos tal cual
                        valoresLimpios.Add(valor.Key, valor.Value);
                    }
                }
            }

            if (valoresLimpios.Count == 0) 
            {
                return; 
            }

            string jsonData = JsonSerializer.Serialize(valoresLimpios);

            string sql = "SP_INSERT_ROW_DINAMICO";
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            SqlParameter paramNombreTabla = new SqlParameter("@NombreTabla", nombreTabla);
            SqlParameter paramJsonData = new SqlParameter("@JsonData", jsonData);

            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
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

        public async Task<Dictionary<string, List<MetaOpcione>>> GetOpcionesSelectTablaEmpresaAsync
            (int idEmpresa, string nombreTabla)
        {
            Dictionary<string, List<MetaOpcione>> opciones = new Dictionary<string, List<MetaOpcione>>();

            var idTablaConsulta = from datos in this.context.MetaTablas
                           where datos.IdEmpresa == idEmpresa &&
                           datos.Nombre == nombreTabla
                           select datos.IdTabla;

            int idTabla = await idTablaConsulta.FirstOrDefaultAsync();


            var idColumnas = from datos in this.context.MetaColumnas
                           where datos.IdTabla == idTabla &&
                           datos.TipoDato == "Select"
                           select datos;

            List<MetaColumna> columnas = await idColumnas.ToListAsync();

            if (columnas == null)
            {
                return null;
            }

            foreach (MetaColumna columna in columnas)
            {
                var opcionesColumna = await (from datos in this.context.MetaOpciones
                                where datos.IdColumna == columna.IdColumna
                                select datos).ToListAsync();

                opciones.Add(columna.Nombre, opcionesColumna);
            }

            return opciones;

        }

        public async Task<Dictionary<string, Dictionary<string, string>>> GetOpcionesRelacionTablaEmpresaAsync(int idEmpresa, string nombreTabla)
        {
            var opcionesRelacion = new Dictionary<string, Dictionary<string, string>>();

            string nombreSchema = await (from datos in this.context.Empresas
                                        where datos.IdEmpresa == idEmpresa
                                        select datos.NombreSchema)
                                        .FirstOrDefaultAsync();

            var tablaActual = await (from datos in this.context.MetaTablas
                                     where datos.IdEmpresa == idEmpresa &&
                                     datos.Nombre == nombreTabla
                                     select datos).FirstOrDefaultAsync();

            if (tablaActual == null) 
            {
                return opcionesRelacion;
            }

            var columnasRelacion = await (from datos in this.context.MetaColumnas
                                          where datos.IdTabla == tablaActual.IdTabla &&
                                          datos.TipoDato == "Relacion" &&
                                          datos.IdTablaRelacionada != null
                                          select datos).ToListAsync();

            foreach (var columna in columnasRelacion)
            {
                var diccionarioDatos = new Dictionary<string, string>();

                var tablaVinculada = await (from datos in this.context.MetaTablas
                                            where datos.IdTabla == columna.IdTablaRelacionada
                                            select datos).FirstOrDefaultAsync();

                if (tablaVinculada != null)
                {
                    var primeraColumna = await (from datos in this.context.MetaColumnas
                                                where datos.IdTabla == tablaVinculada.IdTabla
                                                orderby datos.IdColumna
                                                select datos).FirstOrDefaultAsync();

                    string nombreColumnaVisual = primeraColumna != null ? primeraColumna.Nombre : "Id";

                    string sql = $"SELECT [Id], [{nombreColumnaVisual}] AS Mostrar FROM {nombreSchema}.[{tablaVinculada.Nombre}]"; 
                    
                    using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
                    {
                        com.CommandType = System.Data.CommandType.Text;
                        com.CommandText = sql;

                        await com.Connection.OpenAsync();

                        DbDataReader reader = await com.ExecuteReaderAsync();

                        while (await reader.ReadAsync())
                        {
                            string idFila = reader["Id"].ToString();
                            string nombreFila = reader["Mostrar"].ToString();

                            diccionarioDatos.Add(idFila, nombreFila); 
                        }
                        await reader.CloseAsync();
                        await com.Connection.CloseAsync();
                    }
                }

                opcionesRelacion.Add(columna.Nombre, diccionarioDatos);
            }
            
            return opcionesRelacion;
        }

        public async Task UpdateCeldaAsync(int idEmpresa, string nombreTabla, int idFila, string columna, string valor)
        {

            string sql = "SP_UPDATE_ROW_DINAMICO";

            object valorFinal;

            if (string.IsNullOrEmpty(valor))
            {
                valorFinal = DBNull.Value;
            }
            else if (DateTime.TryParseExact(valor, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaConvertida))
            {
                // Si es una fecha del HTML, la convertimos al formato universal irrompible de SQL Server
                valorFinal = fechaConvertida.ToString("yyyy-MM-ddTHH:mm:ss");
            }
            else
            {
                // Si es texto, número, o "1"/"0", lo dejamos tal cual
                valorFinal = valor;
            }

            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            SqlParameter paramNombreTabla = new SqlParameter("@NombreTabla", nombreTabla);
            SqlParameter paramIdFila = new SqlParameter("@IdFila", idFila);
            SqlParameter paramColumna = new SqlParameter("@Columna", columna);
            SqlParameter paramValor = new SqlParameter("@Valor", valorFinal);

            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;

                com.Parameters.Add(paramIdEmpresa);
                com.Parameters.Add(paramNombreTabla);
                com.Parameters.Add(paramIdFila);
                com.Parameters.Add(paramColumna);
                com.Parameters.Add(paramValor);

                
                await com.Connection.OpenAsync();           
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
            }
        }

        public async Task DeleteRegistrosAsync(int idEmpresa, string nombreTabla, List<int> idsFilas)
        {
            string idsFormateados = string.Join(",", idsFilas);
            string sql = "SP_DELETE_ROWS_MULTIPLE_DINAMICO"; 
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            SqlParameter paramNombreTabla = new SqlParameter("@NombreTabla", nombreTabla);
            SqlParameter paramIdsFilas = new SqlParameter("@IdsFilas", idsFormateados);
            using (var com = this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;

                com.Parameters.Add(paramIdEmpresa);
                com.Parameters.Add(paramNombreTabla);
                com.Parameters.Add(paramIdsFilas);

                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
            }
        }

        public async Task DeleteRegistroAsync(int idEmpresa, string nombreTabla, int idFila)
        {
            
            string sql = "SP_DELETE_ROW_DINAMICO";
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            SqlParameter paramNombreTabla = new SqlParameter("@NombreTabla", nombreTabla);
            SqlParameter paramIdFila = new SqlParameter("@IdFila", idFila);
            using (var com = this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;

                com.Parameters.Add(paramIdEmpresa);
                com.Parameters.Add(paramNombreTabla);
                com.Parameters.Add(paramIdFila);

                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
            }
        }

        public async Task DeleteTablasEmpresaAsync(int idEmpresa, string nombreTabla)
        {
            string sql = "SP_DELETE_TABLA_EMPRESA";
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            SqlParameter paramNombreTabla = new SqlParameter("@nombreTabla", nombreTabla);

            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;
                com.Parameters.Add(paramIdEmpresa);
                com.Parameters.Add(paramNombreTabla);

                await com.Connection.OpenAsync();

                await com.ExecuteNonQueryAsync();

                await com.Connection.CloseAsync();
            }
        }

        public async Task DeleteColumnaAsync(int idEmpresa, string nombreTabla, string nombreColumna)
        {
            string sql = "SP_DROP_COLUMNA_TABLA_SCHEMA_EMPRESA";
            SqlParameter paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            SqlParameter paramNombreTabla = new SqlParameter("@nombreTabla", nombreTabla);
            SqlParameter paramNombreColumna = new SqlParameter("@nombreColumna", nombreColumna);

            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.CommandText = sql;

                com.Parameters.Add(paramIdEmpresa);
                com.Parameters.Add(paramNombreTabla);
                com.Parameters.Add(paramNombreColumna);

                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
            }
        }
    }
}
