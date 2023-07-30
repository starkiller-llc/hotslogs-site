select q.PlayersInGame, count(*) as Count from
(select group_concat(concat(p.`Name`, '#', p.BattleTag) order by p.`Name`) as PlayersInGame
from Replay r
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
join Player p on p.PlayerID = rc.PlayerID
where r.TimestampCreated > date_add(now(), interval -5 day)
group by r.ReplayID) q
group by q.PlayersInGame
having Count > 30
order by Count desc