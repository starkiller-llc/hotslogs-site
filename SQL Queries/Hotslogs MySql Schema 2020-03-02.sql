-- MySQL dump 10.13  Distrib 8.0.19, for Win64 (x86_64)
--
-- Host: 198.71.53.97    Database: heroesdata
-- ------------------------------------------------------
-- Server version	8.0.17

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
-- Table structure for table `buildnumbers`
--

DROP TABLE IF EXISTS `buildnumbers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `buildnumbers` (
  `buildnumber` int(11) NOT NULL,
  `builddate` date DEFAULT NULL,
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
  `DataEvent` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
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
  `EventID` int(11) NOT NULL AUTO_INCREMENT,
  `EventIDParent` int(11) DEFAULT NULL,
  `EventName` varchar(100) NOT NULL,
  `EventOrder` int(11) NOT NULL,
  `EventGamesPlayed` int(11) NOT NULL,
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
  `PlayerID` int(11) NOT NULL,
  `GroupFinderListingTypeID` int(11) NOT NULL,
  `Information` varchar(200) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `MMRSearchRadius` int(11) NOT NULL,
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
  `pkid` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(45) NOT NULL,
  `icon` varchar(255) NOT NULL DEFAULT '~/Images/Heroes/Portraits/AutoSelect.png',
  PRIMARY KEY (`pkid`)
) ENGINE=InnoDB AUTO_INCREMENT=97 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `herotalentinformation`
--

DROP TABLE IF EXISTS `herotalentinformation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `herotalentinformation` (
  `Character` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `ReplayBuildFirst` int(11) NOT NULL,
  `ReplayBuildLast` int(11) NOT NULL,
  `TalentID` int(11) NOT NULL,
  `TalentTier` int(11) NOT NULL,
  `TalentName` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `TalentDescription` varchar(1000) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  PRIMARY KEY (`Character`,`ReplayBuildFirst`,`TalentID`),
  KEY `IX_ReplayBuildFirst_ReplayBuildLast` (`ReplayBuildFirst`,`ReplayBuildLast`),
  KEY `IX_Character_ReplayBuildFirst_ReplayBuildLast` (`Character`,`ReplayBuildFirst`,`ReplayBuildLast`),
  KEY `IX_Character_ReplayBuildFirst` (`Character`,`ReplayBuildFirst`),
  KEY `IX_Character_TalentID` (`Character`,`TalentID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hluser`
--

DROP TABLE IF EXISTS `hluser`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hluser` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `email` varchar(45) NOT NULL,
  `username` varchar(45) NOT NULL,
  `expiration` datetime DEFAULT NULL,
  `password` varchar(255) NOT NULL,
  `acceptedTOS` varchar(5) NOT NULL DEFAULT 'false',
  `userGUID` varchar(45) NOT NULL,
  `premium` int(11) NOT NULL DEFAULT '0',
  `resettoken` varchar(255) DEFAULT '',
  `tokengenerated` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  UNIQUE KEY `email_UNIQUE` (`email`),
  UNIQUE KEY `username_UNIQUE` (`username`)
) ENGINE=InnoDB AUTO_INCREMENT=3784 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hotsapireplays`
--

DROP TABLE IF EXISTS `hotsapireplays`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hotsapireplays` (
  `id` int(10) unsigned NOT NULL,
  `parsed_id` int(10) unsigned DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  `filename` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `size` int(10) unsigned NOT NULL,
  `game_type` enum('QuickMatch','UnrankedDraft','HeroLeague','TeamLeague','Brawl','StormLeague') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `game_date` datetime DEFAULT NULL,
  `game_length` smallint(5) unsigned DEFAULT NULL,
  `game_map_id` int(10) unsigned DEFAULT NULL,
  `game_version` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `region` tinyint(3) unsigned DEFAULT NULL,
  `fingerprint` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `processed` tinyint(4) NOT NULL DEFAULT '0',
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
  `pkid` int(11) NOT NULL AUTO_INCREMENT,
  `Hero` varchar(30) DEFAULT NULL,
  `TalentID` int(11) NOT NULL DEFAULT '0',
  `Sort` int(11) NOT NULL DEFAULT '0',
  `Level` int(11) NOT NULL DEFAULT '1',
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
  `PlayerID` int(11) NOT NULL,
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
  `PlayerID` int(11) NOT NULL,
  `GameMode` int(11) NOT NULL,
  `CurrentMMR` int(11) NOT NULL,
  `LeagueID` int(11) DEFAULT NULL,
  `LeagueRank` int(11) DEFAULT NULL,
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
  `LeagueID` int(11) NOT NULL,
  `LeagueName` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `RequiredGames` int(11) NOT NULL,
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
  `IdentifierID` int(11) NOT NULL,
  `Type` int(11) NOT NULL,
  `PrimaryName` varchar(200) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `AttributeName` varchar(4) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `Group` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin DEFAULT NULL,
  `SubGroup` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin DEFAULT NULL,
  `AliasesCSV` varchar(2000) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
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
  `LogErrorID` int(11) NOT NULL AUTO_INCREMENT,
  `AbsoluteUri` varchar(500) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `UserAgent` varchar(500) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `UserHostAddress` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `UserID` int(11) DEFAULT NULL,
  `ErrorMessage` mediumtext CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `DateTimeErrorOccurred` timestamp NOT NULL DEFAULT '2020-01-20 19:27:33',
  PRIMARY KEY (`LogErrorID`),
  KEY `FK_LogError_my_aspnet_users_idx` (`UserID`),
  KEY `IX_DateTimeErrorOccurred` (`DateTimeErrorOccurred`),
  KEY `IX_UserHostAddress` (`UserHostAddress`),
  CONSTRAINT `FK_LogError_my_aspnet_users` FOREIGN KEY (`UserID`) REFERENCES `my_aspnet_users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=517905 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
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
-- Table structure for table `my_aspnet_membership`
--

DROP TABLE IF EXISTS `my_aspnet_membership`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `my_aspnet_membership` (
  `userId` int(11) NOT NULL DEFAULT '0',
  `Email` varchar(128) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Comment` varchar(255) CHARACTER SET utf8 COLLATE utf8_bin DEFAULT NULL,
  `Password` varchar(128) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `PasswordKey` char(32) CHARACTER SET utf8 COLLATE utf8_bin DEFAULT NULL,
  `PasswordFormat` tinyint(4) DEFAULT NULL,
  `PasswordQuestion` varchar(255) CHARACTER SET utf8 COLLATE utf8_bin DEFAULT NULL,
  `PasswordAnswer` varchar(255) CHARACTER SET utf8 COLLATE utf8_bin DEFAULT NULL,
  `IsApproved` tinyint(1) DEFAULT NULL,
  `LastActivityDate` datetime DEFAULT NULL,
  `LastLoginDate` datetime DEFAULT NULL,
  `LastPasswordChangedDate` datetime DEFAULT NULL,
  `CreationDate` datetime DEFAULT NULL,
  `IsLockedOut` tinyint(1) DEFAULT NULL,
  `LastLockedOutDate` datetime DEFAULT NULL,
  `FailedPasswordAttemptCount` int(10) unsigned DEFAULT NULL,
  `FailedPasswordAttemptWindowStart` datetime DEFAULT NULL,
  `FailedPasswordAnswerAttemptCount` int(10) unsigned DEFAULT NULL,
  `FailedPasswordAnswerAttemptWindowStart` datetime DEFAULT NULL,
  PRIMARY KEY (`userId`),
  UNIQUE KEY `Email_UNIQUE` (`Email`),
  CONSTRAINT `FK_my_aspnet_membership_my_aspnet_users` FOREIGN KEY (`userId`) REFERENCES `my_aspnet_users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin COMMENT='2';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `my_aspnet_profiles`
--

DROP TABLE IF EXISTS `my_aspnet_profiles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `my_aspnet_profiles` (
  `userId` int(11) NOT NULL,
  `valueindex` longtext CHARACTER SET utf8 COLLATE utf8_bin,
  `stringdata` longtext CHARACTER SET utf8 COLLATE utf8_bin,
  `binarydata` longblob,
  `lastUpdatedDate` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`userId`),
  CONSTRAINT `FK_my_aspnet_profiles_my_aspnet_users` FOREIGN KEY (`userId`) REFERENCES `my_aspnet_users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `my_aspnet_users`
--

DROP TABLE IF EXISTS `my_aspnet_users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `my_aspnet_users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `applicationId` int(11) NOT NULL,
  `name` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `isAnonymous` tinyint(1) NOT NULL DEFAULT '1',
  `lastActivityDate` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name_UNIQUE` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=496341 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `player`
--

DROP TABLE IF EXISTS `player`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `player` (
  `PlayerID` int(11) NOT NULL AUTO_INCREMENT,
  `BattleNetRegionId` int(11) NOT NULL,
  `BattleNetSubId` int(11) NOT NULL,
  `BattleNetId` int(11) NOT NULL,
  `Name` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `BattleTag` int(11) DEFAULT NULL,
  `TimestampCreated` timestamp NOT NULL DEFAULT '2020-01-15 00:00:00',
  PRIMARY KEY (`PlayerID`),
  UNIQUE KEY `Unique_BattleNet` (`BattleNetRegionId`,`BattleNetSubId`,`BattleNetId`),
  KEY `IX_BattleNetId` (`BattleNetId`),
  KEY `IX_BattleNetRegionId_BattleNetSubId_BattleNetId` (`BattleNetRegionId`,`BattleNetSubId`,`BattleNetId`),
  KEY `IX_BattleNetRegionId_BattleNetSubId` (`BattleNetRegionId`,`BattleNetSubId`),
  KEY `IX_Name` (`Name`),
  KEY `IX_BattleNetRegionId_PlayerID` (`BattleNetRegionId`,`PlayerID`)
) ENGINE=InnoDB AUTO_INCREMENT=13707195 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `playeraggregate`
--

DROP TABLE IF EXISTS `playeraggregate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `playeraggregate` (
  `PlayerID` int(11) NOT NULL,
  `GameMode` int(11) NOT NULL,
  `GamesPlayedTotal` int(11) NOT NULL,
  `GamesPlayedWithMMR` int(11) NOT NULL,
  `GamesPlayedRecently` int(11) NOT NULL,
  `FavoriteCharacter` int(11) NOT NULL,
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
  `PlayerIDAlt` int(11) NOT NULL,
  `PlayerIDMain` int(11) NOT NULL,
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
  `PlayerID` int(11) NOT NULL,
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
  `PlayerID` int(11) NOT NULL,
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
  `PlayerID` int(11) NOT NULL,
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
  `PlayerID` int(11) NOT NULL,
  `GameMode` int(11) NOT NULL,
  `MilestoneDate` date NOT NULL,
  `MMRMean` double NOT NULL,
  `MMRStandardDeviation` double NOT NULL,
  `MMRRating` int(11) NOT NULL,
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
-- Table structure for table `premiumaccount`
--

DROP TABLE IF EXISTS `premiumaccount`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `premiumaccount` (
  `PlayerID` int(11) NOT NULL,
  `TimestampSupporterSince` timestamp NOT NULL,
  `TimestampSupporterExpiration` timestamp NOT NULL,
  PRIMARY KEY (`PlayerID`),
  CONSTRAINT `FK_PremiumAccount_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
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
  `ReplayID` int(11) NOT NULL AUTO_INCREMENT,
  `ReplayBuild` int(11) NOT NULL,
  `GameMode` int(11) NOT NULL,
  `MapID` int(11) NOT NULL,
  `ReplayLength` time NOT NULL,
  `ReplayHash` binary(16) NOT NULL,
  `TimestampReplay` timestamp NOT NULL,
  `TimestampCreated` timestamp NOT NULL,
  `HOTSAPIFingerprint` varchar(36) CHARACTER SET utf8 COLLATE utf8_bin DEFAULT NULL,
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
) ENGINE=InnoDB AUTO_INCREMENT=136743651 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replaycharacter`
--

DROP TABLE IF EXISTS `replaycharacter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replaycharacter` (
  `ReplayID` int(11) NOT NULL,
  `PlayerID` int(11) NOT NULL,
  `IsAutoSelect` bit(1) NOT NULL,
  `CharacterID` int(11) NOT NULL,
  `CharacterLevel` int(11) NOT NULL,
  `IsWinner` bit(1) NOT NULL,
  `MMRBefore` int(11) DEFAULT NULL,
  `MMRChange` int(11) DEFAULT NULL,
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
-- Table structure for table `replaycharactermatchaward`
--

DROP TABLE IF EXISTS `replaycharactermatchaward`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replaycharactermatchaward` (
  `ReplayID` int(11) NOT NULL,
  `PlayerID` int(11) NOT NULL,
  `MatchAwardType` int(11) NOT NULL,
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
  `ReplayID` int(11) NOT NULL,
  `PlayerID` int(11) NOT NULL,
  `Level` int(11) DEFAULT NULL,
  `Takedowns` int(11) NOT NULL,
  `SoloKills` int(11) NOT NULL,
  `Assists` int(11) NOT NULL,
  `Deaths` int(11) NOT NULL,
  `HighestKillStreak` int(11) DEFAULT NULL,
  `HeroDamage` int(11) NOT NULL,
  `SiegeDamage` int(11) NOT NULL,
  `StructureDamage` int(11) NOT NULL,
  `MinionDamage` int(11) NOT NULL,
  `CreepDamage` int(11) NOT NULL,
  `SummonDamage` int(11) NOT NULL,
  `TimeCCdEnemyHeroes` time DEFAULT NULL,
  `Healing` int(11) DEFAULT NULL,
  `SelfHealing` int(11) NOT NULL,
  `DamageTaken` int(11) DEFAULT NULL,
  `ExperienceContribution` int(11) NOT NULL,
  `TownKills` int(11) NOT NULL,
  `TimeSpentDead` time NOT NULL,
  `MercCampCaptures` int(11) NOT NULL,
  `WatchTowerCaptures` int(11) NOT NULL,
  `MetaExperience` int(11) NOT NULL,
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
  `ReplayID` int(11) NOT NULL,
  `PlayerID` int(11) NOT NULL,
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
  `ReplayID` int(11) NOT NULL,
  `PlayerID` int(11) NOT NULL,
  `TalentID` int(11) NOT NULL,
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
  `ReplayID` int(11) NOT NULL,
  `PlayerID` int(11) NOT NULL,
  `UpgradeEventType` int(11) NOT NULL,
  `UpgradeEventValue` int(11) NOT NULL,
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
  `ReplayID` int(11) NOT NULL,
  `IsWinner` bit(1) NOT NULL,
  `GameTimeMinute` int(11) NOT NULL,
  `MinionXP` int(11) NOT NULL,
  `CreepXP` int(11) NOT NULL,
  `StructureXP` int(11) NOT NULL,
  `HeroXP` int(11) NOT NULL,
  `TrickleXP` int(11) NOT NULL,
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
  `ReplayShareID` int(11) NOT NULL AUTO_INCREMENT,
  `ReplayID` int(11) NOT NULL,
  `PlayerIDSharedBy` int(11) NOT NULL,
  `AlteredReplayFileName` varchar(200) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `Title` varchar(200) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `Description` mediumtext CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `UpvoteScore` int(11) NOT NULL,
  PRIMARY KEY (`ReplayShareID`),
  KEY `FK_ReplayShare_Replay_idx` (`ReplayID`),
  KEY `FK_ReplayShare_Player_idx` (`PlayerIDSharedBy`),
  KEY `IX_UpvoteScore` (`UpvoteScore`),
  KEY `IX_UpvoteScore_Title` (`UpvoteScore`,`Title`),
  KEY `IX_Title` (`Title`),
  CONSTRAINT `FK_ReplayShare_Player` FOREIGN KEY (`PlayerIDSharedBy`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReplayShare_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=22314 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `replayteamheroban`
--

DROP TABLE IF EXISTS `replayteamheroban`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `replayteamheroban` (
  `ReplayID` int(11) NOT NULL,
  `CharacterID` int(11) NOT NULL,
  `IsWinner` bit(1) NOT NULL,
  `BanPhase` int(11) NOT NULL,
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
  `ReplayID` int(11) NOT NULL,
  `IsWinner` bit(1) NOT NULL,
  `TeamObjectiveType` int(11) NOT NULL,
  `TimeSpan` time NOT NULL,
  `PlayerID` int(11) DEFAULT NULL,
  `Value` int(11) NOT NULL,
  PRIMARY KEY (`ReplayID`,`IsWinner`,`TeamObjectiveType`,`TimeSpan`),
  KEY `IX_TeamObjectiveType` (`TeamObjectiveType`),
  KEY `IX_PlayerID` (`PlayerID`),
  CONSTRAINT `FK_this_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_this_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `talentimagemapping`
--

DROP TABLE IF EXISTS `talentimagemapping`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `talentimagemapping` (
  `TalentName` varchar(45) DEFAULT NULL,
  `TalentImage` varchar(255) DEFAULT NULL,
  `HeroName` varchar(50) DEFAULT NULL,
  `Character` varchar(45) DEFAULT NULL,
  UNIQUE KEY `idx_talentimagemapping_TalentName_HeroName` (`TalentName`,`HeroName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
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
-- Table structure for table `user`
--

DROP TABLE IF EXISTS `user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user` (
  `UserID` int(11) NOT NULL,
  `PlayerID` int(11) NOT NULL,
  PRIMARY KEY (`UserID`),
  KEY `FK_User_Player_idx` (`PlayerID`),
  CONSTRAINT `FK_User_Player` FOREIGN KEY (`PlayerID`) REFERENCES `player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_User_myaspnetusers` FOREIGN KEY (`UserID`) REFERENCES `my_aspnet_users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
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
  `IsHotslogsPremiumConverted` int(11) DEFAULT NULL,
  `TimestampCreated` datetime NOT NULL,
  `TimestampLastUpdated` datetime NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `email_UNIQUE` (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-03-02 13:28:06
