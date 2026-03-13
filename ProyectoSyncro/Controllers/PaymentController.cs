using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using ProyectoSyncro.Repositories;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace ProyectoSyncro.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly SettingsRepository repo;

        public PaymentController(SettingsRepository repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            var domain = "https://localhost:7100";

            string idEmpresa = HttpContext.User.FindFirst("IdEmpresa").Value;
            string emailUsuario = HttpContext.User.FindFirst("Email")?.Value;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                CustomerEmail = emailUsuario, 
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 2900, // 29.00 €
                            Currency = "eur",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Plan Premium - Syncro",
                                Description = "Tablas infinitas. Registros ilimitados."
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                ClientReferenceId = idEmpresa, 
                SuccessUrl = domain + "/Payment/Success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + "/Settings/Index",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Redirect(session.Url); // Llevamos al usuario a Stripe
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