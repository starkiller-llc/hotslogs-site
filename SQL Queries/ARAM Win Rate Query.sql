select
la.PrimaryName as `Character`,
sum(q.GamesPlayedHero) as GamesPlayedHero,
concat(format(sum(q.WinPercentHero) * 100,1),'%') as WinPercentHero,
concat(la.PrimaryName, '|', sum(q.GamesPlayedHero), '|', concat(format(sum(q.WinPercentHero) * 100,1),'%'))
from
(select
rc.CharacterID,
count(*) as GamesPlayedHero,
sum(rc.IsWinner) / count(*) as WinPercentHero
from ReplayCharacter rc
left join ReplayCharacter rcMirror on rcMirror.ReplayID = rc.ReplayID and rcMirror.PlayerID != rc.PlayerID and rcMirror.CharacterID = rc.CharacterID
join Replay r use index (IX_GameMode) on r.ReplayID = rc.ReplayID
join Player p on p.PlayerID = rc.PlayerID
join LeaderboardRanking lr on lr.PlayerID = rc.PlayerID and lr.GameMode = 4
where r.GameMode = -1 and r.MapID = 1011 and rc.IsAutoSelect = true and rcMirror.ReplayID is null
group by rc.CharacterID) q
join LocalizationAlias la on la.IdentifierID = q.CharacterID
group by la.PrimaryName
order by sum(q.`WinPercentHero`) desc