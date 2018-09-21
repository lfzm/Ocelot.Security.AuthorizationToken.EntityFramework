/*
Navicat MySQL Data Transfer

Source Server         : 测试服务器
Source Server Version : 50720
Source Host           : 120.78.175.212:3306
Source Database       : zop_uc

Target Server Type    : MYSQL
Target Server Version : 50720
File Encoding         : 65001

Date: 2018-09-21 16:28:53
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for ocelot_securitytoken
-- ----------------------------
DROP TABLE IF EXISTS `ocelot_securitytoken`;
CREATE TABLE `ocelot_securitytoken` (
  `Token` varchar(1000) NOT NULL,
  `Expiration` datetime(6) NOT NULL,
  `WarnInfo` varchar(1000) DEFAULT NULL,
  `AddTime` datetime(6) NOT NULL DEFAULT '2018-09-21 16:27:43.615000',
  PRIMARY KEY (`Token`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
