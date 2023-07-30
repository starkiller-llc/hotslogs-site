namespace DiscordWebhooks;

public class ServiceMessage
{
    public string ServiceName { get; set; }
    public string Message { get; set; }

    public WebhookMessage GetMessage()
    {
        return new WebhookMessage
        {
            Username = "Admin Console",
            Embeds = new[]
            {
                new Embed
                {
                    Title = ServiceName,
                    Description = Message,
                },
            },
        };
    }
}
