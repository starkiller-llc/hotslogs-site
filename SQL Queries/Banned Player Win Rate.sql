select
r.GameMode, count(*) as Count, sum(rc.IsWinner) / count(*) as WinRate
from PlayerBanned pb
join ReplayCharacter rc on rc.PlayerID = pb.PlayerID
join Replay r use index (IX_TimestampReplay) on r.ReplayID = rc.ReplayID
where r.TimestampReplay > date_add(now(), interval -7 day)
group by r.GameMode