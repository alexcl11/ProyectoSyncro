using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Repositories;
using System.Text.Json;
using System.Text;

namespace ProyectoSyncro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiIntegrationController : ControllerBase
    {
        private BaseRepository _repo;
        // Pon aquí tu token inventado
        private readonly string API_KEY = "d39d71ed-f863-4d94-95f6-19a8872e57e2";

        // Pon aquí la URL que te da n8n en el nodo "When chat message received" (Test o Production)
        private readonly string N8N_WEBHOOK_URL = "https://n8n.canovasleads.com/webhook/4de99221-f815-4c8d-80b5-78b6dd4b19d0/chat";

        public AiIntegrationController(BaseRepository repo)
        {
            _repo = repo;
        }

        // ==========================================
        // 1. EL JAVASCRIPT LLAMA A ESTE MÉTODO
        // ==========================================
        [Authorize]
        [HttpPost("AskAi")]
        public async Task<IActionResult> AskAi([FromForm] string prompt)
        {
            int idEmpresa = int.Parse(HttpContext.User.FindFirst("IdEmpresa").Value);

            string mensajeParaN8n = $"[IdEmpresa: {idEmpresa}]\nUsuario: {prompt}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var payload = new
                    {
                        action = "sendMessage",
                        sessionId = idEmpresa.ToString(),
                        chatInput = mensajeParaN8n
                    };

                    var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                    // 🔥 Si n8n está apagado, esto fallará, pero el catch lo atrapará
                    var response = await client.PostAsync(N8N_WEBHOOK_URL, jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var n8nResult = await response.Content.ReadAsStringAsync();

                        using var doc = JsonDocument.Parse(n8nResult);
                        string textoRespuesta = doc.RootElement.GetProperty("output").GetString();

                        return Ok(new { respuesta = textoRespuesta });
                    }

                    return BadRequest("Error de comunicación con los circuitos de la IA.");
                }
            }
            catch (HttpRequestException)
            {
                // 🛡️ Atrapamos el error de "Conexión rechazada"
                return BadRequest("El servidor de inteligencia artificial (n8n) está desconectado o no está escuchando.");
            }
            catch (Exception ex)
            {
                // 🛡️ Atrapamos cualquier otro error inesperado
                return BadRequest("Error interno al contactar con la IA: " + ex.Message);
            }
        }

        // ==========================================
        // 2. N8N (EL AGENTE) LLAMA A ESTOS MÉTODOS
        // ==========================================

        [HttpPost("create-table")]
        public async Task<IActionResult> CreateTableFromAi([FromBody] AiTableRequest request)
        {
            if (Request.Headers["x-api-key"] != API_KEY) return Unauthorized("Token inválido");

            try
            {
                await _repo.CreateTablaEmpresaAsync(request.IdEmpresa, request.NombreTabla);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpPost("create-column")]
        public async Task<IActionResult> CreateColumnFromAi([FromBody] AiColumnRequest request)
        {
            if (Request.Headers["x-api-key"] != API_KEY) return Unauthorized("Token inválido");

            try
            {
                await _repo.CreateColumnaTablaAsync(request.IdEmpresa, request.NombreTabla, request.NombreColumna, request.TipoDato, null);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }
    }

    // Clases para leer el JSON que manda n8n
    public class AiTableRequest { public int IdEmpresa { get; set; } public string NombreTabla { get; set; } }
    public class AiColumnRequest { public int IdEmpresa { get; set; } public string NombreTabla { get; set; } public string NombreColumna { get; set; } public string TipoDato { get; set; } }
}