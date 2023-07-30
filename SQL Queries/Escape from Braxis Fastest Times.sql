select
q2.ReplayID,
rtoDifficulty.`Value` as Difficulty,
r.ReplayLength as VictoryTime,
rtoStage2.TimeSpan as Stage2Complete,
rtoStage1.TimeSpan as Stage1Complete,
case when p.BattleNetRegionId = 1 then 'US' when p.BattleNetRegionId = 2 then 'EU' when p.BattleNetRegionId = 3 then 'KR' when p.BattleNetRegionId = 5 then 'CN' end as Region,
q2.Players,
group_concat(laCharacter.PrimaryName order by laCharacter.PrimaryName) as TeamComposition,
concat('https://www.hotslogs.com/Player/MatchSummaryContainer?ReplayID=', r.ReplayID) as ReplayLink
from
	(select
	min(q3.ReplayID) as ReplayID,
	FastestTimePerTeam.ReplayLength,
	q3.BattleNetRegionId,
	q3.Players
	from
		(select
		r.ReplayID,
		r.ReplayLength,
		p.BattleNetRegionId,
		group_concat(p.`Name` order by p.`Name`) as Players
		from Replay r
		join ReplayCharacter rc on rc.ReplayID = r.ReplayID
		join Player p on p.PlayerID = rc.PlayerID
		where r.MapID = 1021
		and rc.IsWinner = 1
		group by r.ReplayID) q3
	join
		(select
		min(q.ReplayLength) as ReplayLength,
		q.BattleNetRegionId,
		q.Players
		from
			(select
			r.ReplayLength,
			p.BattleNetRegionId,
			group_concat(p.`Name` order by p.`Name`) as Players
			from Replay r
			join ReplayCharacter rc on rc.ReplayID = r.ReplayID
			join Player p on p.PlayerID = rc.PlayerID
			where r.MapID = 1021
			and rc.IsWinner = 1
			group by r.ReplayID) q
		group by q.BattleNetRegionId, q.Players) FastestTimePerTeam
	on FastestTimePerTeam.ReplayLength = q3.ReplayLength
	and FastestTimePerTeam.BattleNetRegionId = q3.BattleNetRegionId
	and FastestTimePerTeam.Players = q3.Players
	group by q3.ReplayLength, q3.BattleNetRegionId, q3.Players) q2
join Replay r on r.ReplayID = q2.ReplayID
join ReplayCharacter rc on rc.ReplayID = r.ReplayID
join Player p on p.PlayerID = rc.PlayerID
join LocalizationAlias laCharacter on laCharacter.IdentifierID = rc.CharacterID
join ReplayTeamObjective rtoDifficulty on rtoDifficulty.ReplayID = r.ReplayID and rtoDifficulty.TeamObjectiveType = 102102 and rtoDifficulty.`Value` = 1
left join ReplayTeamObjective rtoStage1 on rtoStage1.ReplayID = r.ReplayID and rtoStage1.TeamObjectiveType = 102101 and rtoStage1.IsWinner = 1 and rtoStage1.`Value` = 1
left join ReplayTeamObjective rtoStage2 on rtoStage2.ReplayID = r.ReplayID and rtoStage2.TeamObjectiveType = 102101 and rtoStage2.IsWinner = 1 and rtoStage2.`Value` = 2
group by r.ReplayID
order by r.ReplayLength asc
limit 5000