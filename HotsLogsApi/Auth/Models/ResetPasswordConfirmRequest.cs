namespace HotsLogsApi.Auth.Models;

public class ResetPasswordConfirmRequest
{
    public int Id { get; set; }
    public string Token { get; set; }
    public string NewPassword { get; set; }
}
