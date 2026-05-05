using Azure.Security.KeyVault.Secrets;
using System.Text.Json;

namespace ProyectoSyncro.Services
{
    public class AiApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        private SecretClient _secretClient;

        public AiApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, SecretClient secretClient)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _secretClient = secretClient;
        }

        public async Task<string> AskAiAsync(string prompt, string tablaActual)
        {
            KeyVaultSecret n8nWebhookSecret = await _secretClient.GetSecretAsync("n8nwebhook");
            string n8nWebhookUrl = n8nWebhookSecret.Value;
            var idEmpresa = _httpContextAccessor.HttpContext?.User.FindFirst("IdEmpresa")?.Value;
            var payload = new
            {
                action = "sendMessage",
                sessionId = idEmpresa,
                chatInput = $"[IdEmpresa: {idEmpresa}]\n[CONTEXTO: El usuario está viendo la tabla '{tablaActual}'].\nUsuario: {prompt}"
            };
            var response = await _httpClient.PostAsJsonAsync(n8nWebhookUrl, payload);
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            return doc.RootElement.GetProperty("output").GetString();
        }
    }
}