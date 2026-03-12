using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Models;
using ProyectoSyncro.Repositories;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProyectoSyncro.Controllers
    {
        public class AuthController : Controller
        {
            private AuthRepository repo;

            public AuthController(AuthRepository repo)
            {
                this.repo = repo;
            }

            public IActionResult Register()
            {
                return View();
            }

            [HttpPost]
            public async Task<IActionResult> Register(string nombreEmpresa, string cif, string nombreAdmin, string email, string password)
            {

                await this.repo.RegisterEmpresaUserAsync(cif, nombreEmpresa, nombreAdmin, email, password);



                UserSession nuevoUsuario = await this.repo.LoginUserAsync(email, password);


                if (nuevoUsuario != null)
                {
                    ClaimsIdentity identity = new ClaimsIdentity(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        ClaimTypes.Name,
                        ClaimTypes.Role);


                    identity.AddClaim(new Claim(ClaimTypes.Name, nuevoUsuario.Nombre ?? ""));
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, nuevoUsuario.IdUsuario.ToString()));
                    identity.AddClaim(new Claim("IdEmpresa", nuevoUsuario.IdEmpresa.ToString()));


                    identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));

                    identity.AddClaim(new Claim("Email", nuevoUsuario.Email ?? email));
                    identity.AddClaim(new Claim("NombreEmpresa", nuevoUsuario.NombreEmpresa ?? ""));
                    identity.AddClaim(new Claim("Plan", "Free"));

                    ClaimsPrincipal userPrincipal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        userPrincipal,
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddHours(8)
                        });

                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    ViewData["MENSAJE"] = "Hubo un error al registrar la empresa.";
                    return View();
                }
            }

            public IActionResult Login()
            {
                return View();
            }

            [HttpPost]
            public async Task<IActionResult> Login(string email, string password)
            {
                UserSession usuario = await this.repo.LoginUserAsync(email, password);

                if (usuario != null)
                {
                    ClaimsIdentity identity = new ClaimsIdentity(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            ClaimTypes.Name,
                            ClaimTypes.Role
                        );

                    identity.AddClaim(new Claim(ClaimTypes.Name, usuario.Nombre ?? ""));
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()));
                    identity.AddClaim(new Claim("IdEmpresa", usuario.IdEmpresa.ToString()));

                    string roleUser = usuario.Admin ? "Admin" : "Estandar";
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleUser));

                    identity.AddClaim(new Claim("Email", usuario.Email ?? email));
                    identity.AddClaim(new Claim("NombreEmpresa", usuario.NombreEmpresa ?? ""));
                    string planEmpresa = usuario.IsPremium ? "Premium" : "Free";
                    identity.AddClaim(new Claim("Plan", planEmpresa));

                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            principal,
                            new AuthenticationProperties
                            {
                                IsPersistent = true,
                                ExpiresUtc = DateTime.UtcNow.AddHours(8)
                            }
                        );

                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    TempData["InvalidCredentials"] = "Credenciales incorrectas";
                }
                return View();
            }
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Auth");
        }
    } 
    }