select
la.PrimaryName as `Character`,
sum(q.GamesPlayedHeroWithHeroLevel5) as GamesPlayedHeroWithHeroLevel5,
sum(q.GamesPlayedHero) as GamesPlayedHero,
concat(format(sum(q.WinPercentHeroWithHeroLevel5) * 100,1),'%') as WinPercentHeroWithHeroLevel5,
concat(format(sum(q.WinPercentHero) * 100,1),'%') as WinPercentHero,
concat(format((sum(q.WinPercentHeroWithHeroLevel5) - sum(q.WinPercentHero)) * 100,1),'%') as WinPercentDiff,
concat(la.PrimaryName, '|', sum(q.GamesPlayedHeroWithHeroLevel5), '|', concat(format(sum(q.WinPercentHeroWithHeroLevel5) * 100,1),'%'))
from
(select
rc.CharacterID,
count(*) as GamesPlayedHeroWithHeroLevel5,
sum(rc.IsWinner) / count(*) as WinPercentHeroWithHeroLevel5,
null as GamesPlayedHero,
null as WinPercentHero
from ReplayCharacter rc
join Replay r on r.ReplayID = rc.ReplayID
join Player p on p.PlayerID = rc.PlayerID
join LeaderboardRanking lr on lr.PlayerID = rc.PlayerID and lr.GameMode = 4
where r.GameMode = 4 and r.TimestampReplay > date_add(now(), interval -7 day) and r.ReplayBuild = 40431
and rc.CharacterLevel >= 5
group by rc.CharacterID
union
select
rc.CharacterID,
null as GamesPlayedHeroWithHeroLevel5,
null as WinPercentHeroWithHeroLevel5,
count(*) as GamesPlayedHero,
sum(rc.IsWinner) / count(*) as WinPercentHero
from ReplayCharacter rc
join Replay r on r.ReplayID = rc.ReplayID
join Player p on p.PlayerID = rc.PlayerID
join LeaderboardRanking lr on lr.PlayerID = rc.PlayerID and lr.GameMode = 4
where r.GameMode = 4 and r.TimestampReplay > date_add(now(), interval -7 day) and r.ReplayBuild = 40431
group by rc.CharacterID) q
join LocalizationAlias la on la.IdentifierID = q.CharacterID
group by la.PrimaryName
order by sum(q.`WinPercentHeroWithHeroLevel5`) desc