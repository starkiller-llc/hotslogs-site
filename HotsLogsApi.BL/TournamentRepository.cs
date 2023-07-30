using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using ServiceStackReplacement;
using System;
using System.Linq;
using System.Threading.Tasks;
using ApiModels = HotsLogsApi.Models;

namespace HotsLogsApi.BL;

public class TournamentRepository
{
    private readonly HeroesdataContext _dc;
    private readonly MyDbWrapper _redis;

    public TournamentRepository(HeroesdataContext dc, MyDbWrapper redis)
    {
        _dc = dc;
        _redis = redis;
    }

    public async Task<int> CreateTournament(ApiModels.Tournament t)
    {
        try
        {
            var tournament = new Tournament
            {
                TournamentName = t.TournamentName,
                TournamentDescription = t.TournamentDescription,
                RegistrationDeadline = t.RegistrationDeadline,
                IsPublic = t.IsPublic,
                MaxNumTeams = t.MaxNumTeams > 0 ? t.MaxNumTeams : null,
                EntryFee = t.EntryFee,
            };
            await _dc.Tournament.AddAsync(tournament);
            await _dc.SaveChangesAsync();
            return 0;
        }
        catch
        {
            return 1;
        }
    }

    public async Task<ApiModels.Tournament> CreateTournamentMatches()
    {
        var utcTime = DateTime.Now.ToUniversalTime();
        var time = TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
        var sub = new TimeSpan(0, 6, 0, 0);
        time = time.Subtract(sub);
        var day = time.DayOfWeek;
        var hour = time.Hour;
        var minute = time.Minute;
        var second = time.Second;
        int addDays;
        switch (day)
        {
            case DayOfWeek.Monday:
                addDays = 6;
                break;
            case DayOfWeek.Tuesday:
                addDays = 5;
                break;
            case DayOfWeek.Wednesday:
                addDays = 4;
                break;
            case DayOfWeek.Thursday:
                addDays = 3;
                break;
            case DayOfWeek.Friday:
                addDays = 2;
                break;
            case DayOfWeek.Saturday:
                addDays = 1;
                break;
            case DayOfWeek.Sunday:
            default:
                addDays = 0;
                break;
        }

        var addTime = new TimeSpan(addDays + 7, 23 - hour, 60 - minute, 60 - second);
        var subtractTime = new TimeSpan(0, 0, 2, 0);
        var matchDeadline = time.Add(addTime).Subtract(subtractTime);

        var tournaments = await _dc.Tournament.Where(x => x.EndDate == null).ToArrayAsync();
        var ids = tournaments.Select(x => x.TournamentId);
        var teams =
            (await _dc.TournamentTeamDB.Where(x => ids.Contains(x.TournamentId)).ToArrayAsync()).OrderBy(x => x.TeamId);
        var matches = await _dc.TournamentMatch.Where(x => ids.Contains(x.TournamentId)).ToArrayAsync();
        foreach (var t in tournaments)
        {
            var tournamentId = t.TournamentId;
            var curMatches = matches.Where(x => x.TournamentId == tournamentId).ToArray();
            if (curMatches.Length == 0)
            {
                // add the first round to the tournament
                var curTeams = teams.Where(x => x.TournamentId == tournamentId).ToArray().OrderBy(x => x.TeamId)
                    .ToArray();
                var n = curTeams.Length;
                if ((t.MaxNumTeams == null || n < t.MaxNumTeams) && t.RegistrationDeadline > utcTime)
                {
                    continue;
                }

                if (n % 2 == 1)
                {
                    n = n - 1;
                    curTeams = curTeams.Take(n).ToArray();
                }

                var numPlayingW2 = Convert.ToInt32(Math.Pow(2, Math.Floor(Math.Log(n - 1, 2))));
                var numPlayingW1 = numPlayingW2 * 2;
                var numMatchesW1 = Convert.ToInt32(numPlayingW1 / 2);
                var actualMatchesW1 = n - numPlayingW2;
                var numByes = numMatchesW1 - actualMatchesW1;
                for (var j = 0; j < numByes; j++)
                {
                    var team1Id = curTeams[j].TeamId;
                    var newMatch = new TournamentMatchDB
                    {
                        TournamentId = tournamentId,
                        RoundNum = 1,
                        MatchCreated = DateTime.Now.Add(new TimeSpan(0, 0, 0, j)),
                        MatchDeadline = matchDeadline,
                        Team1Id = team1Id,
                        Team2Id = team1Id,
                    };
                    await _dc.TournamentMatch.AddAsync(newMatch);
                }

                for (var j = 0; j < actualMatchesW1; j++)
                {
                    var team1Id = curTeams[numByes + (j * 2)].TeamId;
                    var team2Id = curTeams[numByes + 1 + (j * 2)].TeamId;
                    var newMatch = new TournamentMatchDB
                    {
                        TournamentId = tournamentId,
                        RoundNum = 1,
                        MatchCreated = DateTime.Now.Add(new TimeSpan(0, 0, 0, numByes + j)),
                        MatchDeadline = matchDeadline,
                        Team1Id = team1Id,
                        Team2Id = team2Id,
                    };
                    await _dc.TournamentMatch.AddAsync(newMatch);
                }
            }
            else
            {
                // add another round to the tournament
                var curRound = curMatches.Select(x => x.RoundNum).Max();
                var curDeadline = curMatches.Where(x => x.RoundNum == curRound).Select(x => x.MatchDeadline).Max();
                var rdMatches = curMatches.Where(x => x.RoundNum == curRound).ToArray();
                var finishedMatches = rdMatches.Where(x => x.WinningTeamId != null).ToArray();
                // time to create a new round
                if (rdMatches.Length == finishedMatches.Length || curDeadline < utcTime)
                {
                    // if they didnt play their match, select a random team to advance
                    foreach (var t1 in rdMatches)
                    {
                        if (t1.WinningTeamId == null)
                        {
                            var random = new Random();
                            var n = random.Next(0, 2);
                            t1.WinningTeamId = n == 0 ? t1.Team1Id : t1.Team2Id;
                        }
                    }

                    var curTeams = rdMatches.OrderBy(x => x.Team1Id).Select(x => x.WinningTeamId).ToArray();
                    var numMatches = Math.Floor(Convert.ToDouble(curTeams.Length) / 2);

                    // if we have finished the tournament, update the EndDate
                    if (numMatches == 0)
                    {
                        t.EndDate = utcTime;
                        _dc.Tournament.Update(t);
                    }

                    // otherwise, create new matches
                    for (var j = 0; j < numMatches; j++)
                    {
                        var team1Id = curTeams[j * 2];
                        var team2Id = curTeams[1 + (j * 2)];
                        var newMatch = new TournamentMatchDB
                        {
                            TournamentId = tournamentId,
                            RoundNum = curRound + 1,
                            MatchCreated = DateTime.Now + new TimeSpan(0, 0, 0, j),
                            MatchDeadline = matchDeadline,
                            Team1Id = Convert.ToInt32(team1Id),
                            Team2Id = Convert.ToInt32(team2Id),
                        };
                        await _dc.TournamentMatch.AddAsync(newMatch);
                    }
                }
            }
        }

        await _dc.SaveChangesAsync();
        return null;
    }

    public async Task<ApiModels.TournamentMatch[]> GetMatchesForTournament(int tournamentId)
    {
        var matches = await _dc.TournamentMatch.Where(x => x.TournamentId == tournamentId).ToArrayAsync();
        var teams = await _dc.TournamentTeamDB.Where(x => x.TournamentId == tournamentId).ToArrayAsync();
        var a = new ApiModels.TournamentMatch[matches.Length];
        for (var i = 0; i < matches.Length; i++)
        {
            var m = matches[i];
            var team1Name = teams.FirstOrDefault(x => x.TeamId == m.Team1Id)?.TeamName;
            var team2Name = teams.FirstOrDefault(x => x.TeamId == m.Team2Id)?.TeamName;
            var cur = new ApiModels.TournamentMatch
            {
                MatchId = m.MatchId,
                TournamentId = m.TournamentId,
                ReplayId = m.ReplayId,
                RoundNum = m.RoundNum,
                MatchCreated = m.MatchCreated,
                MatchDeadline = m.MatchDeadline,
                MatchTime = m.MatchTime,
                Team1Id = m.Team1Id,
                Team2Id = m.Team2Id,
                Team1Name = team1Name,
                Team2Name = team2Name,
                WinningTeamId = m.WinningTeamId,
            };
            a[i] = cur;
        }

        return a;
    }

    public async Task<ApiModels.PlayerTournamentMatch[]> GetPlayerTournaments(int playerId)
    {
        var todaysDate = DateTime.Now;
        var results = await (from p in _dc.TournamentParticipant
                             join tm in _dc.TournamentTeamDB on p.TeamId equals tm.TeamId
                             join t in _dc.Tournament on p.TournamentId equals t.TournamentId
                             join pl in _dc.Players on p.Battletag equals pl.Name + '#' + pl.BattleTag
                             join m in _dc.TournamentMatch on t.TournamentId equals m.TournamentId
                             join t1 in _dc.TournamentTeamDB on m.Team1Id equals t1.TeamId
                             join t2 in _dc.TournamentTeamDB on m.Team2Id equals t2.TeamId
                             let team1Name = t1.TeamName
                             let team2Name = t2.TeamName
                             let wonMatch = tm.TeamId == m.WinningTeamId ? 1 : 0
                             where (t.EndDate == null || t.EndDate >= todaysDate) && pl.PlayerId == playerId &&
                                   (tm.TeamId == m.Team1Id || tm.TeamId == m.Team2Id)
                             orderby t.RegistrationDeadline descending, t.TournamentId, m.RoundNum descending
                             select new
                             {
                                 p.TournamentId,
                                 t.TournamentName,
                                 m.MatchId,
                                 m.MatchDeadline,
                                 m.RoundNum,
                                 m.ReplayId,
                                 tm.TeamId,
                                 tm.TeamName,
                                 m.Team1Id,
                                 m.Team2Id,
                                 Team1Name = team1Name,
                                 Team2Name = team2Name,
                                 m.WinningTeamId,
                             }).ToListAsync();
        results = results.GroupBy(x => x.TournamentId).Select(x => x.First()).ToList();

        var matches = new ApiModels.PlayerTournamentMatch[results.Count];
        for (var i = 0; i < results.Count(); i++)
        {
            var v = results[i];
            int? wonMatch = v.WinningTeamId == null ? null : v.WinningTeamId == v.TeamId ? 1 : 0;
            var m = new ApiModels.PlayerTournamentMatch
            {
                TournamentId = v.TournamentId,
                TournamentName = v.TournamentName,
                MatchId = v.MatchId,
                MatchDeadline = v.MatchDeadline,
                RoundNum = v.RoundNum,
                ReplayId = v.ReplayId,
                TeamId = v.TeamId,
                TeamName = v.TeamName,
                OppTeamId = v.TeamId == v.Team1Id ? v.Team2Id : v.Team1Id,
                OppTeamName = v.TeamId == v.Team1Id ? v.Team2Name : v.Team1Name,
                WonMatch = wonMatch,
            };
            matches[i] = m;
        }

        return matches;
    }

    public async Task<ApiModels.Tournament> GetTournamentById(int tournamentId)
    {
        var t = await _dc.Tournament.SingleOrDefaultAsync(x => x.TournamentId == tournamentId);
        var numTeams = await _dc.TournamentTeamDB.CountAsync(x => x.TournamentId == tournamentId);
        var cur = new ApiModels.Tournament
        {
            TournamentId = t.TournamentId,
            TournamentName = t.TournamentName,
            TournamentDescription = t.TournamentDescription,
            RegistrationDeadline = t.RegistrationDeadline,
            EndDate = t.EndDate,
            IsPublic = t.IsPublic,
            MaxNumTeams = t.MaxNumTeams,
            EntryFee = t.EntryFee,
            NumTeams = numTeams,
        };
        return cur;
    }

    public async Task<ApiModels.Tournament[]> GetTournaments()
    {
        var tournaments = await _dc.Tournament.ToArrayAsync();
        var teams = await _dc.TournamentTeamDB.ToArrayAsync();
        var a = new ApiModels.Tournament[tournaments.Length];
        for (var i = 0; i < tournaments.Length; i++)
        {
            var t = tournaments[i];
            var numTeams = teams.Count(x => x.TournamentId == t.TournamentId);
            var cur = new ApiModels.Tournament
            {
                TournamentId = t.TournamentId,
                TournamentName = t.TournamentName,
                TournamentDescription = t.TournamentDescription,
                RegistrationDeadline = t.RegistrationDeadline,
                EndDate = t.EndDate,
                IsPublic = t.IsPublic,
                MaxNumTeams = t.MaxNumTeams,
                EntryFee = t.EntryFee,
                NumTeams = numTeams,
            };
            a[i] = cur;
        }

        return a;
    }

    public async Task<int> RegisterForTournament(ApiModels.TournamentRegistrationApplication r)
    {
        try
        {
            var team = new TournamentTeamDB
            {
                TournamentId = r.TournamentId,
                TeamName = r.TeamName,
                CaptainEmail = r.CaptainEmail,
                PaypalEmail = r.PaypalEmail,
                IsPaid = 0,
                RegistrationDate = DateTime.Now,
            };
            await _dc.TournamentTeamDB.AddAsync(team);
            await _dc.SaveChangesAsync();
            try
            {
                var p1 = new TournamentParticipant
                {
                    TournamentId = r.TournamentId,
                    TeamId = team.TeamId,
                    Battletag = r.Battletag1,
                };
                await _dc.TournamentParticipant.AddAsync(p1);
                var p2 = new TournamentParticipant
                {
                    TournamentId = r.TournamentId,
                    TeamId = team.TeamId,
                    Battletag = r.Battletag2,
                };
                await _dc.TournamentParticipant.AddAsync(p2);
                var p3 = new TournamentParticipant
                {
                    TournamentId = r.TournamentId,
                    TeamId = team.TeamId,
                    Battletag = r.Battletag3,
                };
                await _dc.TournamentParticipant.AddAsync(p3);
                var p4 = new TournamentParticipant
                {
                    TournamentId = r.TournamentId,
                    TeamId = team.TeamId,
                    Battletag = r.Battletag4,
                };
                await _dc.TournamentParticipant.AddAsync(p4);
                var p5 = new TournamentParticipant
                {
                    TournamentId = r.TournamentId,
                    TeamId = team.TeamId,
                    Battletag = r.Battletag5,
                };
                await _dc.TournamentParticipant.AddAsync(p5);
                if ((r.Battletag6 != null) & (r.Battletag6 != ""))
                {
                    var p6 = new TournamentParticipant
                    {
                        TournamentId = r.TournamentId,
                        TeamId = team.TeamId,
                        Battletag = r.Battletag6,
                    };
                    await _dc.TournamentParticipant.AddAsync(p6);
                }

                if ((r.Battletag7 != null) & (r.Battletag7 != ""))
                {
                    var p7 = new TournamentParticipant
                    {
                        TournamentId = r.TournamentId,
                        TeamId = team.TeamId,
                        Battletag = r.Battletag7,
                    };
                    await _dc.TournamentParticipant.AddAsync(p7);
                }

                var participantChanges = await _dc.SaveChangesAsync();
                return 0;
            }
            catch
            {
                var createdTeam = _dc.TournamentTeamDB.SingleOrDefault(x => x.TeamId == team.TeamId);
                if (createdTeam != null)
                {
                    _dc.TournamentTeamDB.Remove(createdTeam);
                    await _dc.SaveChangesAsync();
                }

                return 1;
            }
        }
        catch
        {
            return 1;
        }
    }

    public async Task<ApiModels.TournamentMatch> SetMatchWinner(int matchId, int winningTeamId)
    {
        var results = from s in _dc.TournamentMatch
                      where s.WinningTeamId.Equals(1)
                      select s;

        var query = $@"UPDATE heroesdata.tournament_match
            SET WinningTeamId = {winningTeamId}
            WHERE MatchId = {matchId}; SELECT * FROM heroesdata.tournament_match WHERE MatchId = {matchId}; ";
        var execute = await _dc.TournamentMatch.FromSqlRaw(query).ToArrayAsync();
        var m = await _dc.TournamentMatch.Where(x => x.MatchId == matchId).SingleOrDefaultAsync();
        var match = new ApiModels.TournamentMatch
        {
            MatchId = m.MatchId,
            TournamentId = m.TournamentId,
            ReplayId = m.ReplayId,
            RoundNum = m.RoundNum,
            MatchCreated = m.MatchCreated,
            MatchDeadline = m.MatchDeadline,
            MatchTime = m.MatchTime,
            Team1Id = m.Team1Id,
            Team2Id = m.Team2Id,
            Team1Name = "",
            Team2Name = "",
            WinningTeamId = m.WinningTeamId,
        };
        return match;
    }

    public async Task<ApiModels.TournamentMatch> SetReplayId(int matchId, string replayGuid)
    {
        var results = from r in _dc.Replays
                      where r.Hotsapifingerprint.Equals(replayGuid)
                      select r.ReplayId;
        if (!results.Any())
        {
            return null;
        }

        var replayId = await results.FirstOrDefaultAsync();

        var query = $@"UPDATE heroesdata.tournament_match
            SET ReplayId = '{replayId}'
            WHERE MatchId = {matchId}; SELECT * FROM heroesdata.tournament_match WHERE MatchId = {matchId};";
        var execute = await _dc.TournamentMatch.FromSqlRaw(query).ToArrayAsync();
        var m = await _dc.TournamentMatch.Where(x => x.MatchId == matchId).SingleOrDefaultAsync();
        var match = new ApiModels.TournamentMatch
        {
            MatchId = m.MatchId,
            TournamentId = m.TournamentId,
            ReplayId = m.ReplayId,
            RoundNum = m.RoundNum,
            MatchCreated = m.MatchCreated,
            MatchDeadline = m.MatchDeadline,
            MatchTime = m.MatchTime,
            Team1Id = m.Team1Id,
            Team2Id = m.Team2Id,
            Team1Name = "",
            Team2Name = "",
            WinningTeamId = m.WinningTeamId,
        };
        return match;
    }

    public async Task<int> UpdateTournamenMatchDeadline(int TournamentId)
    {
        try
        {
            var past = DateTime.Today.AddDays(-120).ToString("yyyy-MM-dd");
            var query = $@"UPDATE heroesdata.tournament_match
                SET MatchDeadline = '{past}'
                WHERE TournamentId = {TournamentId}; SELECT * FROM heroesdata.tournament_match WHERE TournamentId = {TournamentId}; ";
            var execute = await _dc.TournamentMatch.FromSqlRaw(query).ToArrayAsync();
            return 0;
        }
        catch
        {
            return 1;
        }
    }

    public async Task<int> UpdateTournamentRegistrationDeadline(int TournamentId)
    {
        try
        {
            var past = DateTime.Today.AddDays(-120).ToString("yyyy-MM-dd");
            var query = $@"UPDATE heroesdata.tournament
                SET RegistrationDeadline = '{past}'
                WHERE TournamentId = {TournamentId}; SELECT * FROM heroesdata.tournament WHERE TournamentId = {TournamentId}; ";
            var execute = await _dc.Tournament.FromSqlRaw(query).ToArrayAsync();
            return 0;
        }
        catch
        {
            return 1;
        }
    }
}
