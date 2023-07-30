select distinct(r.ReplayID)
from Replay r
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
left join LocalizationAlias la on la.PrimaryName = r.Map and la.`Type` = 0
left join LocalizationAlias la2 on la2.PrimaryName = rc.`Character` and la2.`Type` = 1
where la.`Type` is null or la2.`Type` is null