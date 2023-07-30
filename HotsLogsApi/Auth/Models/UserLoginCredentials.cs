namespace HotsLogsApi.Auth.Models;

public class UserLoginCredentials
{
    public string Username { get; set; }
    public string Password { get; set; }
    public bool RememberMe { get; set; }
}
