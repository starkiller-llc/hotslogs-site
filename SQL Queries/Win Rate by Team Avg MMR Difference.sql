set @GameMode = 4;
set @MaxMMRSpread = 9999;
set @MinMMR = -9999;
set @DaysToInclude = -30;

select
q2.Positive300Sum / (q2.Positive300Sum + q2.Negative300Sum) as WinRate300MMRDiff,
q2.Positive200Sum / (q2.Positive200Sum + q2.Negative200Sum) as WinRate200MMRDiff,
q2.Positive100Sum / (q2.Positive100Sum + q2.Negative100Sum) as WinRate100MMRDiff,
q2.Positive50Sum / (q2.Positive50Sum + q2.Negative50Sum) as WinRate50MMRDiff,
q2.Positive10Sum / (q2.Positive10Sum + q2.Negative10Sum) as WinRate10MMRDiff,
q2.Positive0Sum / (q2.Positive0Sum + q2.Negative0Sum) as WinRate0MMRDiff,
q2.Positive300Sum + q2.Negative300Sum as Count300MMRDiff,
q2.Positive200Sum + q2.Negative200Sum as Count200MMRDiff,
q2.Positive100Sum + q2.Negative100Sum as Count100MMRDiff,
q2.Positive50Sum + q2.Negative50Sum as Count50MMRDiff,
q2.Positive10Sum + q2.Negative10Sum as Count10MMRDiff,
q2.Positive0Sum + q2.Negative0Sum as Count0MMRDiff,
q2.ExcludedGames
from
(select
sum(case when q.MMRSpread < @MaxMMRSpread and q.AvgMMRDifference > 300 then 1 else 0 end) as Positive300Sum,
sum(case when q.MMRSpread < @MaxMMRSpread and q.AvgMMRDifference > 200 and q.AvgMMRDifference <= 300 then 1 else 0 end) as Positive200Sum,
sum(case when q.MMRSpread < @MaxMMRSpread and q.AvgMMRDifference > 100 and q.AvgMMRDifference <= 200 then 1 else 0 end) as Positive100Sum,
sum(case when q.MMRSpread < @MaxMMRSpread and q.AvgMMRDifference > 50 and q.AvgMMRDifference <= 100 then 1 else 0 end) as Positive50Sum,
sum(case when q.MMRSpread < @MaxMMRSpread and q.AvgMMRDifference > 10 and q.AvgMMRDifference <= 50 then 1 else 0 end) as Positive10Sum,
sum(case when q.MMRSpread < @MaxMMRSpread and q.AvgMMRDifference > 0 and q.AvgMMRDifference <= 10 then 1 else 0 end) as Positive0Sum,
sum(case when q.MMRSpread < @MaxMMRSpread and q.AvgMMRDifference >= -10 and q.AvgMMRDifference < 0 then 1 else 0 end) as Negative0Sum,
sum(case when q.MMRSpread < @MaxMMRSpread and q.AvgMMRDifference >= -50 and q.AvgMMRDifference < -10 then 1 else 0 end) as Negative10Sum,
sum(case when q.MMRSpread < @MaxMMRSpread and q.AvgMMRDifference >= -100 and q.AvgMMRDifference < -50 then 1 else 0 end) as Negative50Sum,
sum(case when q.MMRSpread < @MaxMMRSpread and q.AvgMMRDifference >= -200 and q.AvgMMRDifference < -100 then 1 else 0 end) as Negative100Sum,
sum(case when q.MMRSpread < @MaxMMRSpread and q.AvgMMRDifference >= -300 and q.AvgMMRDifference < -200 then 1 else 0 end) as Negative200Sum,
sum(case when q.MMRSpread < @MaxMMRSpread and q.AvgMMRDifference < -300 then 1 else 0 end) as Negative300Sum,
sum(case when q.MMRSpread >= @MaxMMRSpread then 1 else 0 end) as ExcludedGames
from
(select
max(rc.MMRBefore) - min(rc.MMRBefore) as MMRSpread,
avg(case when rc.IsWinner = true then rc.MMRBefore else null end) - avg(case when rc.IsWinner = false then rc.MMRBefore else null end) as AvgMMRDifference
from Replay r
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
where r.TimestampReplay > date_add(now(), interval @DaysToInclude day)
and r.GameMode = @GameMode
and rc.MMRBefore is not null
group by r.ReplayID
having min(rc.MMRBefore) > @MinMMR) q) q2;