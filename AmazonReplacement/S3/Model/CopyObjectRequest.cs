namespace Amazon.S3.Model;

public class CopyObjectRequest
{
    public string SourceBucket { get; set; }
    public string SourceKey { get; set; }
    public string DestinationBucket { get; set; }
    public string DestinationKey { get; set; }
    public S3StorageClass StorageClass { get; set; }
}
