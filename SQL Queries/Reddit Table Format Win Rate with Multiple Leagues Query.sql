select concat(q2.`Character`, '|', q2.GamesPlayedHero, '|', q2.GamesPlayedTeam, '|', q2.WinPercentHero, '|', q2.WinPercentTeam, '|', q2.WinPercentDiff) from
(select
q.`Character`,
sum(q.GamesPlayedHero) as GamesPlayedHero,
sum(q.GamesPlayedTeam) as GamesPlayedTeam,
concat(format(sum(q.WinPercentHero) * 100,1),'%') as WinPercentHero,
concat(format(sum(q.WinPercentTeam) * 100,1),'%') as WinPercentTeam,
concat(format((sum(q.WinPercentTeam) - sum(q.WinPercentHero)) * 100,1),'%') as WinPercentDiff
from
(select
rc.`Character`,
count(*) as GamesPlayedHero,
sum(rc.IsWinner) / count(*) as WinPercentHero,
null as GamesPlayedTeam,
null as WinPercentTeam
from ReplayCharacter rc
join Replay r on r.ReplayID = rc.ReplayID
join Player p on p.PlayerID = rc.PlayerID
where r.GameMode = 4 and r.TimestampReplay > date_add(now(), interval -7 day)
group by rc.`Character`
union
select
rc.`Character`,
null as GamesPlayedHero,
null as WinPercentHero,
count(*) as GamesPlayedTeam,
sum(rc.IsWinner) / count(*) as WinPercentTeam
from ReplayCharacter rc
join Replay r on r.ReplayID = rc.ReplayID
join Player p on p.PlayerID = rc.PlayerID
where r.GameMode = 5 and r.TimestampReplay > date_add(now(), interval -7 day)
group by rc.`Character`) q
group by q.`Character`
order by sum(q.`WinPercentTeam`) desc) q2