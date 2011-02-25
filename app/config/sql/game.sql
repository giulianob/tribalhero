-- MySQL dump 10.13  Distrib 5.1.41, for Win32 (ia32)
--
-- Host: localhost    Database: game
-- ------------------------------------------------------
-- Server version	5.1.41

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
-- Table structure for table `active_actions`
--

DROP TABLE IF EXISTS `active_actions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `active_actions` (
  `id` int(10) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `object_id` int(10) unsigned NOT NULL,
  `type` int(10) NOT NULL,
  `worker_type` int(11) NOT NULL,
  `worker_index` tinyint(3) unsigned NOT NULL,
  `count` smallint(9) unsigned NOT NULL,
  `begin_time` datetime NOT NULL,
  `next_time` datetime NOT NULL,
  `end_time` datetime NOT NULL,
  `properties` text,
  PRIMARY KEY (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `battle_managers`
--

DROP TABLE IF EXISTS `battle_managers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `battle_managers` (
  `city_id` int(10) unsigned NOT NULL,
  `battle_id` int(10) unsigned NOT NULL,
  `battle_started` tinyint(1) NOT NULL,
  `round` int(10) unsigned NOT NULL,
  `turn` int(10) unsigned NOT NULL,
  `report_flag` tinyint(1) NOT NULL,
  `report_started` tinyint(1) NOT NULL,
  `report_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `battle_report_objects`
--

DROP TABLE IF EXISTS `battle_report_objects`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `battle_report_objects` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `object_id` int(10) unsigned NOT NULL,
  `battle_report_troop_id` int(10) unsigned NOT NULL,
  `type` smallint(5) unsigned NOT NULL,
  `level` tinyint(3) unsigned NOT NULL,
  `hp` smallint(5) unsigned NOT NULL,
  `count` mediumint(8) unsigned NOT NULL,
  `damage_taken` int(10) unsigned NOT NULL,
  `damage_dealt` int(10) unsigned NOT NULL,
  `formation_type` tinyint(3) unsigned NOT NULL,
  `hits_dealt` smallint(5) unsigned NOT NULL,
  `hits_dealt_by_unit` int(10) unsigned NOT NULL,
  `hits_received` smallint(5) unsigned NOT NULL,
  PRIMARY KEY (`id`),
  KEY `battle_report_troop_id` (`battle_report_troop_id`),
  KEY `object_id` (`object_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `battle_report_troops`
--

DROP TABLE IF EXISTS `battle_report_troops`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `battle_report_troops` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `battle_report_id` int(10) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `group_id` int(10) unsigned NOT NULL,
  `troop_stub_id` tinyint(3) unsigned NOT NULL,
  `state` tinyint(3) unsigned NOT NULL,
  `is_attacker` tinyint(1) NOT NULL,
  `gold` int(10) NOT NULL,
  `crop` int(10) NOT NULL,
  `iron` int(10) NOT NULL,
  `wood` int(10) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `battle_report_id` (`battle_report_id`),
  KEY `combat_object_id` (`group_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `battle_report_views`
--

DROP TABLE IF EXISTS `battle_report_views`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `battle_report_views` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `city_id` int(10) unsigned NOT NULL,
  `troop_stub_id` tinyint(3) unsigned NOT NULL,
  `battle_id` int(10) unsigned NOT NULL,
  `group_id` int(10) unsigned NOT NULL,
  `is_attacker` tinyint(1) NOT NULL,
  `loot_crop` int(11) NOT NULL DEFAULT '0',
  `loot_wood` int(11) NOT NULL DEFAULT '0',
  `loot_iron` int(11) NOT NULL DEFAULT '0',
  `loot_gold` int(11) NOT NULL DEFAULT '0',
  `bonus_crop` int(11) NOT NULL DEFAULT '0',
  `bonus_wood` int(11) NOT NULL DEFAULT '0',
  `bonus_iron` int(11) NOT NULL DEFAULT '0',
  `bonus_gold` int(11) NOT NULL DEFAULT '0',
  `read` tinyint(1) NOT NULL,
  `created` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `battle_report_troop_id` (`group_id`),
  KEY `city_id` (`city_id`),
  KEY `battle_id` (`battle_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `battle_reports`
--

DROP TABLE IF EXISTS `battle_reports`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `battle_reports` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `created` datetime NOT NULL,
  `battle_id` int(10) unsigned NOT NULL,
  `round` smallint(5) unsigned NOT NULL,
  `turn` smallint(5) unsigned NOT NULL,
  `ready` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `battle_id` (`battle_id`),
  KEY `created` (`created`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `battles`
--

DROP TABLE IF EXISTS `battles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `battles` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `city_id` int(10) unsigned NOT NULL,
  `created` datetime NOT NULL,
  `ended` datetime DEFAULT NULL,
  `read` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `ended` (`ended`),
  KEY `created` (`created`),
  KEY `city_id` (`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `chain_actions`
--

DROP TABLE IF EXISTS `chain_actions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `chain_actions` (
  `id` int(10) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `type` int(11) NOT NULL,
  `current_action_id` int(10) unsigned DEFAULT NULL,
  `chain_callback` varchar(32) NOT NULL,
  `chain_state` tinyint(3) unsigned NOT NULL,
  `is_visible` tinyint(1) NOT NULL,
  `properties` text,
  PRIMARY KEY (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cities`
--

DROP TABLE IF EXISTS `cities`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `cities` (
  `id` int(10) unsigned NOT NULL,
  `player_id` int(10) unsigned NOT NULL,
  `name` varchar(32) NOT NULL,
  `x` int(10) unsigned NOT NULL,
  `y` int(10) unsigned NOT NULL,
  `radius` tinyint(3) unsigned NOT NULL,
  `hide_new_units` tinyint(1) NOT NULL,
  `loot_stolen` int(10) unsigned NOT NULL,
  `attack_point` int(10) NOT NULL,
  `defense_point` int(11) NOT NULL,
  `gold` int(11) NOT NULL,
  `wood` int(11) NOT NULL,
  `iron` int(11) NOT NULL,
  `crop` int(11) NOT NULL,
  `labor` int(11) NOT NULL,
  `crop_upkeep` int(11) NOT NULL,
  `gold_realize_time` datetime NOT NULL,
  `wood_realize_time` datetime NOT NULL,
  `iron_realize_time` datetime NOT NULL,
  `crop_realize_time` datetime NOT NULL,
  `labor_realize_time` datetime NOT NULL,
  `gold_production_rate` int(11) NOT NULL,
  `wood_production_rate` int(11) NOT NULL,
  `iron_production_rate` int(11) NOT NULL,
  `crop_production_rate` int(11) NOT NULL,
  `labor_production_rate` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `player_id` (`player_id`),
  KEY `loot_stolen` (`loot_stolen`),
  KEY `attack_point` (`attack_point`),
  KEY `defense_point` (`defense_point`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `combat_structures`
--

DROP TABLE IF EXISTS `combat_structures`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `combat_structures` (
  `id` int(10) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `last_round` int(10) unsigned NOT NULL,
  `rounds_participated` int(10) NOT NULL,
  `damage_dealt` int(11) NOT NULL,
  `damage_received` int(11) NOT NULL,
  `hits_dealt` smallint(6) unsigned NOT NULL,
  `hits_dealt_by_unit` int(10) unsigned NOT NULL,
  `hits_received` smallint(6) unsigned NOT NULL,
  `group_id` int(10) unsigned NOT NULL,
  `structure_city_id` int(10) unsigned NOT NULL,
  `structure_id` int(10) unsigned NOT NULL,
  `hp` mediumint(8) unsigned NOT NULL,
  `type` smallint(5) unsigned NOT NULL,
  `level` tinyint(3) unsigned NOT NULL,
  `max_hp` smallint(5) unsigned NOT NULL,
  `attack` smallint(5) unsigned NOT NULL,
  `splash` tinyint(3) unsigned NOT NULL,
  `defense` smallint(5) unsigned NOT NULL,
  `range` tinyint(3) unsigned NOT NULL,
  `stealth` tinyint(3) unsigned NOT NULL,
  `speed` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `combat_units`
--

DROP TABLE IF EXISTS `combat_units`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `combat_units` (
  `id` int(10) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `last_round` int(10) unsigned NOT NULL,
  `rounds_participated` int(10) NOT NULL,
  `damage_dealt` int(11) NOT NULL,
  `damage_received` int(11) NOT NULL,
  `group_id` int(10) unsigned NOT NULL,
  `formation_type` tinyint(3) unsigned NOT NULL,
  `level` tinyint(3) unsigned NOT NULL,
  `type` smallint(5) unsigned NOT NULL,
  `count` smallint(5) unsigned NOT NULL,
  `left_over_hp` smallint(5) unsigned NOT NULL,
  `troop_stub_city_id` int(10) unsigned NOT NULL,
  `troop_stub_id` tinyint(3) unsigned NOT NULL,
  `is_local` tinyint(1) NOT NULL,
  `damage_min_dealt` smallint(5) unsigned NOT NULL,
  `damage_max_dealt` smallint(5) unsigned NOT NULL,
  `damage_min_received` smallint(5) unsigned NOT NULL,
  `damage_max_received` smallint(5) unsigned NOT NULL,
  `hits_dealt` smallint(5) unsigned NOT NULL,
  `hits_dealt_by_unit` int(10) unsigned NOT NULL,
  `hits_received` smallint(5) unsigned NOT NULL,
  `loot_crop` int(11) NOT NULL DEFAULT '0',
  `loot_wood` int(11) NOT NULL DEFAULT '0',
  `loot_iron` int(11) NOT NULL DEFAULT '0',
  `loot_gold` int(11) NOT NULL DEFAULT '0',
  `loot_labor` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `forests`
--

DROP TABLE IF EXISTS `forests`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `forests` (
  `id` int(10) unsigned NOT NULL,
  `x` int(10) unsigned NOT NULL,
  `y` int(10) unsigned NOT NULL,
  `level` tinyint(3) unsigned NOT NULL,
  `labor` smallint(5) unsigned NOT NULL,
  `rate` float NOT NULL,
  `capacity` int(10) NOT NULL,
  `state` tinyint(3) unsigned NOT NULL,
  `state_parameters` text NOT NULL,
  `last_realize_time` datetime NOT NULL,
  `lumber` int(11) NOT NULL,
  `upkeep` int(11) NOT NULL,
  `deplete_time` datetime NOT NULL,
  `in_world` tinyint(1) NOT NULL,
  `structures` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `market`
--

DROP TABLE IF EXISTS `market`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `market` (
  `resource_type` tinyint(3) unsigned NOT NULL,
  `incoming` int(10) NOT NULL,
  `outgoing` int(10) NOT NULL,
  `price` int(10) NOT NULL,
  `quantity_per_change` int(10) NOT NULL,
  PRIMARY KEY (`resource_type`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `messages`
--

DROP TABLE IF EXISTS `messages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `messages` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `sender_player_id` int(11) NOT NULL,
  `recipient_player_id` int(11) NOT NULL,
  `subject` varchar(150) NOT NULL,
  `message` text NOT NULL,
  `sender_state` tinyint(4) NOT NULL,
  `recipient_state` tinyint(4) NOT NULL,
  `created` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `created` (`created`),
  KEY `sender_player_id` (`sender_player_id`,`sender_state`),
  KEY `recipient_player_id` (`recipient_player_id`,`recipient_state`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `notifications`
--

DROP TABLE IF EXISTS `notifications`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `notifications` (
  `city_id` int(11) unsigned NOT NULL,
  `object_id` int(11) unsigned NOT NULL,
  `action_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`city_id`,`object_id`,`action_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `notifications_list`
--

DROP TABLE IF EXISTS `notifications_list`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `notifications_list` (
  `city_id` int(10) unsigned NOT NULL,
  `object_id` int(10) unsigned NOT NULL,
  `action_id` int(10) unsigned NOT NULL,
  `subscription_city_id` int(10) unsigned NOT NULL,
  KEY `city_id` (`city_id`,`object_id`,`action_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `passive_actions`
--

DROP TABLE IF EXISTS `passive_actions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `passive_actions` (
  `id` int(10) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `object_id` int(10) unsigned NOT NULL,
  `type` int(11) NOT NULL,
  `begin_time` datetime NOT NULL,
  `next_time` datetime NOT NULL,
  `end_time` datetime NOT NULL,
  `is_chain` tinyint(1) NOT NULL,
  `is_scheduled` tinyint(1) NOT NULL,
  `is_visible` tinyint(1) NOT NULL,
  `properties` text,
  PRIMARY KEY (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `players`
--

DROP TABLE IF EXISTS `players`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `players` (
  `id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `facebook_id` int(11) DEFAULT NULL,
  `name` varchar(64) NOT NULL,
  `password` varchar(64) NOT NULL DEFAULT '8a9719ce5a3aa86d1ccb00047497291274f7feae',
  `email_address` varchar(256) NOT NULL,
  `login_key` varchar(64) DEFAULT NULL,
  `login_key_date` datetime DEFAULT NULL,
  `session_id` varchar(128) DEFAULT NULL,
  `created` datetime NOT NULL,
  `last_login` datetime NOT NULL,
  `deleted` tinyint(4) NOT NULL DEFAULT '0',
  `banned` tinyint(1) NOT NULL DEFAULT '0',
  `admin` tinyint(1) NOT NULL DEFAULT '0',
  `online` tinyint(1) NOT NULL DEFAULT '0',
  `reset_key` varchar(64) DEFAULT NULL,
  `reset_key_date` datetime NOT NULL DEFAULT '0000-00-00 00:00:00',
  PRIMARY KEY (`id`),
  KEY `email` (`email_address`),
  KEY `login_key` (`login_key`),
  KEY `session_id` (`session_id`),
  KEY `facebook_id` (`facebook_id`)
) ENGINE=InnoDB AUTO_INCREMENT=50201 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `rankings`
--

DROP TABLE IF EXISTS `rankings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `rankings` (
  `player_id` int(10) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `rank` int(11) NOT NULL,
  `type` tinyint(4) NOT NULL,
  `value` int(11) NOT NULL,
  KEY `player_id` (`player_id`),
  KEY `city_id` (`city_id`),
  KEY `type` (`type`),
  KEY `rank` (`rank`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `reference_stubs`
--

DROP TABLE IF EXISTS `reference_stubs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `reference_stubs` (
  `id` smallint(5) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `object_id` int(10) unsigned NOT NULL,
  `action_id` int(10) unsigned NOT NULL,
  `is_active` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `reported_objects`
--

DROP TABLE IF EXISTS `reported_objects`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `reported_objects` (
  `city_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `reported_objects_list`
--

DROP TABLE IF EXISTS `reported_objects_list`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `reported_objects_list` (
  `city_id` int(10) unsigned NOT NULL,
  `combat_object_id` int(10) unsigned NOT NULL,
  KEY `city_id` (`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `reported_troops`
--

DROP TABLE IF EXISTS `reported_troops`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `reported_troops` (
  `city_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `reported_troops_list`
--

DROP TABLE IF EXISTS `reported_troops_list`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `reported_troops_list` (
  `city_id` int(10) unsigned NOT NULL,
  `combat_troop_id` int(10) unsigned NOT NULL,
  `troop_stub_city_id` int(10) unsigned NOT NULL,
  `troop_stub_id` tinyint(3) unsigned NOT NULL,
  KEY `city_id` (`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `schema_migrations`
--

DROP TABLE IF EXISTS `schema_migrations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `schema_migrations` (
  `version` varchar(255) DEFAULT NULL,
  UNIQUE KEY `idx_schema_migrations_version` (`version`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `simple_objects`
--

DROP TABLE IF EXISTS `simple_objects`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `simple_objects` (
  `id` int(11) NOT NULL,
  `an_int` int(11) NOT NULL,
  `a_string` varchar(16) NOT NULL,
  `a_float` float NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COMMENT='Used in Database/DatabasePerformanceTest.cs';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `structure_properties`
--

DROP TABLE IF EXISTS `structure_properties`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `structure_properties` (
  `city_id` int(10) unsigned NOT NULL,
  `structure_id` int(10) unsigned NOT NULL,
  UNIQUE KEY `city_id_2` (`city_id`,`structure_id`),
  KEY `city_id` (`city_id`),
  KEY `structure_id` (`structure_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `structure_properties_list`
--

DROP TABLE IF EXISTS `structure_properties_list`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `structure_properties_list` (
  `city_id` int(10) unsigned NOT NULL,
  `structure_id` int(10) unsigned NOT NULL,
  `name` varchar(16) NOT NULL,
  `value` varchar(32) NOT NULL,
  `datatype` tinyint(4) unsigned NOT NULL,
  KEY `combo_index` (`city_id`,`structure_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `structures`
--

DROP TABLE IF EXISTS `structures`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `structures` (
  `id` int(11) unsigned NOT NULL,
  `city_id` int(11) unsigned NOT NULL,
  `x` int(11) unsigned NOT NULL,
  `y` int(11) unsigned NOT NULL,
  `hp` smallint(5) unsigned NOT NULL,
  `type` smallint(6) unsigned NOT NULL,
  `level` tinyint(3) unsigned NOT NULL,
  `labor` tinyint(3) unsigned NOT NULL,
  `is_blocked` tinyint(1) NOT NULL,
  `in_world` tinyint(1) NOT NULL,
  `state` tinyint(3) unsigned NOT NULL,
  `state_parameters` text NOT NULL,
  PRIMARY KEY (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `system_variables`
--

DROP TABLE IF EXISTS `system_variables`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `system_variables` (
  `name` varchar(32) NOT NULL,
  `datatype` tinyint(4) unsigned NOT NULL,
  `value` varchar(64) NOT NULL,
  PRIMARY KEY (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `technologies`
--

DROP TABLE IF EXISTS `technologies`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `technologies` (
  `city_id` int(10) unsigned NOT NULL,
  `owner_id` int(10) unsigned NOT NULL,
  `owner_location` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`city_id`,`owner_id`,`owner_location`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `technologies_list`
--

DROP TABLE IF EXISTS `technologies_list`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `technologies_list` (
  `city_id` int(10) unsigned NOT NULL,
  `owner_id` int(10) unsigned NOT NULL,
  `owner_location` tinyint(3) unsigned NOT NULL,
  `type` int(10) unsigned NOT NULL,
  `level` tinyint(3) unsigned NOT NULL,
  KEY `city_id` (`city_id`,`owner_id`,`owner_location`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `troop_stubs`
--

DROP TABLE IF EXISTS `troop_stubs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `troop_stubs` (
  `id` tinyint(3) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `stationed_city_id` int(10) unsigned NOT NULL,
  `state` tinyint(3) unsigned NOT NULL,
  `formations` smallint(5) unsigned NOT NULL,
  PRIMARY KEY (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `troop_stubs_list`
--

DROP TABLE IF EXISTS `troop_stubs_list`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `troop_stubs_list` (
  `id` tinyint(3) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `formation_type` tinyint(3) unsigned NOT NULL,
  `type` smallint(5) unsigned NOT NULL,
  `count` smallint(5) unsigned NOT NULL,
  KEY `id` (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `troop_templates`
--

DROP TABLE IF EXISTS `troop_templates`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `troop_templates` (
  `city_id` int(11) unsigned NOT NULL,
  `troop_stub_id` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`city_id`,`troop_stub_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `troop_templates_list`
--

DROP TABLE IF EXISTS `troop_templates_list`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `troop_templates_list` (
  `city_id` int(11) unsigned NOT NULL,
  `troop_stub_id` tinyint(3) unsigned NOT NULL,
  `type` smallint(5) unsigned NOT NULL,
  `level` tinyint(3) unsigned NOT NULL,
  `max_hp` smallint(5) unsigned NOT NULL,
  `attack` smallint(5) unsigned NOT NULL,
  `splash` tinyint(3) unsigned NOT NULL,
  `defense` smallint(5) unsigned NOT NULL,
  `range` tinyint(3) unsigned NOT NULL,
  `stealth` tinyint(3) unsigned NOT NULL,
  `speed` tinyint(3) unsigned NOT NULL,
  `carry` smallint(5) unsigned NOT NULL,
  KEY `city_id` (`city_id`,`troop_stub_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `troops`
--

DROP TABLE IF EXISTS `troops`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `troops` (
  `id` int(10) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `troop_stub_id` tinyint(3) unsigned NOT NULL,
  `x` int(10) unsigned NOT NULL,
  `y` int(10) unsigned NOT NULL,
  `target_x` int(10) unsigned NOT NULL,
  `target_y` int(10) unsigned NOT NULL,
  `state` tinyint(3) unsigned NOT NULL,
  `state_parameters` text NOT NULL,
  `gold` int(11) NOT NULL,
  `crop` int(11) NOT NULL,
  `iron` int(11) NOT NULL,
  `wood` int(11) NOT NULL,
  `is_blocked` tinyint(1) NOT NULL,
  `in_world` tinyint(1) NOT NULL,
  `attack_point` int(11) NOT NULL,
  `attack_radius` tinyint(3) unsigned NOT NULL,
  `speed` tinyint(3) unsigned NOT NULL,
  `stamina` smallint(6) NOT NULL,
  PRIMARY KEY (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `unit_templates`
--

DROP TABLE IF EXISTS `unit_templates`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `unit_templates` (
  `city_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `unit_templates_list`
--

DROP TABLE IF EXISTS `unit_templates_list`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `unit_templates_list` (
  `city_id` int(10) unsigned NOT NULL,
  `type` smallint(5) unsigned NOT NULL,
  `level` tinyint(4) unsigned NOT NULL,
  KEY `city_id` (`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2011-02-24 20:28:16
-- MySQL dump 10.13  Distrib 5.1.41, for Win32 (ia32)
--
-- Host: localhost    Database: game
-- ------------------------------------------------------
-- Server version	5.1.41

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
-- Dumping data for table `schema_migrations`
--

LOCK TABLES `schema_migrations` WRITE;
/*!40000 ALTER TABLE `schema_migrations` DISABLE KEYS */;
/*!40000 ALTER TABLE `schema_migrations` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2011-02-24 20:28:16
