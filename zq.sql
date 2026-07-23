/*
Navicat MySQL Data Transfer

Source Server         : localhost_3306
Source Server Version : 50717
Source Host           : localhost:3306
Source Database       : zq

Target Server Type    : MYSQL
Target Server Version : 50717
File Encoding         : 65001

Date: 2026-07-19 01:22:21
*/

SET FOREIGN_KEY_CHECKS=0;
-- ----------------------------
-- Table structure for `accounts`
-- ----------------------------
DROP TABLE IF EXISTS `accounts`;
CREATE TABLE `accounts` (
  `Username` char(25) NOT NULL DEFAULT '',
  `Password` char(16) DEFAULT '',
  `IP` char(15) DEFAULT '',
  `LastCheck` bigint(255) unsigned DEFAULT '0',
  `State` tinyint(5) unsigned DEFAULT '0',
  `EntityID` bigint(18) unsigned NOT NULL AUTO_INCREMENT,
  `Email` char(100) DEFAULT '',
  `Question` char(100) DEFAULT NULL,
  `answer` char(30) DEFAULT NULL,
  `Country` char(110) DEFAULT '',
  `City` char(100) DEFAULT '',
  `secretquestion` char(45) DEFAULT '',
  `realname` char(25) DEFAULT '',
  `machine` char(50) DEFAULT '',
  `lastvote` char(50) DEFAULT '',
  `mobilenumber` bigint(18) DEFAULT '0',
  `securitycode` varchar(100) DEFAULT '',
  `date` varchar(0) DEFAULT '',
  `joined` varchar(220) DEFAULT NULL,
  `Online` bigint(20) DEFAULT NULL,
  `RecoveryToken` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Username`) USING BTREE,
  UNIQUE KEY `a` (`EntityID`) USING BTREE
) ENGINE=MyISAM AUTO_INCREMENT=1000001 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of accounts
-- ----------------------------
INSERT INTO `accounts` VALUES ('666', '666', '', '0', '0', '1000001', '', null, null, '', '', '', '', '', '', '0', '', '', null, null, null);

-- ----------------------------
-- Table structure for `alert`
-- ----------------------------
DROP TABLE IF EXISTS `alert`;
CREATE TABLE `alert` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `body` text,
  `user` varchar(255) DEFAULT NULL,
  `date` varchar(255) DEFAULT NULL,
  `watch` bigint(20) DEFAULT '0',
  `link` varchar(255) DEFAULT NULL,
  `tik` bigint(20) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of alert
-- ----------------------------

-- ----------------------------
-- Table structure for `banned`
-- ----------------------------
DROP TABLE IF EXISTS `banned`;
CREATE TABLE `banned` (
  `UID` varchar(16) NOT NULL,
  `username` varchar(16) NOT NULL DEFAULT '',
  `Hours` bigint(18) unsigned NOT NULL DEFAULT '0',
  `StartBan` bigint(255) unsigned NOT NULL DEFAULT '0',
  `Reason` varchar(255) NOT NULL DEFAULT '',
  PRIMARY KEY (`UID`) USING BTREE
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of banned
-- ----------------------------

-- ----------------------------
-- Table structure for `bannedmac`
-- ----------------------------
DROP TABLE IF EXISTS `bannedmac`;
CREATE TABLE `bannedmac` (
  `username` varchar(16) NOT NULL,
  `MacID` varchar(16) NOT NULL DEFAULT '0000000000000000',
  PRIMARY KEY (`username`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of bannedmac
-- ----------------------------
INSERT INTO `bannedmac` VALUES ('aimbot', 'E0D55EC84476');

-- ----------------------------
-- Table structure for `category`
-- ----------------------------
DROP TABLE IF EXISTS `category`;
CREATE TABLE `category` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of category
-- ----------------------------

-- ----------------------------
-- Table structure for `comment`
-- ----------------------------
DROP TABLE IF EXISTS `comment`;
CREATE TABLE `comment` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `comment` text,
  `post` bigint(20) DEFAULT NULL,
  `user` varchar(255) DEFAULT NULL,
  `date` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of comment
-- ----------------------------

-- ----------------------------
-- Table structure for `configuration`
-- ----------------------------
DROP TABLE IF EXISTS `configuration`;
CREATE TABLE `configuration` (
  `EntityID` int(11) NOT NULL,
  `LastChar` varchar(255) DEFAULT NULL,
  `Online` varchar(255) DEFAULT NULL,
  `GWWinner` varchar(255) DEFAULT NULL,
  `serveronline` int(11) DEFAULT NULL,
  PRIMARY KEY (`EntityID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of configuration
-- ----------------------------

-- ----------------------------
-- Table structure for `cpanel`
-- ----------------------------
DROP TABLE IF EXISTS `cpanel`;
CREATE TABLE `cpanel` (
  `Website_Name` varchar(18) NOT NULL,
  `Website_url` text,
  `Domain` text,
  `date` varchar(255) DEFAULT NULL,
  `Time` varchar(255) DEFAULT NULL,
  `Email` text,
  `password` text,
  `Host` text,
  `Port` text,
  `SMTPSecure` text,
  `mate` longtext,
  `sitemap` varchar(255) DEFAULT NULL,
  `GM` bigint(20) DEFAULT NULL,
  `codegm` bigint(20) NOT NULL,
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `Transfer` varchar(255) DEFAULT NULL,
  `version` varchar(255) DEFAULT NULL,
  `max_execution` bigint(20) DEFAULT NULL,
  `King` bigint(20) DEFAULT NULL,
  `prince` bigint(20) DEFAULT NULL,
  `Doke` bigint(20) DEFAULT NULL,
  `limits` bigint(20) DEFAULT NULL,
  `boy` bigint(20) DEFAULT NULL,
  `girl` bigint(20) DEFAULT NULL,
  `language` varchar(255) DEFAULT NULL,
  `paypal_true` varchar(255) DEFAULT NULL,
  `email_paypal` varchar(255) DEFAULT NULL,
  `logo` varchar(255) DEFAULT NULL,
  `card_active` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of cpanel
-- ----------------------------
INSERT INTO `cpanel` VALUES ('ClassicCO', 'https://google.com', 'https://google.com', '', '', 'admin@google.com.net', 'black123456', 'smtp.gmail.com', '465', 'ssl', 'conquer v 5517', 'false', '4', '1231', '1', 'False', 'TITANIUM', '60', '3', '15', '30', '10', '7', '2', 'Einglish', 'true', '', '0', 'True');

-- ----------------------------
-- Table structure for `downloads`
-- ----------------------------
DROP TABLE IF EXISTS `downloads`;
CREATE TABLE `downloads` (
  `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `img` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `size` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `Type` int(11) NOT NULL,
  `link` text COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of downloads
-- ----------------------------
INSERT INTO `downloads` VALUES ('1', 'Mega', '', '540', '1', 'https://mega.nz');
INSERT INTO `downloads` VALUES ('2', 'MediaFire', '', '540', '1', 'https://www.mediafire.com');
INSERT INTO `downloads` VALUES ('3', 'GoogleDrive', '', '540', '1', 'https://drive.google.com');

-- ----------------------------
-- Table structure for `email`
-- ----------------------------
DROP TABLE IF EXISTS `email`;
CREATE TABLE `email` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `uid` bigint(20) DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  `statues` bigint(20) DEFAULT '0',
  `Date` varchar(255) DEFAULT NULL,
  `kay` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of email
-- ----------------------------

-- ----------------------------
-- Table structure for `eventflags`
-- ----------------------------
DROP TABLE IF EXISTS `eventflags`;
CREATE TABLE `eventflags` (
  `SS_FBTop` int(15) NOT NULL,
  `SpeedWarTop` int(15) NOT NULL,
  `SpecialTop` int(15) NOT NULL,
  `KingTop` int(15) NOT NULL,
  `PrinceTop` int(15) NOT NULL,
  `DukeTop` int(15) NOT NULL,
  `EarlTop` int(15) NOT NULL,
  `Owner` varchar(15) NOT NULL,
  PRIMARY KEY (`Owner`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of eventflags
-- ----------------------------

-- ----------------------------
-- Table structure for `eventprize`
-- ----------------------------
DROP TABLE IF EXISTS `eventprize`;
CREATE TABLE `eventprize` (
  `SS_FBCps` bigint(255) NOT NULL,
  `SpeedWarCps` bigint(255) NOT NULL,
  `Top_SpecialCps` bigint(255) NOT NULL,
  `KingCps` bigint(255) NOT NULL,
  `PrinceCps` bigint(255) NOT NULL,
  `DukeCps` bigint(255) NOT NULL,
  `EarlCps` bigint(255) NOT NULL,
  `Owner` varchar(15) NOT NULL,
  PRIMARY KEY (`Owner`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of eventprize
-- ----------------------------

-- ----------------------------
-- Table structure for `events`
-- ----------------------------
DROP TABLE IF EXISTS `events`;
CREATE TABLE `events` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  `start` varchar(255) DEFAULT NULL,
  `end` varchar(255) DEFAULT NULL,
  `month` varchar(255) DEFAULT NULL,
  `days` bigint(255) DEFAULT NULL,
  `Hours` varchar(20) DEFAULT NULL,
  `type_event` bigint(20) DEFAULT '0',
  `Prize` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of events
-- ----------------------------

-- ----------------------------
-- Table structure for `guildwar`
-- ----------------------------
DROP TABLE IF EXISTS `guildwar`;
CREATE TABLE `guildwar` (
  `HourOff` tinyint(4) DEFAULT NULL,
  `FlameActive` tinyint(4) DEFAULT NULL,
  `Owner` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of guildwar
-- ----------------------------

-- ----------------------------
-- Table structure for `items`
-- ----------------------------
DROP TABLE IF EXISTS `items`;
CREATE TABLE `items` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user` varchar(255) DEFAULT NULL,
  `Date` varchar(255) DEFAULT NULL,
  `item` varchar(255) DEFAULT NULL,
  `quantity` varchar(255) DEFAULT NULL,
  `price` varchar(255) DEFAULT NULL,
  `code` varchar(20) DEFAULT NULL,
  `item_order` varchar(255) DEFAULT NULL,
  `staute` int(11) DEFAULT '0',
  `id_store` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of items
-- ----------------------------

-- ----------------------------
-- Table structure for `likes`
-- ----------------------------
DROP TABLE IF EXISTS `likes`;
CREATE TABLE `likes` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `userid` varchar(255) DEFAULT NULL,
  `postid` bigint(20) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of likes
-- ----------------------------

-- ----------------------------
-- Table structure for `log_payments`
-- ----------------------------
DROP TABLE IF EXISTS `log_payments`;
CREATE TABLE `log_payments` (
  `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `user` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `name` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  `log` text COLLATE utf8mb4_unicode_ci,
  `username` text COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of log_payments
-- ----------------------------

-- ----------------------------
-- Table structure for `logs`
-- ----------------------------
DROP TABLE IF EXISTS `logs`;
CREATE TABLE `logs` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `data` varchar(5000) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of logs
-- ----------------------------

-- ----------------------------
-- Table structure for `marketitems`
-- ----------------------------
DROP TABLE IF EXISTS `marketitems`;
CREATE TABLE `marketitems` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `PlayerName` varchar(50) NOT NULL,
  `ItemName` varchar(100) NOT NULL,
  `Price` int(11) NOT NULL,
  `Timestamp` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Avatar` smallint(6) DEFAULT '0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of marketitems
-- ----------------------------

-- ----------------------------
-- Table structure for `migrations`
-- ----------------------------
DROP TABLE IF EXISTS `migrations`;
CREATE TABLE `migrations` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `migration` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `batch` int(11) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of migrations
-- ----------------------------

-- ----------------------------
-- Table structure for `mined_items`
-- ----------------------------
DROP TABLE IF EXISTS `mined_items`;
CREATE TABLE `mined_items` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `player_name` varchar(255) NOT NULL,
  `item_name` varchar(255) NOT NULL,
  `mined_at` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of mined_items
-- ----------------------------

-- ----------------------------
-- Table structure for `news`
-- ----------------------------
DROP TABLE IF EXISTS `news`;
CREATE TABLE `news` (
  `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `title` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `img` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `content` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `type` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `byGM` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of news
-- ----------------------------

-- ----------------------------
-- Table structure for `onlineplayers`
-- ----------------------------
DROP TABLE IF EXISTS `onlineplayers`;
CREATE TABLE `onlineplayers` (
  `Online` int(255) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Online`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of onlineplayers
-- ----------------------------
INSERT INTO `onlineplayers` VALUES ('0');

-- ----------------------------
-- Table structure for `orders`
-- ----------------------------
DROP TABLE IF EXISTS `orders`;
CREATE TABLE `orders` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user` varchar(255) DEFAULT NULL,
  `date` varchar(255) DEFAULT NULL,
  `code` varchar(255) DEFAULT NULL,
  `statue` varchar(255) DEFAULT '0',
  `IP` varchar(255) DEFAULT NULL,
  `methed` varchar(255) DEFAULT NULL,
  `price` float(10,0) DEFAULT NULL,
  `mount` bigint(20) DEFAULT '0',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of orders
-- ----------------------------

-- ----------------------------
-- Table structure for `patches`
-- ----------------------------
DROP TABLE IF EXISTS `patches`;
CREATE TABLE `patches` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `type` varchar(255) DEFAULT NULL,
  `Issuance` varchar(255) DEFAULT NULL,
  `link` text,
  `size` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of patches
-- ----------------------------

-- ----------------------------
-- Table structure for `payments`
-- ----------------------------
DROP TABLE IF EXISTS `payments`;
CREATE TABLE `payments` (
  `id` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `username` varchar(11) COLLATE utf8mb4_unicode_ci NOT NULL,
  `txn_id` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `founds` int(11) DEFAULT NULL,
  `claimed` int(11) NOT NULL DEFAULT '0',
  `item_name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `payment_gross` double(8,2) NOT NULL,
  `mc_gross` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `payer_id` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `payment_status` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `points` int(11) NOT NULL,
  `token` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Date` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of payments
-- ----------------------------

-- ----------------------------
-- Table structure for `phone`
-- ----------------------------
DROP TABLE IF EXISTS `phone`;
CREATE TABLE `phone` (
  `id` int(255) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  `methed` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of phone
-- ----------------------------

-- ----------------------------
-- Table structure for `playerdata`
-- ----------------------------
DROP TABLE IF EXISTS `playerdata`;
CREATE TABLE `playerdata` (
  `UID` int(11) NOT NULL,
  `Name` varchar(100) DEFAULT NULL,
  `Class` tinyint(4) DEFAULT NULL,
  `Avatar` smallint(6) DEFAULT NULL,
  `Map` int(11) DEFAULT NULL,
  `X` smallint(6) DEFAULT NULL,
  `Y` smallint(6) DEFAULT NULL,
  `CountVote` int(11) DEFAULT NULL,
  `Agility` smallint(6) DEFAULT NULL,
  `Strength` smallint(6) DEFAULT NULL,
  `Spirit` smallint(6) DEFAULT NULL,
  `Vitality` smallint(6) DEFAULT NULL,
  `Atributes` smallint(6) DEFAULT NULL,
  `ConquerPoints` int(11) DEFAULT NULL,
  `Money` int(11) DEFAULT NULL,
  `ExpireVip` datetime DEFAULT NULL,
  `VotePoints` int(11) DEFAULT NULL,
  `HeavenBlessing` int(11) DEFAULT NULL,
  `DbKilled` int(11) DEFAULT NULL,
  `Drop_Meteors` int(11) DEFAULT NULL,
  `Drop_Stone` int(11) DEFAULT NULL,
  PRIMARY KEY (`UID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of playerdata
-- ----------------------------

-- ----------------------------
-- Table structure for `posts`
-- ----------------------------
DROP TABLE IF EXISTS `posts`;
CREATE TABLE `posts` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` text,
  `body` text,
  `date` datetime DEFAULT NULL,
  `img` text,
  `view` bigint(20) DEFAULT '0',
  `likes` bigint(20) DEFAULT '0',
  `background` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of posts
-- ----------------------------

-- ----------------------------
-- Table structure for `reply_tickets`
-- ----------------------------
DROP TABLE IF EXISTS `reply_tickets`;
CREATE TABLE `reply_tickets` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  `id_tik` bigint(20) DEFAULT NULL,
  `body` text,
  `Date` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of reply_tickets
-- ----------------------------

-- ----------------------------
-- Table structure for `resgates`
-- ----------------------------
DROP TABLE IF EXISTS `resgates`;
CREATE TABLE `resgates` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `PlayerID` int(11) NOT NULL,
  `IP` varchar(45) NOT NULL,
  `ActionType` varchar(20) NOT NULL,
  `ClaimDate` datetime NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `IP` (`IP`,`ActionType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of resgates
-- ----------------------------

-- ----------------------------
-- Table structure for `send_mail`
-- ----------------------------
DROP TABLE IF EXISTS `send_mail`;
CREATE TABLE `send_mail` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `uid` bigint(20) DEFAULT NULL,
  `code` varchar(255) DEFAULT NULL,
  `date` varchar(255) DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of send_mail
-- ----------------------------

-- ----------------------------
-- Table structure for `servercontrol`
-- ----------------------------
DROP TABLE IF EXISTS `servercontrol`;
CREATE TABLE `servercontrol` (
  `NormalDb_Drop` bigint(8) NOT NULL,
  `VipDb_Drop` bigint(8) NOT NULL,
  `Vip_Drop_Meteors` bigint(8) NOT NULL,
  `Normal_Drop_Meteors` bigint(8) NOT NULL,
  `Vip_Drop_Stone` bigint(8) NOT NULL,
  `Normal_Drop_Stone` bigint(8) NOT NULL,
  `Owner` varchar(15) NOT NULL,
  `Max_DragonBall_Normal` tinyint(5) NOT NULL,
  `Max_Meteors_Normal` tinyint(5) NOT NULL,
  `Max_Stone_Normal` tinyint(5) NOT NULL,
  `Max_DragonBall_Vip` tinyint(5) NOT NULL,
  `Max_Meteors_Vip` tinyint(5) NOT NULL,
  `Max_Stone_Vip` tinyint(5) NOT NULL,
  PRIMARY KEY (`Owner`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of servercontrol
-- ----------------------------
INSERT INTO `servercontrol` VALUES ('4800', '3700', '905', '1220', '2635', '2755', '', '1', '1', '1', '1', '1', '1');

-- ----------------------------
-- Table structure for `servers`
-- ----------------------------
DROP TABLE IF EXISTS `servers`;
CREATE TABLE `servers` (
  `Name` varchar(16) CHARACTER SET utf8 NOT NULL DEFAULT '',
  `IP` varchar(16) CHARACTER SET utf8 DEFAULT NULL,
  `Port` int(16) unsigned DEFAULT NULL,
  `TransferKey` varchar(64) CHARACTER SET latin1 COLLATE latin1_general_cs NOT NULL,
  `TransferSalt` varchar(64) CHARACTER SET latin1 COLLATE latin1_general_cs NOT NULL,
  PRIMARY KEY (`Name`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of servers
-- ----------------------------
INSERT INTO `servers` VALUES ('Emulator', '192.168.0.109', '5816', 'EypKhLvYJ3zdLCTyz9Ak8RAgM78tY5F32b7CUXDuLDJDFBH8H67BWy9QThmaN5VS', 'MyqVgBf3ytALHWLXbJxSUX4uFEu3Xmz2UAY9sTTm8AScB7Kk2uwqDSnuNJske4BJ');

-- ----------------------------
-- Table structure for `sitemap`
-- ----------------------------
DROP TABLE IF EXISTS `sitemap`;
CREATE TABLE `sitemap` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `url` varchar(255) DEFAULT NULL,
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of sitemap
-- ----------------------------

-- ----------------------------
-- Table structure for `social`
-- ----------------------------
DROP TABLE IF EXISTS `social`;
CREATE TABLE `social` (
  `facebook` varchar(255) NOT NULL,
  `Youtube` varchar(255) DEFAULT NULL,
  `chat` text,
  `DISCORD` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of social
-- ----------------------------

-- ----------------------------
-- Table structure for `socket_attempts`
-- ----------------------------
DROP TABLE IF EXISTS `socket_attempts`;
CREATE TABLE `socket_attempts` (
  `player_id` int(11) NOT NULL,
  `item_uid` int(11) NOT NULL,
  `meteor_attempts` int(11) DEFAULT '0',
  PRIMARY KEY (`player_id`,`item_uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of socket_attempts
-- ----------------------------

-- ----------------------------
-- Table structure for `store`
-- ----------------------------
DROP TABLE IF EXISTS `store`;
CREATE TABLE `store` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `item` varchar(255) DEFAULT NULL,
  `price` float(10,0) DEFAULT NULL,
  `mount` bigint(20) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of store
-- ----------------------------
INSERT INTO `store` VALUES ('1', '1', '1', '1');

-- ----------------------------
-- Table structure for `tickets`
-- ----------------------------
DROP TABLE IF EXISTS `tickets`;
CREATE TABLE `tickets` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  `date` varchar(255) DEFAULT NULL,
  `title` varchar(255) DEFAULT NULL,
  `body` text,
  `Department` varchar(255) DEFAULT NULL,
  `case_ti` int(11) DEFAULT '0',
  `reply` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of tickets
-- ----------------------------

-- ----------------------------
-- Table structure for `vip_claims`
-- ----------------------------
DROP TABLE IF EXISTS `vip_claims`;
CREATE TABLE `vip_claims` (
  `player_id` int(10) unsigned NOT NULL,
  `ip` varchar(45) NOT NULL,
  `claim_date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY `ip` (`ip`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of vip_claims
-- ----------------------------

-- ----------------------------
-- Table structure for `vodacard`
-- ----------------------------
DROP TABLE IF EXISTS `vodacard`;
CREATE TABLE `vodacard` (
  `Email` varchar(50) CHARACTER SET utf8 DEFAULT NULL,
  `fbID` varchar(50) CHARACTER SET utf8 DEFAULT NULL,
  `cardNum` varchar(50) CHARACTER SET utf8 DEFAULT NULL,
  `hmCash` varchar(50) CHARACTER SET utf8 DEFAULT NULL,
  `phNum` varchar(50) CHARACTER SET utf8 DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Records of vodacard
-- ----------------------------

-- ----------------------------
-- Table structure for `votesystem`
-- ----------------------------
DROP TABLE IF EXISTS `votesystem`;
CREATE TABLE `votesystem` (
  `Id` int(11) NOT NULL,
  `Ip` varchar(45) NOT NULL,
  `Timestamp` datetime NOT NULL,
  `VotePoints` int(11) DEFAULT '1',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of votesystem
-- ----------------------------

-- ----------------------------
-- Table structure for `vtm_comments`
-- ----------------------------
DROP TABLE IF EXISTS `vtm_comments`;
CREATE TABLE `vtm_comments` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Comment` text CHARACTER SET utf8,
  `Date` time DEFAULT NULL,
  `link_news` int(11) DEFAULT NULL,
  `link_user` int(11) DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE,
  KEY `UserNews` (`link_news`) USING BTREE,
  KEY `UserComment` (`link_user`) USING BTREE,
  CONSTRAINT `vtm_comments_ibfk_1` FOREIGN KEY (`link_user`) REFERENCES `accounts` (`UID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `vtm_comments_ibfk_2` FOREIGN KEY (`link_news`) REFERENCES `vtm_newshome` (`ID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1 ROW_FORMAT=COMPACT;

-- ----------------------------
-- Records of vtm_comments
-- ----------------------------

-- ----------------------------
-- Table structure for `vtm_newshome`
-- ----------------------------
DROP TABLE IF EXISTS `vtm_newshome`;
CREATE TABLE `vtm_newshome` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `post_title` varchar(255) DEFAULT NULL,
  `post_content` longtext,
  `post_image` varchar(255) DEFAULT NULL,
  `post_date` datetime DEFAULT NULL,
  `post_views` int(11) DEFAULT '0',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1 ROW_FORMAT=COMPACT;

-- ----------------------------
-- Records of vtm_newshome
-- ----------------------------
INSERT INTO `vtm_newshome` VALUES ('1', 'Patch 4267', 'Blackout - (September 4, 2026)\n\n\n', 'no-image.png', '2025-09-08 15:16:29', '1');

-- ----------------------------
-- Procedure structure for `delete_character`
-- ----------------------------
DROP PROCEDURE IF EXISTS `delete_character`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `delete_character`(IN p_EntityID BIGINT)
BEGIN
  -- Apaga personagem principal
  DELETE FROM entities WHERE UID = p_EntityID;

  -- Apaga inventário
  DELETE FROM items WHERE OwnerUID = p_EntityID;

  -- Apaga baú
  DELETE FROM warehouse WHERE OwnerUID = p_EntityID;

  -- Remove da guilda
  DELETE FROM guildmembers WHERE EntityID = p_EntityID;

  -- Apaga amigos/inimigos
  DELETE FROM friends WHERE EntityID = p_EntityID;
  DELETE FROM enemies WHERE EntityID = p_EntityID;

  -- Apaga magias/skills
  DELETE FROM magics WHERE OwnerUID = p_EntityID;
  DELETE FROM skills WHERE OwnerUID = p_EntityID;

  -- Opcional: resetar vínculo na tabela accounts
  UPDATE accounts SET EntityID = NULL WHERE EntityID = p_EntityID;
END
;;
DELIMITER ;
