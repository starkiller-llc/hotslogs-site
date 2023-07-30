CREATE DATABASE  IF NOT EXISTS `HeroesData` /*!40100 DEFAULT CHARACTER SET utf8 */;
USE `HeroesData`;
-- MySQL dump 10.13  Distrib 5.7.17, for Win64 (x86_64)
--
-- ------------------------------------------------------
-- Server version	5.7.16-log

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `DataUpdate`
--

DROP TABLE IF EXISTS `DataUpdate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DataUpdate` (
  `DataEvent` varchar(100) COLLATE utf8_bin NOT NULL,
  `LastUpdated` timestamp NOT NULL,
  PRIMARY KEY (`DataEvent`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Event`
--

DROP TABLE IF EXISTS `Event`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Event` (
  `EventID` int(11) NOT NULL AUTO_INCREMENT,
  `EventIDParent` int(11) DEFAULT NULL,
  `EventName` varchar(100) NOT NULL,
  `EventOrder` int(11) NOT NULL,
  `EventGamesPlayed` int(11) NOT NULL,
  `IsEnabled` bit(1) NOT NULL,
  PRIMARY KEY (`EventID`),
  UNIQUE KEY `EventName_UNIQUE` (`EventName`),
  KEY `FK_EventIDParent_EventID_idx` (`EventIDParent`),
  CONSTRAINT `FK_EventIDParent_EventID` FOREIGN KEY (`EventIDParent`) REFERENCES `Event` (`EventID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1156 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `GroupFinderListing`
--

DROP TABLE IF EXISTS `GroupFinderListing`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `GroupFinderListing` (
  `PlayerID` int(11) NOT NULL,
  `GroupFinderListingTypeID` int(11) NOT NULL,
  `Information` varchar(200) COLLATE utf8_bin NOT NULL,
  `MMRSearchRadius` int(11) NOT NULL,
  `TimestampExpiration` timestamp NOT NULL,
  PRIMARY KEY (`PlayerID`),
  KEY `IX_GroupFinderListingTypeID` (`GroupFinderListingTypeID`),
  KEY `IX_TimestampExpiration` (`TimestampExpiration`),
  CONSTRAINT `FK_GroupFinderListing_Player` FOREIGN KEY (`PlayerID`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `HeroTalentInformation`
--

DROP TABLE IF EXISTS `HeroTalentInformation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `HeroTalentInformation` (
  `Character` varchar(50) COLLATE utf8_bin NOT NULL,
  `ReplayBuildFirst` int(11) NOT NULL,
  `ReplayBuildLast` int(11) NOT NULL,
  `TalentID` int(11) NOT NULL,
  `TalentTier` int(11) NOT NULL,
  `TalentName` varchar(50) COLLATE utf8_bin NOT NULL,
  `TalentDescription` varchar(1000) COLLATE utf8_bin NOT NULL,
  PRIMARY KEY (`Character`,`ReplayBuildFirst`,`TalentID`),
  KEY `IX_ReplayBuildFirst_ReplayBuildLast` (`ReplayBuildFirst`,`ReplayBuildLast`),
  KEY `IX_Character_ReplayBuildFirst_ReplayBuildLast` (`Character`,`ReplayBuildFirst`,`ReplayBuildLast`),
  KEY `IX_Character_ReplayBuildFirst` (`Character`,`ReplayBuildFirst`),
  KEY `IX_Character_TalentID` (`Character`,`TalentID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `LeaderboardOptOut`
--

DROP TABLE IF EXISTS `LeaderboardOptOut`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `LeaderboardOptOut` (
  `PlayerID` int(11) NOT NULL,
  PRIMARY KEY (`PlayerID`),
  CONSTRAINT `FK_LeaderboardOptOut_Player` FOREIGN KEY (`PlayerID`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `LeaderboardRanking`
--

DROP TABLE IF EXISTS `LeaderboardRanking`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `LeaderboardRanking` (
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
  CONSTRAINT `FK_LeaderboardRanking_League` FOREIGN KEY (`LeagueID`) REFERENCES `League` (`LeagueID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_LeaderboardRanking_Player` FOREIGN KEY (`PlayerID`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `League`
--

DROP TABLE IF EXISTS `League`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `League` (
  `LeagueID` int(11) NOT NULL,
  `LeagueName` varchar(50) COLLATE utf8_bin NOT NULL,
  `RequiredGames` int(11) NOT NULL,
  PRIMARY KEY (`LeagueID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `LocalizationAlias`
--

DROP TABLE IF EXISTS `LocalizationAlias`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `LocalizationAlias` (
  `IdentifierID` int(11) NOT NULL,
  `Type` int(11) NOT NULL,
  `PrimaryName` varchar(200) COLLATE utf8_bin NOT NULL,
  `AttributeName` varchar(4) COLLATE utf8_bin NOT NULL,
  `Group` varchar(50) COLLATE utf8_bin DEFAULT NULL,
  `SubGroup` varchar(50) COLLATE utf8_bin DEFAULT NULL,
  `AliasesCSV` varchar(2000) COLLATE utf8_bin NOT NULL,
  PRIMARY KEY (`IdentifierID`),
  UNIQUE KEY `AttributeName_UNIQUE` (`AttributeName`),
  KEY `IX_Type` (`Type`),
  KEY `IX_Group` (`Group`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `LogError`
--

DROP TABLE IF EXISTS `LogError`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `LogError` (
  `LogErrorID` int(11) NOT NULL AUTO_INCREMENT,
  `AbsoluteUri` varchar(500) COLLATE utf8_bin NOT NULL,
  `UserAgent` varchar(500) COLLATE utf8_bin NOT NULL,
  `UserHostAddress` varchar(50) COLLATE utf8_bin NOT NULL,
  `UserID` int(11) DEFAULT NULL,
  `ErrorMessage` mediumtext COLLATE utf8_bin NOT NULL,
  `DateTimeErrorOccurred` timestamp NOT NULL,
  PRIMARY KEY (`LogErrorID`),
  KEY `FK_LogError_my_aspnet_users_idx` (`UserID`),
  KEY `IX_DateTimeErrorOccurred` (`DateTimeErrorOccurred`),
  KEY `IX_UserHostAddress` (`UserHostAddress`),
  CONSTRAINT `FK_LogError_my_aspnet_users` FOREIGN KEY (`UserID`) REFERENCES `my_aspnet_users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=206271 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `MountInformation`
--

DROP TABLE IF EXISTS `MountInformation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `MountInformation` (
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
-- Table structure for table `Player`
--

DROP TABLE IF EXISTS `Player`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Player` (
  `PlayerID` int(11) NOT NULL AUTO_INCREMENT,
  `BattleNetRegionId` int(11) NOT NULL,
  `BattleNetSubId` int(11) NOT NULL,
  `BattleNetId` int(11) NOT NULL,
  `Name` varchar(50) CHARACTER SET utf8 NOT NULL,
  `BattleTag` int(11) DEFAULT NULL,
  `TimestampCreated` timestamp NOT NULL,
  PRIMARY KEY (`PlayerID`),
  UNIQUE KEY `Unique_BattleNet` (`BattleNetRegionId`,`BattleNetSubId`,`BattleNetId`),
  KEY `IX_BattleNetId` (`BattleNetId`),
  KEY `IX_BattleNetRegionId_BattleNetSubId_BattleNetId` (`BattleNetRegionId`,`BattleNetSubId`,`BattleNetId`),
  KEY `IX_BattleNetRegionId_BattleNetSubId` (`BattleNetRegionId`,`BattleNetSubId`),
  KEY `IX_Name` (`Name`),
  KEY `IX_BattleNetRegionId_PlayerID` (`BattleNetRegionId`,`PlayerID`)
) ENGINE=InnoDB AUTO_INCREMENT=10935209 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PlayerAggregate`
--

DROP TABLE IF EXISTS `PlayerAggregate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PlayerAggregate` (
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
  CONSTRAINT `FK_PlayerAggregate_LocalizationAlias` FOREIGN KEY (`FavoriteCharacter`) REFERENCES `LocalizationAlias` (`IdentifierID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_PlayerAggregate_Player` FOREIGN KEY (`PlayerID`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PlayerAlt`
--

DROP TABLE IF EXISTS `PlayerAlt`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PlayerAlt` (
  `PlayerIDAlt` int(11) NOT NULL,
  `PlayerIDMain` int(11) NOT NULL,
  PRIMARY KEY (`PlayerIDAlt`),
  KEY `FK_PlayerIDAlt_idx` (`PlayerIDAlt`),
  KEY `FK_PlayerIDMain` (`PlayerIDMain`),
  CONSTRAINT `FK_PlayerIDAlt` FOREIGN KEY (`PlayerIDAlt`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_PlayerIDMain` FOREIGN KEY (`PlayerIDMain`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PlayerBanned`
--

DROP TABLE IF EXISTS `PlayerBanned`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PlayerBanned` (
  `PlayerID` int(11) NOT NULL,
  PRIMARY KEY (`PlayerID`),
  CONSTRAINT `FK_PlayerBanned_Player` FOREIGN KEY (`PlayerID`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PlayerBannedLeaderboard`
--

DROP TABLE IF EXISTS `PlayerBannedLeaderboard`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PlayerBannedLeaderboard` (
  `PlayerID` int(11) NOT NULL,
  PRIMARY KEY (`PlayerID`),
  CONSTRAINT `FK_PlayerBannedLeaderboard_Player` FOREIGN KEY (`PlayerID`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PlayerDisableNameChange`
--

DROP TABLE IF EXISTS `PlayerDisableNameChange`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PlayerDisableNameChange` (
  `PlayerID` int(11) NOT NULL,
  PRIMARY KEY (`PlayerID`),
  CONSTRAINT `FK_PlayerDisableNameChange_Player` FOREIGN KEY (`PlayerID`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PlayerMMRMilestoneV3`
--

DROP TABLE IF EXISTS `PlayerMMRMilestoneV3`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PlayerMMRMilestoneV3` (
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
  CONSTRAINT `FK_PlayerMMRMilestoneV3_Player` FOREIGN KEY (`PlayerID`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PlayerMMRReset`
--

DROP TABLE IF EXISTS `PlayerMMRReset`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PlayerMMRReset` (
  `ResetDate` date NOT NULL,
  `Title` varchar(50) NOT NULL,
  `MMRMeanMultiplier` double NOT NULL,
  `MMRStandardDeviationGapMultiplier` double NOT NULL,
  `IsClampOutliers` bit(1) NOT NULL,
  PRIMARY KEY (`ResetDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PremiumAccount`
--

DROP TABLE IF EXISTS `PremiumAccount`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PremiumAccount` (
  `PlayerID` int(11) NOT NULL,
  `TimestampSupporterSince` timestamp NOT NULL,
  `TimestampSupporterExpiration` timestamp NOT NULL,
  PRIMARY KEY (`PlayerID`),
  CONSTRAINT `FK_PremiumAccount_Player` FOREIGN KEY (`PlayerID`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PremiumPayment`
--

DROP TABLE IF EXISTS `PremiumPayment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PremiumPayment` (
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
-- Table structure for table `Replay`
--

DROP TABLE IF EXISTS `Replay`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Replay` (
  `ReplayID` int(11) NOT NULL AUTO_INCREMENT,
  `ReplayBuild` int(11) NOT NULL,
  `GameMode` int(11) NOT NULL,
  `MapID` int(11) NOT NULL,
  `ReplayLength` time NOT NULL,
  `ReplayHash` binary(16) NOT NULL,
  `TimestampReplay` timestamp NOT NULL,
  `TimestampCreated` timestamp NOT NULL,
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
) ENGINE=InnoDB AUTO_INCREMENT=133850506 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ReplayCharacter`
--

DROP TABLE IF EXISTS `ReplayCharacter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ReplayCharacter` (
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
  CONSTRAINT `FK_ReplayCharacter_Player` FOREIGN KEY (`PlayerID`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReplayCharacter_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `Replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ReplayCharacterMatchAward`
--

DROP TABLE IF EXISTS `ReplayCharacterMatchAward`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ReplayCharacterMatchAward` (
  `ReplayID` int(11) NOT NULL,
  `PlayerID` int(11) NOT NULL,
  `MatchAwardType` int(11) NOT NULL,
  PRIMARY KEY (`ReplayID`,`PlayerID`,`MatchAwardType`),
  KEY `IX_ReplayID_PlayerID` (`ReplayID`,`PlayerID`),
  KEY `IX_ReplayID` (`ReplayID`),
  KEY `IX_PlayerID` (`PlayerID`),
  KEY `IX_MatchAwardType` (`MatchAwardType`),
  KEY `IX_PlayerID_MatchAwardType` (`PlayerID`,`MatchAwardType`),
  CONSTRAINT `FK_ReplayCharacterMatchAward_Player` FOREIGN KEY (`PlayerID`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReplayCharacterMatchAward_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `Replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReplayCharacterMatchAward_ReplayCharacter` FOREIGN KEY (`ReplayID`, `PlayerID`) REFERENCES `ReplayCharacter` (`ReplayID`, `PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ReplayCharacterScoreResult`
--

DROP TABLE IF EXISTS `ReplayCharacterScoreResult`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ReplayCharacterScoreResult` (
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
  CONSTRAINT `FK_ReplayCharacterScoreResult_ReplayCharacter` FOREIGN KEY (`ReplayID`, `PlayerID`) REFERENCES `ReplayCharacter` (`ReplayID`, `PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ReplayCharacterSilenced`
--

DROP TABLE IF EXISTS `ReplayCharacterSilenced`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ReplayCharacterSilenced` (
  `ReplayID` int(11) NOT NULL,
  `PlayerID` int(11) NOT NULL,
  PRIMARY KEY (`ReplayID`,`PlayerID`),
  CONSTRAINT `FK_ReplayCharacterSilenced_ReplayCharacter` FOREIGN KEY (`ReplayID`, `PlayerID`) REFERENCES `ReplayCharacter` (`ReplayID`, `PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ReplayCharacterTalent`
--

DROP TABLE IF EXISTS `ReplayCharacterTalent`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ReplayCharacterTalent` (
  `ReplayID` int(11) NOT NULL,
  `PlayerID` int(11) NOT NULL,
  `TalentID` int(11) NOT NULL,
  PRIMARY KEY (`ReplayID`,`PlayerID`,`TalentID`),
  KEY `IX_ReplayID_PlayerID` (`ReplayID`,`PlayerID`),
  KEY `IX_ReplayID` (`ReplayID`),
  KEY `IX_PlayerID` (`PlayerID`),
  CONSTRAINT `FK_ReplayCharacterTalent_Player` FOREIGN KEY (`PlayerID`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReplayCharacterTalent_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `Replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReplayCharacterTalent_ReplayCharacter` FOREIGN KEY (`ReplayID`, `PlayerID`) REFERENCES `ReplayCharacter` (`ReplayID`, `PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ReplayCharacterUpgradeEventReplayLengthPercent`
--

DROP TABLE IF EXISTS `ReplayCharacterUpgradeEventReplayLengthPercent`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ReplayCharacterUpgradeEventReplayLengthPercent` (
  `ReplayID` int(11) NOT NULL,
  `PlayerID` int(11) NOT NULL,
  `UpgradeEventType` int(11) NOT NULL,
  `UpgradeEventValue` int(11) NOT NULL,
  `ReplayLengthPercent` decimal(15,13) NOT NULL,
  PRIMARY KEY (`ReplayID`,`PlayerID`,`UpgradeEventType`,`UpgradeEventValue`),
  KEY `IX_UpgradeEventType` (`UpgradeEventType`),
  KEY `IX_UpgradeEventType_UpgradeEventValue` (`UpgradeEventType`,`UpgradeEventValue`),
  CONSTRAINT `FK_this_ReplayCharacter` FOREIGN KEY (`ReplayID`, `PlayerID`) REFERENCES `ReplayCharacter` (`ReplayID`, `PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ReplayPeriodicXPBreakdown`
--

DROP TABLE IF EXISTS `ReplayPeriodicXPBreakdown`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ReplayPeriodicXPBreakdown` (
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
  CONSTRAINT `FK_ReplayPeriodicXPBreakdown_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `Replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ReplayShare`
--

DROP TABLE IF EXISTS `ReplayShare`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ReplayShare` (
  `ReplayShareID` int(11) NOT NULL AUTO_INCREMENT,
  `ReplayID` int(11) NOT NULL,
  `PlayerIDSharedBy` int(11) NOT NULL,
  `AlteredReplayFileName` varchar(200) COLLATE utf8_bin NOT NULL,
  `Title` varchar(200) COLLATE utf8_bin NOT NULL,
  `Description` mediumtext COLLATE utf8_bin NOT NULL,
  `UpvoteScore` int(11) NOT NULL,
  PRIMARY KEY (`ReplayShareID`),
  KEY `FK_ReplayShare_Replay_idx` (`ReplayID`),
  KEY `FK_ReplayShare_Player_idx` (`PlayerIDSharedBy`),
  KEY `IX_UpvoteScore` (`UpvoteScore`),
  KEY `IX_UpvoteScore_Title` (`UpvoteScore`,`Title`),
  KEY `IX_Title` (`Title`),
  CONSTRAINT `FK_ReplayShare_Player` FOREIGN KEY (`PlayerIDSharedBy`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_ReplayShare_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `Replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=22314 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ReplayTeamHeroBan`
--

DROP TABLE IF EXISTS `ReplayTeamHeroBan`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ReplayTeamHeroBan` (
  `ReplayID` int(11) NOT NULL,
  `CharacterID` int(11) NOT NULL,
  `IsWinner` bit(1) NOT NULL,
  `BanPhase` int(11) NOT NULL,
  PRIMARY KEY (`ReplayID`,`CharacterID`),
  KEY `IX_ReplayID_IsWinner` (`ReplayID`,`IsWinner`),
  KEY `IX_IsWinner` (`IsWinner`),
  KEY `IX_CharacterID` (`CharacterID`),
  CONSTRAINT `FK_ReplayTeamHeroBan_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `Replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ReplayTeamObjective`
--

DROP TABLE IF EXISTS `ReplayTeamObjective`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ReplayTeamObjective` (
  `ReplayID` int(11) NOT NULL,
  `IsWinner` bit(1) NOT NULL,
  `TeamObjectiveType` int(11) NOT NULL,
  `TimeSpan` time NOT NULL,
  `PlayerID` int(11) DEFAULT NULL,
  `Value` int(11) NOT NULL,
  PRIMARY KEY (`ReplayID`,`IsWinner`,`TeamObjectiveType`,`TimeSpan`),
  KEY `IX_TeamObjectiveType` (`TeamObjectiveType`),
  KEY `IX_PlayerID` (`PlayerID`),
  CONSTRAINT `FK_this_Player` FOREIGN KEY (`PlayerID`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_this_Replay` FOREIGN KEY (`ReplayID`) REFERENCES `Replay` (`ReplayID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `UnknownData`
--

DROP TABLE IF EXISTS `UnknownData`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `UnknownData` (
  `UnknownData` varchar(100) COLLATE utf8_bin NOT NULL,
  PRIMARY KEY (`UnknownData`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `User`
--

DROP TABLE IF EXISTS `User`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `User` (
  `UserID` int(11) NOT NULL,
  `PlayerID` int(11) NOT NULL,
  PRIMARY KEY (`UserID`),
  KEY `FK_User_Player_idx` (`PlayerID`),
  CONSTRAINT `FK_User_Player` FOREIGN KEY (`PlayerID`) REFERENCES `Player` (`PlayerID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_User_myaspnetusers` FOREIGN KEY (`UserID`) REFERENCES `my_aspnet_users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ZAMUser`
--

DROP TABLE IF EXISTS `ZAMUser`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ZAMUser` (
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

--
-- Table structure for table `my_aspnet_membership`
--

DROP TABLE IF EXISTS `my_aspnet_membership`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `my_aspnet_membership` (
  `userId` int(11) NOT NULL DEFAULT '0',
  `Email` varchar(128) CHARACTER SET utf8 NOT NULL,
  `Comment` varchar(255) COLLATE utf8_bin DEFAULT NULL,
  `Password` varchar(128) COLLATE utf8_bin NOT NULL,
  `PasswordKey` char(32) COLLATE utf8_bin DEFAULT NULL,
  `PasswordFormat` tinyint(4) DEFAULT NULL,
  `PasswordQuestion` varchar(255) COLLATE utf8_bin DEFAULT NULL,
  `PasswordAnswer` varchar(255) COLLATE utf8_bin DEFAULT NULL,
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
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `my_aspnet_profiles` (
  `userId` int(11) NOT NULL,
  `valueindex` longtext COLLATE utf8_bin,
  `stringdata` longtext COLLATE utf8_bin,
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
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `my_aspnet_users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `applicationId` int(11) NOT NULL,
  `name` varchar(100) CHARACTER SET utf8 NOT NULL,
  `isAnonymous` tinyint(1) NOT NULL DEFAULT '1',
  `lastActivityDate` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name_UNIQUE` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=492565 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2018-01-08 16:42:01
