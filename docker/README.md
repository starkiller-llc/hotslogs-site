# Docker Development Environment

## Setup

Make sure you have Docker installed. If you are using _Docker Toolbox_ (probably if you are on Windows 7) - make sure the docker virtual machine has ports 3306 (MySql) and 6379 (Redis) mapped to the host.

All the scripts and files discussed in this readme are in the `docker` directory.

## Create the data volumes from snapshot data

Run `create-data-container.cmd` to create two volumes (sql and redis). This will take a while, it will extract the 2.6 GB `hotslogs-data.tgz` (please request this file from maintainers and add to the `docker` directory) and copy it into the volumes.

## Build the Docker Image

Change directory into `docker`, then Run `build.cmd` - it will create a Docker image called `hotslogs` which include a sample database.

## Start using `docker-compose`

In the `docker` directory, run `docker compose up -d` it will start the database and the Redis servers on localhost.

Now you're ready to run the HOTSLogs web application locally.

## Building from scratch

(Still writing this section...)
