select
p.Name,
rc.PlayerID,
rc.IsWinner,
avg(timestampdiff(hour, r.TimestampReplay, r.TimestampCreated)) as TimeDifference
from Replay r
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
join Player p on p.PlayerID = rc.PlayerID
where r.TimestampReplay > date_add(NOW(), interval -7 day)
and p.LeagueID = 0 and (p.IsEligibleForLeaderboard = true or p.PlayerID = 23098)
group by p.Name, rc.PlayerID, rc.IsWinner