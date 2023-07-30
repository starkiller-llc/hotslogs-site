// ReSharper disable InconsistentNaming

namespace HotsLogsApi.BL.Migration.Models;

public class PageArgs
{
    public int length { get; set; }
    public int pageIndex { get; set; }
    public int pageSize { get; set; }
    public int previousPageIndex { get; set; }
}
