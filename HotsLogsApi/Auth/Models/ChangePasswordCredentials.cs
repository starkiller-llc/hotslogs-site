namespace HotsLogsApi.Auth.Models;

public class ChangePasswordCredentials
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}
