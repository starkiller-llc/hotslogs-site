select * from Player p
left join PlayerAlt pa on pa.PlayerIDAlt = p.PlayerID
where p.BattleNetRegionId > 90 and pa.PlayerIDAlt is null