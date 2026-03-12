using Microsoft.AspNetCore.Authorization;
using ProyectoSyncro.Repositories; // Cambia esto por tu namespace real
using System.Security.Claims;

namespace ProyectoSyncro.Policies
{
    public class FreeTierRequirement : IAuthorizationRequirement
    {
    }

    public class FreeTierTableLimitHandler : AuthorizationHandler<FreeTierRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, FreeTierRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated) return;

            if (context.User.HasClaim("Plan", "Premium"))
            {
                context.Succeed(requirement);
                return;
            }


            if (context.Resource is HttpContext httpContext)
            {
                var repo = httpContext.RequestServices.GetService<BaseRepository>();

                var claimIdEmpresa = context.User.FindFirst("IdEmpresa");

                if (claimIdEmpresa != null && repo != null)
                {
                    int idEmpresa = int.Parse(claimIdEmpresa.Value);

                    var tablas = await repo.GetTablasEmpresaAsync(idEmpresa);

                    if (tablas.Count < 3)
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail();
                    }
                }
            }
        }
    }
}