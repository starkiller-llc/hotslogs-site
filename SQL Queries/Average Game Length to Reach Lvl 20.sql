select 'Real Dream Team' as TeamName, avg(q.FirstMinutePastLvl20) as AvgTimeToLvl20, min(FirstMinutePastLvl20) as MinTimeToLvl20, count(*) as TotalGames from
(select rc.ReplayID, min(rpb.GameTimeMinute) as FirstMinutePastLvl20 from ReplayCharacter rc
join Replay r on r.ReplayID = rc.ReplayID
join ReplayPeriodicXPBreakdown rpb on rpb.ReplayID = rc.ReplayID and rpb.IsWinner = rc.IsWinner
where rc.PlayerID in (173570,502036,715283,877855,1575117,4515129) and r.GameMode in (1028,1027,1026,1025,1018,1016,1017,1014) and (rpb.MinionXP + rpb.CreepXP + rpb.StructureXP + rpb.HeroXP + rpb.TrickleXP) > 71801
group by rc.ReplayID) q
union all
select 'Dark Blaze' as TeamName, avg(q.FirstMinutePastLvl20) as AvgTimeToLvl20, min(FirstMinutePastLvl20) as MinTimeToLvl20, count(*) as TotalGames from
(select rc.ReplayID, min(rpb.GameTimeMinute) as FirstMinutePastLvl20 from ReplayCharacter rc
join Replay r on r.ReplayID = rc.ReplayID
join ReplayPeriodicXPBreakdown rpb on rpb.ReplayID = rc.ReplayID and rpb.IsWinner = rc.IsWinner
where rc.PlayerID in (172852,175040,181989,237893,3454123) and r.GameMode in (1028,1027,1026,1025,1018,1016,1017,1014) and (rpb.MinionXP + rpb.CreepXP + rpb.StructureXP + rpb.HeroXP + rpb.TrickleXP) > 71801
group by rc.ReplayID) q
union all
select 'We Volin' as TeamName, avg(q.FirstMinutePastLvl20) as AvgTimeToLvl20, min(FirstMinutePastLvl20) as MinTimeToLvl20, count(*) as TotalGames from
(select rc.ReplayID, min(rpb.GameTimeMinute) as FirstMinutePastLvl20 from ReplayCharacter rc
join Replay r on r.ReplayID = rc.ReplayID
join ReplayPeriodicXPBreakdown rpb on rpb.ReplayID = rc.ReplayID and rpb.IsWinner = rc.IsWinner
where rc.PlayerID in (166996,292816,1737010,3491913,4507975,751168) and r.GameMode in (1028,1027,1026,1025,1018,1016,1017,1014) and (rpb.MinionXP + rpb.CreepXP + rpb.StructureXP + rpb.HeroXP + rpb.TrickleXP) > 71801
group by rc.ReplayID) q
union all
select 'Tricky Turtles' as TeamName, avg(q.FirstMinutePastLvl20) as AvgTimeToLvl20, min(FirstMinutePastLvl20) as MinTimeToLvl20, count(*) as TotalGames from
(select rc.ReplayID, min(rpb.GameTimeMinute) as FirstMinutePastLvl20 from ReplayCharacter rc
join Replay r on r.ReplayID = rc.ReplayID
join ReplayPeriodicXPBreakdown rpb on rpb.ReplayID = rc.ReplayID and rpb.IsWinner = rc.IsWinner
where rc.PlayerID in (1000818,1321975,1502364,2066159,2315216,2103697) and r.GameMode in (1028,1027,1026,1025,1018,1016,1017,1014) and (rpb.MinionXP + rpb.CreepXP + rpb.StructureXP + rpb.HeroXP + rpb.TrickleXP) > 71801
group by rc.ReplayID) q