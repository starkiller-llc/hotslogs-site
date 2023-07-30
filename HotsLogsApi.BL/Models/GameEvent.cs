namespace HotsLogsApi.Models;

public class GameEvent
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ParentId { get; set; }
    public string ParentName { get; set; }
}
