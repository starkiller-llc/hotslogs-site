namespace Amazon.SimpleEmail.Model;

public class SendEmailRequest
{
    public string Source { get; set; }
    public Destination Destination { get; set; }
    public Message Message { get; set; }
}
