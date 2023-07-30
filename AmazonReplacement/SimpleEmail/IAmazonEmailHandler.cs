using Amazon.SimpleEmail.Model;

namespace Amazon.SimpleEmail;

public interface IAmazonEmailHandler
{
    SendEmailResponse SendEmail(SendEmailRequest sendemailrequest);
}
