using Amazon.S3.Model;
using Amazon.SimpleEmail.Model;
using System.Collections.Generic;

namespace Amazon.S3;

public interface IAmazonHandler
{
    void DeleteObject(string bucketName, string key);

    // S3 Client
    byte[] GetObject(string bucketName, string key);
    void PutObject(string bucketName, string key, byte[] bytes, ICollection<KeyValuePair<string, string>> metadata);

    public PutObjectResponse PutObject(PutObjectRequest putObjectRequest);
    // Simple Email
    void SendEmail(SendEmailRequest sendEmailRequest);
}
