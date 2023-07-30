using Amazon.SimpleEmail.Model;
using System.Collections.Generic;

namespace HotsLogsApi.BL.Amazon;

public interface IAmazonReplacementHandler
{
    void DeleteObject(string bucketName, string key);

    // S3 Client
    byte[] GetObject(string bucketName, string key);
    void PutObject(string bucketName, string key, byte[] bytes, ICollection<KeyValuePair<string, string>> metadata);

    // Simple Email
    void SendEmail(SendEmailRequest sendEmailRequest);
}
