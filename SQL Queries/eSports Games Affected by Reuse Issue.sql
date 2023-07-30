select
concat(eParent.EventName, ' - ', e.EventName) as EventName,
count(*) / 10 as GamesPlayed
from Replay r
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
join Player p on p.PlayerID = rc.PlayerID
join `Event` e on e.EventID = r.GameMode
join `Event` eParent on eParent.EventID = e.EventIDParent
where r.GameMode > 1000 and r.TimestampCreated < date_add(now(), interval -90 day) and p.BattleNetRegionId = 98
group by e.EventName
order by e.EventName