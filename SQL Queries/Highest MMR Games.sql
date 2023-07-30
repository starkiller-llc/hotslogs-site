select r.ReplayID, r.ReplayBuild, r.TimestampReplay, r.ReplayFileName, sum(rc.MMRBefore)/10 as MMRAverage, group_concat(p.`Name`) as Players
from Replay r
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
join Player p on p.PlayerID = rc.PlayerID
where r.ReplayBuild = (select max(innerR.ReplayBuild) from Replay innerR)
and r.GameMode = 4
and p.BattleNetRegionId = 1
group by r.ReplayID, r.ReplayBuild, r.TimestampReplay, r.ReplayFileName
order by sum(rc.MMRBefore)/10 desc limit 10