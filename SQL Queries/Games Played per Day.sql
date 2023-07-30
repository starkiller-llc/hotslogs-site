select
date_format(r.TimestampReplay,'%Y-%m-%d') as TimestampDate,
count(*) as GamesPlayed
from Replay r use index (IX_GameMode_TimestampReplay)
where r.TimestampReplay > date_add(now(), interval -50 day) and r.TimestampReplay < now() and r.GameMode = 5
group by TimestampDate