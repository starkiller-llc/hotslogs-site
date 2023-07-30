namespace HotsLogsApi.BL.Migration.Overview.Models;

public class GameModeListEntry
{
    public GameModeListEntry(string gameModeDisplayText, int gameModeId)
    {
        GameModeDisplayText = gameModeDisplayText;
        GameModeID = gameModeId;
    }

    public string GameModeDisplayText { get; }
    public int GameModeID { get; }
}
