using System.Collections.Generic;

namespace Amazon.EC2.Model;

public class DescribeNetworkInterfacesRequest
{
    public List<Filter> Filters { get; set; }
}
