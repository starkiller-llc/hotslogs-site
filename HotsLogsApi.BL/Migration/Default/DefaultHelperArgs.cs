using System;

namespace HotsLogsApi.BL.Migration.Default;

public class DefaultHelperArgs
{
    public DateTime ReferenceDate { get; set; }
    public bool IsMobileDevice { get; set; }
    public bool IsPostBack { get; set; }
    public bool MonkeyBrokerScriptVisible { get; set; }
}
