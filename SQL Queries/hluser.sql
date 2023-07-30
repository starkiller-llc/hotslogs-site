CREATE TABLE `hluser` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `email` varchar(45) NOT NULL,
  `username` varchar(45) NOT NULL,
  `expiration` datetime DEFAULT NULL,
  `password` varchar(255) NOT NULL,
  `acceptedTOS` varchar(5) NOT NULL DEFAULT 'false',
  `userGUID` varchar(45) NOT NULL,
  `premium` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  UNIQUE KEY `email_UNIQUE` (`email`),
  UNIQUE KEY `username_UNIQUE` (`username`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;
