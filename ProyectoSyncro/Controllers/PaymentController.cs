using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Models;
using ProyectoSyncro.Repositories;
using Stripe.Checkout;
using System.Security.Claims;

namespace ProyectoSyncro.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private SettingsRepository repo;
        private BaseRepository baseRepo;
        public PaymentController(SettingsRepository repo, BaseRepository baseRepo)
        {
            this.repo = repo;
            this.baseRepo = baseRepo;
        }

        [HttpGet]
        public async  Task<IActionResult> Checkout()
        {
            var request = HttpContext.Request;
            var domain = $"{request.Scheme}://{request.Host}";

            string idEmpresa = HttpContext.User.FindFirst("IdEmpresa").Value;
            string emailUsuario = HttpContext.User.FindFirst("Email")?.Value;

            Empresa empresa = await this.repo.GetEmpresaAsync(int.Parse(idEmpresa));

            ViewData["TablasEmpresa"] = await this.baseRepo.GetTablasEmpresaAsync(int.Parse(idEmpresa));
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
                
                            // Le decimos que es recurrente cada mes
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
                // Cambiamos a modo suscripción
                Mode = "subscription",
                ClientReferenceId = idEmpresa,
                ReturnUrl = domain + "/Payment/Success?session_id={CHECKOUT_SESSION_ID}",
            };
            var service = new SessionService();
            Session session = service.Create(options);

            ViewBag.ClientSecret = session.ClientSecret;
            ViewBag.PublicKey = "pk_test_51TASDdKG8fHbwgC1LrWG6Il8hlGN60tw80CxrmU2959IqvYlBTwiTLMfiyvWdFTuMLxcQvnBeezT4481wBkMtKSv00HzfYHEhB";

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Success(string session_id)
        {
            var service = new SessionService();
            Session session = service.Get(session_id);

            // Si el pago se ha cobrado correctamente
            if (session.PaymentStatus == "paid")
            {
                int idEmpresa = int.Parse(session.ClientReferenceId);

                // Actualizamos la BD a Premium
                await this.repo.UpdateEmpresaPremiumAsync(idEmpresa);

                // Cerramos sesión para que al volver a entrar se refresquen sus permisos
                await HttpContext.SignOutAsync();

                return RedirectToAction("Login", "Auth", new { mensaje = "¡Pago exitoso! Vuelve a iniciar sesión para disfrutar de Premium." });
            }

            return RedirectToAction("Index", "Dashboard");
        }
    }
}