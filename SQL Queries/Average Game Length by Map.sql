select
la.PrimaryName,
q.AvgLength
from
(select
r.MapID,
count(*) as GamesPlayed,
sec_to_time(avg(time_to_sec(r.ReplayLength))) as AvgLength
from Replay r use index (IX_TimestampReplay)
where r.TimestampReplay > date_add(now(), interval -3 day)
group by r.MapID) q
join LocalizationAlias la on la.IdentifierID = q.MapID
order by q.MapID