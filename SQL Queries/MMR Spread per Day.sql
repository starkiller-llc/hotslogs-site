select
date_format(q.TimestampReplay,'%Y-%m-%d') as TimestampDate,
avg(q.MMRSpread) as AvgMMRSpread,
concat(date_format(q.TimestampReplay,'%Y-%m-%d'), '|', avg(q.MMRSpread)) as RedditFormat
from (select
r.ReplayID,
r.TimestampReplay,
max(rc.MMRBefore) - min(rc.MMRBefore) as MMRSpread
from Replay r use index (IX_GameMode_TimestampReplay)
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
where r.GameMode = 4 and r.TimestampReplay > date_add(now(), interval -100 day) and r.TimestampReplay < now() and rc.MMRBefore is not null
group by r.ReplayID, r.TimestampReplay) q
group by TimestampDate