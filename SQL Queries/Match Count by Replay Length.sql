select
minute(r.ReplayLength) as `Minute`, count(*) as Count
from Replay r
where r.TimestampReplay > date_add(now(), interval -7 day)
group by `Minute`
order by `Minute`