select case
when q.GameMode = 3 then 'Quick Match'
when q.GameMode = 4 then 'Hero League'
when q.GameMode = 5 then 'Team League' else q.GameMode end as GameMode, q.NumberOfRoles, count(*) as GamesPlayed from
(select r.ReplayID, r.GameMode, sum(case when la.`Group` = 'Warrior' then 1 else 0 end) as NumberOfRoles
from Replay r use index (IX_TimestampReplay)
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
join LocalizationAlias la on la.IdentifierID = rc.CharacterID
where r.TimestampReplay > date_add(now(), interval -1 day)
group by r.ReplayID, r.GameMode) q
group by q.GameMode, q.NumberOfRoles