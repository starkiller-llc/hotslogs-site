using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace HotsLogsApi.Auth.IsAdmin;

public class IsAdminHandler : AuthorizationHandler<IsAdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsAdminRequirement requirement)
    {
        var isAdmin = bool.Parse(context.User.FindFirst("hotslogs.admin")?.Value ?? "false");
        if (isAdmin)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
