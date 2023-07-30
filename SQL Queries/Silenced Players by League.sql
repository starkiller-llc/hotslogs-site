select q2.*, q2.Count / q2.LeagueTotal as PercentOfLeague from
(select q.*, (select count(*) from LeaderboardRanking where GameMode = 4 and LeagueID = q.LeagueID) as LeagueTotal from
(select lr.GameMode, lr.LeagueID, count(*) as Count
from PlayerBanned pb
join LeaderboardRanking lr on lr.PlayerID = pb.PlayerID and lr.GameMode = 4
where lr.LeagueID is not null
group by lr.GameMode, lr.LeagueID) q) q2