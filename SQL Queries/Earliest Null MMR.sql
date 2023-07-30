select p.BattleNetRegionId, r.GameMode, min(TimestampReplay) as EarliestNullMMR
from Replay r
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
join Player p on p.PlayerID = rc.PlayerID
where rc.MMRBefore is null and r.GameMode in (3,4,5)
group by p.BattleNetRegionId, r.GameMode order by p.BattleNetRegionId, r.GameMode