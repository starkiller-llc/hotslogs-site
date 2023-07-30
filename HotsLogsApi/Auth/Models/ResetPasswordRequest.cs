namespace HotsLogsApi.Auth.Models;

public class ResetPasswordRequest
{
    public string Email { get; set; }
    public string CaptchaResponse { get; set; }
}
