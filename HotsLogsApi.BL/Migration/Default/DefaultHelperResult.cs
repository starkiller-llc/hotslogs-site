using JetBrains.Annotations;
using System.Collections.Generic;

namespace HotsLogsApi.BL.Migration.Default;

[PublicAPI]
public class DefaultHelperResult
{
    private List<CharacterStatisticsViewData> _dataSource;
    private bool _gamesBannedVisible;
    private bool _gamesPlayedVisible;
    private string _importantNote;
    private bool _importantNoteVisible;
    private string _lastUpdated;
    private int _numberOfPatchesShown;
    private IEnumerable<KeyValuePair<string, string>> _roleButtonsDataSource;
    private IEnumerable<KeyValuePair<string, string>> _roles;
    private string _totalGamesPlayedMessage;
    private bool _winPercentDelta;

    public DefaultConstants Constants { get; set; } = new DefaultConstants();

    public bool ImportantNoteVisibleSet { get; set; }

    public bool ImportantNoteVisible
    {
        get => _importantNoteVisible;
        set
        {
            _importantNoteVisible = value;
            ImportantNoteVisibleSet = true;
        }
    }

    public bool ImportantNoteSet { get; set; }

    public string ImportantNote
    {
        get => _importantNote;
        set
        {
            _importantNote = value;
            ImportantNoteSet = true;
        }
    }

    public bool TotalGamesPlayedMessageSet { get; set; }

    public string TotalGamesPlayedMessage
    {
        get => _totalGamesPlayedMessage;
        set
        {
            _totalGamesPlayedMessage = value;
            TotalGamesPlayedMessageSet = true;
        }
    }

    public bool DataSourceSet { get; set; }

    public List<CharacterStatisticsViewData> DataSource
    {
        get => _dataSource;
        set
        {
            _dataSource = value;
            DataSourceSet = true;
        }
    }

    public bool NumberOfPatchesShownSet { get; set; }

    public int NumberOfPatchesShown
    {
        get => _numberOfPatchesShown;
        set
        {
            _numberOfPatchesShown = value;
            NumberOfPatchesShownSet = true;
        }
    }

    public bool LastUpdatedSet { get; set; }

    public string LastUpdated
    {
        get => _lastUpdated;
        set
        {
            _lastUpdated = value;
            LastUpdatedSet = true;
        }
    }

    public bool RolesSet { get; set; }

    public IEnumerable<KeyValuePair<string, string>> Roles
    {
        get => _roles;
        set
        {
            _roles = value;
            RolesSet = true;
        }
    }

    public bool RoleButtonsDataSourceSet { get; set; }

    public IEnumerable<KeyValuePair<string, string>> RoleButtonsDataSource
    {
        get => _roleButtonsDataSource;
        set
        {
            _roleButtonsDataSource = value;
            RoleButtonsDataSourceSet = true;
        }
    }

    public bool GamesPlayedVisibleSet { get; set; }

    public bool GamesPlayedVisible
    {
        get => _gamesPlayedVisible;
        set
        {
            _gamesPlayedVisible = value;
            GamesPlayedVisibleSet = true;
        }
    }

    public bool GamesBannedVisibleSet { get; set; }

    public bool GamesBannedVisible
    {
        get => _gamesBannedVisible;
        set
        {
            _gamesBannedVisible = value;
            GamesBannedVisibleSet = true;
        }
    }

    public bool WinPercentDeltaVisibleSet { get; set; }

    public bool WinPercentDeltaVisible
    {
        get => _winPercentDelta;
        set
        {
            _winPercentDelta = value;
            WinPercentDeltaVisibleSet = true;
        }
    }
}
