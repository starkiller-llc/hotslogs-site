using System;

namespace HelperCore;

public class GameEvent
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ParentId { get; set; }
    public string ParentName { get; set; }
    public int BuildFirst { get; set; }
    public int BuildLast { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}
