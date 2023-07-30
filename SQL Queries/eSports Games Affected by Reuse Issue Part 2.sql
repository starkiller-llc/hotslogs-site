select
q.*,
count(*) as OuterGamesPlayed
from (select
p.PlayerID,
count(*) as GamesPlayed
from Replay r
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
join Player p on p.PlayerID = rc.PlayerID
where r.GameMode > 1000 and p.BattleNetRegionId != 98
group by p.PlayerID
order by GamesPlayed desc) q
left join ReplayCharacter rcOuter on rcOuter.PlayerID = q.PlayerID
left join Replay rOuter on rOuter.ReplayID = rcOuter.ReplayID
where (rOuter.GameMode is null or rOuter.GameMode in (3,4,5,6))
group by q.PlayerID
having OuterGamesPlayed < 30
order by OuterGamesPlayed desc