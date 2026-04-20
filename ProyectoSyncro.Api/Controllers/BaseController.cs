using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Api.Repositories;
using ProyectoSyncro.Models;

namespace ProyectoSyncro.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 Protege todo el controlador
    public class BaseController : ControllerBase
    {
        private readonly BaseRepository _repo;

        public BaseController(BaseRepository repo)
        {
            _repo = repo;
        }

        // Helper para sacar el IdEmpresa del Token JWT de forma segura
        private int GetIdEmpresaToken()
        {
            return int.Parse(User.FindFirst("IdEmpresa").Value);
        }

        #region DTOs (Objetos de Transferencia de Datos para recibir JSONs complejos)
        public class TablaRequest { public string NombreTabla { get; set; } }
        public class ColumnaRequest { public string NombreColumna { get; set; } public string TipoDato { get; set; } public string? NombreTablaRelacionada { get; set; } }
        public class OpcionRequest { public string Valor { get; set; } public string Color { get; set; } }
        public class UpdateCeldaRequest { public string Columna { get; set; } public string Valor { get; set; } }
        public class RenameTablaRequest { public string NombreTablaNew { get; set; } }
        public class RenameColumnaRequest { public string NombreColumnaNew { get; set; } public string TipoDato { get; set; } public string? NombreTablaRelacionada { get; set; } }
        #endregion

        // 1. GET: api/base/tablas
        [HttpGet("tablas")]
        public async Task<IActionResult> GetTablas()
        {
            int idEmpresa = GetIdEmpresaToken();
            var tablas = await _repo.GetTablasEmpresaAsync(idEmpresa);
            return Ok(tablas);
        }

        // 2. GET: api/base/tablas/{nombreTabla}/datos
        [HttpGet("tablas/{nombreTabla}/datos")]
        public async Task<IActionResult> GetDatosTabla(string nombreTabla, [FromQuery] string sortCol = "Id", [FromQuery] string sortDir = "DESC", [FromQuery] string filterCol = null, [FromQuery] string filterOp = null, [FromQuery] string filterVal = null)
        {
            int idEmpresa = GetIdEmpresaToken();
            var datos = await _repo.GetDatosTablaEmpresaAsync(idEmpresa, nombreTabla, sortCol, sortDir, filterCol, filterOp, filterVal);
            return Ok(datos);
        }

        // 3. POST: api/base/tablas
        [HttpPost("tablas")]
        [Authorize(Policy = "FreeTierLimit")] // 🛡️ Política de límites
        public async Task<IActionResult> CrearTabla([FromBody] TablaRequest request)
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.CreateTablaEmpresaAsync(idEmpresa, request.NombreTabla);
            return Ok(new { mensaje = "Tabla creada con éxito" });
        }

        // 4. POST: api/base/tablas/{nombreTabla}/columnas
        [HttpPost("tablas/{nombreTabla}/columnas")]
        public async Task<IActionResult> CrearColumna(string nombreTabla, [FromBody] ColumnaRequest request)
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.CreateColumnaTablaAsync(idEmpresa, nombreTabla, request.NombreColumna, request.TipoDato, request.NombreTablaRelacionada);
            return Ok(new { mensaje = "Columna creada con éxito" });
        }

        // 5. POST: api/base/tablas/{nombreTabla}/columnas/{nombreColumna}/opciones
        [HttpPost("tablas/{nombreTabla}/columnas/{nombreColumna}/opciones")]
        public async Task<IActionResult> InsertarOpcionColumna(string nombreTabla, string nombreColumna, [FromBody] OpcionRequest request)
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.InsertarOpcionColumnaAsync(idEmpresa, nombreTabla, nombreColumna, request.Valor, request.Color);
            return Ok(new { mensaje = "Opción añadida con éxito" });
        }

        // 6. GET: api/base/tablas/{nombreTabla}/columnas
        [HttpGet("tablas/{nombreTabla}/columnas")]
        public async Task<IActionResult> GetColumnasTabla(string nombreTabla)
        {
            int idEmpresa = GetIdEmpresaToken();
            var columnas = await _repo.GetColumnasTablaAsync(idEmpresa, nombreTabla);
            return Ok(columnas);
        }

        // 7. POST: api/base/tablas/{nombreTabla}/registros
        [HttpPost("tablas/{nombreTabla}/registros")]
        public async Task<IActionResult> InsertarRegistro(string nombreTabla, [FromBody] Dictionary<string, string> valores)
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.InsertRegistroTablaEmpresaAsync(idEmpresa, nombreTabla, valores);
            return Ok(new { mensaje = "Registro insertado con éxito" });
        }

        // 8. GET: api/base/tablas/{nombreTabla}/opciones-select
        [HttpGet("tablas/{nombreTabla}/opciones-select")]
        public async Task<IActionResult> GetOpcionesSelect(string nombreTabla)
        {
            int idEmpresa = GetIdEmpresaToken();
            var opciones = await _repo.GetOpcionesSelectTablaEmpresaAsync(idEmpresa, nombreTabla);
            if (opciones == null) return NotFound("Tabla o columnas select no encontradas.");
            return Ok(opciones);
        }

        // 9. GET: api/base/tablas/{nombreTabla}/opciones-relacion
        [HttpGet("tablas/{nombreTabla}/opciones-relacion")]
        public async Task<IActionResult> GetOpcionesRelacion(string nombreTabla)
        {
            int idEmpresa = GetIdEmpresaToken();
            var opciones = await _repo.GetOpcionesRelacionTablaEmpresaAsync(idEmpresa, nombreTabla);
            return Ok(opciones);
        }

        // 10. PUT: api/base/tablas/{nombreTabla}/registros/{idFila}
        [HttpPut("tablas/{nombreTabla}/registros/{idFila}")]
        public async Task<IActionResult> UpdateCelda(string nombreTabla, int idFila, [FromBody] UpdateCeldaRequest request)
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.UpdateCeldaAsync(idEmpresa, nombreTabla, idFila, request.Columna, request.Valor);
            return Ok(new { mensaje = "Celda actualizada" });
        }

        // 11. DELETE: api/base/tablas/{nombreTabla}/registros/multiple
        [HttpDelete("tablas/{nombreTabla}/registros/multiple")]
        public async Task<IActionResult> DeleteRegistrosMultiple(string nombreTabla, [FromBody] List<int> idsFilas)
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.DeleteRegistrosAsync(idEmpresa, nombreTabla, idsFilas);
            return Ok(new { mensaje = "Registros eliminados" });
        }

        // 12. DELETE: api/base/tablas/{nombreTabla}/registros/{idFila}
        [HttpDelete("tablas/{nombreTabla}/registros/{idFila}")]
        public async Task<IActionResult> DeleteRegistro(string nombreTabla, int idFila)
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.DeleteRegistroAsync(idEmpresa, nombreTabla, idFila);
            return Ok(new { mensaje = "Registro eliminado" });
        }

        // 13. DELETE: api/base/tablas/{nombreTabla}
        [HttpDelete("tablas/{nombreTabla}")]
        public async Task<IActionResult> DeleteTabla(string nombreTabla)
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.DeleteTablasEmpresaAsync(idEmpresa, nombreTabla);
            return Ok(new { mensaje = "Tabla eliminada" });
        }

        // 14. DELETE: api/base/tablas/{nombreTabla}/columnas/{nombreColumna}
        [HttpDelete("tablas/{nombreTabla}/columnas/{nombreColumna}")]
        public async Task<IActionResult> DeleteColumna(string nombreTabla, string nombreColumna)
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.DeleteColumnaAsync(idEmpresa, nombreTabla, nombreColumna);
            return Ok(new { mensaje = "Columna eliminada" });
        }

        // 15. PUT: api/base/tablas/{nombreTablaOld}/rename
        [HttpPut("tablas/{nombreTablaOld}/rename")]
        public async Task<IActionResult> RenameTabla(string nombreTablaOld, [FromBody] RenameTablaRequest request)
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.RenameTablaAsync(idEmpresa, nombreTablaOld, request.NombreTablaNew);
            return Ok(new { mensaje = "Tabla renombrada con éxito" });
        }

        // 16. PUT: api/base/tablas/{nombreTabla}/columnas/{nombreColumnaOld}/rename
        [HttpPut("tablas/{nombreTabla}/columnas/{nombreColumnaOld}/rename")]
        public async Task<IActionResult> RenameColumna(string nombreTabla, string nombreColumnaOld, [FromBody] RenameColumnaRequest request)
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.RenameColumnaAsync(idEmpresa, nombreTabla, nombreColumnaOld, request.NombreColumnaNew, request.TipoDato, request.NombreTablaRelacionada);
            return Ok(new { mensaje = "Columna actualizada con éxito" });
        }
    }
}