select CONCAT(YEAR(p.TimestampCreated), '-', lpad(WEEK(p.TimestampCreated), 2, '0')) as YearWeek, count(*) as Count
from Player p
group by CONCAT(YEAR(p.TimestampCreated), '-', lpad(WEEK(p.TimestampCreated), 2, '0'))
order by YearWeek