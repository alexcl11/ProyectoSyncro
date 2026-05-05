using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Models;
using ProyectoSyncro.Services;
using Stripe.Checkout;

namespace ProyectoSyncro.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly SettingsApiService _settingsService;
        private readonly BaseApiService _baseService;
        private readonly SecretClient _secretClient;
        public PaymentController(SettingsApiService settingsService, BaseApiService baseService, SecretClient secretClient)
        {
            _settingsService = settingsService;
            _baseService = baseService;
            _secretClient = secretClient;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var request = HttpContext.Request;
            var host = request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? request.Host.Value;
            var scheme = request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? request.Scheme;
            var domain = $"{scheme}://{host}";

            string idEmpresa = HttpContext.User.FindFirst("IdEmpresa").Value;
            string emailUsuario = HttpContext.User.FindFirst("Email")?.Value;

            Empresa empresa = await _settingsService.GetEmpresaAsync();

            ViewData["TablasEmpresa"] = await _baseService.GetTablasAsync();
            ViewData["NombreUser"] = HttpContext.User.Identity.Name;
            ViewData["NombreEmpresa"] = empresa.NombreEmpresa;

            var options = new SessionCreateOptions
            {
                UiMode = "embedded",
                PaymentMethodTypes = new List<string> { "card" },
                CustomerEmail = emailUsuario,
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 999, // 9.99€
                            Currency = "eur",
                            Recurring = new SessionLineItemPriceDataRecurringOptions
                            {
                                Interval = "month",
                            },
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Plan Premium Mensual - Syncro",
                                Description = "Suscripción mensual: Tablas infinitas y registros ilimitados."
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "subscription",
                ClientReferenceId = idEmpresa,
                ReturnUrl = domain + "/Payment/Success?session_id={CHECKOUT_SESSION_ID}",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            ViewBag.ClientSecret = session.ClientSecret;
            KeyVaultSecret secretStripe = await _secretClient.GetSecretAsync("stripe-publickey");
            string publicKeyStripe = secretStripe.Value;
            ViewBag.PublicKey = publicKeyStripe;

            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Success(string session_id)
        {
            var service = new SessionService();
            Session session = service.Get(session_id);

            if (session.PaymentStatus == "paid")
            {
                // Avisamos a la API para que lo actualice en la Base de Datos
                await _settingsService.ActivarPremiumAsync();

                // Cerramos sesión para forzar la recarga del Token y los permisos Premium
                await HttpContext.SignOutAsync();
                HttpContext.Session.Clear();

                return RedirectToAction("Login", "Auth", new { mensaje = "¡Pago exitoso! Vuelve a iniciar sesión para disfrutar de Premium." });
            }

            return RedirectToAction("Index", "Dashboard");
        }
    }
}