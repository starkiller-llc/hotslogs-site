reset master;
flush tables with read lock;
reset master;
set global innodb_fast_shutdown=0;
shutdown;
