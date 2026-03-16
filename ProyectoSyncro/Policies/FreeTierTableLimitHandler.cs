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

           
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null)
            {
                var repo = httpContext.RequestServices.GetService<BaseRepository>();
                var claimIdEmpresa = context.User.FindFirst("IdEmpresa");

                if (claimIdEmpresa != null && repo != null)
                {
                    int idEmpresa = int.Parse(claimIdEmpresa.Value);

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