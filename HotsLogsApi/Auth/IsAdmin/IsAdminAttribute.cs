using Microsoft.AspNetCore.Authorization;

namespace HotsLogsApi.Auth.IsAdmin;

public class IsAdminAttribute : AuthorizeAttribute
{
    public IsAdminAttribute()
    {
        Policy = "IsAdmin";
    }
}
