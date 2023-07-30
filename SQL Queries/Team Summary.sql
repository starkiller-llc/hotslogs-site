select t.TeamName, p.PlayerID, p.`Name`, p.BattleTag,
	(select group_concat(q.TopHero) from (select concat(la.PrimaryName, ' (', count(*), ')') as TopHero from ReplayCharacter rc join LocalizationAlias la on la.IdentifierID = rc.CharacterID
	where rc.PlayerID = p.PlayerID
	group by la.PrimaryName
	order by count(*) desc
	limit 3) q) as Top3Heroes
from Team t
join PlayerToTeam p2t on p2t.TeamID = t.TeamID
join Player p on p.PlayerID = p2t.PlayerID
where t.TeamID != 1