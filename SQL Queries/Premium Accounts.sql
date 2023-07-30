select
concat(p.`Name`, '#', p.BattleTag) as BattleTag,
pa.*,
pp.TransactionID,
pp.Email as PaypalEmail,
pp.PaymentAmountGross,
pp.PaymentAmountFee,
pp.TimestampPayment,
u.UserID,
m.Email as HotsLogsEmail,
m.CreationDate as HotsLogsRegistrationDate
from PremiumAccount pa
join Player p on p.PlayerID = pa.PlayerID
left join PremiumPayment pp on pp.TimestampPayment = TimestampSupporterSince
left join `User` u on u.PlayerID = pa.PlayerID
left join my_aspnet_membership m on m.userId = u.UserID
where pa.TimestampSupporterExpiration > now()
limit 10000