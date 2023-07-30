select
q2.`Name`,
q2.UniquePlayers,
q2.WinPercent,
concat(q2.`Name`, '|', q2.UniquePlayers, '|', format(q2.WinPercent * 100, 1), '%') as RedditTableFormat
from
(select
q.`Name`,
q.UniquePlayers,
(select sum(rc2.IsWinner) / count(*) from Player p2 join ReplayCharacter rc2 on rc2.PlayerID = p2.PlayerID where p2.`Name` = q.`Name`) as WinPercent
from
(select `Name`, count(*) as UniquePlayers
from Player p
where p.TimestampCreated > date_add(now(), interval -30 day) and (p.`Name` like '%smurf%' or p.`Name` like '%vulture%')
group by p.`Name`
having UniquePlayers >= 10
order by UniquePlayers desc) q) q2
order by q2.WinPercent desc