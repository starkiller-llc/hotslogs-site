select
q3.*,
concat(format(q3.PercentOfGameDisconnected * 100, 1), '%|',
q3.GamesPlayedQuickMatch, '|',
q3.GamesPlayedHeroLeague, '|',
format(q3.WinPercentWithDisconnectedPlayerQuickMatch * 100, 1), '%|',
format(q3.WinPercentWithDisconnectedPlayerHeroLeague * 100, 1), '%') as RedditFormat
from (select
1 - (q2.TalentCount / 7) as PercentOfGameDisconnected,
sum(case when q2.GameMode = 3 then 1 else 0 end) as GamesPlayedQuickMatch,
sum(case when q2.GameMode = 4 then 1 else 0 end) as GamesPlayedHeroLeague,
sum(case when q2.GameMode = 3 then q2.IsWinner else 0 end) / sum(case when q2.GameMode = 3 then 1 else 0 end) as WinPercentWithDisconnectedPlayerQuickMatch,
sum(case when q2.GameMode = 4 then q2.IsWinner else 0 end) / sum(case when q2.GameMode = 4 then 1 else 0 end) as WinPercentWithDisconnectedPlayerHeroLeague
from (select
q.ReplayID,
q.PlayerID,
q.GameMode,
q.IsWinner,
q.TalentCount
from (select
rc.ReplayID,
rc.PlayerID,
r.GameMode,
rc.IsWinner,
sum(case when rct.ReplayID is not null then 1 else 0 end) as TalentCount
from ReplayCharacter rc
left join ReplayCharacterTalent rct on rct.ReplayID = rc.ReplayID and rct.PlayerID = rc.PlayerID
join Replay r use index (IX_GameMode_TimestampReplay) on r.ReplayID = rc.ReplayID
where r.TimestampReplay > date_add(now(), interval -15 day) and r.TimestampReplay < now() and r.GameMode in (3,4,5) and rc.MMRBefore > 2500
group by rc.ReplayID, rc.PlayerID, r.GameMode, rc.IsWinner
having TalentCount < 6) q
join ReplayCharacterTalent rct on rct.ReplayID = q.ReplayID and rct.PlayerID != q.PlayerID
group by q.ReplayID, q.PlayerID, q.GameMode, q.IsWinner, q.TalentCount
having count(*) = 63) q2
group by q2.TalentCount) q3
order by q3.PercentOfGameDisconnected