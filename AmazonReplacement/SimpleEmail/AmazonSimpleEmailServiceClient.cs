using Amazon.SimpleEmail.Model;
using System;

namespace Amazon.SimpleEmail;

public class AmazonSimpleEmailServiceClient : IDisposable
{
    public delegate void sendEmail(SendEmailRequest sendEmailRequest);

    private readonly string _awsAccessKeyId;
    private readonly string _awsSecretAccessKey;
    private readonly string _region;

    public AmazonSimpleEmailServiceClient(string awsAccessKeyId, string awsSecretAccessKey, string region)
    {
        _awsAccessKeyId = awsAccessKeyId;
        _awsSecretAccessKey = awsSecretAccessKey;
        _region = region;
    }

    public static sendEmail SendEmailDelegate { get; set; }

    public void Dispose() { }

    public SendEmailResponse SendEmail(SendEmailRequest sendEmailRequest)
    {
        SendEmailDelegate?.Invoke(sendEmailRequest);
        return new SendEmailResponse();
    }
}
