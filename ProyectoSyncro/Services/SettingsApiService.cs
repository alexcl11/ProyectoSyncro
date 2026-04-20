using Newtonsoft.Json;
using ProyectoSyncro.Models;
using System.Net.Http.Headers;

namespace ProyectoSyncro.Services
{
    public class SettingsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SettingsApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
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

        public async Task<Empresa> GetEmpresaAsync()
        {
            AddAuth();
            var response = await _httpClient.GetAsync("api/Settings/empresa");
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Empresa>(await response.Content.ReadAsStringAsync());
            return null;
        }

        public async Task UpdateEmpresaAsync(string cif, string nombre, bool activa)
        {
            AddAuth();
            await _httpClient.PutAsJsonAsync("api/Settings/empresa", new { Cif = cif, Nombre = nombre, Activa = activa });
        }

        public async Task DeleteEmpresaAsync()
        {
            AddAuth();
            await _httpClient.DeleteAsync("api/Settings/empresa");
        }

        public async Task<List<Usuario>> GetUsuariosEmpresaAsync()
        {
            AddAuth();
            var response = await _httpClient.GetAsync("api/Settings/usuarios");
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<Usuario>>(await response.Content.ReadAsStringAsync());
            return new List<Usuario>();
        }

        public async Task CreateUserAsync(string nombre, string email, bool esAdmin, string password)
        {
            AddAuth();
            await _httpClient.PostAsJsonAsync("api/Settings/usuarios", new { Nombre = nombre, Email = email, EsAdmin = esAdmin, Password = password });
        }

        public async Task UpdateUserAsync(int idUsuario, string nombre, string email, bool esAdmin)
        {
            AddAuth();
            await _httpClient.PutAsJsonAsync($"api/Settings/usuarios/{idUsuario}", new { Nombre = nombre, Email = email, EsAdmin = esAdmin });
        }

        public async Task DeleteUserAsync(int idUsuario)
        {
            AddAuth();
            await _httpClient.DeleteAsync($"api/Settings/usuarios/{idUsuario}");
        }

        public async Task ActivarPremiumAsync()
        {
            AddAuth();
            await _httpClient.PutAsync("api/Settings/empresa/premium/enable", null);
        }

        public async Task CancelarPremiumAsync()
        {
            AddAuth();
            await _httpClient.PutAsync("api/Settings/empresa/premium/disable", null);
        }
    }
}