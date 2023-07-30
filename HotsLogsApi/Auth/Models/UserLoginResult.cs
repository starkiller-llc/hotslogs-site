namespace HotsLogsApi.Auth.Models;

public class UserLoginResult
{
    public bool Success { get; set; }
    public int Error { get; set; }
    public string ErrorName { get; set; }
}
