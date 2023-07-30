using System;

namespace HotsAdminConsole;

internal class HotsServiceInfo
{
    public string Name { get; set; }
    public Type ServiceType { get; set; }
    public string Title { get; set; }
    public bool KeepRunning { get; set; }
    public int Port { get; set; }
    public double Sort { get; set; }
    public bool AutoStart { get; set; }
    public string ComboText { get; set; }
    public ServiceForm ServiceForm { get; set; }
}
