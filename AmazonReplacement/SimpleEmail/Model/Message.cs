namespace Amazon.SimpleEmail.Model;

public class Message
{
    public Message(Content content, Body body)
    {
        Content = content;
        Body = body;
    }

    public Content Content { get; }
    public Body Body { get; }
}
