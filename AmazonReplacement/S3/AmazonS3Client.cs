using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Amazon.S3;

public class AmazonS3Client : IDisposable
{
    public delegate void deleteObject(string bucketName, string key);

    public delegate byte[] getObject(string bucketName, string key);

    public delegate void putObject(
        string bucketName,
        string key,
        byte[] bytes,
        ICollection<KeyValuePair<string, string>> metadata);

    private static readonly Dictionary<Tuple<string, string>, byte[]> Objects =
        new Dictionary<Tuple<string, string>, byte[]>();

    private readonly string _awsAccessKeyId;
    private readonly string _awsSecretAccessKey;
    private readonly string _region;

    public AmazonS3Client(string awsAccessKeyId, string awsSecretAccessKey, string region)
    {
        _awsAccessKeyId = awsAccessKeyId;
        _awsSecretAccessKey = awsSecretAccessKey;
        _region = region;
    }

    public static getObject GetObjectDelegate { get; set; }
    public static putObject PutObjectDelegate { get; set; }
    public static deleteObject DeleteObjectDelegate { get; set; }

    public void Dispose() { }

    public CopyObjectResponse CopyObject(CopyObjectRequest copyObjectRequest)
    {
        lock (Objects)
        {
            var key = Tuple.Create(copyObjectRequest.SourceBucket, copyObjectRequest.SourceKey);
            var destKey = Tuple.Create(copyObjectRequest.DestinationBucket, copyObjectRequest.DestinationKey);
            if (!Objects.ContainsKey(key))
            {
                Fetch(key);
            }

            if (Objects.ContainsKey(key))
            {
                Objects[destKey] = Objects[key];
                Deposit(destKey, Objects[key], null);
            }

            throw new KeyNotFoundException($"BucketName {key.Item1}, Key {key.Item2} not found");
        }
    }

    public DeleteObjectResponse DeleteObject(DeleteObjectRequest deleteObjectRequest)
    {
        lock (Objects)
        {
            var key = Tuple.Create(deleteObjectRequest.BucketName, deleteObjectRequest.Key);
            Remove(key);
            if (Objects.ContainsKey(key))
            {
                Objects.Remove(key);
            }

            return new DeleteObjectResponse
            {
                HttpStatusCode = HttpStatusCode.OK,
            };
        }
    }

    public GetObjectResponse GetObject(GetObjectRequest getObjectRequest)
    {
        lock (Objects)
        {
            var key = Tuple.Create(getObjectRequest.BucketName, getObjectRequest.Key);
            if (!Objects.ContainsKey(key))
            {
                Fetch(key);
            }

            if (Objects.ContainsKey(key))
            {
                return new GetObjectResponse
                {
                    ResponseStream = new MemoryStream(Objects[key]),
                };
            }

            return null;
        }
    }

    public GetObjectMetadataResponse GetObjectMetadata(GetObjectMetadataRequest getObjectMetadataRequest)
    {
        lock (Objects)
        {
            var key = Tuple.Create(getObjectMetadataRequest.BucketName, getObjectMetadataRequest.Key);
            if (!Objects.ContainsKey(key))
            {
                Fetch(key);
            }

            if (Objects.ContainsKey(key))
            {
                return new GetObjectMetadataResponse
                {
                    ContentLength = Objects[key].Length,
                };
            }

            throw new KeyNotFoundException($"BucketName {key.Item1}, Key {key.Item2} not found");
        }
    }

    public ListBucketsResponse ListBuckets(ListBucketsRequest listBucketsRequest)
    {
        lock (Objects)
        {
            var buckets = Objects.Keys
                .Select(x => x.Item1)
                .Distinct()
                .Select(
                    x => new Bucket
                    {
                        BucketName = x,
                    }).ToList();
            return new ListBucketsResponse
            {
                Buckets = buckets,
            };
        }
    }

    public PutBucketResponse PutBucket(PutBucketRequest putBucketRequest)
    {
        lock (Objects)
        {
            var key = Tuple.Create(putBucketRequest.BucketName, Guid.Empty.ToString());
            Objects[key] = Array.Empty<byte>();
            Deposit(key, Array.Empty<byte>(), null);
            return new PutBucketResponse();
        }
    }

    public PutObjectResponse PutObject(PutObjectRequest putObjectRequest)
    {
        lock (Objects)
        {
            var ownedStream = false;
            var stream = putObjectRequest.InputStream;
            if (stream is null)
            {
                ownedStream = true;
                stream = File.OpenRead(putObjectRequest.FilePath);
            }

            try
            {
                var length = stream.Length;
                var br = new BinaryReader(stream);
                var bytes = br.ReadBytes(Convert.ToInt32(length));
                var key = Tuple.Create(putObjectRequest.BucketName, putObjectRequest.Key);
                Objects[key] = bytes;
                Deposit(key, bytes, putObjectRequest.Metadata);
                return new PutObjectResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                };
            }
            finally
            {
                if (ownedStream)
                {
                    stream.Dispose();
                }
            }
        }
    }

    private void Deposit(Tuple<string, string> key, byte[] bytes, MetadataCollection metadata)
    {
        try
        {
            PutObjectDelegate?.Invoke(key.Item1, key.Item2, bytes, metadata);
        }
        catch
        {
            // ignored
        }
    }

    private void Fetch(Tuple<string, string> key)
    {
        try
        {
            var obj = GetObjectDelegate?.Invoke(key.Item1, key.Item2);
            if (obj != null)
            {
                Objects[key] = obj;
            }
        }
        catch
        {
            // ignored
        }
    }

    private void Remove(Tuple<string, string> key)
    {
        try
        {
            DeleteObjectDelegate?.Invoke(key.Item1, key.Item2);
        }
        catch
        {
            // ignored
        }
    }
}
