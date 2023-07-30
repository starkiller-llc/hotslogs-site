select
q.*,
concat(q.WatchTowerCaptures, '|', q.Count, '|', format(q.WinRate * 100, 1), '%') as RedditFormat
from
(select
rcsr.WatchTowerCaptures,
count(*) as Count,
sum(rc.IsWinner) / count(*) as WinRate
from ReplayCharacterScoreResult rcsr
join ReplayCharacter rc on rc.ReplayID = rcsr.ReplayID and rc.PlayerID = rcsr.PlayerID
group by rcsr.WatchTowerCaptures) q
order by q.WatchTowerCaptures desc