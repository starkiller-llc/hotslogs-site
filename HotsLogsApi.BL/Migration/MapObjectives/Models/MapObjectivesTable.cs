namespace HotsLogsApi.BL.Migration.MapObjectives.Models;

public class MapObjectivesTable
{
    public string Heading { get; set; }
    public string FieldName { get; set; }
    public MapObjectivesTableRow[] Data { get; set; }
}
