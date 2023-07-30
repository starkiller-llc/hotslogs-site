@echo off
echo Continue to overwrite redis and sql volumes with snapshot data
pause
docker volume rm hotslogs-sql
docker volume rm hotslogs-redis
docker volume create hotslogs-sql
docker volume create hotslogs-redis
@echo on
docker run --rm -v hotslogs-redis:/data -v hotslogs-sql:/var/lib/mysql -v %CD%:/mnt ubuntu tar xzvf /mnt/hotslogs-data.tgz
