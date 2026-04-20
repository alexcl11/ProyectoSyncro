using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Api.Repositories;

namespace ProyectoSyncro.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // No pedimos JWT porque es para n8n (Server-to-Server)
    public class AiIntegrationController : ControllerBase
    {
        private readonly BaseRepository _repo;
        private readonly string API_KEY;

        public AiIntegrationController(BaseRepository repo, IConfiguration config)
        {
            _repo = repo;
            // IMPORTANTE: Asegúrate de tener "N8nConfig:ApiKey" en tu appsettings.json de la API
            API_KEY = config["N8nConfig:ApiKey"];
        }

        // --- DTOs (Lo que nos manda n8n por POST) ---
        public class AiTableRequest { public int IdEmpresa { get; set; } public string NombreTabla { get; set; } }
        public class AiColumnRequest { public int IdEmpresa { get; set; } public string NombreTabla { get; set; } public string NombreColumna { get; set; } public string TipoDato { get; set; } public string? Opciones { get; set; } }
        public class AiInsertRequest { public int IdEmpresa { get; set; } public string NombreTabla { get; set; } public Dictionary<string, string> Valores { get; set; } }
        public class AiReadRequest { public int IdEmpresa { get; set; } public string NombreTabla { get; set; } }

        // --- VALIDACIÓN DE SEGURIDAD INTERNA ---
        private bool IsApiKeyValid()
        {
            var providedKey = Request.Headers["x-api-key"].FirstOrDefault();
            return providedKey == API_KEY;
        }

        // ==========================================
        // ENDPOINTS PARA N8N
        // ==========================================

        [HttpPost("create-table")]
        public async Task<IActionResult> CreateTableFromAi([FromBody] AiTableRequest request)
        {
            if (!IsApiKeyValid()) return Unauthorized("API Key inválida");
            try
            {
                await _repo.CreateTablaEmpresaAsync(request.IdEmpresa, request.NombreTabla);
                return Ok(new { success = true });
            }
            catch (Exception ex) { return BadRequest(new { success = false, error = ex.Message }); }
        }

        [HttpPost("create-column")]
        public async Task<IActionResult> CreateColumnFromAi([FromBody] AiColumnRequest request)
        {
            if (!IsApiKeyValid()) return Unauthorized("API Key inválida");
            try
            {
                string? tablaRelacionada = request.TipoDato.Equals("Relacion", StringComparison.OrdinalIgnoreCase) ? request.Opciones : null;

                await _repo.CreateColumnaTablaAsync(request.IdEmpresa, request.NombreTabla, request.NombreColumna, request.TipoDato, tablaRelacionada);

                if (request.TipoDato.Equals("Select", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(request.Opciones))
                {
                    string[] opcionesArray = request.Opciones.Split(',');
                    string[] coloresDefecto = { "#3b82f6", "#ef4444", "#10b981", "#f59e0b", "#8b5cf6" };

                    for (int i = 0; i < opcionesArray.Length; i++)
                    {
                        string valorOpcion = opcionesArray[i].Trim();
                        if (!string.IsNullOrEmpty(valorOpcion))
                        {
                            string colorAsignado = coloresDefecto[i % coloresDefecto.Length];
                            await _repo.InsertarOpcionColumnaAsync(request.IdEmpresa, request.NombreTabla, request.NombreColumna, valorOpcion, colorAsignado);
                        }
                    }
                }
                return Ok(new { success = true });
            }
            catch (Exception ex) { return BadRequest(new { success = false, error = ex.Message }); }
        }

        [HttpPost("insert-record")]
        public async Task<IActionResult> InsertRecordFromAi([FromBody] AiInsertRequest request)
        {
            if (!IsApiKeyValid()) return Unauthorized("API Key inválida");
            try
            {
                await _repo.InsertRegistroTablaEmpresaAsync(request.IdEmpresa, request.NombreTabla, request.Valores);
                return Ok(new { success = true });
            }
            catch (Exception ex) { return BadRequest(new { success = false, error = ex.Message }); }
        }

        [HttpPost("read-records")]
        public async Task<IActionResult> ReadRecordsFromAi([FromBody] AiReadRequest request)
        {
            if (!IsApiKeyValid()) return Unauthorized("API Key inválida");
            try
            {
                var datos = await _repo.GetDatosTablaEmpresaAsync(request.IdEmpresa, request.NombreTabla);
                return Ok(new { success = true, datos = datos });
            }
            catch (Exception ex) { return BadRequest(new { success = false, error = ex.Message }); }
        }
    }
}