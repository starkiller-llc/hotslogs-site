namespace DiscordWebhooks;

public class WebhookMessage
{
    public string Username { get; set; }
    public string Content { get; set; }
    public string AvatarUrl { get; set; }
    public Embed[] Embeds { get; set; }
    public bool? Tts { get; set; }
}
