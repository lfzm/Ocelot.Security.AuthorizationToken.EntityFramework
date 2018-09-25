
SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for ocelot_securitytoken
-- ----------------------------
DROP TABLE IF EXISTS `ocelot_securitytoken`;
CREATE TABLE `ocelot_securitytoken` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Token` varchar(2000) NOT NULL,
  `Expiration` datetime(6) NOT NULL,
  `WarnInfo` varchar(1000) DEFAULT NULL,
  `AddTime` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
