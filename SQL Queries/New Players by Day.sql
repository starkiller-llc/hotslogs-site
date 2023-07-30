select
q.`Date`,
q.UniqueAccounts,
concat(q.`Date`, '|', q.UniqueAccounts) as RedditTableFormat
from
(select
concat(year(p.TimestampCreated), '-', lpad(month(p.TimestampCreated), 2, '0'), '-', lpad(day(p.TimestampCreated), 2, '0')) as `Date`,
count(*) as UniqueAccounts
from Player p
where p.TimestampCreated > date_add(now(), interval -30 day) and
(p.`Name` like '%smurf%' or p.`Name` like '%vulture%')
group by `Date`) q
order by q.`Date` asc