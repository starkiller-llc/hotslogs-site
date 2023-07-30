-- MySQL dump 10.13  Distrib 8.0.19, for Win64 (x86_64)
--
-- Host: 104.254.245.56    Database: valorant
-- ------------------------------------------------------
-- Server version	8.0.21

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `valorant`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `valorant` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `valorant`;

--
-- Table structure for table `damage`
--

DROP TABLE IF EXISTS `damage`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `damage` (
  `id` int NOT NULL AUTO_INCREMENT,
  `player_round_stats_id` int NOT NULL,
  `receiver` varchar(45) NOT NULL,
  `damage` int NOT NULL,
  `legshots` int NOT NULL,
  `bodyshots` int NOT NULL,
  `headshots` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_damage_player_round_stats_idx` (`player_round_stats_id`),
  KEY `FK_receiver_player_idx` (`receiver`),
  CONSTRAINT `FK_damage_player_round_stats` FOREIGN KEY (`player_round_stats_id`) REFERENCES `player_round_stats` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_receiver_player` FOREIGN KEY (`receiver`) REFERENCES `player` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `damage`
--

LOCK TABLES `damage` WRITE;
/*!40000 ALTER TABLE `damage` DISABLE KEYS */;
/*!40000 ALTER TABLE `damage` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `kill`
--

DROP TABLE IF EXISTS `kill`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `kill` (
  `id` int NOT NULL,
  `player_round_stats_id` int NOT NULL,
  `game_time` int NOT NULL,
  `round_time` int NOT NULL,
  `killer` varchar(45) NOT NULL,
  `victim` varchar(45) NOT NULL,
  `victim_x` int NOT NULL,
  `victim_y` int NOT NULL,
  `finishing_damage_type` varchar(45) NOT NULL,
  `finishing_damage_item` varchar(45) NOT NULL,
  `finishing_is_secondary_fire_mode` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_kill_player_round_stats_idx` (`player_round_stats_id`),
  KEY `FK_killer_player_idx` (`killer`),
  KEY `FK_victim_player_idx` (`victim`),
  CONSTRAINT `FK_kill_player_round_stats` FOREIGN KEY (`player_round_stats_id`) REFERENCES `player_round_stats` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_killer_player` FOREIGN KEY (`killer`) REFERENCES `player` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_victim_player` FOREIGN KEY (`victim`) REFERENCES `player` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `kill`
--

LOCK TABLES `kill` WRITE;
/*!40000 ALTER TABLE `kill` DISABLE KEYS */;
/*!40000 ALTER TABLE `kill` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `kill_assistant`
--

DROP TABLE IF EXISTS `kill_assistant`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `kill_assistant` (
  `id` int NOT NULL AUTO_INCREMENT,
  `kill_id` int NOT NULL,
  `player_uuid` varchar(45) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_kill_assistant_kill_idx` (`kill_id`),
  KEY `FK_kill_assistant_player_idx` (`player_uuid`),
  CONSTRAINT `FK_kill_assistant_kill` FOREIGN KEY (`kill_id`) REFERENCES `kill` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_kill_assistant_player` FOREIGN KEY (`player_uuid`) REFERENCES `player` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `kill_assistant`
--

LOCK TABLES `kill_assistant` WRITE;
/*!40000 ALTER TABLE `kill_assistant` DISABLE KEYS */;
/*!40000 ALTER TABLE `kill_assistant` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `kill_location`
--

DROP TABLE IF EXISTS `kill_location`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `kill_location` (
  `id` int NOT NULL AUTO_INCREMENT,
  `kill_id` int NOT NULL,
  `player_uuid` varchar(45) NOT NULL,
  `view_radians` double NOT NULL,
  `x` int NOT NULL,
  `y` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_kill_location_kill_idx` (`kill_id`),
  CONSTRAINT `FK_kill_location_kill` FOREIGN KEY (`kill_id`) REFERENCES `kill` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `kill_location`
--

LOCK TABLES `kill_location` WRITE;
/*!40000 ALTER TABLE `kill_location` DISABLE KEYS */;
/*!40000 ALTER TABLE `kill_location` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `match`
--

DROP TABLE IF EXISTS `match`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `match` (
  `id` varchar(45) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `match`
--

LOCK TABLES `match` WRITE;
/*!40000 ALTER TABLE `match` DISABLE KEYS */;
/*!40000 ALTER TABLE `match` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `match_info`
--

DROP TABLE IF EXISTS `match_info`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `match_info` (
  `id` int NOT NULL AUTO_INCREMENT,
  `match_id` varchar(45) NOT NULL,
  `map_id` varchar(45) NOT NULL,
  `game_length_millis` int NOT NULL,
  `game_start_millis` bigint NOT NULL,
  `provisioning_flow_id` varchar(45) NOT NULL,
  `is_completed` tinyint(1) NOT NULL,
  `custom_game_name` varchar(45) NOT NULL,
  `queue_id` varchar(45) NOT NULL,
  `game_mode` varchar(45) NOT NULL,
  `is_ranked` tinyint(1) NOT NULL,
  `season_id` varchar(45) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_matchInfo_match_idx` (`match_id`),
  CONSTRAINT `FK_matchInfo_match` FOREIGN KEY (`match_id`) REFERENCES `match` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `match_info`
--

LOCK TABLES `match_info` WRITE;
/*!40000 ALTER TABLE `match_info` DISABLE KEYS */;
/*!40000 ALTER TABLE `match_info` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `match_player`
--

DROP TABLE IF EXISTS `match_player`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `match_player` (
  `id` int NOT NULL AUTO_INCREMENT,
  `match_id` varchar(45) NOT NULL,
  `player_uuid` varchar(45) NOT NULL,
  `team_id` varchar(45) NOT NULL,
  `party_id` varchar(45) NOT NULL,
  `character_id` varchar(45) NOT NULL,
  `competitive_tier` int NOT NULL,
  `player_card` varchar(45) NOT NULL,
  `player_title` varchar(45) NOT NULL,
  `stats_score` int NOT NULL,
  `stats_rounds_played` int NOT NULL,
  `stats_kills` int NOT NULL,
  `stats_deaths` int NOT NULL,
  `stats_assists` int NOT NULL,
  `stats_playtime_millis` int NOT NULL,
  `casts_grenade` int NOT NULL,
  `casts_ability1` int NOT NULL,
  `casts_ability2` int NOT NULL,
  `casts_ultimate` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_player_match_idx` (`match_id`),
  KEY `FK_match_player_player_idx` (`player_uuid`),
  CONSTRAINT `FK_match_player_player` FOREIGN KEY (`player_uuid`) REFERENCES `player` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_player_match` FOREIGN KEY (`match_id`) REFERENCES `match` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `match_player`
--

LOCK TABLES `match_player` WRITE;
/*!40000 ALTER TABLE `match_player` DISABLE KEYS */;
/*!40000 ALTER TABLE `match_player` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `match_player_location`
--

DROP TABLE IF EXISTS `match_player_location`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `match_player_location` (
  `id` int NOT NULL AUTO_INCREMENT,
  `round_result_id` int NOT NULL,
  `plant_or_defuse` varchar(45) NOT NULL COMMENT 'Valid values are ''plant'' or ''defuse''',
  `player_uuid` varchar(45) NOT NULL,
  `view_radians` double NOT NULL,
  `x` int NOT NULL,
  `y` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_location_result_idx` (`round_result_id`),
  CONSTRAINT `FK_location_result` FOREIGN KEY (`round_result_id`) REFERENCES `round_result` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `match_player_location`
--

LOCK TABLES `match_player_location` WRITE;
/*!40000 ALTER TABLE `match_player_location` DISABLE KEYS */;
/*!40000 ALTER TABLE `match_player_location` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `player`
--

DROP TABLE IF EXISTS `player`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `player` (
  `id` varchar(45) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `player`
--

LOCK TABLES `player` WRITE;
/*!40000 ALTER TABLE `player` DISABLE KEYS */;
/*!40000 ALTER TABLE `player` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `player_round_stats`
--

DROP TABLE IF EXISTS `player_round_stats`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `player_round_stats` (
  `id` int NOT NULL AUTO_INCREMENT,
  `round_result_id` int NOT NULL,
  `player_uuid` varchar(45) NOT NULL,
  `score` int NOT NULL,
  `economy_loadout_value` int NOT NULL,
  `economy_weapon` varchar(45) NOT NULL,
  `economy_armor` varchar(45) NOT NULL,
  `economy_remaining` int NOT NULL,
  `economy_spent` int NOT NULL,
  `ability_grenade_effects` varchar(45) NOT NULL,
  `ability_ability1_effects` varchar(45) NOT NULL,
  `ability_ability2_effects` varchar(45) NOT NULL,
  `ability_ultimate_effects` varchar(45) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_player_round_stats_results_idx` (`round_result_id`),
  CONSTRAINT `FK_player_round_stats_results` FOREIGN KEY (`round_result_id`) REFERENCES `round_result` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `player_round_stats`
--

LOCK TABLES `player_round_stats` WRITE;
/*!40000 ALTER TABLE `player_round_stats` DISABLE KEYS */;
/*!40000 ALTER TABLE `player_round_stats` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `round_result`
--

DROP TABLE IF EXISTS `round_result`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `round_result` (
  `id` int NOT NULL AUTO_INCREMENT,
  `match_id` varchar(45) NOT NULL,
  `round_num` int NOT NULL,
  `round_result` varchar(45) NOT NULL,
  `round_ceremony` varchar(45) NOT NULL,
  `winning_team` varchar(45) NOT NULL,
  `bomb_planter` varchar(45) NOT NULL,
  `bomb_defuser` varchar(45) NOT NULL,
  `plant_round_time` int NOT NULL,
  `plant_site` varchar(45) NOT NULL,
  `defuse_round_time` int NOT NULL,
  `round_result_code` varchar(45) NOT NULL,
  `plant_x` int NOT NULL,
  `plant_y` int NOT NULL,
  `defuse_x` int NOT NULL,
  `defuse_y` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_results_match_idx` (`match_id`),
  KEY `FK_planter_player_idx` (`bomb_planter`),
  KEY `FK_defuser_player_idx` (`bomb_defuser`),
  CONSTRAINT `FK_defuser_player` FOREIGN KEY (`bomb_defuser`) REFERENCES `player` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_planter_player` FOREIGN KEY (`bomb_planter`) REFERENCES `player` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_results_match` FOREIGN KEY (`match_id`) REFERENCES `match` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `round_result`
--

LOCK TABLES `round_result` WRITE;
/*!40000 ALTER TABLE `round_result` DISABLE KEYS */;
/*!40000 ALTER TABLE `round_result` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `site_user`
--

DROP TABLE IF EXISTS `site_user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `site_user` (
  `id` varchar(255) NOT NULL,
  `theme` varchar(45) NOT NULL DEFAULT 'dark',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `site_user`
--

LOCK TABLES `site_user` WRITE;
/*!40000 ALTER TABLE `site_user` DISABLE KEYS */;
INSERT INTO `site_user` VALUES ('35bfccde-2fad-49ed-a700-c224916f43a2','dark'),('573b1d33-8cd9-4ca7-8f97-ed675fa2c778','dark'),('fb15c4f6-cd54-4a93-9b3a-eca85f8d6b55','dark');
/*!40000 ALTER TABLE `site_user` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `team`
--

DROP TABLE IF EXISTS `team`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `team` (
  `id` int NOT NULL AUTO_INCREMENT,
  `match_id` varchar(45) NOT NULL,
  `team_id` varchar(45) NOT NULL,
  `won` tinyint(1) NOT NULL,
  `rounds_played` int NOT NULL,
  `rounds_won` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_team_match_idx` (`match_id`),
  CONSTRAINT `FK_team_match` FOREIGN KEY (`match_id`) REFERENCES `match` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `team`
--

LOCK TABLES `team` WRITE;
/*!40000 ALTER TABLE `team` DISABLE KEYS */;
/*!40000 ALTER TABLE `team` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Current Database: `valorant_identity`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `valorant_identity` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `valorant_identity`;

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(95) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__efmigrationshistory`
--

LOCK TABLES `__efmigrationshistory` WRITE;
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory` VALUES ('20200831143424_InitialGrant','3.1.7'),('20200831143451_InitialConfig','3.1.7'),('20200831145918_CreateIdentitySchema','3.1.7');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_apiresourceclaims`
--

DROP TABLE IF EXISTS `is4_apiresourceclaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_apiresourceclaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Type` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ApiResourceId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ApiResourceClaims_ApiResourceId` (`ApiResourceId`),
  CONSTRAINT `FK_ApiResourceClaims_Is4_ApiResources_ApiResourceId` FOREIGN KEY (`ApiResourceId`) REFERENCES `is4_apiresources` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_apiresourceclaims`
--

LOCK TABLES `is4_apiresourceclaims` WRITE;
/*!40000 ALTER TABLE `is4_apiresourceclaims` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_apiresourceclaims` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_apiresourceproperties`
--

DROP TABLE IF EXISTS `is4_apiresourceproperties`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_apiresourceproperties` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Key` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ApiResourceId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ApiResourceProperties_ApiResourceId` (`ApiResourceId`),
  CONSTRAINT `FK_ApiResourceProperties_Is4_ApiResources_ApiResourceId` FOREIGN KEY (`ApiResourceId`) REFERENCES `is4_apiresources` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_apiresourceproperties`
--

LOCK TABLES `is4_apiresourceproperties` WRITE;
/*!40000 ALTER TABLE `is4_apiresourceproperties` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_apiresourceproperties` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_apiresources`
--

DROP TABLE IF EXISTS `is4_apiresources`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_apiresources` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Enabled` tinyint(1) NOT NULL,
  `Name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DisplayName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `AllowedAccessTokenSigningAlgorithms` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ShowInDiscoveryDocument` tinyint(1) NOT NULL,
  `Created` datetime(6) NOT NULL,
  `Updated` datetime(6) DEFAULT NULL,
  `LastAccessed` datetime(6) DEFAULT NULL,
  `NonEditable` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Is4_ApiResources_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_apiresources`
--

LOCK TABLES `is4_apiresources` WRITE;
/*!40000 ALTER TABLE `is4_apiresources` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_apiresources` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_apiresourcescopes`
--

DROP TABLE IF EXISTS `is4_apiresourcescopes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_apiresourcescopes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Scope` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ApiResourceId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ApiResourceScopes_ApiResourceId` (`ApiResourceId`),
  CONSTRAINT `FK_ApiResourceScopes_Is4_ApiResources_ApiResourceId` FOREIGN KEY (`ApiResourceId`) REFERENCES `is4_apiresources` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_apiresourcescopes`
--

LOCK TABLES `is4_apiresourcescopes` WRITE;
/*!40000 ALTER TABLE `is4_apiresourcescopes` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_apiresourcescopes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_apiresourcesecrets`
--

DROP TABLE IF EXISTS `is4_apiresourcesecrets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_apiresourcesecrets` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Expiration` datetime(6) DEFAULT NULL,
  `Type` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Created` datetime(6) NOT NULL,
  `ApiResourceId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ApiResourceSecrets_ApiResourceId` (`ApiResourceId`),
  CONSTRAINT `FK_ApiResourceSecrets_Is4_ApiResources_ApiResourceId` FOREIGN KEY (`ApiResourceId`) REFERENCES `is4_apiresources` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_apiresourcesecrets`
--

LOCK TABLES `is4_apiresourcesecrets` WRITE;
/*!40000 ALTER TABLE `is4_apiresourcesecrets` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_apiresourcesecrets` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_apiscopeclaims`
--

DROP TABLE IF EXISTS `is4_apiscopeclaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_apiscopeclaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Type` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ScopeId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ApiScopeClaims_ScopeId` (`ScopeId`),
  CONSTRAINT `FK_ApiScopeClaims_Is4_ApiScopes_ScopeId` FOREIGN KEY (`ScopeId`) REFERENCES `is4_apiscopes` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_apiscopeclaims`
--

LOCK TABLES `is4_apiscopeclaims` WRITE;
/*!40000 ALTER TABLE `is4_apiscopeclaims` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_apiscopeclaims` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_apiscopeproperties`
--

DROP TABLE IF EXISTS `is4_apiscopeproperties`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_apiscopeproperties` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Key` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ScopeId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ApiScopeProperties_ScopeId` (`ScopeId`),
  CONSTRAINT `FK_ApiScopeProperties_Is4_ApiScopes_ScopeId` FOREIGN KEY (`ScopeId`) REFERENCES `is4_apiscopes` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_apiscopeproperties`
--

LOCK TABLES `is4_apiscopeproperties` WRITE;
/*!40000 ALTER TABLE `is4_apiscopeproperties` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_apiscopeproperties` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_apiscopes`
--

DROP TABLE IF EXISTS `is4_apiscopes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_apiscopes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Enabled` tinyint(1) NOT NULL,
  `Name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DisplayName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Required` tinyint(1) NOT NULL,
  `Emphasize` tinyint(1) NOT NULL,
  `ShowInDiscoveryDocument` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Is4_ApiScopes_Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_apiscopes`
--

LOCK TABLES `is4_apiscopes` WRITE;
/*!40000 ALTER TABLE `is4_apiscopes` DISABLE KEYS */;
INSERT INTO `is4_apiscopes` VALUES (1,1,'scope2','scope2',NULL,0,0,1),(2,1,'scope1','scope1',NULL,0,0,1),(3,1,'valorant','valorant',NULL,0,0,1);
/*!40000 ALTER TABLE `is4_apiscopes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_aspnetroleclaims`
--

DROP TABLE IF EXISTS `is4_aspnetroleclaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_aspnetroleclaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RoleId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClaimType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ClaimValue` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_AspNetRoleClaims_RoleId` (`RoleId`),
  CONSTRAINT `FK_AspNetRoleClaims_Is4AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `is4_aspnetroles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_aspnetroleclaims`
--

LOCK TABLES `is4_aspnetroleclaims` WRITE;
/*!40000 ALTER TABLE `is4_aspnetroleclaims` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_aspnetroleclaims` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_aspnetroles`
--

DROP TABLE IF EXISTS `is4_aspnetroles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_aspnetroles` (
  `Id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `NormalizedName` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `RoleNameIndex` (`NormalizedName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_aspnetroles`
--

LOCK TABLES `is4_aspnetroles` WRITE;
/*!40000 ALTER TABLE `is4_aspnetroles` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_aspnetroles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_aspnetuserclaims`
--

DROP TABLE IF EXISTS `is4_aspnetuserclaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_aspnetuserclaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClaimType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ClaimValue` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_AspNetUserClaims_UserId` (`UserId`),
  CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `is4_aspnetusers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_aspnetuserclaims`
--

LOCK TABLES `is4_aspnetuserclaims` WRITE;
/*!40000 ALTER TABLE `is4_aspnetuserclaims` DISABLE KEYS */;
INSERT INTO `is4_aspnetuserclaims` VALUES (9,'fb15c4f6-cd54-4a93-9b3a-eca85f8d6b55','name','Aviad Pineles'),(10,'fb15c4f6-cd54-4a93-9b3a-eca85f8d6b55','email','paviad2@gmail.com'),(11,'573b1d33-8cd9-4ca7-8f97-ed675fa2c778','name','Darryl Roman'),(12,'573b1d33-8cd9-4ca7-8f97-ed675fa2c778','email','darrylroman@gmail.com');
/*!40000 ALTER TABLE `is4_aspnetuserclaims` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_aspnetuserlogins`
--

DROP TABLE IF EXISTS `is4_aspnetuserlogins`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_aspnetuserlogins` (
  `LoginProvider` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProviderKey` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProviderDisplayName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `UserId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`LoginProvider`,`ProviderKey`),
  KEY `IX_AspNetUserLogins_UserId` (`UserId`),
  CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `is4_aspnetusers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_aspnetuserlogins`
--

LOCK TABLES `is4_aspnetuserlogins` WRITE;
/*!40000 ALTER TABLE `is4_aspnetuserlogins` DISABLE KEYS */;
INSERT INTO `is4_aspnetuserlogins` VALUES ('Google','103751997981305260077','Google','fb15c4f6-cd54-4a93-9b3a-eca85f8d6b55'),('Google','112351696952500135619','Google','573b1d33-8cd9-4ca7-8f97-ed675fa2c778');
/*!40000 ALTER TABLE `is4_aspnetuserlogins` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_aspnetuserroles`
--

DROP TABLE IF EXISTS `is4_aspnetuserroles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_aspnetuserroles` (
  `UserId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `RoleId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`UserId`,`RoleId`),
  KEY `IX_AspNetUserRoles_RoleId` (`RoleId`),
  CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `is4_aspnetusers` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_AspNetUserRoles_Is4AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `is4_aspnetroles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_aspnetuserroles`
--

LOCK TABLES `is4_aspnetuserroles` WRITE;
/*!40000 ALTER TABLE `is4_aspnetuserroles` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_aspnetuserroles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_aspnetusers`
--

DROP TABLE IF EXISTS `is4_aspnetusers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_aspnetusers` (
  `Id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `UserName` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `NormalizedUserName` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Email` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `NormalizedEmail` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `EmailConfirmed` tinyint(1) NOT NULL,
  `PasswordHash` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `SecurityStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `PhoneNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `PhoneNumberConfirmed` tinyint(1) NOT NULL,
  `TwoFactorEnabled` tinyint(1) NOT NULL,
  `LockoutEnd` datetime(6) DEFAULT NULL,
  `LockoutEnabled` tinyint(1) NOT NULL,
  `AccessFailedCount` int NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UserNameIndex` (`NormalizedUserName`),
  KEY `EmailIndex` (`NormalizedEmail`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_aspnetusers`
--

LOCK TABLES `is4_aspnetusers` WRITE;
/*!40000 ALTER TABLE `is4_aspnetusers` DISABLE KEYS */;
INSERT INTO `is4_aspnetusers` VALUES ('35bfccde-2fad-49ed-a700-c224916f43a2','test@test.com','TEST@TEST.COM','test@test.com','TEST@TEST.COM',0,'AQAAAAEAACcQAAAAENcGbZwa42AheDA2mpeyoBliUXn7KXUiLv8vANgAiqJYbMCb2bHKqGNhkFD3Pe2Jwg==','V5XOUDY3EXTTJQ5EZ7JIAQCT3CQGPSNO','b9cd7bb3-2016-4827-a434-c5a4114bc6df',NULL,0,0,NULL,1,0),('573b1d33-8cd9-4ca7-8f97-ed675fa2c778','823586bd-6c4b-4877-a7b7-ae741ba955c5','823586BD-6C4B-4877-A7B7-AE741BA955C5','darrylroman@gmail.com','DARRYLROMAN@GMAIL.COM',0,NULL,'2IMC6T5EIH3SYOFC5AHRCC7CL7CREJ23','e1ab0480-2f53-4b43-a2b0-8edadaa59c64',NULL,0,0,NULL,1,0),('fb15c4f6-cd54-4a93-9b3a-eca85f8d6b55','3909706b-38d4-4510-8575-790cb603804b','3909706B-38D4-4510-8575-790CB603804B','paviad2@gmail.com','PAVIAD2@GMAIL.COM',0,NULL,'LD2WYKJ5IAIONM64NVLVC6JXKHDLYX35','e0c1e1ce-90fb-40a9-aa63-4351d9e7e92a',NULL,0,0,NULL,1,0);
/*!40000 ALTER TABLE `is4_aspnetusers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_aspnetusertokens`
--

DROP TABLE IF EXISTS `is4_aspnetusertokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_aspnetusertokens` (
  `UserId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `LoginProvider` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`UserId`,`LoginProvider`,`Name`),
  CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `is4_aspnetusers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_aspnetusertokens`
--

LOCK TABLES `is4_aspnetusertokens` WRITE;
/*!40000 ALTER TABLE `is4_aspnetusertokens` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_aspnetusertokens` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_clientclaims`
--

DROP TABLE IF EXISTS `is4_clientclaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_clientclaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Type` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientClaims_ClientId` (`ClientId`),
  CONSTRAINT `FK_ClientClaims_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `is4_clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_clientclaims`
--

LOCK TABLES `is4_clientclaims` WRITE;
/*!40000 ALTER TABLE `is4_clientclaims` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_clientclaims` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_clientcorsorigins`
--

DROP TABLE IF EXISTS `is4_clientcorsorigins`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_clientcorsorigins` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Origin` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientCorsOrigins_ClientId` (`ClientId`),
  CONSTRAINT `FK_ClientCorsOrigins_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `is4_clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_clientcorsorigins`
--

LOCK TABLES `is4_clientcorsorigins` WRITE;
/*!40000 ALTER TABLE `is4_clientcorsorigins` DISABLE KEYS */;
INSERT INTO `is4_clientcorsorigins` VALUES (1,'http://localhost:4200',2);
/*!40000 ALTER TABLE `is4_clientcorsorigins` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_clientgranttypes`
--

DROP TABLE IF EXISTS `is4_clientgranttypes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_clientgranttypes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `GrantType` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientGrantTypes_ClientId` (`ClientId`),
  CONSTRAINT `FK_ClientGrantTypes_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `is4_clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_clientgranttypes`
--

LOCK TABLES `is4_clientgranttypes` WRITE;
/*!40000 ALTER TABLE `is4_clientgranttypes` DISABLE KEYS */;
INSERT INTO `is4_clientgranttypes` VALUES (1,'client_credentials',1),(2,'authorization_code',2);
/*!40000 ALTER TABLE `is4_clientgranttypes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_clientidprestrictions`
--

DROP TABLE IF EXISTS `is4_clientidprestrictions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_clientidprestrictions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Provider` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientIdPRestrictions_ClientId` (`ClientId`),
  CONSTRAINT `FK_ClientIdPRestrictions_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `is4_clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_clientidprestrictions`
--

LOCK TABLES `is4_clientidprestrictions` WRITE;
/*!40000 ALTER TABLE `is4_clientidprestrictions` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_clientidprestrictions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_clientpostlogoutredirecturis`
--

DROP TABLE IF EXISTS `is4_clientpostlogoutredirecturis`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_clientpostlogoutredirecturis` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `PostLogoutRedirectUri` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientPostLogoutRedirectUris_ClientId` (`ClientId`),
  CONSTRAINT `FK_ClientPostLogoutRedirectUris_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `is4_clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_clientpostlogoutredirecturis`
--

LOCK TABLES `is4_clientpostlogoutredirecturis` WRITE;
/*!40000 ALTER TABLE `is4_clientpostlogoutredirecturis` DISABLE KEYS */;
INSERT INTO `is4_clientpostlogoutredirecturis` VALUES (1,'https://localhost:5001/signout-callback-oidc',2),(2,'http://localhost:4200/',2),(3,'https://valorant.hotslogs.com/',2);
/*!40000 ALTER TABLE `is4_clientpostlogoutredirecturis` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_clientproperties`
--

DROP TABLE IF EXISTS `is4_clientproperties`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_clientproperties` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Key` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientProperties_ClientId` (`ClientId`),
  CONSTRAINT `FK_ClientProperties_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `is4_clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_clientproperties`
--

LOCK TABLES `is4_clientproperties` WRITE;
/*!40000 ALTER TABLE `is4_clientproperties` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_clientproperties` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_clientredirecturis`
--

DROP TABLE IF EXISTS `is4_clientredirecturis`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_clientredirecturis` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RedirectUri` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientRedirectUris_ClientId` (`ClientId`),
  CONSTRAINT `FK_ClientRedirectUris_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `is4_clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_clientredirecturis`
--

LOCK TABLES `is4_clientredirecturis` WRITE;
/*!40000 ALTER TABLE `is4_clientredirecturis` DISABLE KEYS */;
INSERT INTO `is4_clientredirecturis` VALUES (1,'https://localhost:5001/signin-oidc',2),(2,'http://localhost:4200/callback',2),(3,'https://valorant.hotslogs.com/callback',2),(4,'https://valorant.hotslogs.com/silent-renew.html',2),(5,'https://localhost:44300/silent-renew.html',2);
/*!40000 ALTER TABLE `is4_clientredirecturis` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_clients`
--

DROP TABLE IF EXISTS `is4_clients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_clients` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Enabled` tinyint(1) NOT NULL,
  `ClientId` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProtocolType` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `RequireClientSecret` tinyint(1) NOT NULL,
  `ClientName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ClientUri` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `LogoUri` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `RequireConsent` tinyint(1) NOT NULL,
  `AllowRememberConsent` tinyint(1) NOT NULL,
  `AlwaysIncludeUserClaimsInIdToken` tinyint(1) NOT NULL,
  `RequirePkce` tinyint(1) NOT NULL,
  `AllowPlainTextPkce` tinyint(1) NOT NULL,
  `RequireRequestObject` tinyint(1) NOT NULL,
  `AllowAccessTokensViaBrowser` tinyint(1) NOT NULL,
  `FrontChannelLogoutUri` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `FrontChannelLogoutSessionRequired` tinyint(1) NOT NULL,
  `BackChannelLogoutUri` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `BackChannelLogoutSessionRequired` tinyint(1) NOT NULL,
  `AllowOfflineAccess` tinyint(1) NOT NULL,
  `IdentityTokenLifetime` int NOT NULL,
  `AllowedIdentityTokenSigningAlgorithms` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `AccessTokenLifetime` int NOT NULL,
  `AuthorizationCodeLifetime` int NOT NULL,
  `ConsentLifetime` int DEFAULT NULL,
  `AbsoluteRefreshTokenLifetime` int NOT NULL,
  `SlidingRefreshTokenLifetime` int NOT NULL,
  `RefreshTokenUsage` int NOT NULL,
  `UpdateAccessTokenClaimsOnRefresh` tinyint(1) NOT NULL,
  `RefreshTokenExpiration` int NOT NULL,
  `AccessTokenType` int NOT NULL,
  `EnableLocalLogin` tinyint(1) NOT NULL,
  `IncludeJwtId` tinyint(1) NOT NULL,
  `AlwaysSendClientClaims` tinyint(1) NOT NULL,
  `ClientClaimsPrefix` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `PairWiseSubjectSalt` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Created` datetime(6) NOT NULL,
  `Updated` datetime(6) DEFAULT NULL,
  `LastAccessed` datetime(6) DEFAULT NULL,
  `UserSsoLifetime` int DEFAULT NULL,
  `UserCodeType` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `DeviceCodeLifetime` int NOT NULL,
  `NonEditable` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Clients_ClientId` (`ClientId`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_clients`
--

LOCK TABLES `is4_clients` WRITE;
/*!40000 ALTER TABLE `is4_clients` DISABLE KEYS */;
INSERT INTO `is4_clients` VALUES (1,1,'m2m.client','oidc',1,'Client Credentials Client',NULL,NULL,NULL,0,1,0,1,0,0,0,NULL,1,NULL,1,0,300,NULL,3600,300,NULL,2592000,1296000,1,0,1,0,1,1,0,'client_',NULL,'2020-09-01 09:38:16.785908',NULL,NULL,NULL,NULL,300,0),(2,1,'interactive','oidc',1,NULL,NULL,NULL,NULL,0,1,0,1,0,0,0,NULL,1,NULL,1,1,300,NULL,600,300,NULL,2592000,1296000,1,0,1,0,1,1,0,'client_',NULL,'2020-09-01 09:38:16.892595',NULL,NULL,NULL,NULL,300,0);
/*!40000 ALTER TABLE `is4_clients` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_clientscopes`
--

DROP TABLE IF EXISTS `is4_clientscopes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_clientscopes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Scope` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientScopes_ClientId` (`ClientId`),
  CONSTRAINT `FK_ClientScopes_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `is4_clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_clientscopes`
--

LOCK TABLES `is4_clientscopes` WRITE;
/*!40000 ALTER TABLE `is4_clientscopes` DISABLE KEYS */;
INSERT INTO `is4_clientscopes` VALUES (1,'scope1',1),(2,'openid',2),(3,'profile',2),(4,'valorant',2),(5,'email',2);
/*!40000 ALTER TABLE `is4_clientscopes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_clientsecrets`
--

DROP TABLE IF EXISTS `is4_clientsecrets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_clientsecrets` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Description` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Expiration` datetime(6) DEFAULT NULL,
  `Type` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Created` datetime(6) NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientSecrets_ClientId` (`ClientId`),
  CONSTRAINT `FK_ClientSecrets_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `is4_clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_clientsecrets`
--

LOCK TABLES `is4_clientsecrets` WRITE;
/*!40000 ALTER TABLE `is4_clientsecrets` DISABLE KEYS */;
INSERT INTO `is4_clientsecrets` VALUES (1,NULL,'fU7fRb+g6YdlniuSqviOLWNkda1M/MuPtH6zNI9inF8=',NULL,'SharedSecret','2020-09-01 09:38:16.786208',1),(2,NULL,'o90IbCACXKUkunXoa18cODcLKnQTbjOo5ihEw9j58+8=',NULL,'SharedSecret','2020-09-01 09:38:16.892598',2);
/*!40000 ALTER TABLE `is4_clientsecrets` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_devicecodes`
--

DROP TABLE IF EXISTS `is4_devicecodes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_devicecodes` (
  `UserCode` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DeviceCode` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `SubjectId` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `SessionId` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ClientId` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Description` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `CreationTime` datetime(6) NOT NULL,
  `Expiration` datetime(6) NOT NULL,
  `Data` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`UserCode`),
  UNIQUE KEY `IX_DeviceCodes_DeviceCode` (`DeviceCode`),
  KEY `IX_DeviceCodes_Expiration` (`Expiration`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_devicecodes`
--

LOCK TABLES `is4_devicecodes` WRITE;
/*!40000 ALTER TABLE `is4_devicecodes` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_devicecodes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_identityresourceclaims`
--

DROP TABLE IF EXISTS `is4_identityresourceclaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_identityresourceclaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Type` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IdentityResourceId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_IdentityResourceClaims_IdentityResourceId` (`IdentityResourceId`),
  CONSTRAINT `FK_IdentityResourceClaims_IdentityResources_IdentityResourceId` FOREIGN KEY (`IdentityResourceId`) REFERENCES `is4_identityresources` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_identityresourceclaims`
--

LOCK TABLES `is4_identityresourceclaims` WRITE;
/*!40000 ALTER TABLE `is4_identityresourceclaims` DISABLE KEYS */;
INSERT INTO `is4_identityresourceclaims` VALUES (1,'website',1),(2,'picture',1),(3,'profile',1),(4,'preferred_username',1),(5,'nickname',1),(6,'middle_name',1),(7,'given_name',1),(8,'family_name',1),(9,'name',1),(10,'gender',1),(11,'birthdate',1),(12,'zoneinfo',1),(13,'locale',1),(14,'updated_at',1),(15,'sub',2),(16,'email',3);
/*!40000 ALTER TABLE `is4_identityresourceclaims` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_identityresourceproperties`
--

DROP TABLE IF EXISTS `is4_identityresourceproperties`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_identityresourceproperties` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Key` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IdentityResourceId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_IdentityResourceProperties_IdentityResourceId` (`IdentityResourceId`),
  CONSTRAINT `FK_IdentityResourceProperties_IdentityResources_IdentityResourc~` FOREIGN KEY (`IdentityResourceId`) REFERENCES `is4_identityresources` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_identityresourceproperties`
--

LOCK TABLES `is4_identityresourceproperties` WRITE;
/*!40000 ALTER TABLE `is4_identityresourceproperties` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_identityresourceproperties` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_identityresources`
--

DROP TABLE IF EXISTS `is4_identityresources`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_identityresources` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Enabled` tinyint(1) NOT NULL,
  `Name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DisplayName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Required` tinyint(1) NOT NULL,
  `Emphasize` tinyint(1) NOT NULL,
  `ShowInDiscoveryDocument` tinyint(1) NOT NULL,
  `Created` datetime(6) NOT NULL,
  `Updated` datetime(6) DEFAULT NULL,
  `NonEditable` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_IdentityResources_Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_identityresources`
--

LOCK TABLES `is4_identityresources` WRITE;
/*!40000 ALTER TABLE `is4_identityresources` DISABLE KEYS */;
INSERT INTO `is4_identityresources` VALUES (1,1,'profile','User profile','Your user profile information (first name, last name, etc.)',0,1,1,'2020-09-01 09:38:17.100974',NULL,0),(2,1,'openid','Your user identifier',NULL,1,0,1,'2020-09-01 09:38:17.084088',NULL,0),(3,1,'email','User email',NULL,1,0,1,'2020-09-01 12:42:00.000000',NULL,0);
/*!40000 ALTER TABLE `is4_identityresources` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `is4_persistedgrants`
--

DROP TABLE IF EXISTS `is4_persistedgrants`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `is4_persistedgrants` (
  `Key` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `SubjectId` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `SessionId` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ClientId` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Description` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `CreationTime` datetime(6) NOT NULL,
  `Expiration` datetime(6) DEFAULT NULL,
  `ConsumedTime` datetime(6) DEFAULT NULL,
  `Data` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Key`),
  KEY `IX_PersistedGrants_Expiration` (`Expiration`),
  KEY `IX_PersistedGrants_SubjectId_ClientId_Type` (`SubjectId`,`ClientId`,`Type`),
  KEY `IX_PersistedGrants_SubjectId_SessionId_Type` (`SubjectId`,`SessionId`,`Type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `is4_persistedgrants`
--

LOCK TABLES `is4_persistedgrants` WRITE;
/*!40000 ALTER TABLE `is4_persistedgrants` DISABLE KEYS */;
/*!40000 ALTER TABLE `is4_persistedgrants` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-09-17 18:31:49
