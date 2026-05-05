using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Models;
using ProyectoSyncro.Services;
using Stripe;
using System.Security.Claims;

namespace ProyectoSyncro.Controllers
{
    [Authorize]
    public class SettingsController : BaseController
    {
        private readonly SettingsApiService _settingsService;
        private SecretClient _secretClient;
        public SettingsController(BaseApiService baseService, SettingsApiService settingsService, SecretClient secretClient) : base(baseService)
        {
            _settingsService = settingsService;
            _secretClient = secretClient;
        }

        public async Task<IActionResult> Index()
        {
            bool esAdmin = HttpContext.User.IsInRole("Admin");
            string emailUser = HttpContext.User.FindFirst("Email")?.Value ?? "";

            Empresa empresa = await _settingsService.GetEmpresaAsync();

            ViewData["EsAdmin"] = esAdmin;
            ViewData["CifEmpresa"] = empresa.Cifempresa;
            ViewData["EmailUser"] = emailUser;
            ViewData["EmpresaActiva"] = empresa.Activo;

            if (esAdmin)
            {
                List<Usuario> equipo = await _settingsService.GetUsuariosEmpresaAsync();
                ViewData["Equipo"] = equipo;
            }

            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateEmpresa(string cif, string nombreEmpresa, string empresaActiva)
        {
            bool isActiva = (empresaActiva == "on");
            await _settingsService.UpdateEmpresaAsync(cif, nombreEmpresa, isActiva);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePerfil(string nombreUser, string email)
        {
            int idUsuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            bool esAdmin = HttpContext.User.IsInRole("Admin");

            await _settingsService.UpdateUserAsync(idUsuario, nombreUser, email, esAdmin);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(string nombre, string email, string password, bool esAdmin)
        {
            bool esPremium = User.HasClaim("Plan", "Premium");

            if (!esPremium)
            {
                var equipoActual = await _settingsService.GetUsuariosEmpresaAsync();
                if (equipoActual.Count >= 3)
                {
                    return BadRequest("Límite de 3 usuarios alcanzado en el plan gratuito.");
                }
            }

            await _settingsService.CreateUserAsync(nombre, email, esAdmin, password);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateUsuarioEquipo(int idUsuario, string nombreUser, string email, string esAdmin)
        {
            bool isAdmin = (esAdmin == "on");
            await _settingsService.UpdateUserAsync(idUsuario, nombreUser, email, isAdmin);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteUsuarioEquipo(int idUsuario)
        {
            int miIdUsuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (miIdUsuario == idUsuario)
                return BadRequest("No puedes eliminar tu propia cuenta de administrador.");

            await _settingsService.DeleteUserAsync(idUsuario);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteEmpresa()
        {
            await _settingsService.DeleteEmpresaAsync();
            await HttpContext.SignOutAsync();
            return Ok(new { url = Url.Action("Login", "Auth") });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CancelarPremium()
        {
            try
            {
                string emailUsuario = HttpContext.User.FindFirst("Email")?.Value;
                if (string.IsNullOrEmpty(emailUsuario)) return BadRequest("Email no encontrado.");

                KeyVaultSecret secretKeyStripe = await _secretClient.GetSecretAsync("stripe-secretkey");
                string secretKey = secretKeyStripe.Value;
                StripeConfiguration.ApiKey = secretKey;

                var customerService = new CustomerService();
                var customers = await customerService.ListAsync(new CustomerListOptions { Email = emailUsuario });
                var customer = customers.FirstOrDefault();

                if (customer != null)
                {
                    var subscriptionService = new SubscriptionService();
                    var subscriptions = await subscriptionService.ListAsync(new SubscriptionListOptions
                    {
                        Customer = customer.Id,
                        Status = "active"
                    });

                    var activeSubscription = subscriptions.FirstOrDefault();

                    if (activeSubscription != null)
                    {
                        await subscriptionService.CancelAsync(activeSubscription.Id);

                    }
                }

                await _settingsService.CancelarPremiumAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Error al cancelar en Stripe: " + ex.Message);
            }
        }
    }
}