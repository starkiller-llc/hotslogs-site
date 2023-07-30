using System;

namespace DiscordWebhooks;

public class Embed
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int? Color { get; set; }
    public Author Author { get; set; }
    public string Url { get; set; }
    public Field[] Fields { get; set; }
    public Image Image { get; set; }
    public Image Thumbnail { get; set; }
    public Footer Footer { get; set; }
    public DateTime? Timestamp { get; set; }
}
