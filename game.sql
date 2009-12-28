-- phpMyAdmin SQL Dump
-- version 3.2.0.1
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: Dec 28, 2009 at 11:57 AM
-- Server version: 5.1.37
-- PHP Version: 5.3.0

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Database: `game`
--

-- --------------------------------------------------------

--
-- Table structure for table `active_actions`
--

DROP TABLE IF EXISTS `active_actions`;
CREATE TABLE `active_actions` (
  `id` smallint(5) unsigned NOT NULL,
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

-- --------------------------------------------------------

--
-- Table structure for table `battles`
--

DROP TABLE IF EXISTS `battles`;
CREATE TABLE `battles` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `city_id` int(10) unsigned NOT NULL,
  `created` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `city_id` (`city_id`,`created`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `battle_managers`
--

DROP TABLE IF EXISTS `battle_managers`;
CREATE TABLE `battle_managers` (
  `city_id` int(10) unsigned NOT NULL,
  `battle_id` int(10) unsigned NOT NULL,
  `battle_started` tinyint(1) NOT NULL,
  `round` int(10) unsigned NOT NULL,
  `turn` int(10) unsigned NOT NULL,
  `stamina` smallint(6) unsigned NOT NULL,
  `report_flag` tinyint(1) NOT NULL,
  `report_started` tinyint(1) NOT NULL,
  `report_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `battle_reports`
--

DROP TABLE IF EXISTS `battle_reports`;
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
) ENGINE=InnoDB  DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `battle_report_objects`
--

DROP TABLE IF EXISTS `battle_report_objects`;
CREATE TABLE `battle_report_objects` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `battle_report_troop_id` int(10) unsigned NOT NULL,
  `type` smallint(5) unsigned NOT NULL,
  `level` tinyint(3) unsigned NOT NULL,
  `hp` smallint(5) unsigned NOT NULL,
  `count` mediumint(8) unsigned NOT NULL,
  `damage_taken` int(10) unsigned NOT NULL,
  `damage_dealt` int(10) unsigned NOT NULL,
  `formation_type` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`id`),
  KEY `battle_report_troop_id` (`battle_report_troop_id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `battle_report_troops`
--

DROP TABLE IF EXISTS `battle_report_troops`;
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
  KEY `combat_object_id` (`group_id`),
  KEY `state` (`state`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `chain_actions`
--

DROP TABLE IF EXISTS `chain_actions`;
CREATE TABLE `chain_actions` (
  `id` smallint(5) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `object_id` int(10) unsigned NOT NULL,
  `type` int(11) NOT NULL,
  `current_action_id` smallint(6) unsigned DEFAULT NULL,
  `chain_callback` varchar(32) NOT NULL,
  `chain_state` tinyint(3) unsigned NOT NULL,
  `is_visible` tinyint(1) NOT NULL,
  `properties` text,
  PRIMARY KEY (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `cities`
--

DROP TABLE IF EXISTS `cities`;
CREATE TABLE `cities` (
  `id` int(10) unsigned NOT NULL,
  `player_id` int(10) unsigned NOT NULL,
  `name` varchar(32) NOT NULL,
  `gold` int(11) NOT NULL,
  `wood` int(11) NOT NULL,
  `iron` int(11) NOT NULL,
  `crop` int(11) NOT NULL,
  `labor` int(11) NOT NULL,
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
  KEY `player_id` (`player_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `combat_structures`
--

DROP TABLE IF EXISTS `combat_structures`;
CREATE TABLE `combat_structures` (
  `id` int(10) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `last_round` int(10) unsigned NOT NULL,
  `rounds_participated` int(10) NOT NULL,
  `damage_dealt` int(11) NOT NULL,
  `damage_received` int(11) NOT NULL,
  `group_id` int(10) unsigned NOT NULL,
  `structure_city_id` int(10) unsigned NOT NULL,
  `structure_id` int(10) unsigned NOT NULL,
  `hp` mediumint(8) unsigned NOT NULL,
  `type` smallint(5) unsigned NOT NULL,
  `level` tinyint(3) unsigned NOT NULL,
  `max_hp` smallint(5) unsigned NOT NULL,
  `attack` tinyint(3) unsigned NOT NULL,
  `defense` tinyint(3) unsigned NOT NULL,
  `range` tinyint(3) unsigned NOT NULL,
  `stealth` tinyint(3) unsigned NOT NULL,
  `speed` tinyint(3) unsigned NOT NULL,
  `reward` smallint(5) unsigned NOT NULL,
  PRIMARY KEY (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `combat_units`
--

DROP TABLE IF EXISTS `combat_units`;
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
  `troop_stub_city_id` int(10) unsigned NOT NULL,
  `troop_stub_id` tinyint(3) unsigned NOT NULL,
  `is_local` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `market`
--

DROP TABLE IF EXISTS `market`;
CREATE TABLE `market` (
  `resource_type` tinyint(3) unsigned NOT NULL,
  `incoming` int(10) NOT NULL,
  `outgoing` int(10) NOT NULL,
  `price` int(10) NOT NULL,
  `quantity_per_change` int(10) NOT NULL,
  PRIMARY KEY (`resource_type`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `messages`
--

DROP TABLE IF EXISTS `messages`;
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
  KEY `recipient_player_id` (`recipient_player_id`),
  KEY `sender_player_id` (`sender_player_id`),
  KEY `created` (`created`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `notifications`
--

DROP TABLE IF EXISTS `notifications`;
CREATE TABLE `notifications` (
  `city_id` int(11) unsigned NOT NULL,
  `object_id` int(11) unsigned NOT NULL,
  `action_id` smallint(5) unsigned NOT NULL,
  PRIMARY KEY (`city_id`,`object_id`,`action_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `notifications_list`
--

DROP TABLE IF EXISTS `notifications_list`;
CREATE TABLE `notifications_list` (
  `city_id` int(10) unsigned NOT NULL,
  `object_id` int(10) unsigned NOT NULL,
  `action_id` smallint(5) unsigned NOT NULL,
  `subscription_city_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`city_id`,`object_id`,`action_id`,`subscription_city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `passive_actions`
--

DROP TABLE IF EXISTS `passive_actions`;
CREATE TABLE `passive_actions` (
  `id` smallint(5) unsigned NOT NULL,
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

-- --------------------------------------------------------

--
-- Table structure for table `players`
--

DROP TABLE IF EXISTS `players`;
CREATE TABLE `players` (
  `id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `facebook_id` int(11) DEFAULT NULL,
  `name` varchar(64) NOT NULL,
  `password` varchar(64) NOT NULL DEFAULT '8a9719ce5a3aa86d1ccb00047497291274f7feae',
  `email_address` varchar(256) NOT NULL,
  `login_key` varchar(64) DEFAULT NULL,
  `login_key_date` datetime DEFAULT NULL,
  `session_id` varchar(128) NOT NULL,
  `deleted` tinyint(4) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `email` (`email_address`),
  KEY `login_key` (`login_key`),
  KEY `session_id` (`session_id`),
  KEY `facebook_id` (`facebook_id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `reference_stubs`
--

DROP TABLE IF EXISTS `reference_stubs`;
CREATE TABLE `reference_stubs` (
  `id` smallint(5) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `object_id` int(10) unsigned NOT NULL,
  `action_id` smallint(5) unsigned NOT NULL,
  `is_active` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `reported_objects`
--

DROP TABLE IF EXISTS `reported_objects`;
CREATE TABLE `reported_objects` (
  `city_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `reported_objects_list`
--

DROP TABLE IF EXISTS `reported_objects_list`;
CREATE TABLE `reported_objects_list` (
  `city_id` int(10) unsigned NOT NULL,
  `combat_object_id` int(10) unsigned NOT NULL,
  KEY `city_id` (`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `reported_troops`
--

DROP TABLE IF EXISTS `reported_troops`;
CREATE TABLE `reported_troops` (
  `city_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `reported_troops_list`
--

DROP TABLE IF EXISTS `reported_troops_list`;
CREATE TABLE `reported_troops_list` (
  `city_id` int(10) unsigned NOT NULL,
  `combat_troop_id` int(10) unsigned NOT NULL,
  `troop_stub_city_id` int(10) unsigned NOT NULL,
  `troop_stub_id` tinyint(3) unsigned NOT NULL,
  KEY `city_id` (`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `structures`
--

DROP TABLE IF EXISTS `structures`;
CREATE TABLE `structures` (
  `id` int(11) unsigned NOT NULL,
  `city_id` int(11) unsigned NOT NULL,
  `x` int(11) unsigned NOT NULL,
  `y` int(11) unsigned NOT NULL,
  `hp` smallint(5) unsigned NOT NULL,
  `type` smallint(6) unsigned NOT NULL,
  `level` tinyint(3) unsigned NOT NULL,
  `labor` tinyint(3) unsigned NOT NULL,
  `state` tinyint(3) unsigned NOT NULL,
  `state_parameters` text NOT NULL,
  PRIMARY KEY (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `structure_properties`
--

DROP TABLE IF EXISTS `structure_properties`;
CREATE TABLE `structure_properties` (
  `city_id` int(10) unsigned NOT NULL,
  `structure_id` int(10) unsigned NOT NULL,
  UNIQUE KEY `city_id_2` (`city_id`,`structure_id`),
  KEY `city_id` (`city_id`),
  KEY `structure_id` (`structure_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `structure_properties_list`
--

DROP TABLE IF EXISTS `structure_properties_list`;
CREATE TABLE `structure_properties_list` (
  `city_id` int(10) unsigned NOT NULL,
  `structure_id` int(10) unsigned NOT NULL,
  `name` varchar(16) NOT NULL,
  `value` varchar(32) NOT NULL,
  `datatype` tinyint(4) unsigned NOT NULL,
  PRIMARY KEY (`city_id`,`structure_id`,`name`),
  KEY `city_id` (`city_id`,`structure_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `system_variables`
--

DROP TABLE IF EXISTS `system_variables`;
CREATE TABLE `system_variables` (
  `name` varchar(32) NOT NULL,
  `datatype` tinyint(4) unsigned NOT NULL,
  `value` varchar(64) NOT NULL,
  PRIMARY KEY (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `technologies`
--

DROP TABLE IF EXISTS `technologies`;
CREATE TABLE `technologies` (
  `city_id` int(10) unsigned NOT NULL,
  `owner_id` int(10) unsigned NOT NULL,
  `owner_location` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`city_id`,`owner_id`,`owner_location`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `technologies_list`
--

DROP TABLE IF EXISTS `technologies_list`;
CREATE TABLE `technologies_list` (
  `city_id` int(10) unsigned NOT NULL,
  `owner_id` int(10) unsigned NOT NULL,
  `owner_location` tinyint(3) unsigned NOT NULL,
  `type` int(10) unsigned NOT NULL,
  `level` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`city_id`,`owner_id`,`owner_location`,`type`),
  KEY `city_id` (`city_id`,`owner_id`,`owner_location`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `troops`
--

DROP TABLE IF EXISTS `troops`;
CREATE TABLE `troops` (
  `id` int(10) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `troop_stub_id` tinyint(3) unsigned NOT NULL,
  `x` int(10) unsigned NOT NULL,
  `y` int(10) unsigned NOT NULL,
  `state` tinyint(3) unsigned NOT NULL,
  `state_parameters` text NOT NULL,
  PRIMARY KEY (`id`,`city_id`),
  UNIQUE KEY `city_id` (`city_id`,`troop_stub_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `troop_stubs`
--

DROP TABLE IF EXISTS `troop_stubs`;
CREATE TABLE `troop_stubs` (
  `id` tinyint(3) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `stationed_city_id` int(10) unsigned NOT NULL,
  `state` tinyint(3) unsigned NOT NULL,
  `formations` smallint(5) unsigned NOT NULL,
  PRIMARY KEY (`id`,`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `troop_stubs_list`
--

DROP TABLE IF EXISTS `troop_stubs_list`;
CREATE TABLE `troop_stubs_list` (
  `id` tinyint(3) unsigned NOT NULL,
  `city_id` int(10) unsigned NOT NULL,
  `formation_type` tinyint(3) unsigned NOT NULL,
  `type` smallint(5) unsigned NOT NULL,
  `count` smallint(5) unsigned NOT NULL,
  PRIMARY KEY (`id`,`city_id`,`formation_type`,`type`),
  KEY `city_id` (`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `troop_templates`
--

DROP TABLE IF EXISTS `troop_templates`;
CREATE TABLE `troop_templates` (
  `city_id` int(11) unsigned NOT NULL,
  `troop_stub_id` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`city_id`,`troop_stub_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `troop_templates_list`
--

DROP TABLE IF EXISTS `troop_templates_list`;
CREATE TABLE `troop_templates_list` (
  `city_id` int(11) unsigned NOT NULL,
  `troop_stub_id` tinyint(3) unsigned NOT NULL,
  `type` smallint(5) unsigned NOT NULL,
  `level` tinyint(3) unsigned NOT NULL,
  `max_hp` smallint(5) unsigned NOT NULL,
  `attack` tinyint(3) unsigned NOT NULL,
  `defense` tinyint(3) unsigned NOT NULL,
  `range` tinyint(3) unsigned NOT NULL,
  `stealth` tinyint(3) unsigned NOT NULL,
  `speed` tinyint(3) unsigned NOT NULL,
  `reward` smallint(5) unsigned NOT NULL,
  PRIMARY KEY (`city_id`,`troop_stub_id`,`type`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `unit_templates`
--

DROP TABLE IF EXISTS `unit_templates`;
CREATE TABLE `unit_templates` (
  `city_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`city_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `unit_templates_list`
--

DROP TABLE IF EXISTS `unit_templates_list`;
CREATE TABLE `unit_templates_list` (
  `city_id` int(10) unsigned NOT NULL,
  `type` smallint(5) unsigned NOT NULL,
  `level` tinyint(4) unsigned NOT NULL,
  PRIMARY KEY (`city_id`,`type`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
