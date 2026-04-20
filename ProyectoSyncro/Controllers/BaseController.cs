using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProyectoSyncro.Services;
using System.Security.Claims;

namespace ProyectoSyncro.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected readonly BaseApiService _baseService;

        public BaseController(BaseApiService baseService)
        {
            _baseService = baseService;
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

            // ¡Ya no pasamos el IdEmpresa! El servicio inyecta el token y la API se encarga
            await _baseService.CreateTablaAsync(nombreTabla);
            return RedirectToAction("Index", "Dashboard", new { tabla = nombreTabla });
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                // Pedimos las tablas a la API para el menú lateral
                this.ViewData["TablasEmpresa"] = await _baseService.GetTablasAsync();

                this.ViewData["NombreUser"] = context.HttpContext.User.Identity.Name;
                this.ViewData["EsPremium"] = context.HttpContext.User.HasClaim("Plan", "Premium");

                var claimNombreEmpresa = context.HttpContext.User.FindFirst("NombreEmpresa");
                if (claimNombreEmpresa != null)
                {
                    this.ViewData["NombreEmpresa"] = claimNombreEmpresa.Value;
                }

                await next();
            }
            else
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}