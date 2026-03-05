using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Extensions;
using ProyectoSyncro.Models;
using ProyectoSyncro.Repositories;

namespace ProyectoSyncro.Controllers
{
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
            var user = HttpContext.Session.GetObject<UserSession>("User");
            if (user == null) return RedirectToAction("Login", "Auth");

            Empresa empresa = await this.repo.GetEmpresaAsync(user.IdEmpresa);
            ViewData["NombreUser"] = user.Nombre;
            ViewData["EsAdmin"] = user.Admin;
            ViewData["NombreEmpresa"] = empresa.NombreEmpresa;
            ViewData["CifEmpresa"] = empresa.Cifempresa;
            ViewData["EmailUser"] = user.Email;
            ViewData["EmpresaActiva"] = empresa.Activo;
            if (user.Admin)
            {
                List<Usuario> equipo = await this.repo.GetUsuariosEmpresaAsync(user.IdEmpresa);
                ViewData["Equipo"] = equipo;
            }
            ViewData["TablasEmpresa"] = await this.baseRepo.GetTablasEmpresaAsync(user.IdEmpresa);
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UpdateEmpresa(string cif, string nombreEmpresa, string empresaActiva)
        {
            var user = HttpContext.Session.GetObject<UserSession>("User");
            if (user == null || !user.Admin) return Unauthorized("No tienes permisos para realizar esta acción.");

            bool isActiva = (empresaActiva == "on");

            try
            {
                await this.repo.UpdateEmpresaAsync(user.IdEmpresa, cif, nombreEmpresa, isActiva);

                user.NombreEmpresa = nombreEmpresa;
                HttpContext.Session.SetObject("User", user);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Ocurrió un error al intentar actualizar la empresa.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePerfil(string nombreUser, string email)
        {
            var user = HttpContext.Session.GetObject<UserSession>("User");
            if (user == null) return Unauthorized();


            try
            {

                await this.repo.UpdateUsuarioAsync(user.IdUsuario, user.IdEmpresa, nombreUser,
                    email, user.Admin);

                user.Nombre = nombreUser;
                HttpContext.Session.SetObject("User", user);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Ocurrió un error al intentar actualizar el perfil.");
            }
        }

        public async Task<IActionResult> CreateUser(string nombre, string email, string password, bool esAdmin)
        {
            var user = HttpContext.Session.GetObject<UserSession>("User");
            if (user == null) return Unauthorized();
            try
            {

                await this.repo.CreateUserAsync(user.IdEmpresa, nombre,
                    email, esAdmin, password);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Ocurrió un error al intentar actualizar el perfil.");

            }
        }
    }
}
