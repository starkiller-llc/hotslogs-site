using HotsLogsApi.Auth;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotsLogsApi;

public class MyUserClaimsPrincipalFactory : IUserClaimsPrincipalFactory<ApplicationUser>
{
    public Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var princ = UserHelper.CreatePrincipal(user);
        return Task.FromResult(princ);
    }
}
