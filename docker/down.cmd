docker exec docker-db-1 mysql -u root HeroesData -e "reset master; flush tables with read lock; reset master; set global innodb_fast_shutdown=0; shutdown;"
docker exec docker-redis-1 redis-cli shutdown
docker compose stop
