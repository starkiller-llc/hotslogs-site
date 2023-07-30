select
q3.TimestampDate,
q3.GamesPlayed,
q3.`0000to0200` / q3.GamesPlayed as `0000to0200`,
q3.`0200to0400` / q3.GamesPlayed as `0200to0400`,
q3.`0400to0600` / q3.GamesPlayed as `0400to0600`,
q3.`0600to0800` / q3.GamesPlayed as `0600to0800`,
q3.`0800to1000` / q3.GamesPlayed as `0800to1000`,
q3.`1000to1200` / q3.GamesPlayed as `1000to1200`,
q3.`1200to1400` / q3.GamesPlayed as `1200to1400`,
q3.`1400to1600` / q3.GamesPlayed as `1400to1600`,
q3.`1600to1800` / q3.GamesPlayed as `1600to1800`,
q3.`1600to1800` / q3.GamesPlayed as `1600to1800`,
q3.`1800to2000` / q3.GamesPlayed as `1800to2000`,
q3.`2000+` / q3.GamesPlayed as `2000+`
from (select
date_format(q2.TimestampReplay,'%Y-%m-%d') as TimestampDate,
count(*) as GamesPlayed,
sum(`0000to0200`) as `0000to0200`,
sum(`0200to0400`) as `0200to0400`,
sum(`0400to0600`) as `0400to0600`,
sum(`0600to0800`) as `0600to0800`,
sum(`0800to1000`) as `0800to1000`,
sum(`1000to1200`) as `1000to1200`,
sum(`1200to1400`) as `1200to1400`,
sum(`1400to1600`) as `1400to1600`,
sum(`1600to1800`) as `1600to1800`,
sum(`1800to2000`) as `1800to2000`,
sum(`2000+`) as `2000+`
from (select
q.ReplayID,
q.TimestampReplay,
case when q.MMRSpread > 0 and q.MMRSpread <= 200 then 1 else 0 end as `0000to0200`,
case when q.MMRSpread > 200 and q.MMRSpread <= 400 then 1 else 0 end as `0200to0400`,
case when q.MMRSpread > 400 and q.MMRSpread <= 600 then 1 else 0 end as `0400to0600`,
case when q.MMRSpread > 600 and q.MMRSpread <= 800 then 1 else 0 end as `0600to0800`,
case when q.MMRSpread > 800 and q.MMRSpread <= 1000 then 1 else 0 end as `0800to1000`,
case when q.MMRSpread > 1000 and q.MMRSpread <= 1200 then 1 else 0 end as `1000to1200`,
case when q.MMRSpread > 1200 and q.MMRSpread <= 1400 then 1 else 0 end as `1200to1400`,
case when q.MMRSpread > 1400 and q.MMRSpread <= 1600 then 1 else 0 end as `1400to1600`,
case when q.MMRSpread > 1600 and q.MMRSpread <= 1800 then 1 else 0 end as `1600to1800`,
case when q.MMRSpread > 1800 and q.MMRSpread <= 2000 then 1 else 0 end as `1800to2000`,
case when q.MMRSpread > 2000 then 1 else 0 end as `2000+`
from (select
r.ReplayID,
r.TimestampReplay,
max(rc.MMRBefore) - min(rc.MMRBefore) as MMRSpread
from Replay r use index (IX_GameMode_TimestampReplay)
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
where r.GameMode = 3 and r.TimestampReplay > date_add(now(), interval -30 day) and r.TimestampReplay < now() and rc.MMRBefore is not null
group by r.ReplayID, r.TimestampReplay) q) q2
group by TimestampDate) q3