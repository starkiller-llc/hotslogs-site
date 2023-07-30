namespace HotsLogsApi.Models;

public class HotsLogsOptions
{
    public string HotsLogsEmailPassword { get; set; }
    public string CaptchaKey { get; set; }
    public string CaptchaSecret { get; set; }
    public string CaptchaUrl { get; set; }
    public string RecoveryMailUsername { get; set; }
    public string RecoveryMailPassword { get; set; }
    public string GoogleAnalyticsPropertyId { get; set; }
    public string DiscordGithubHook { get; set; }
    public string DiscordNodeHook { get; set; }
    public string AwsAccessKeyID { get; set; }
    public string AwsSecretAccessKey { get; set; }
}
