using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HotsLogsApi.Auth;

public static class UserHelper
{
    public static ClaimsPrincipal CreatePrincipal(ApplicationUser user)
    {
        var stamp = GenerateSecurityStamp(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName),
            new(
                "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider",
                "ASP.NET Identity"),
            new("AspNet.Identity.SecurityStamp", stamp),
        };

        if (user.Admin)
        {
            claims.Add(new Claim("hotslogs.admin", "true"));
        }

        var identity = new ClaimsIdentity(claims, "Identity.Application");
        var principal = new ClaimsPrincipal(identity);
        return principal;
    }

    public static string GenerateSecurityStamp(ApplicationUser user)
    {
        // If any of these fields change, then the user is logged out when revalidating.
        var info = new
        {
            user.Email,
            user.Guid,
            user.PasswordHash,
            user.IsBattleNetOAuthAuthorized,
            user.Premium,
        };
        var json = JsonConvert.SerializeObject(info);
        var hash = Sha256Hash(json);
        return hash;
    }

    private static string Sha256Hash(string value)
    {
        var sb = new StringBuilder();

        using (var hash = SHA256.Create())
        {
            var enc = Encoding.UTF8;
            var result = hash.ComputeHash(enc.GetBytes(value));

            foreach (var b in result)
            {
                sb.Append(b.ToString("x2"));
            }
        }

        return sb.ToString();
    }
}
