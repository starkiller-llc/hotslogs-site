using Amazon.EC2.Model;
using System;
using System.Collections.Generic;

namespace Amazon.EC2;

public class AmazonEC2Client : IDisposable
{
    private readonly string _awsAccessKeyId;
    private readonly string _awsSecretAccessKey;
    private readonly string _region;

    public AmazonEC2Client(string awsAccessKeyId, string awsSecretAccessKey, string region)
    {
        _awsAccessKeyId = awsAccessKeyId;
        _awsSecretAccessKey = awsSecretAccessKey;
        _region = region;
    }

    public void Dispose() { }

    public DescribeNetworkInterfacesResponse DescribeNetworkInterfaces(
        DescribeNetworkInterfacesRequest describeNetworkInterfacesRequest)
    {
        return new DescribeNetworkInterfacesResponse
        {
            NetworkInterfaces = new List<NetworkInterface>(),
        };
    }
}
