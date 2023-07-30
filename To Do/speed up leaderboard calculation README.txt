update PlayerAggregate table on Match History generation, including GameMode-specific FavoriteHero

change MMR PlayerAggregate generation to exclude players who have already been calculated from the above
change MMR PlayerAggregate FavoriteHero calculation to be GameMode specific

change Hero Leaderboard to have different game modes, also using different game mode MMR

add favorite hero to all leaderboards

look into simultaneous 1k player aggregate batch queries

Try this:
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ;
SELECT * FROM TABLE_NAME ;
COMMIT ;