select
count(q.PlayerID) as ActivePlayers,
sum(case when loo.PlayerID is not null then 1 else 0 end) as CountLeaderboardOptOut,
sum(case when pb.PlayerID is not null then 1 else 0 end) as CountPlayerBanned
from
(select
distinct(rc.PlayerID) as PlayerID
from Replay r
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
where r.TimestampReplay > date_add(now(), interval -7 day) and r.TimestampReplay < now()) q
left join LeaderboardOptOut loo on loo.PlayerID = q.PlayerID
left join PlayerBanned pb on pb.PlayerID = q.PlayerID