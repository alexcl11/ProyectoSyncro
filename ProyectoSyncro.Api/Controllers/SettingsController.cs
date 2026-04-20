using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Api.Repositories;
using ProyectoSyncro.Models;

namespace ProyectoSyncro.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 Requiere token JWT por defecto
    public class SettingsController : ControllerBase
    {
        private readonly SettingsRepository _repo;

        public SettingsController(SettingsRepository repo)
        {
            _repo = repo;
        }

        // Helper seguro para obtener la empresa del token
        private int GetIdEmpresaToken()
        {
            return int.Parse(User.FindFirst("IdEmpresa").Value);
        }

        #region DTOs (Data Transfer Objects)
        public class UpdateEmpresaRequest { public string Cif { get; set; } public string Nombre { get; set; } public bool Activa { get; set; } }
        public class CreateUserRequest { public string Nombre { get; set; } public string Email { get; set; } public bool EsAdmin { get; set; } public string Password { get; set; } }
        public class UpdateUserRequest { public string Nombre { get; set; } public string Email { get; set; } public bool EsAdmin { get; set; } }
        #endregion


        // ==========================================
        // SECCIÓN EMPRESA
        // ==========================================

        // 1. GET: api/settings/empresa
        [HttpGet("empresa")]
        public async Task<IActionResult> GetEmpresa()
        {
            int idEmpresa = GetIdEmpresaToken();
            var empresa = await _repo.GetEmpresaAsync(idEmpresa);
            if (empresa == null) return NotFound("Empresa no encontrada");
            return Ok(empresa);
        }

        // 2. PUT: api/settings/empresa
        [HttpPut("empresa")]
        [Authorize(Roles = "Admin")] // 🛡️ Solo el jefe puede editar la empresa
        public async Task<IActionResult> UpdateEmpresa([FromBody] UpdateEmpresaRequest request)
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.UpdateEmpresaAsync(idEmpresa, request.Cif, request.Nombre, request.Activa);
            return Ok(new { mensaje = "Datos de la empresa actualizados." });
        }

        // 3. DELETE: api/settings/empresa
        [HttpDelete("empresa")]
        [Authorize(Roles = "Admin")] // 🛡️ ¡Peligro! Solo Admin
        public async Task<IActionResult> DeleteEmpresa()
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.DeleteEmpresaAsync(idEmpresa);
            return Ok(new { mensaje = "Empresa eliminada de forma permanente." });
        }


        // ==========================================
        // SECCIÓN USUARIOS
        // ==========================================

        // 4. GET: api/settings/usuarios
        [HttpGet("usuarios")]
        [Authorize(Roles = "Admin")] // 🛡️ Solo el admin ve la lista de empleados
        public async Task<IActionResult> GetUsuarios()
        {
            int idEmpresa = GetIdEmpresaToken();
            var usuarios = await _repo.GetUsuariosEmpresaAsync(idEmpresa);
            return Ok(usuarios);
        }

        // 5. POST: api/settings/usuarios
        [HttpPost("usuarios")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUsuario([FromBody] CreateUserRequest request)
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.CreateUserAsync(idEmpresa, request.Nombre, request.Email, request.EsAdmin, request.Password);
            return Ok(new { mensaje = "Usuario creado con éxito." });
        }

        // 6. PUT: api/settings/usuarios/{idUsuario}
        [HttpPut("usuarios/{idUsuario}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUsuario(int idUsuario, [FromBody] UpdateUserRequest request)
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.UpdateUserAsync(idUsuario, idEmpresa, request.Nombre, request.Email, request.EsAdmin);
            return Ok(new { mensaje = "Usuario actualizado." });
        }

        // 7. DELETE: api/settings/usuarios/{idUsuario}
        [HttpDelete("usuarios/{idUsuario}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUsuario(int idUsuario)
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.DeleteUserAsync(idUsuario, idEmpresa);
            return Ok(new { mensaje = "Usuario eliminado." });
        }


        // ==========================================
        // SECCIÓN SUSCRIPCIONES (STRIPE / PREMIUM)
        // ==========================================

        // 8. PUT: api/settings/empresa/premium/enable
        [HttpPut("empresa/premium/enable")]
        // Dependiendo de cómo configures Stripe, puede que esto lo llame un Webhook sin token (AllowAnonymous)
        // O que lo llame tu MVC cuando vuelve de Stripe con éxito (Authorize). Lo dejamos Authorize por defecto.
        public async Task<IActionResult> EnablePremium()
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.UpdateEmpresaPremiumAsync(idEmpresa);
            return Ok(new { mensaje = "Suscripción Premium activada." });
        }

        // 9. PUT: api/settings/empresa/premium/disable
        [HttpPut("empresa/premium/disable")]
        public async Task<IActionResult> DisablePremium()
        {
            int idEmpresa = GetIdEmpresaToken();
            await _repo.CancelarPlanEmpresaAsync(idEmpresa);
            return Ok(new { mensaje = "Suscripción Premium cancelada." });
        }
    }
}