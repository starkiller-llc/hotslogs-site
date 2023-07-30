using System;
using System.IO;

namespace Amazon;

public class Response : IDisposable
{
    public Stream ResponseStream { get; set; }

    public void Dispose()
    {
        ResponseStream?.Dispose();
    }
}
