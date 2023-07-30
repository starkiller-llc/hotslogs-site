select sum(case when q.IsWinner = 1 then Count else 0 end) / sum(Count) as WinPercent, sum(Count) as GamesPlayed from 
(select rcCharacter1.IsWinner, count(*) as Count from ReplayCharacter rcCharacter1
join Replay r on r.ReplayID = rcCharacter1.ReplayID
join ReplayCharacter rcCharacter2 on rcCharacter2.ReplayID = rcCharacter1.ReplayID and rcCharacter2.IsWinner = rcCharacter1.IsWinner
where rcCharacter1.`Character` = 'Zeratul'
and rcCharacter2.`Character` = 'Jaina'
and r.TimestampReplay > date_add(now(), interval -7 day)
and r.GameMode = 4
group by rcCharacter1.IsWinner) q