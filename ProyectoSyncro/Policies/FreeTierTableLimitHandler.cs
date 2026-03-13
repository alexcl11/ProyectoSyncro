using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; // Necesario para IHttpContextAccessor
using ProyectoSyncro.Repositories; // Cambia esto por tu namespace real
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProyectoSyncro.Policies
{
    public class FreeTierRequirement : IAuthorizationRequirement
    {
    }

    public class FreeTierTableLimitHandler : AuthorizationHandler<FreeTierRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // 1. Inyectamos el IHttpContextAccessor en el constructor
        public FreeTierTableLimitHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, FreeTierRequirement requirement)
        {
            // Comprobamos si está autenticado
            if (!context.User.Identity.IsAuthenticated) return;

            // Si el usuario es Premium, le dejamos pasar instantáneamente
            if (context.User.HasClaim("Plan", "Premium"))
            {
                context.Succeed(requirement);
                return;
            }

            // 2. MAGIA AQUÍ: Obtenemos el HttpContext a través del Accessor de forma 100% segura
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null)
            {
                // Obtenemos el repositorio desde los servicios de la petición
                var repo = httpContext.RequestServices.GetService<BaseRepository>();
                var claimIdEmpresa = context.User.FindFirst("IdEmpresa");

                if (claimIdEmpresa != null && repo != null)
                {
                    int idEmpresa = int.Parse(claimIdEmpresa.Value);

                    // Contamos cuántas tablas tiene
                    var tablas = await repo.GetTablasEmpresaAsync(idEmpresa);

                    // Si es plan Free y tiene 0, 1, o 2 tablas, lo dejamos pasar
                    if (tablas.Count < 3)
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        // Si ya tiene 3 o más y no es Premium, le bloqueamos la acción
                        context.Fail();
                    }
                }
            }
        }
    }
}