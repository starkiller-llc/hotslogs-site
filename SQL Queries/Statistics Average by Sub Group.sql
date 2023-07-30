select
la.SubGroup,
minute(r.ReplayLength) as ReplayMinute,
avg(time_to_sec(r.ReplayLength) / 60) as AverageMinute,
count(*) as Count,
avg(rcsr.SiegeDamage) as AvgStatistic,
case
when la.SubGroup = 'Ambusher' then 39650.8784
when la.SubGroup = 'Bruiser' then 56279.2007
when la.SubGroup = 'Burst Damage' then 80059.8403
when la.SubGroup = 'Healer' then 34368.0366
when la.SubGroup = 'Siege' then 89921.5782
when la.SubGroup = 'Support' then 41878.2177
when la.SubGroup = 'Sustained Damage' then 69700.4116
when la.SubGroup = 'Tank' then 52705.0069
when la.SubGroup = 'Utility' then 107470.1092
else 0 end / avg(rcsr.SiegeDamage) as AvgStatisticRelatedTo20Min
from ReplayCharacterScoreResult rcsr
join ReplayCharacter rc on rc.ReplayID = rcsr.ReplayID and rc.PlayerID = rcsr.PlayerID
join LocalizationAlias la on la.IdentifierID = rc.CharacterID
join Replay r use index(IX_TimestampReplay) on r.ReplayID = rcsr.ReplayID
where r.TimestampReplay > date_add(now(), interval -1 day)
group by ReplayMinute, la.SubGroup
having ReplayMinute >= 10 and ReplayMinute <= 30