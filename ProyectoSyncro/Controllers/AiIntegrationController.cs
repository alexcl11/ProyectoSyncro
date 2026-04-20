using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Services;

namespace ProyectoSyncro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiIntegrationController : ControllerBase
    {
        private readonly AiApiService _aiService;

        public AiIntegrationController(AiApiService aiService)
        {
            _aiService = aiService;
        }

        [Authorize]
        [HttpPost("AskAi")]
        public async Task<IActionResult> AskAi([FromForm] string prompt, [FromForm] string tablaActual = "")
        {
            try
            {
                // Delegamos toda la conexión con n8n a nuestro servicio
                string respuesta = await _aiService.AskAiAsync(prompt, tablaActual);

                return Ok(new { respuesta = respuesta });
            }
            catch (Exception ex)
            {
                // Atrapamos errores de conexión (n8n caído, etc.)
                return BadRequest("Error interno al contactar con la IA: " + ex.Message);
            }
        }
    }
}