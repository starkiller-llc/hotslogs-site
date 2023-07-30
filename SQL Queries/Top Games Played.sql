select
case when loo.PlayerID is null then p.`Name` else '*Hidden*' end as PlayerName,
q.GamesPlayedTotal,
q.GamesPlayedLast30Days,
la.PrimaryName as FavoriteHero,
concat(case when loo.PlayerID is null then p.`Name` else '*Hidden*' end, '|', q.GamesPlayedTotal, '|', q.GamesPlayedLast30Days, '|', la.PrimaryName) as RedditFormat
from (select
pa.PlayerID,
sum(pa.GamesPlayedTotal) as GamesPlayedTotal,
sum(pa.GamesPlayedRecently) as GamesPlayedLast30Days,
max(case when pa.GameMode = 4 then pa.FavoriteCharacter else null end) as FavoriteCharacter
from PlayerAggregate pa
group by pa.PlayerID
order by GamesPlayedTotal desc
limit 100) q
join Player p on p.PlayerID = q.PlayerID
left join LeaderboardOptOut loo on loo.PlayerID = p.PlayerID
join LocalizationAlias la on la.IdentifierID = q.FavoriteCharacter