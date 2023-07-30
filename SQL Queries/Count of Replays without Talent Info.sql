select count(distinct(r.ReplayID)) as Count from Replay r
left join ReplayCharacterTalent rct on rct.ReplayID = r.ReplayID
where r.ReplayBuild = 38236 and rct.ReplayID is null
union
select count(distinct(r.ReplayID)) as Count from Replay r
left join ReplayCharacterTalent rct on rct.ReplayID = r.ReplayID
where r.ReplayBuild = 38236 and rct.ReplayID is not null