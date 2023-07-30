using Amazon.ElasticLoadBalancing.Model;
using System;
using System.Collections.Generic;

namespace Amazon.ElasticLoadBalancing;

public class AmazonElasticLoadBalancingClient : IDisposable
{
    private string _awsAccessKeyId;
    private string _awsSecretAccessKey;
    private string _region;

    public AmazonElasticLoadBalancingClient(string awsAccessKeyId, string awsSecretAccessKey, string region)
    {
        _awsAccessKeyId = awsAccessKeyId;
        _awsSecretAccessKey = awsSecretAccessKey;
        _region = region;
    }

    public void Dispose() { }

    public DescribeInstanceHealthResponse DescribeInstanceHealth(
        DescribeInstanceHealthRequest describeInstanceHealthRequest)
    {
        return new DescribeInstanceHealthResponse
        {
            InstanceStates = new List<InstanceState>(),
        };
    }
}
