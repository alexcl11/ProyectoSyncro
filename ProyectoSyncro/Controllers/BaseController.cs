using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProyectoSyncro.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ProyectoSyncro.Controllers
{
    // Etiqueta global: Todos los controladores que hereden de BaseController requerirán Login
    [Authorize]
    public class BaseController : Controller
    {
        private readonly BaseRepository repo;

        public BaseController(BaseRepository repo)
        {
            this.repo = repo;
        }

        [HttpPost]
        public async Task<IActionResult> Create(string nombreTabla)
        {
            var authService = HttpContext.RequestServices.GetService<IAuthorizationService>();

            var authResult = await authService.AuthorizeAsync(User, "LimitesFreeTablas");

            if (!authResult.Succeeded)
            {
                TempData["ErrorLimites"] = "true";
                string tablaActual = Request.Headers["Referer"].ToString().Split("tabla=").LastOrDefault() ?? "";
                return RedirectToAction("Index", "Dashboard", new { tabla = tablaActual });
            }

            int idEmpresa = int.Parse(HttpContext.User.FindFirst("IdEmpresa").Value);
            await this.repo.CreateTablaEmpresaAsync(idEmpresa, nombreTabla);

            return RedirectToAction("Index", "Dashboard", new { tabla = nombreTabla });
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 2. En lugar de Session, comprobamos si la Identidad (Cookie) es válida
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                int idEmpresa = int.Parse(context.HttpContext.User.FindFirst("IdEmpresa").Value);
                List<string> tablas = await this.repo.GetTablasEmpresaAsync(idEmpresa);

                // Como BaseController hereda de Controller, usamos this.ViewData directamente
                this.ViewData["TablasEmpresa"] = tablas;
                this.ViewData["NombreUser"] = context.HttpContext.User.Identity.Name;

                // Extraemos el nombre de la empresa del pasaporte
                var claimNombreEmpresa = context.HttpContext.User.FindFirst("NombreEmpresa");
                if (claimNombreEmpresa != null)
                {
                    this.ViewData["NombreEmpresa"] = claimNombreEmpresa.Value;
                }

                // Dejamos que la petición continúe su camino hacia el Dashboard o Settings
                await next();
            }
            else
            {
                // Si alguien llega hasta aquí sin estar logueado, lo expulsamos al Login
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}