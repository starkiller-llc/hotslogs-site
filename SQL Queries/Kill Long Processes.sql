select concat('KILL ',id,';') from information_schema.processlist where user='root' and time > 10