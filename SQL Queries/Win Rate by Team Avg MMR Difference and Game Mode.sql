select
r.GameMode,
count(*) as GamesPlayed,
sum(rc.IsWinner) / count(*) as WinPercent,
avg(rcTeam.AvgMMRBefore) as AvgWinningMMR,
avg(rcEnemy.AvgMMRBefore) as AvgLosingMMR,
sum(case when abs(rcTeam.AvgMMRBefore - rcEnemy.AvgMMRBefore) < 100 then 1 else 0 end) as GamesPlayedWithin100MMR,
sum(case when abs(rcTeam.AvgMMRBefore - rcEnemy.AvgMMRBefore) < 100 then rc.IsWinner else 0 end) / sum(case when abs(rcTeam.AvgMMRBefore - rcEnemy.AvgMMRBefore) < 100 then 1 else 0 end) as WinRateWithin100MMR,
sum(case when abs(rcTeam.AvgMMRBefore - rcEnemy.AvgMMRBefore) < 50 then 1 else 0 end) as GamesPlayedWithin50MMR,
sum(case when abs(rcTeam.AvgMMRBefore - rcEnemy.AvgMMRBefore) < 50 then rc.IsWinner else 0 end) / sum(case when abs(rcTeam.AvgMMRBefore - rcEnemy.AvgMMRBefore) < 50 then 1 else 0 end) as WinRateWithin50MMR,
sum(case when rcTeam.AvgMMRBefore > 2500 and abs(rcTeam.AvgMMRBefore - rcEnemy.AvgMMRBefore) < 100 then 1 else 0 end) as GamesPlayedWithin100MMRAnd3k,
sum(case when rcTeam.AvgMMRBefore > 2500 and abs(rcTeam.AvgMMRBefore - rcEnemy.AvgMMRBefore) < 100 then rc.IsWinner else 0 end) / sum(case when rcTeam.AvgMMRBefore > 2500 and abs(rcTeam.AvgMMRBefore - rcEnemy.AvgMMRBefore) < 100 then 1 else 0 end) as WinRateWithin100MMRAnd3k
from ReplayCharacter rc
join Replay r on r.ReplayID = rc.ReplayID
join (select rcTeamInner.ReplayID, avg(rcTeamInner.MMRBefore) as AvgMMRBefore from ReplayCharacter rcTeamInner join Replay rTeamInner on rTeamInner.ReplayID = rcTeamInner.ReplayID where rTeamInner.TimestampReplay > date_add(now(), interval -15 day) and rcTeamInner.IsWinner = 1 group by rcTeamInner.ReplayID) rcTeam on rcTeam.ReplayID = r.ReplayID
join (select rcEnemyInner.ReplayID, avg(rcEnemyInner.MMRBefore) as AvgMMRBefore from ReplayCharacter rcEnemyInner join Replay rEnemyInner on rEnemyInner.ReplayID = rcEnemyInner.ReplayID where rEnemyInner.TimestampReplay > date_add(now(), interval -15 day) and rcEnemyInner.IsWinner = 0 group by rcEnemyInner.ReplayID) rcEnemy on rcEnemy.ReplayID = r.ReplayID
left join ReplayCharacter rcMirror on rcMirror.ReplayID = r.ReplayID and rcMirror.CharacterID = rc.CharacterID and rcMirror.IsWinner != rc.IsWinner
where r.TimestampReplay > date_add(now(), interval -15 day)
and r.TimestampReplay < now()
and rc.CharacterID = 53
and rc.CharacterLevel >= 5
and rcMirror.ReplayID is null
and r.GameMode in (3,4,5)
group by r.GameMode