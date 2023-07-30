select
rc.CharacterID,
count(*) as GamesPlayedHeroWithHeroLevel5,
sum(rc.IsWinner) / count(*) as WinPercentHeroWithHeroLevel5
from ReplayCharacter rc
join ReplayCharacter rcPlayedWith on rcPlayedWith.ReplayID = rc.ReplayID and rcPlayedWith.CharacterID = 45 and rcPlayedWith.IsWinner = rc.IsWinner
join Replay r on r.ReplayID = rc.ReplayID
join Player p on p.PlayerID = rc.PlayerID
join LeaderboardRanking lr on lr.PlayerID = rc.PlayerID and lr.GameMode = 4
where r.GameMode = 4 and r.TimestampReplay > date_add(now(), interval -7 day)
and rc.CharacterLevel >= 5 and rcPlayedWith.CharacterLevel >= 5
and rc.CharacterID = 44
group by rc.CharacterID