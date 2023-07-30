select
q.*,
concat(case when q.BattleNetRegionId = 1 then 'US' when 2 then 'EU' when 3 then 'KR' when 5 then 'CN' end, '|', q.`Name`, '|', q.GamesPlayed, '|', concat(format(q.WinPercent * 100,1),'%'), '|', q.KDRatio) as Reddit
from (select
p.BattleNetRegionId,
rc.PlayerID,
p.`Name`,
count(*) as GamesPlayed,
sum(rc.IsWinner) / count(*) as WinPercent,
sum(rcsr.SoloKills) / sum(rcsr.Deaths) as KDRatio
from Replay r
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
join ReplayCharacterScoreResult rcsr on rcsr.ReplayID = rc.ReplayID and rcsr.PlayerID = rc.PlayerID
join Player p on p.PlayerID = rc.PlayerID
left join LeaderboardOptOut loo on loo.PlayerID = p.PlayerID
where r.GameMode = 7 and r.TimestampReplay > date_add(now(), interval -7 day) and loo.PlayerID is null
group by rc.PlayerID
having GamesPlayed >= 4) q
order by q.KDRatio desc
limit 50000