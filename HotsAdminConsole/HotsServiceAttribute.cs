using System;

namespace HotsAdminConsole;

[AttributeUsage(AttributeTargets.Class)]
internal class HotsServiceAttribute : Attribute
{
    public HotsServiceAttribute(string title)
    {
        Title = title;
    }

    public string Title { get; set; }
    public bool KeepRunning { get; set; }
    public double Sort { get; set; }
    public bool AutoStart { get; set; }
    public int Port { get; set; }
}
