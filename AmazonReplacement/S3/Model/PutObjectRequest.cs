using System.IO;

namespace Amazon.S3.Model;

public class PutObjectRequest
{
    public string BucketName { get; set; }
    public string Key { get; set; }
    public string FilePath { get; set; }
    public S3StorageClass StorageClass { get; set; }
    public Stream InputStream { get; set; }
    public MetadataCollection Metadata { get; set; }
}
