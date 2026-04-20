using Newtonsoft.Json;

namespace ProyectoSyncro.Services
{
    public class AuthApiService
    {
        private readonly HttpClient _httpClient;

        public AuthApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", new { Email = email, Password = password });
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<dynamic>(result).token;
        }

        public async Task<bool> RegisterAsync(string cif, string nombreEmpresa, string nombreUsuario, string email, string password)
        {
            var data = new { Cif = cif, NombreEmpresa = nombreEmpresa, NombreUsuario = nombreUsuario, Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/Auth/register", data);
            return response.IsSuccessStatusCode;
        }
    }
}