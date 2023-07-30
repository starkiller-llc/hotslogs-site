select
la.PrimaryName,
q.Count
from
(select rc.CharacterID, count(distinct(rc.PlayerID)) as Count
from ReplayCharacter rc
group by rc.CharacterID) q
join LocalizationAlias la on la.IdentifierID = q.CharacterID
order by q.Count desc