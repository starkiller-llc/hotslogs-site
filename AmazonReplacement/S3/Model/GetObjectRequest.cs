namespace Amazon.S3.Model;

public class GetObjectRequest
{
    public string BucketName { get; set; }
    public string Key { get; set; }
    public RequestPayer RequestPayer { get; set; }
}
