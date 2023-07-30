select
la.PrimaryName as Hero,
sum(q.GamesPlayedHeroWithNewBuild) as GamesPlayedHeroWithNewBuild,
sum(q.GamesPlayedHero) as GamesPlayedHero,
concat(format(sum(q.WinPercentHeroWithNewBuild) * 100,1),'%') as WinPercentHeroWithNewBuild,
concat(format(sum(q.WinPercentHero) * 100,1),'%') as WinPercentHero,
concat(format((sum(q.WinPercentHeroWithNewBuild) - sum(q.WinPercentHero)) * 100,1),'%') as WinPercentDiff,
concat(la.PrimaryName, '|', sum(q.GamesPlayedHeroWithNewBuild), '|', concat(format(sum(q.WinPercentHeroWithNewBuild) * 100,1),'%'), '|', concat(format((sum(q.WinPercentHeroWithNewBuild) - sum(q.WinPercentHero)) * 100,1),'%'))
from
(select
rc.CharacterID,
count(*) as GamesPlayedHeroWithNewBuild,
sum(rc.IsWinner) / count(*) as WinPercentHeroWithNewBuild,
null as GamesPlayedHero,
null as WinPercentHero
from ReplayCharacter rc
join Replay r on r.ReplayID = rc.ReplayID
join Player p on p.PlayerID = rc.PlayerID
join LeaderboardRanking lr on lr.PlayerID = rc.PlayerID and lr.GameMode = 4
where r.GameMode = 4 and r.TimestampReplay > date_add(now(), interval -7 day) and r.ReplayBuild = (select max(ReplayBuild) from Replay)
and rc.CharacterLevel >= 5
group by rc.CharacterID
union
select
rc.CharacterID,
null as GamesPlayedHeroWithNewBuild,
null as WinPercentHeroWithNewBuild,
count(*) as GamesPlayedHero,
sum(rc.IsWinner) / count(*) as WinPercentHero
from ReplayCharacter rc
join Replay r on r.ReplayID = rc.ReplayID
join Player p on p.PlayerID = rc.PlayerID
join LeaderboardRanking lr on lr.PlayerID = rc.PlayerID and lr.GameMode = 4
where r.GameMode = 4 and r.TimestampReplay > date_add(now(), interval -7 day)
and rc.CharacterLevel >= 5
group by rc.CharacterID) q
join LocalizationAlias la on la.IdentifierID = q.CharacterID
group by la.PrimaryName
order by sum(q.WinPercentHeroWithNewBuild) desc, sum(q.WinPercentHeroWithNewBuild) - sum(q.WinPercentHero)