CREATE DATABASE  IF NOT EXISTS `heroesdata` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `heroesdata`;
-- MySQL dump 10.13  Distrib 8.0.19, for Win64 (x86_64)
--
-- Host: 104.254.245.56    Database: heroesdata
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
-- Table structure for table `amazonreplacementbucket`
--

DROP TABLE IF EXISTS `amazonreplacementbucket`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `amazonreplacementbucket` (
  `Id` varchar(200) NOT NULL,
  `Blob` longblob,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `blogposts`
--

DROP TABLE IF EXISTS `blogposts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `blogposts` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Content` text NOT NULL,
  `CreateDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ExpireDate` timestamp NULL DEFAULT NULL,
  `Tags` varchar(45) NOT NULL DEFAULT '@main@',
  `Priority` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `buildnumbers`
--

DROP TABLE IF EXISTS `buildnumbers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `buildnumbers` (
  `buildnumber` int NOT NULL,
  `builddate` date DEFAULT NULL,
  `version` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`buildnumber`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `dataupdate`
--

DROP TABLE IF EXISTS `dataupdate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `dataupdate` (
  `DataEvent` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `LastUpdated` timestamp NOT NULL,
  PRIMARY KEY (`DataEvent`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `event`
--

DROP TABLE IF EXISTS `event`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `event` (
  `EventID` int NOT NULL AUTO_INCREMENT,
  `EventIDParent` int DEFAULT NULL,
  `EventName` varchar(100) NOT NULL,
  `EventOrder` int NOT NULL,
  `EventGamesPlayed` int NOT NULL,
  `IsEnabled` bit(1) NOT NULL,
  PRIMARY KEY (`EventID`),
  UNIQUE KEY `EventName_UNIQUE` (`EventName`),
  KEY `FK_EventIDParent_EventID_idx` (`EventIDParent`),
  CONSTRAINT `FK_EventIDParent_EventID` FOREIGN KEY (`EventIDParent`) REFERENCES `event` (`EventID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1156 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fingerprint_date`
--

DROP TABLE IF EXISTS `fingerprint_date`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fingerprint_date` (
  `2017-08-27 14:51:18` datetime DEFAULT NULL,
  `725ba498-2728-d326-b6ac-11129c55b212` text
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `groupfinderlisting`
--

DROP TABLE IF EXISTS `groupfinderlisting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `groupfinderlisting` (
  `PlayerID` int NOT NULL,
  `GroupFinderListingTypeID` int NOT NULL,
  `Information` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `MMRSearchRadius` int NOT NULL,
  `TimestampExpiration` timestamp NOT NULL,
  PRIMARY KEY (`PlayerID`),
  KEY `IX_GroupFinderListingTypeID` (`GroupFinderListingTypeID`),
  KEY `IX_TimestampExpiration` (`TimestampExpiration`),
  CONSTRAINT `FK_GroupFinderListing_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `heroiconinformation`
--

DROP TABLE IF EXISTS `heroiconinformation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `heroiconinformation` (
  `pkid` int NOT NULL AUTO_INCREMENT,
  `name` varchar(45) NOT NULL,
  `icon` varchar(255) NOT NULL DEFAULT '~/Images/Heroes/Portraits/AutoSelect.png',
  PRIMARY KEY (`pkid`)
) ENGINE=InnoDB AUTO_INCREMENT=99 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `herotalentinformation`
--

DROP TABLE IF EXISTS `herotalentinformation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `herotalentinformation` (
  `Character` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `ReplayBuildFirst` int NOT NULL,
  `ReplayBuildLast` int NOT NULL,
  `TalentID` int NOT NULL,
  `TalentTier` int NOT NULL,
  `TalentName` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `TalentDescription` varchar(1000) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  PRIMARY KEY (`Character`,`ReplayBuildFirst`,`TalentID`),
  KEY `IX_ReplayBuildFirst_ReplayBuildLast` (`ReplayBuildFirst`,`ReplayBuildLast`),
  KEY `IX_Character_ReplayBuildFirst_ReplayBuildLast` (`Character`,`ReplayBuildFirst`,`ReplayBuildLast`),
  KEY `IX_Character_ReplayBuildFirst` (`Character`,`ReplayBuildFirst`),
  KEY `IX_Character_TalentID` (`Character`,`TalentID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hotsapireplays`
--

DROP TABLE IF EXISTS `hotsapireplays`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hotsapireplays` (
  `id` int unsigned NOT NULL,
  `parsed_id` int unsigned DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  `filename` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `size` int unsigned NOT NULL,
  `game_type` enum('QuickMatch','UnrankedDraft','HeroLeague','TeamLeague','Brawl','StormLeague') CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `game_date` datetime DEFAULT NULL,
  `game_length` smallint unsigned DEFAULT NULL,
  `game_map_id` int unsigned DEFAULT NULL,
  `game_version` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `region` tinyint unsigned DEFAULT NULL,
  `fingerprint` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `processed` tinyint NOT NULL DEFAULT '0',
  `deleted` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `replays_filename_unique` (`filename`),
  UNIQUE KEY `replays_fingerprint_v3_index` (`fingerprint`),
  UNIQUE KEY `replays_parsed_id_uindex` (`parsed_id`),
  KEY `replays_game_type_index` (`game_type`),
  KEY `replays_game_date_index` (`game_date`),
  KEY `replays_processed_deleted_index` (`processed`,`deleted`),
  KEY `replays_created_at_index` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hotsapitalents`
--

DROP TABLE IF EXISTS `hotsapitalents`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hotsapitalents` (
  `pkid` int NOT NULL AUTO_INCREMENT,
  `Hero` varchar(30) DEFAULT NULL,
  `TalentID` int NOT NULL DEFAULT '0',
  `Sort` int NOT NULL DEFAULT '0',
  `Level` int NOT NULL DEFAULT '1',
  `Name` varchar(100) DEFAULT NULL,
  `Title` varchar(100) DEFAULT NULL,
  `Description` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`pkid`)
) ENGINE=InnoDB AUTO_INCREMENT=12995 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `leaderboardoptout`
--

DROP TABLE IF EXISTS `leaderboardoptout`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `leaderboardoptout` (
  `PlayerID` int NOT NULL,
  PRIMARY KEY (`PlayerID`),
  CONSTRAINT `FK_LeaderboardOptOut_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `leaderboardranking`
--

DROP TABLE IF EXISTS `leaderboardranking`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `leaderboardranking` (
  `PlayerID` int NOT NULL,
  `GameMode` int NOT NULL,
  `CurrentMMR` int NOT NULL,
  `LeagueID` int DEFAULT NULL,
  `LeagueRank` int DEFAULT NULL,
  `IsEligibleForLeaderboard` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`PlayerID`,`GameMode`),
  KEY `FK_LeaderboardRanking_League_idx` (`LeagueID`),
  KEY `IX_GameMode_CurrentMMR` (`GameMode`,`CurrentMMR`),
  KEY `IX_LeagueID` (`LeagueID`),
  KEY `IX_LeagueID_LeagueRank` (`LeagueID`,`LeagueRank`),
  KEY `IX_IsEligibleForLeaderboard` (`IsEligibleForLeaderboard`),
  KEY `IX_GameMode_LeagueID_LeagueRank` (`GameMode`,`LeagueID`,`LeagueRank`),
  CONSTRAINT `FK_LeaderboardRanking_League` FOREIGN KEY (`LeagueID`) REFERENCES `league` (`LeagueID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_LeaderboardRanking_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `league`
--

DROP TABLE IF EXISTS `league`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `league` (
  `LeagueID` int NOT NULL,
  `LeagueName` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `RequiredGames` int NOT NULL,
  PRIMARY KEY (`LeagueID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `localizationalias`
--

DROP TABLE IF EXISTS `localizationalias`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `localizationalias` (
  `IdentifierID` int NOT NULL,
  `Type` int NOT NULL,
  `PrimaryName` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `AttributeName` varchar(40) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Group` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `SubGroup` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `AliasesCSV` varchar(2000) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `NewGroup` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  PRIMARY KEY (`IdentifierID`),
  UNIQUE KEY `AttributeName_UNIQUE` (`AttributeName`),
  KEY `IX_Type` (`Type`),
  KEY `IX_Group` (`Group`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `logerror`
--

DROP TABLE IF EXISTS `logerror`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `logerror` (
  `LogErrorID` int NOT NULL AUTO_INCREMENT,
  `AbsoluteUri` varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `UserAgent` varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `UserHostAddress` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `UserID` int DEFAULT NULL,
  `ErrorMessage` mediumtext CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `DateTimeErrorOccurred` timestamp NOT NULL DEFAULT '2020-01-20 13:27:33',
  `Referer` varchar(500) COLLATE utf8_bin DEFAULT NULL,
  PRIMARY KEY (`LogErrorID`),
  KEY `FK_LogError_my_aspnet_users_idx` (`UserID`),
  KEY `IX_DateTimeErrorOccurred` (`DateTimeErrorOccurred`),
  KEY `IX_UserHostAddress` (`UserHostAddress`),
  CONSTRAINT `FK_LogError_my_aspnet_users` FOREIGN KEY (`UserID`) REFERENCES `net48_users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=973015 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `missingtalents`
--

DROP TABLE IF EXISTS `missingtalents`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `missingtalents` (
  `Character` varchar(50) NOT NULL,
  `Build` int NOT NULL,
  `TalentID` int NOT NULL,
  PRIMARY KEY (`Character`,`Build`,`TalentID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mmrrecalc`
--

DROP TABLE IF EXISTS `mmrrecalc`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mmrrecalc` (
  `BattleNetRegionID` int NOT NULL,
  `GameMode` int NOT NULL,
  `TipOld` timestamp NULL DEFAULT NULL,
  `TipRecent` timestamp NULL DEFAULT NULL,
  `TipManual` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`BattleNetRegionID`,`GameMode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mountinformation`
--

DROP TABLE IF EXISTS `mountinformation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mountinformation` (
  `AttributeId` varchar(4) NOT NULL,
  `Name` varchar(100) NOT NULL,
  `Description` varchar(1000) NOT NULL,
  `Franchise` varchar(50) NOT NULL,
  `ReleaseDate` date NOT NULL,
  PRIMARY KEY (`AttributeId`),
  UNIQUE KEY `Name_UNIQUE` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `net48_users`
--

DROP TABLE IF EXISTS `net48_users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `net48_users` (
  `id` int NOT NULL AUTO_INCREMENT,
  `email` varchar(45) CHARACTER SET utf8 NOT NULL,
  `username` varchar(45) CHARACTER SET utf8 NOT NULL,
  `password` varchar(255) CHARACTER SET utf8 NOT NULL,
  `acceptedTOS` bit(1) NOT NULL DEFAULT b'0',
  `userGUID` varchar(45) CHARACTER SET utf8 NOT NULL,
  `premium` int NOT NULL DEFAULT '0',
  `resettoken` varchar(255) CHARACTER SET utf8 DEFAULT '',
  `tokengenerated` datetime DEFAULT NULL,
  `admin` int DEFAULT '0',
  `subscriptionid` varchar(45) CHARACTER SET utf8 DEFAULT NULL,
  `applicationId` int NOT NULL,
  `isAnonymous` tinyint(1) NOT NULL DEFAULT '1',
  `lastActivityDate` datetime DEFAULT NULL,
  `IsBattleNetOAuthAuthorized` bit(1) NOT NULL DEFAULT b'0',
  `IsGroupFinderAuthorized3` bit(1) NOT NULL DEFAULT b'1',
  `IsGroupFinderAuthorized4` bit(1) NOT NULL DEFAULT b'1',
  `IsGroupFinderAuthorized5` bit(1) NOT NULL DEFAULT b'1',
  `playerID` int DEFAULT NULL,
  `LastLoginDate` datetime DEFAULT NULL,
  `LastPasswordChangedDate` datetime DEFAULT NULL,
  `CreationDate` datetime DEFAULT NULL,
  `IsLockedOut` tinyint(1) DEFAULT NULL,
  `LastLockedOutDate` datetime DEFAULT NULL,
  `FailedPasswordAttemptCount` int unsigned DEFAULT NULL,
  `FailedPasswordAttemptWindowStart` datetime DEFAULT NULL,
  `FailedPasswordAnswerAttemptCount` int unsigned DEFAULT NULL,
  `FailedPasswordAnswerAttemptWindowStart` datetime DEFAULT NULL,
  `expiration` datetime DEFAULT NULL,
  `PremiumSupporterSince` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `username_UNIQUE` (`username`),
  UNIQUE KEY `email_UNIQUE` (`email`),
  KEY `FK_player_idx` (`playerID`),
  CONSTRAINT `FK_player` FOREIGN KEY (`playerID`) REFERENCES `player` (`PlayerID`)
) ENGINE=InnoDB AUTO_INCREMENT=512766 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `obsolete_premiumaccount`
--

DROP TABLE IF EXISTS `obsolete_premiumaccount`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `obsolete_premiumaccount` (
  `PlayerID` int DEFAULT NULL,
  `TimestampSupporterSince` timestamp NOT NULL,
  `TimestampSupporterExpiration` timestamp NOT NULL,
  `UserID` int NOT NULL,
  PRIMARY KEY (`UserID`),
  KEY `FK_User_idx` (`UserID`),
  KEY `FK_PremiumAccount_Player` (`PlayerID`),
  CONSTRAINT `FK_PremiumAccount_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_User` FOREIGN KEY (`UserID`) REFERENCES `net48_users` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `player`
--

DROP TABLE IF EXISTS `player`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `player` (
  `PlayerID` int NOT NULL AUTO_INCREMENT,
  `BattleNetRegionId` int NOT NULL,
  `BattleNetSubId` int NOT NULL,
  `BattleNetId` int NOT NULL,
  `Name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_as_ci NOT NULL,
  `BattleTag` int DEFAULT NULL,
  `TimestampCreated` timestamp NOT NULL DEFAULT '2020-01-14 18:00:00',
  PRIMARY KEY (`PlayerID`),
  UNIQUE KEY `Unique_BattleNet` (`BattleNetRegionId`,`BattleNetSubId`,`BattleNetId`),
  KEY `IX_BattleNetId` (`BattleNetId`),
  KEY `IX_BattleNetRegionId_BattleNetSubId_BattleNetId` (`BattleNetRegionId`,`BattleNetSubId`,`BattleNetId`),
  KEY `IX_BattleNetRegionId_BattleNetSubId` (`BattleNetRegionId`,`BattleNetSubId`),
  KEY `IX_Name` (`Name`),
  KEY `IX_BattleNetRegionId_PlayerID` (`BattleNetRegionId`,`PlayerID`)
) ENGINE=InnoDB AUTO_INCREMENT=16635316 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_as_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `playeraggregate`
--

DROP TABLE IF EXISTS `playeraggregate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `playeraggregate` (
  `PlayerID` int NOT NULL,
  `GameMode` int NOT NULL,
  `GamesPlayedTotal` int NOT NULL,
  `GamesPlayedWithMMR` int NOT NULL,
  `GamesPlayedRecently` int NOT NULL,
  `FavoriteCharacter` int NOT NULL,
  `TimestampLastUpdated` timestamp NOT NULL,
  PRIMARY KEY (`PlayerID`,`GameMode`),
  KEY `IX_TimestampLastUpdated` (`TimestampLastUpdated`),
  KEY `IX_GameMode_TimestampLastUpdated` (`GameMode`,`TimestampLastUpdated`),
  KEY `FK_PlayerAggregate_LocalizationAlias_idx` (`FavoriteCharacter`),
  CONSTRAINT `FK_PlayerAggregate_LocalizationAlias` FOREIGN KEY (`FavoriteCharacter`) REFERENCES `localizationalias` (`IdentifierID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_PlayerAggregate_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `playeralt`
--

DROP TABLE IF EXISTS `playeralt`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `playeralt` (
  `PlayerIDAlt` int NOT NULL,
  `PlayerIDMain` int NOT NULL,
  PRIMARY KEY (`PlayerIDAlt`),
  KEY `FK_PlayerIDAlt_idx` (`PlayerIDAlt`),
  KEY `FK_PlayerIDMain` (`PlayerIDMain`),
  CONSTRAINT `FK_PlayerIDAlt` FOREIGN KEY (`PlayerIDAlt`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_PlayerIDMain` FOREIGN KEY (`PlayerIDMain`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `playerbanned`
--

DROP TABLE IF EXISTS `playerbanned`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `playerbanned` (
  `PlayerID` int NOT NULL,
  PRIMARY KEY (`PlayerID`),
  CONSTRAINT `FK_PlayerBanned_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `playerbannedleaderboard`
--

DROP TABLE IF EXISTS `playerbannedleaderboard`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `playerbannedleaderboard` (
  `PlayerID` int NOT NULL,
  PRIMARY KEY (`PlayerID`),
  CONSTRAINT `FK_PlayerBannedLeaderboard_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `playerdisablenamechange`
--

DROP TABLE IF EXISTS `playerdisablenamechange`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `playerdisablenamechange` (
  `PlayerID` int NOT NULL,
  PRIMARY KEY (`PlayerID`),
  CONSTRAINT `FK_PlayerDisableNameChange_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `playermmrmilestonev3`
--

DROP TABLE IF EXISTS `playermmrmilestonev3`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `playermmrmilestonev3` (
  `PlayerID` int NOT NULL,
  `GameMode` int NOT NULL,
  `MilestoneDate` date NOT NULL,
  `MMRMean` double NOT NULL,
  `MMRStandardDeviation` double NOT NULL,
  `MMRRating` int NOT NULL,
  PRIMARY KEY (`PlayerID`,`MilestoneDate`,`GameMode`),
  KEY `IX_PlayerID_MilestoneDate` (`PlayerID`,`MilestoneDate`),
  KEY `IX_MilestoneDate` (`MilestoneDate`),
  KEY `IX_MMRRating` (`MMRRating`),
  KEY `IX_GameMode_MilestoneDate` (`GameMode`,`MilestoneDate`),
  CONSTRAINT `FK_PlayerMMRMilestoneV3_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `playermmrreset`
--

DROP TABLE IF EXISTS `playermmrreset`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `playermmrreset` (
  `ResetDate` date NOT NULL,
  `Title` varchar(50) NOT NULL,
  `MMRMeanMultiplier` double NOT NULL,
  `MMRStandardDeviationGapMultiplier` double NOT NULL,
  `IsClampOutliers` bit(1) NOT NULL,
  PRIMARY KEY (`ResetDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `players_no_replays`
--

DROP TABLE IF EXISTS `players_no_replays`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `players_no_replays` (
  `PlayerID` int NOT NULL DEFAULT '0',
  `BattleNetRegionId` int NOT NULL,
  `BattleNetSubId` int NOT NULL,
  `BattleNetId` int NOT NULL,
  `Name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_as_ci NOT NULL,
  `BattleTag` int DEFAULT NULL,
  `TimestampCreated` timestamp NOT NULL DEFAULT '2020-01-14 18:00:00'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `premiumpayment`
--

DROP TABLE IF EXISTS `premiumpayment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `premiumpayment` (
  `TransactionID` varchar(50) NOT NULL,
  `Email` varchar(128) NOT NULL,
  `ItemTitle` varchar(200) NOT NULL,
  `PaymentAmountGross` decimal(15,4) NOT NULL,
  `PaymentAmountFee` decimal(15,4) NOT NULL,
  `TimestampPayment` timestamp NOT NULL,
  PRIMARY KEY (`TransactionID`),
  KEY `IX_Email` (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replay`
--

DROP TABLE IF EXISTS `replay`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replay` (
  `ReplayID` int NOT NULL AUTO_INCREMENT,
  `ReplayBuild` int NOT NULL,
  `GameMode` int NOT NULL,
  `MapID` int NOT NULL,
  `ReplayLength` time NOT NULL,
  `ReplayHash` binary(16) NOT NULL,
  `TimestampReplay` timestamp NOT NULL,
  `TimestampCreated` timestamp NOT NULL,
  `HOTSAPIFingerprint` varchar(36) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  PRIMARY KEY (`ReplayID`),
  UNIQUE KEY `ReplayHash_UNIQUE` (`ReplayHash`),
  KEY `IX_TimestampReplay` (`TimestampReplay`),
  KEY `IX_ReplayBuild_TimestampReplay` (`ReplayBuild`,`TimestampReplay`),
  KEY `IX_GameMode_TimestampReplay` (`GameMode`,`TimestampReplay`),
  KEY `IX_GameMode_TimestampReplay_ReplayID` (`GameMode`,`TimestampReplay`,`ReplayID`),
  KEY `FK_Replay_LocalizationAlias_idx` (`MapID`),
  KEY `IX_MapID` (`MapID`),
  KEY `IX_TimestampCreated` (`TimestampCreated`),
  KEY `IX_GameMode` (`GameMode`)
) ENGINE=InnoDB AUTO_INCREMENT=149974351 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replay_dups`
--

DROP TABLE IF EXISTS `replay_dups`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replay_dups` (
  `ReplayID` int NOT NULL,
  `ReplayBuild` int NOT NULL,
  `GameMode` int NOT NULL,
  `MapID` int NOT NULL,
  `ReplayLength` time NOT NULL,
  `ReplayHash` binary(16) NOT NULL,
  `TimestampReplay` timestamp NOT NULL,
  `TimestampCreated` timestamp NOT NULL,
  `HOTSAPIFingerprint` varchar(36) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  PRIMARY KEY (`ReplayID`),
  UNIQUE KEY `ReplayHash_UNIQUE` (`ReplayHash`),
  KEY `IX_TimestampReplay` (`TimestampReplay`),
  KEY `IX_ReplayBuild_TimestampReplay` (`ReplayBuild`,`TimestampReplay`),
  KEY `IX_GameMode_TimestampReplay` (`GameMode`,`TimestampReplay`),
  KEY `IX_GameMode_TimestampReplay_ReplayID` (`GameMode`,`TimestampReplay`,`ReplayID`),
  KEY `FK_Replay_LocalizationAlias_idx` (`MapID`),
  KEY `IX_MapID` (`MapID`),
  KEY `IX_TimestampCreated` (`TimestampCreated`),
  KEY `IX_GameMode` (`GameMode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replay_dups2`
--

DROP TABLE IF EXISTS `replay_dups2`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replay_dups2` (
  `ReplayID` int NOT NULL,
  `DupOfReplayID` int NOT NULL,
  PRIMARY KEY (`ReplayID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replay_mirror`
--

DROP TABLE IF EXISTS `replay_mirror`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replay_mirror` (
  `ReplayID` int NOT NULL,
  PRIMARY KEY (`ReplayID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replay_notalents`
--

DROP TABLE IF EXISTS `replay_notalents`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replay_notalents` (
  `ReplayID` int NOT NULL DEFAULT '0',
  `ReplayBuild` int NOT NULL,
  `GameMode` int NOT NULL,
  `MapID` int NOT NULL,
  `ReplayLength` time NOT NULL,
  `ReplayHash` binary(16) NOT NULL,
  `TimestampReplay` timestamp NOT NULL,
  `TimestampCreated` timestamp NOT NULL,
  `HOTSAPIFingerprint` varchar(36) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replay_playertalentbuilds`
--

DROP TABLE IF EXISTS `replay_playertalentbuilds`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replay_playertalentbuilds` (
  `replayid` int NOT NULL DEFAULT '0',
  `playerid` int NOT NULL,
  `talentselection` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`replayid`,`playerid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replay_tmp_todelete`
--

DROP TABLE IF EXISTS `replay_tmp_todelete`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replay_tmp_todelete` (
  `todel` int NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replay_tmp_toreset`
--

DROP TABLE IF EXISTS `replay_tmp_toreset`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replay_tmp_toreset` (
  `todel` int NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replaycharacter`
--

DROP TABLE IF EXISTS `replaycharacter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replaycharacter` (
  `ReplayID` int NOT NULL,
  `PlayerID` int NOT NULL,
  `IsAutoSelect` bit(1) NOT NULL,
  `CharacterID` int NOT NULL,
  `CharacterLevel` int NOT NULL,
  `IsWinner` bit(1) NOT NULL,
  `MMRBefore` int DEFAULT NULL,
  `MMRChange` int DEFAULT NULL,
  PRIMARY KEY (`ReplayID`,`PlayerID`),
  KEY `FK_ReplayCharacter_Replay_idx` (`ReplayID`),
  KEY `FK_ReplayCharacter_Player_idx` (`PlayerID`),
  KEY `IX_Character_IsWinner` (`IsWinner`),
  KEY `IX_MMRBefore` (`MMRBefore`),
  KEY `FK_ReplayCharacter_LocalizationAlias_idx` (`CharacterID`),
  KEY `IX_CharacterID_IsWinner` (`CharacterID`,`IsWinner`),
  KEY `IX_ReplayID_CharacterID` (`ReplayID`,`CharacterID`),
  KEY `IX_CharacterID_CharacterLevel` (`CharacterID`,`CharacterLevel`),
  CONSTRAINT `FK_ReplayCharacter_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReplayCharacter_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replaycharacterdraftorder`
--

DROP TABLE IF EXISTS `replaycharacterdraftorder`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replaycharacterdraftorder` (
  `ReplayID` int NOT NULL,
  `PlayerID` int NOT NULL,
  `DraftOrder` int NOT NULL,
  PRIMARY KEY (`ReplayID`,`PlayerID`),
  KEY `FK_ReplayCharacter_Replay_idx` (`ReplayID`),
  KEY `FK_ReplayCharacter_Player_idx` (`PlayerID`),
  CONSTRAINT `FK_ReplayCharacterDraftOrder_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReplayCharacterDraftOrder_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReplayCharacterDraftOrder_ReplayCharacter` FOREIGN KEY (`ReplayID`, `PlayerID`) REFERENCES `replaycharacter` (`ReplayID`, `PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_as_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replaycharactermatchaward`
--

DROP TABLE IF EXISTS `replaycharactermatchaward`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replaycharactermatchaward` (
  `ReplayID` int NOT NULL,
  `PlayerID` int NOT NULL,
  `MatchAwardType` int NOT NULL,
  PRIMARY KEY (`ReplayID`,`PlayerID`,`MatchAwardType`),
  KEY `IX_ReplayID_PlayerID` (`ReplayID`,`PlayerID`),
  KEY `IX_ReplayID` (`ReplayID`),
  KEY `IX_PlayerID` (`PlayerID`),
  KEY `IX_MatchAwardType` (`MatchAwardType`),
  KEY `IX_PlayerID_MatchAwardType` (`PlayerID`,`MatchAwardType`),
  CONSTRAINT `FK_ReplayCharacterMatchAward_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReplayCharacterMatchAward_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReplayCharacterMatchAward_ReplayCharacter` FOREIGN KEY (`ReplayID`, `PlayerID`) REFERENCES `replaycharacter` (`ReplayID`, `PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replaycharacterscoreresult`
--

DROP TABLE IF EXISTS `replaycharacterscoreresult`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replaycharacterscoreresult` (
  `ReplayID` int NOT NULL,
  `PlayerID` int NOT NULL,
  `Level` int DEFAULT NULL,
  `Takedowns` int NOT NULL,
  `SoloKills` int NOT NULL,
  `Assists` int NOT NULL,
  `Deaths` int NOT NULL,
  `HighestKillStreak` int DEFAULT NULL,
  `HeroDamage` int NOT NULL,
  `SiegeDamage` int NOT NULL,
  `StructureDamage` int NOT NULL,
  `MinionDamage` int NOT NULL,
  `CreepDamage` int NOT NULL,
  `SummonDamage` int NOT NULL,
  `TimeCCdEnemyHeroes` time DEFAULT NULL,
  `Healing` int DEFAULT NULL,
  `SelfHealing` int NOT NULL,
  `DamageTaken` int DEFAULT NULL,
  `ExperienceContribution` int NOT NULL,
  `TownKills` int NOT NULL,
  `TimeSpentDead` time NOT NULL,
  `MercCampCaptures` int NOT NULL,
  `WatchTowerCaptures` int NOT NULL,
  `MetaExperience` int NOT NULL,
  PRIMARY KEY (`ReplayID`,`PlayerID`),
  CONSTRAINT `FK_ReplayCharacterScoreResult_ReplayCharacter` FOREIGN KEY (`ReplayID`, `PlayerID`) REFERENCES `replaycharacter` (`ReplayID`, `PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replaycharactersilenced`
--

DROP TABLE IF EXISTS `replaycharactersilenced`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replaycharactersilenced` (
  `ReplayID` int NOT NULL,
  `PlayerID` int NOT NULL,
  PRIMARY KEY (`ReplayID`,`PlayerID`),
  CONSTRAINT `FK_ReplayCharacterSilenced_ReplayCharacter` FOREIGN KEY (`ReplayID`, `PlayerID`) REFERENCES `replaycharacter` (`ReplayID`, `PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replaycharactertalent`
--

DROP TABLE IF EXISTS `replaycharactertalent`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replaycharactertalent` (
  `ReplayID` int NOT NULL,
  `PlayerID` int NOT NULL,
  `TalentID` int NOT NULL,
  PRIMARY KEY (`ReplayID`,`PlayerID`,`TalentID`),
  KEY `IX_ReplayID_PlayerID` (`ReplayID`,`PlayerID`),
  KEY `IX_ReplayID` (`ReplayID`),
  KEY `IX_PlayerID` (`PlayerID`),
  CONSTRAINT `FK_ReplayCharacterTalent_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReplayCharacterTalent_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReplayCharacterTalent_ReplayCharacter` FOREIGN KEY (`ReplayID`, `PlayerID`) REFERENCES `replaycharacter` (`ReplayID`, `PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replaycharacterupgradeeventreplaylengthpercent`
--

DROP TABLE IF EXISTS `replaycharacterupgradeeventreplaylengthpercent`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replaycharacterupgradeeventreplaylengthpercent` (
  `ReplayID` int NOT NULL,
  `PlayerID` int NOT NULL,
  `UpgradeEventType` int NOT NULL,
  `UpgradeEventValue` int NOT NULL,
  `ReplayLengthPercent` decimal(15,13) NOT NULL,
  PRIMARY KEY (`ReplayID`,`PlayerID`,`UpgradeEventType`,`UpgradeEventValue`),
  KEY `IX_UpgradeEventType` (`UpgradeEventType`),
  KEY `IX_UpgradeEventType_UpgradeEventValue` (`UpgradeEventType`,`UpgradeEventValue`),
  CONSTRAINT `FK_this_ReplayCharacter` FOREIGN KEY (`ReplayID`, `PlayerID`) REFERENCES `replaycharacter` (`ReplayID`, `PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replayperiodicxpbreakdown`
--

DROP TABLE IF EXISTS `replayperiodicxpbreakdown`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replayperiodicxpbreakdown` (
  `ReplayID` int NOT NULL,
  `IsWinner` bit(1) NOT NULL,
  `GameTimeMinute` int NOT NULL,
  `MinionXP` int NOT NULL,
  `CreepXP` int NOT NULL,
  `StructureXP` int NOT NULL,
  `HeroXP` int NOT NULL,
  `TrickleXP` int NOT NULL,
  PRIMARY KEY (`ReplayID`,`IsWinner`,`GameTimeMinute`),
  KEY `IX_ReplayID` (`ReplayID`),
  KEY `IX_IsWinner_GameTimeMinute` (`IsWinner`,`GameTimeMinute`),
  CONSTRAINT `FK_ReplayPeriodicXPBreakdown_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replayshare`
--

DROP TABLE IF EXISTS `replayshare`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replayshare` (
  `ReplayShareID` int NOT NULL AUTO_INCREMENT,
  `ReplayID` int NOT NULL,
  `PlayerIDSharedBy` int NOT NULL,
  `AlteredReplayFileName` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Title` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Description` mediumtext CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `UpvoteScore` int NOT NULL,
  PRIMARY KEY (`ReplayShareID`),
  KEY `FK_ReplayShare_Replay_idx` (`ReplayID`),
  KEY `FK_ReplayShare_Player_idx` (`PlayerIDSharedBy`),
  KEY `IX_UpvoteScore` (`UpvoteScore`),
  KEY `IX_UpvoteScore_Title` (`UpvoteScore`,`Title`),
  KEY `IX_Title` (`Title`),
  CONSTRAINT `FK_ReplayShare_Player` FOREIGN KEY (`PlayerIDSharedBy`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReplayShare_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=22444 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replayteamheroban`
--

DROP TABLE IF EXISTS `replayteamheroban`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replayteamheroban` (
  `ReplayID` int NOT NULL,
  `CharacterID` int NOT NULL,
  `IsWinner` bit(1) NOT NULL,
  `BanPhase` int NOT NULL,
  PRIMARY KEY (`ReplayID`,`CharacterID`),
  KEY `IX_ReplayID_IsWinner` (`ReplayID`,`IsWinner`),
  KEY `IX_IsWinner` (`IsWinner`),
  KEY `IX_CharacterID` (`CharacterID`),
  CONSTRAINT `FK_ReplayTeamHeroBan_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replayteamobjective`
--

DROP TABLE IF EXISTS `replayteamobjective`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replayteamobjective` (
  `ReplayID` int NOT NULL,
  `IsWinner` bit(1) NOT NULL,
  `TeamObjectiveType` int NOT NULL,
  `TimeSpan` time NOT NULL,
  `PlayerID` int DEFAULT NULL,
  `Value` int NOT NULL,
  PRIMARY KEY (`ReplayID`,`IsWinner`,`TeamObjectiveType`,`TimeSpan`),
  KEY `IX_TeamObjectiveType` (`TeamObjectiveType`),
  KEY `IX_PlayerID` (`PlayerID`),
  CONSTRAINT `FK_this_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_this_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `reputation`
--

DROP TABLE IF EXISTS `reputation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `reputation` (
  `PlayerId` int NOT NULL,
  `Reputation` int NOT NULL,
  PRIMARY KEY (`PlayerId`),
  CONSTRAINT `FK_Players` FOREIGN KEY (`PlayerId`) REFERENCES `player` (`PlayerID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `talentimagemapping`
--

DROP TABLE IF EXISTS `talentimagemapping`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `talentimagemapping` (
  `TalentName` varchar(45) NOT NULL,
  `TalentImage` varchar(255) DEFAULT NULL,
  `HeroName` varchar(50) NOT NULL,
  `Character` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`TalentName`,`HeroName`),
  UNIQUE KEY `idx_talentimagemapping_TalentName_HeroName` (`TalentName`,`HeroName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `tmp`
--

DROP TABLE IF EXISTS `tmp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tmp` (
  `hero` varchar(200) CHARACTER SET utf8 NOT NULL,
  `dro` bigint DEFAULT NULL,
  `winrate` decimal(30,4) DEFAULT NULL,
  `games` bigint NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `unknowndata`
--

DROP TABLE IF EXISTS `unknowndata`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `unknowndata` (
  `UnknownData` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  PRIMARY KEY (`UnknownData`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `votes`
--

DROP TABLE IF EXISTS `votes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `votes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `VotingPlayerId` int NOT NULL,
  `TargetPlayerId` int NOT NULL,
  `TargetReplayId` int NOT NULL,
  `Up` bit(1) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=104001 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `zamuser`
--

DROP TABLE IF EXISTS `zamuser`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `zamuser` (
  `ID` varchar(36) NOT NULL,
  `Email` varchar(128) NOT NULL,
  `Username` varchar(100) NOT NULL,
  `PremiumExpiration` datetime DEFAULT NULL,
  `IsHotslogsPremiumConverted` int DEFAULT NULL,
  `TimestampCreated` datetime NOT NULL,
  `TimestampLastUpdated` datetime NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `email_UNIQUE` (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping routines for database 'heroesdata'
--
/*!50003 DROP FUNCTION IF EXISTS `act` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`hotslogs-aviad`@`%` FUNCTION `act`(x float, m float, f float) RETURNS float
    NO SQL
BEGIN

RETURN tanh((x-m)*f)/2.0+0.5;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP FUNCTION IF EXISTS `rankinga` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`hotslogs-aviad`@`%` FUNCTION `rankinga`(
    ver varchar(50),
	mmr int, mmrFactor float, 
    winrate float, winrateFactor float, 
    games float, gamesFactor float,
    magicFactor float) RETURNS float
    NO SQL
BEGIN
return
case
	when ver='v1' then mmr * winrate * (atan((games-gamesFactor)/magicFactor)/pi()+0.5)
    when ver='v2' then ((mmr/mmrFactor)+(EXP(winrateFactor*winrate)-1)/(EXP(winrateFactor)-1))*(ATAN((games-gamesFactor)/magicFactor)/PI()+0.5)
    when ver='v3' then (mmr/mmrFactor)*(act(winrate,0.4,winrateFactor))*(act(games,gamesFactor,magicFactor))
    when ver='hp' then (50+((winrate*100-50)*(games/gamesFactor)))+(mmr/mmrFactor)
end;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP FUNCTION IF EXISTS `rankingb` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`hotslogs-aviad`@`%` FUNCTION `rankingb`(
	mmr float,
    wonGames int,
    playedGames int,
    gamesNeeded int,
    gamesStrictness float
) RETURNS float
    NO SQL
BEGIN
declare winrate float;
set winrate = 1.0 * wonGames / playedGames;
return
case
	when winrate<0.5 then 0
	else rankinga('v3',mmr,30,1.0 * wonGames / playedGames,2.5,playedGames,gamesNeeded,gamesStrictness)
end;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP FUNCTION IF EXISTS `tanh` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`hotslogs-aviad`@`%` FUNCTION `tanh`(x float) RETURNS float
    NO SQL
BEGIN
return
case
	when x>=10 then 1
    when x<=-10 then -1
	else (exp(2*x)-1.0)/(exp(2*x)+1.0)
end;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-06-08  7:05:38
