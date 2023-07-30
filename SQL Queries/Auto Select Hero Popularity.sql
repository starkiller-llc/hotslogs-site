select
la.PrimaryName as `Hero`,
q.GamesPlayed
from
(select
rc.CharacterID, count(*) as GamesPlayed
from
Replay r
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
where r.TimestampReplay > date_add(now(), interval -30 day)
and r.GameMode = 3
and rc.IsAutoSelect
group by rc.CharacterID) q
join LocalizationAlias la on la.IdentifierID = q.CharacterID