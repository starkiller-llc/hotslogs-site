using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using HotsLogsApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotsLogsApi.BL;

public class GameEventsRepository
{
    private readonly HeroesdataContext _dc;

    public GameEventsRepository(HeroesdataContext dc)
    {
        _dc = dc;
    }

    public static GameEventTeam ConvertFromDb(EventTeam arg)
    {
        if (arg is null)
        {
            return null;
        }

        return new GameEventTeam
        {
            Id = arg.TeamId,
            EventId = arg.EventId,
            Name = arg.Name,
            LogoUrl = arg.LogoUrl,
            Players = arg.ReplayCharacterProAssociations.Select(
                x =>
                {
                    var battleTag = x.Player is null
                        ? null
                        : $"{x.Player.Name}#{x.Player.BattleTag}";
                    return new GameEventPlayer
                    {
                        PlayerId = x.PlayerId,
                        BattleTag = battleTag,
                        Name = x.ProName,
                    };
                }).Distinct(PlayerIdComparer.Instance).ToList(),
        };
    }

    public async Task AssociateGameEventPlayer(int eventId, int playerId, string name)
    {
        var replaysOfPlayer = await _dc.Replays
            .Where(x => x.GameMode == eventId && x.ReplayCharacters.Any(y => y.PlayerId == playerId)).ToListAsync();
        replaysOfPlayer.ForEach(
            x =>
            {
                int? teamId = null;

                if (x.ReplayCharacterProAssociations == null)
                {
                    x.ReplayCharacterProAssociations = new List<ReplayCharacterProAssociation>();
                }
                else
                {
                    var sampleAssociation = x.ReplayCharacterProAssociations.FirstOrDefault();
                    teamId = sampleAssociation?.TeamId;
                }

                x.ReplayCharacterProAssociations.Add(
                    new ReplayCharacterProAssociation
                    {
                        PlayerId = playerId,
                        ProName = name,
                        ReplayId = x.ReplayId,
                        TeamId = teamId,
                    });
            });

        await _dc.SaveChangesAsync();
    }

    public async Task<GameEventTeam> AssociateGameEventTeam(
        int replayId,
        bool winningTeam,
        int teamId,
        string teamName)
    {
        if (teamId == 0 && string.IsNullOrWhiteSpace(teamName))
        {
            throw new InvalidOperationException("Either team id or team name must be specified.");
        }

        var replay = await _dc.Replays
            .Include(x => x.ReplayCharacters)
            .ThenInclude(x => x.Player)
            .Include(x => x.ReplayCharacterProAssociations)
            .SingleAsync(x => x.ReplayId == replayId);

        if (teamId == 0)
        {
            var newTeam = new EventTeam
            {
                EventId = replay.GameMode,
                Name = teamName,
            };

            await _dc.EventTeams.AddAsync(newTeam);
            await _dc.SaveChangesAsync();
            teamId = newTeam.TeamId;
        }

        var players = replay.ReplayCharacters.Where(x => x.IsWinner == (winningTeam ? 1ul : 0)).ToList();

        players.ForEach(
            x =>
            {
                x.ReplayCharacterProAssociation ??= new ReplayCharacterProAssociation
                {
                    TeamId = teamId,
                    PlayerId = x.PlayerId,
                };
            });

        await _dc.SaveChangesAsync();
        var team = await _dc.EventTeams.SingleAsync(x => x.TeamId == teamId);
        var rcTeam = ConvertFromDb(team);

        await PropagateAssociations(replay.GameMode);
        await _dc.SaveChangesAsync();

        return rcTeam;
    }

    public async Task<GameEventGamesAndInfo> GetGameEvent(int id)
    {
        var dbEvent = await _dc.Events
            .Include(x => x.Teams)
            .ThenInclude(x => x.ReplayCharacterProAssociations)
            .ThenInclude(x => x.Player)
            .SingleAsync(x => x.EventId == id);

        var teams = dbEvent.Teams.Select(ConvertFromDb).ToList();

        var rc = new GameEventGamesAndInfo
        {
            GameEvent = ConvertFromDb(dbEvent),
            Teams = teams,
        };

        var replays = await _dc.Replays
            .Include(x => x.ReplayCharacters)
            .ThenInclude(x => x.Player)
            .Include(x => x.ReplayCharacterProAssociations)
            .Where(x => x.GameMode == id)
            .OrderBy(x => x.TimestampReplay)
            .ToListAsync();

        var locDic = await _dc.LocalizationAliases.ToDictionaryAsync(x => x.IdentifierId);

        var games = from x in replays
            let loser = GetTeam(x, winner: false)
            let winner = GetTeam(x, winner: true)
            let winningPlayers = GetPlayers(x, winner: true)
            let losingPlayers = GetPlayers(x, winner: false)
            let mapName = locDic.ContainsKey(x.MapId) ? locDic[x.MapId].PrimaryName : null
            select new GameEventGame
            {
                DateTime = x.TimestampReplay,
                ReplayId = x.ReplayId,
                LosingTeamId = loser,
                WinningTeamId = winner,
                WinningPlayers = winningPlayers.ToList(),
                LosingPlayers = losingPlayers.ToList(),
                MapId = x.MapId,
                Map = mapName,
            };

        rc.Games = games.ToList();

        return rc;
    }

    public async Task<IEnumerable<GameEvent>> GetGameEvents()
    {
        var dbEvents = await _dc.Events
            .Include(x => x.EventIdparentNavigation)
            .Where(x => x.IsEnabled != 0)
            .ToListAsync();

        return dbEvents.Select(ConvertFromDb);
    }

    private GameEvent ConvertFromDb(Event arg)
    {
        if (arg is null)
        {
            return null;
        }

        return new GameEvent
        {
            Id = arg.EventId,
            Name = arg.EventName,
            ParentId = arg.EventIdparent,
            ParentName = arg.EventIdparentNavigation?.EventName,
        };
    }

    private IEnumerable<GameEventPlayer> GetPlayers(Replay replay, bool winner)
    {
        var c = replay.ReplayCharacters.Where(x => x.IsWinner == (winner ? 1ul : 0)).ToList();

        var associations = c
            .Where(x => x.ReplayCharacterProAssociation != null)
            .Select(x => x.ReplayCharacterProAssociation)
            .ToDictionary(x => x.PlayerId);

        var rc = c.Select(
            x => new GameEventPlayer
            {
                PlayerId = x.PlayerId,
                BattleTag = $"{x.Player.Name}#{x.Player.BattleTag}",
                Name = associations.ContainsKey(x.PlayerId) ? associations[x.PlayerId].ProName : null,
            });

        return rc;
    }

    private int? GetTeam(Replay replay, bool winner)
    {
        var c = replay.ReplayCharacters.Where(x => x.IsWinner == (winner ? 1ul : 0)).ToList();

        var associations = c
            .Where(x => x.ReplayCharacterProAssociation != null)
            .Select(x => x.ReplayCharacterProAssociation)
            .ToDictionary(x => x.PlayerId);

        var sampleAssociation = associations.Select(x => x.Value).FirstOrDefault();

        var sampleTeam = sampleAssociation?.Team;

        return sampleTeam?.TeamId;
    }

    private async Task PropagateAssociations(int eventId)
    {
        var replays = await _dc.Replays
            .Include(x => x.ReplayCharacters)
            .ThenInclude(x => x.ReplayCharacterProAssociation)
            .Where(x => x.GameMode == eventId)
            .ToListAsync();

        var teams = await _dc.EventTeams
            .Include(x => x.ReplayCharacterProAssociations)
            .Where(x => x.EventId == eventId)
            .ToListAsync();

        foreach (var replay in replays)
        {
            foreach (var replayCharacter in replay.ReplayCharacters)
            {
                if (!teams.Any(
                        x => x.ReplayCharacterProAssociations
                            .Any(z => z.PlayerId == replayCharacter.PlayerId)))
                {
                    continue;
                }

                var sampleTeam = teams.First(
                    x => x.ReplayCharacterProAssociations.Any(z => z.PlayerId == replayCharacter.PlayerId));

                replayCharacter.ReplayCharacterProAssociation ??= new ReplayCharacterProAssociation
                {
                    TeamId = sampleTeam.TeamId,
                };
            }
        }
    }
}
