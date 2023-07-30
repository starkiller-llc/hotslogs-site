select
q2.*,
q2.GamesWonTotal / q2.GamesPlayedTotal as WinRateTotal,
q2.GamesWonWithOtherHealers / q2.GamesPlayedWithOtherHealers as WinRateWithOtherHealers,
q2.GamesWonWithoutOtherHealers / q2.GamesPlayedWithoutOtherHealers as WinRateWithoutOtherHealers
from (select
q.GamesPlayedTotal,
q.GamesPlayedWithOtherHealers,
q.GamesPlayedTotal - q.GamesPlayedWithOtherHealers as GamesPlayedWithoutOtherHealers,
q.GamesWonTotal,
q.GamesWonWithOtherHealers,
q.GamesWonTotal - q.GamesWonWithOtherHealers as GamesWonWithoutOtherHealers
from (select
count(distinct(r.ReplayID)) as GamesPlayedTotal,
count(distinct(case when laRCOtherHealers.`Group` = 'Support' then r.ReplayID else null end)) as GamesPlayedWithOtherHealers,
count(distinct(case when rc.IsWinner then r.ReplayID else null end)) as GamesWonTotal,
count(distinct(case when laRCOtherHealers.`Group` = 'Support' and rc.IsWinner then r.ReplayID else null end)) as GamesWonWithOtherHealers
from Replay r
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
join ReplayCharacter rcOtherHealers on rcOtherHealers.ReplayID = rc.ReplayID and rcOtherHealers.PlayerID != rc.PlayerID
left join ReplayCharacter rcMirrorMatchup on rcMirrorMatchup.ReplayID = rc.ReplayID and rcMirrorMatchup.PlayerID != rc.PlayerID and rcMirrorMatchup.CharacterID = rc.CharacterID
join LocalizationAlias laRC on laRC.IdentifierID = rc.CharacterID
left join LocalizationAlias laRCOtherHealers on laRCOtherHealers.IdentifierID = rcOtherHealers.CharacterID
where r.TimestampReplay > date_add(now(), interval -15 day) and r.GameMode = 3 and rcMirrorMatchup.ReplayID is null
and laRC.PrimaryName = 'Medivh' and rc.CharacterLevel >= 5) q) q2