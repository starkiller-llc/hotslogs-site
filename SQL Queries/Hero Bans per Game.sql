select count(*) as TotalGames, avg(HeroBanCount) as AvgHeroBanCount from
(select r.ReplayID, count(*) as HeroBanCount
from Replay r use index (IX_GameMode_TimestampReplay)
join ReplayTeamHeroBan rthb on rthb.ReplayID = r.ReplayID
where r.GameMode = 4 and r.TimestampReplay > date_add(now(), interval -7 day) and r.TimestampReplay < now()
group by r.ReplayID) q