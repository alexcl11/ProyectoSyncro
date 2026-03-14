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
                // 🔥 1. Le decimos a Stripe que esto irá incrustado en nuestra web
                UiMode = "embedded",

                PaymentMethodTypes = new List<string> { "card" },
                CustomerEmail = emailUsuario,
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 2900,
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

                // 🔥 2. En modo embedded, SuccessUrl se cambia por ReturnUrl
                ReturnUrl = domain + "/Payment/Success?session_id={CHECKOUT_SESSION_ID}",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            // 🔥 3. Pasamos a la vista la "llave" de esta sesión y tu clave pública
            ViewBag.ClientSecret = session.ClientSecret;
            ViewBag.PublicKey = "pk_test_51TASDdKG8fHbwgC1LrWG6Il8hlGN60tw80CxrmU2959IqvYlBTwiTLMfiyvWdFTuMLxcQvnBeezT4481wBkMtKSv00HzfYHEhB";

            // 🔥 4. Devolvemos una Vista nuestra en vez de redirigir
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