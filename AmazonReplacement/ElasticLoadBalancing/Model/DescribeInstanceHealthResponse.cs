using System.Collections.Generic;

namespace Amazon.ElasticLoadBalancing.Model;

public class DescribeInstanceHealthResponse
{
    public List<InstanceState> InstanceStates { get; set; }
}
