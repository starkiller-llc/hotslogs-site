select
q.`Character`,
q.CountUS,
q.CountEU,
q.CountKR,
q.WinUS / q.CountUS as WinRateUS,
q.WinEU / q.CountEU as WinRateEU,
q.WinKR / q.CountKR as WinRateKR
from (select
rc.`Character`,
sum(case when p.BattleNetRegionId = 1 then 1 else 0 end) as `CountUS`,
sum(case when p.BattleNetRegionId = 1 then rc.IsWinner else 0 end) as `WinUS`,
sum(case when p.BattleNetRegionId = 2 then 1 else 0 end) as `CountEU`,
sum(case when p.BattleNetRegionId = 2 then rc.IsWinner else 0 end) as `WinEU`,
sum(case when p.BattleNetRegionId = 3 then 1 else 0 end) as `CountKR`,
sum(case when p.BattleNetRegionId = 3 then rc.IsWinner else 0 end) as `WinKR`
from ReplayCharacter rc
join Replay r on r.ReplayID = rc.ReplayID
join Player p on p.PlayerID = rc.PlayerID
where r.GameMode = 4 and r.TimestampReplay > date_add(now(), interval -10 day)
group by rc.`Character`) q