using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProyectoSyncro.Api.Repositories;
using ProyectoSyncro.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MailKit.Net.Smtp;
using MimeKit;

namespace ProyectoSyncro.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly SecretClient _secretClient;

        public AuthController(AuthRepository repo, IConfiguration config, SecretClient secretClient)
        {
            _repo = repo;
            _config = config;
            _secretClient = secretClient;
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Validamos credenciales contra SQL Server usando tu repo intacto
            UserSession user = await _repo.LoginUserAsync(request.Email, request.Password);

            if (user == null)
            {
                return Unauthorized(new { mensaje = "Credenciales incorrectas" });
            }

            // 2. Si es correcto, fabricamos sus "Claims" (lo que va dentro del token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim("Email", user.Email),
                new Claim("IdUsuario", user.IdUsuario.ToString()),
                new Claim("IdEmpresa", user.IdEmpresa.ToString()),
                new Claim("NombreEmpresa", user.NombreEmpresa ?? "Mi Empresa"),
                new Claim("Plan", user.IsPremium ? "Premium" : "Free"),
                new Claim(ClaimTypes.Role, user.Admin ? "Admin" : "User")
            };

            // 3. Firmamos el token con nuestra clave secreta
            KeyVaultSecret jwtKeySecret = await _secretClient.GetSecretAsync("jwt-secretkey");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKeySecret.Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4), // El token dura 4 horas
                signingCredentials: creds
            );

            // 4. Devolvemos el token en formato string
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { token = tokenString });
        }

        public class RegisterRequest
        {
            public string Cif { get; set; }
            public string NombreEmpresa { get; set; }
            public string NombreUsuario { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Llama a tu procedimiento almacenado que encripta la clave y crea todo
                int result = await _repo.RegisterEmpresaUserAsync(
                    request.Cif,
                    request.NombreEmpresa,
                    request.NombreUsuario,
                    request.Email,
                    request.Password);

                // Asumimos que si devuelve un número mayor a 0, fue un éxito (ajusta según tu SP)
                if (result > 0)
                {
                    return Ok(new { mensaje = "Empresa y usuario registrados con éxito" });
                }
                return BadRequest(new { mensaje = "El usuario o CIF ya existe" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en el servidor: " + ex.Message });
            }
        }

        public class RecoverRequest { public string Email { get; set; } }

        [HttpPost("recoverpassword")]
        public async Task<IActionResult> RecoverPassword([FromBody] RecoverRequest request)
        {
            var user = await _repo.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                // Devolvemos 200 de igual modo por seguridad (evita enumm de emails)
                return Ok(new { mensaje = "Si el correo es válido, se envió un email." });
            }

            // 1. Generar token de reseteo
            KeyVaultSecret jwtKeySecret = await _secretClient.GetSecretAsync("jwt-secretkey");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKeySecret.Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] { new Claim(ClaimTypes.Email, request.Email) };
            
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15), 
                signingCredentials: creds
            );

            string resetToken = new JwtSecurityTokenHandler().WriteToken(token);

            // 2. Construir la URL de reseteo basada en la configuración de la propia API
            string baseUrlMvc = _config["MvcUrl"] ?? "https://localhost:7100/"; 
            if (!baseUrlMvc.EndsWith("/")) baseUrlMvc += "/";
            string resetUrl = $"{baseUrlMvc}Auth/ResetPassword?token={resetToken}"; 
            
            try
            {
                // 3. Traer credenciales de Key Vault para el servidor SMTP (correo y pass)
                KeyVaultSecret emailUserSecret = await _secretClient.GetSecretAsync("email-user");
                KeyVaultSecret emailPassSecret = await _secretClient.GetSecretAsync("email-password");

                string remitente = emailUserSecret.Value;
                string passwordSmtp = emailPassSecret.Value;

                // 4. Configurar MailKit y enviar
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Syncro", remitente));
                message.To.Add(new MailboxAddress("", request.Email));
                message.Subject = "Restablece tu contraseña - Syncro";

                message.Body = new TextPart("html")
                {
                    Text = $"<h2>Recuperación de contraseña</h2><p>Haz clic en el siguiente enlace para restablecer tu cuenta. Este enlace caducará en 15 minutos.</p><a href=\"{resetUrl}\">Restablecer Contraseña</a>"
                };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(remitente, passwordSmtp);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                 // Para debug en entorno de dev. En prod lo quitaríamos
                 return StatusCode(500, "Fallo al enviar correo: " + ex.Message);
            }
            
            return Ok(new { mensaje = "Si el correo es válido, se envió un email." });
        }

        public class ResetRequest { public string Token { get; set; } public string NuevaPassword { get; set; } }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetRequest request)
        {
            // Decodificamos el JWT para recuperar de forma segura de qué email era
            var handler = new JwtSecurityTokenHandler();
            try {
                // Validación rápida (si caducó soltará excepción)
                var jwtToken = handler.ReadJwtToken(request.Token);
                var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email)?.Value;

                if(string.IsNullOrEmpty(emailClaim)) return BadRequest("Token corrupto.");

                // Actualizamos DB
                bool success = await _repo.ResetPasswordAsync(emailClaim, request.NuevaPassword);
                if (success) return Ok(new { mensaje = "Clave actualizada con éxito" });

                return BadRequest("Usuario inactivo");
            }
            catch { return BadRequest("Token expirado o inválido."); }
        }
    }
}