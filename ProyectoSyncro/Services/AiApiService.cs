using System.Text.Json;

namespace ProyectoSyncro.Services
{
    public class AiApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _n8nWebhook;

        public AiApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _n8nWebhook = config["N8nConfig:WebhookUrl"];
        }

        public async Task<string> AskAiAsync(string prompt, string tablaActual)
        {
            var idEmpresa = _httpContextAccessor.HttpContext?.User.FindFirst("IdEmpresa")?.Value;
            var payload = new
            {
                action = "sendMessage",
                sessionId = idEmpresa,
                chatInput = $"[IdEmpresa: {idEmpresa}]\n[CONTEXTO: El usuario está viendo la tabla '{tablaActual}'].\nUsuario: {prompt}"
            };
            var response = await _httpClient.PostAsJsonAsync(_n8nWebhook, payload);
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            return doc.RootElement.GetProperty("output").GetString();
        }
    }
}