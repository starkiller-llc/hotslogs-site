select
dateTable.`Date`,
count(distinct(rc.PlayerID)) as DistinctPlayers
from 
(select adddate('2015-11-23 01:00:00', interval ((t1*10 + t0) * 24) + hh hour) `Date` from
 (select 0 t0 union select 1 union select 2 union select 3 union select 4 union select 5 union select 6 union select 7 union select 8 union select 9) t0,
 (select 0 t1 union select 1 union select 2 union select 3 union select 4 union select 5 union select 6 union select 7 union select 8 union select 9) t1,
 (select 0 hh union select  1 union select  2 union select  3 union select  4 union
	select  5 union select  6 union select  7 union select  8 union select  9 union
	select 10 union select 11 union select 12 union select 13 union select 14 union
	select 15 union select 16 union select 17 union select 18 union select 19 union
	select 20 union select 21 union select 22 union select 23) hours) dateTable
join Replay r on r.TimestampReplay < date_add(dateTable.`Date`, interval 1 hour)
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
where
(rc.CharacterID = 44 or rc.CharacterID = 45) and
dateTable.`Date` < '2015-11-24 01:00:00'
group by dateTable.`Date`
order by dateTable.`Date`