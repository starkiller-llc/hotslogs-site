namespace HotsLogsApi.Auth.Models;

public class RegisterRequest
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string NewPassword { get; set; }
    public string CaptchaResponse { get; set; }
}
