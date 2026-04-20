using Newtonsoft.Json;
using ProyectoSyncro.Models;
using System.Net.Http.Headers;
using System.Text;

namespace ProyectoSyncro.Services
{
    public class BaseApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BaseApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuth()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("TOKEN");
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<string>> GetTablasAsync()
        {
            AddAuth();
            var response = await _httpClient.GetAsync("api/Base/tablas");
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<string>>(await response.Content.ReadAsStringAsync());
            return new List<string>();
        }

        public async Task<List<Dictionary<string, object>>> GetDatosTablaAsync(string tabla, string sortCol, string sortDir, string fCol, string fOp, string fVal)
        {
            AddAuth();
            string url = $"api/Base/tablas/{tabla}/datos?sortCol={sortCol}&sortDir={sortDir}&filterCol={fCol}&filterOp={fOp}&filterVal={fVal}";
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(await response.Content.ReadAsStringAsync());
            return new List<Dictionary<string, object>>();
        }

        public async Task<List<MetaColumna>> GetColumnasTablaAsync(string tabla)
        {
            AddAuth();
            var response = await _httpClient.GetAsync($"api/Base/tablas/{tabla}/columnas");
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<MetaColumna>>(await response.Content.ReadAsStringAsync());
            return new List<MetaColumna>();
        }

        public async Task<Dictionary<string, List<MetaOpcione>>> GetOpcionesSelectAsync(string tabla)
        {
            AddAuth();
            var response = await _httpClient.GetAsync($"api/Base/tablas/{tabla}/opciones-select");
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Dictionary<string, List<MetaOpcione>>>(await response.Content.ReadAsStringAsync());
            return new Dictionary<string, List<MetaOpcione>>();
        }

        public async Task<Dictionary<string, Dictionary<string, string>>> GetOpcionesRelacionAsync(string tabla)
        {
            AddAuth();
            var response = await _httpClient.GetAsync($"api/Base/tablas/{tabla}/opciones-relacion");
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(await response.Content.ReadAsStringAsync());
            return new Dictionary<string, Dictionary<string, string>>();
        }

        public async Task CreateTablaAsync(string nombreTabla)
        {
            AddAuth();
            await _httpClient.PostAsJsonAsync("api/Base/tablas", new { NombreTabla = nombreTabla });
        }

        public async Task CreateColumnaAsync(string nombreTabla, string nombreColumna, string tipoDato, string tablaRelacionada)
        {
            AddAuth();
            await _httpClient.PostAsJsonAsync($"api/Base/tablas/{nombreTabla}/columnas", new { NombreColumna = nombreColumna, TipoDato = tipoDato, NombreTablaRelacionada = tablaRelacionada });
        }

        public async Task InsertarOpcionColumnaAsync(string nombreTabla, string nombreColumna, string valor, string color)
        {
            AddAuth();
            await _httpClient.PostAsJsonAsync($"api/Base/tablas/{nombreTabla}/columnas/{nombreColumna}/opciones", new { Valor = valor, Color = color });
        }

        public async Task InsertRegistroAsync(string nombreTabla, Dictionary<string, string> valores)
        {
            AddAuth();
            await _httpClient.PostAsJsonAsync($"api/Base/tablas/{nombreTabla}/registros", valores);
        }

        public async Task UpdateCeldaAsync(string nombreTabla, int idFila, string columna, string valor)
        {
            AddAuth();
            await _httpClient.PutAsJsonAsync($"api/Base/tablas/{nombreTabla}/registros/{idFila}", new { Columna = columna, Valor = valor });
        }

        public async Task DeleteRegistrosMultipleAsync(string nombreTabla, List<int> idsFilas)
        {
            AddAuth();
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/Base/tablas/{nombreTabla}/registros/multiple")
            {
                Content = new StringContent(JsonConvert.SerializeObject(idsFilas), Encoding.UTF8, "application/json")
            };
            await _httpClient.SendAsync(request);
        }

        public async Task DeleteRegistroAsync(string nombreTabla, int idFila)
        {
            AddAuth();
            await _httpClient.DeleteAsync($"api/Base/tablas/{nombreTabla}/registros/{idFila}");
        }

        public async Task DeleteTablaAsync(string nombreTabla)
        {
            AddAuth();
            await _httpClient.DeleteAsync($"api/Base/tablas/{nombreTabla}");
        }

        public async Task DeleteColumnaAsync(string nombreTabla, string nombreColumna)
        {
            AddAuth();
            await _httpClient.DeleteAsync($"api/Base/tablas/{nombreTabla}/columnas/{nombreColumna}");
        }

        public async Task RenameTablaAsync(string nombreOld, string nombreNew)
        {
            AddAuth();
            await _httpClient.PutAsJsonAsync($"api/Base/tablas/{nombreOld}/rename", new { NombreTablaNew = nombreNew });
        }

        public async Task RenameColumnaAsync(string nombreTabla, string nombreOld, string nombreNew, string tipoDato, string tablaRelacionada)
        {
            AddAuth();
            await _httpClient.PutAsJsonAsync($"api/Base/tablas/{nombreTabla}/columnas/{nombreOld}/rename", new { NombreColumnaNew = nombreNew, TipoDato = tipoDato, NombreTablaRelacionada = tablaRelacionada });
        }
    }
}