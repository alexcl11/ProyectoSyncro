using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Models;
using ProyectoSyncro.Repositories;
using System.Security.Claims;

namespace ProyectoSyncro.Controllers
{
    // Obliga a que cualquier persona que entre a Ajustes esté logueada
    [Authorize]
    public class SettingsController : BaseController
    {
        private SettingsRepository repo;
        private BaseRepository baseRepo;

        public SettingsController(SettingsRepository repo, BaseRepository baseRepo) : base(baseRepo)
        {
            this.repo = repo;
            this.baseRepo = baseRepo;
        }

        public async Task<IActionResult> Index()
        {
            // Extraemos los datos directamente de la Cookie (Claims)
            int idEmpresa = int.Parse(HttpContext.User.FindFirst("IdEmpresa").Value);
            string nombreUser = HttpContext.User.Identity.Name;
            bool esAdmin = HttpContext.User.IsInRole("Admin");
            string emailUser = HttpContext.User.FindFirst("Email")?.Value ?? "";

            Empresa empresa = await this.repo.GetEmpresaAsync(idEmpresa);

            ViewData["NombreUser"] = nombreUser;
            ViewData["EsAdmin"] = esAdmin;
            ViewData["NombreEmpresa"] = empresa.NombreEmpresa;
            ViewData["CifEmpresa"] = empresa.Cifempresa;
            ViewData["EmailUser"] = emailUser;
            ViewData["EmpresaActiva"] = empresa.Activo;
            ViewData["EsPremium"] = empresa.IsPremium;

            if (esAdmin)
            {
                List<Usuario> equipo = await this.repo.GetUsuariosEmpresaAsync(idEmpresa);
                ViewData["Equipo"] = equipo;
            }

            ViewData["TablasEmpresa"] = await this.baseRepo.GetTablasEmpresaAsync(idEmpresa);
            return View();
        }

        // Fíjate en esto: ¡Bloquea automáticamente a cualquiera que no sea Admin!
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateEmpresa(string cif, string nombreEmpresa, string empresaActiva)
        {
            int idEmpresa = int.Parse(HttpContext.User.FindFirst("IdEmpresa").Value);
            bool isActiva = (empresaActiva == "on");

            try
            {
                await this.repo.UpdateEmpresaAsync(idEmpresa, cif, nombreEmpresa, isActiva);
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest("Ocurrió un error al intentar actualizar la empresa.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePerfil(string nombreUser, string email)
        {
            int idUsuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            int idEmpresa = int.Parse(HttpContext.User.FindFirst("IdEmpresa").Value);
            bool esAdmin = HttpContext.User.IsInRole("Admin");

            try
            {
                await this.repo.UpdateUserAsync(idUsuario, idEmpresa, nombreUser, email, esAdmin);

                // OJO: Si cambia su nombre, deberíamos refrescar la cookie, 
                // pero por ahora para el TFM con hacer el update en BBDD es suficiente,
                // se actualizará visualmente en su próximo login.

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest("Ocurrió un error al intentar actualizar el perfil.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(string nombre, string email, string password, bool esAdmin)
        {
            int idEmpresa = int.Parse(HttpContext.User.FindFirst("IdEmpresa").Value);
            bool esPremium = User.HasClaim("Plan", "Premium");

            // Validamos en el servidor por seguridad
            if (!esPremium)
    {
                var equipoActual = await this.repo.GetUsuariosEmpresaAsync(idEmpresa); // Ajusta a tu método
                if (equipoActual.Count >= 3)
                {
                    return BadRequest("Límite de 3 usuarios alcanzado en el plan gratuito.");
                }
            }
            try
            {
                await this.repo.CreateUserAsync(idEmpresa, nombre, email, esAdmin, password);
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest("Ocurrió un error al crear el usuario.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateUsuarioEquipo(int idUsuario, string nombreUser, string email, string esAdmin)
        {
            int idEmpresa = int.Parse(HttpContext.User.FindFirst("IdEmpresa").Value);
            bool isAdmin = (esAdmin == "on");

            try
            {
                await this.repo.UpdateUserAsync(idUsuario, idEmpresa, nombreUser, email, isAdmin);
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest("Error al actualizar el usuario.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteUsuarioEquipo(int idUsuario)
        {
            int miIdUsuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            int idEmpresa = int.Parse(HttpContext.User.FindFirst("IdEmpresa").Value);

            if (miIdUsuario == idUsuario)
            {
                return BadRequest("No puedes eliminar tu propia cuenta de administrador.");
            }

            try
            {
                await this.repo.DeleteUserAsync(idUsuario, idEmpresa);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Error al eliminar el usuario: " + ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteEmpresa()
        {
            int idEmpresa = int.Parse(HttpContext.User.FindFirst("IdEmpresa").Value);

            try
            {
                await this.repo.DeleteEmpresaAsync(idEmpresa);

                await HttpContext.SignOutAsync();

                return Ok(new { url = Url.Action("Login", "Auth") });
            }
            catch (Exception)
            {
                return BadRequest("No se pudo eliminar la empresa.");
            }
        }
    }
}