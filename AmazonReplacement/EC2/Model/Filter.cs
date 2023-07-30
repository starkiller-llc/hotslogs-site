using System.Collections.Generic;

namespace Amazon.EC2.Model;

public class Filter
{
    public string Name { get; set; }
    public List<string> Values { get; set; }
}
