using System;

namespace Amazon.S3;

public class AmazonS3Exception : Exception
{
    public string ErrorCode { get; set; }
}
