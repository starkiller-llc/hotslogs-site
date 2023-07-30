select
la.PrimaryName as `Hero`,
avg(rcsr.WatchTowerCaptures) as AverageWatchTowerCaptures
from ReplayCharacter rc
join ReplayCharacterScoreResult rcsr on rcsr.ReplayID = rc.ReplayID and rcsr.PlayerID = rc.PlayerID
join LocalizationAlias la on la.IdentifierID = rc.CharacterID
join Replay r use index (IX_TimestampReplay) on r.ReplayID = rc.ReplayID
where r.TimestampReplay > date_add(now(), interval -1 day) and r.TimestampReplay < now()
group by la.PrimaryName
order by AverageWatchTowerCaptures desc