/*
 Navicat Premium Data Transfer

 Source Server         : TEST
 Source Server Type    : MySQL
 Source Server Version : 80013
 Source Host           : localhost:3306
 Source Schema         : wcs

 Target Server Type    : MySQL
 Target Server Version : 80013
 File Encoding         : 65001

 Date: 12/09/2019 08:08:56
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for wcs_agv_info
-- ----------------------------
DROP TABLE IF EXISTS `wcs_agv_info`;
CREATE TABLE `wcs_agv_info`  (
  `ID` int(11) NOT NULL COMMENT '唯一识别码',
  `TASK_UID` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT 'WMS任务UID',
  `AGV` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT 'AGV车号',
  `PICKSTATION` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '装货点',
  `DROPSTATION` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '卸货点',
  `MAGIC` int(11) NULL DEFAULT 1 COMMENT '当前任务状态',
  `CREATION_TIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `UPDATE_TIME` timestamp(0) NULL DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_agv_info
-- ----------------------------
INSERT INTO `wcs_agv_info` VALUES (1, NULL, 'AGV01', 'FRT99', 'FRT01', 0, '2019-08-02 08:50:03', NULL);
INSERT INTO `wcs_agv_info` VALUES (2, '1907312', 'AGV01', 'FRT99', 'FRT01', 3, '2019-08-02 14:54:59', NULL);
INSERT INTO `wcs_agv_info` VALUES (13083606, NULL, 'AGV02', 'FRT99', 'FRT01', 4, '2019-08-13 08:36:22', NULL);
INSERT INTO `wcs_agv_info` VALUES (13094937, NULL, 'AGV03', 'FRT99', 'FRT03', 1, '2019-08-13 09:49:48', NULL);

-- ----------------------------
-- Table structure for wcs_command_master
-- ----------------------------
DROP TABLE IF EXISTS `wcs_command_master`;
CREATE TABLE `wcs_command_master`  (
  `WCS_NO` varchar(15) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT 'WCS单号',
  `FRT` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '固定辊台',
  `TASK_UID_1` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '任务UID_1',
  `TASK_UID_2` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '任务UID_2',
  `STEP` varchar(2) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL DEFAULT '1' COMMENT '步骤(1:组成单号；2:请求执行；3:执行中；4:结束)',
  `CREATION_TIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `UPDATE_TIME` timestamp(0) NULL DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`WCS_NO`) USING BTREE,
  INDEX `WCS_NO`(`WCS_NO`) USING BTREE,
  INDEX `WCS_NO_2`(`WCS_NO`) USING BTREE,
  INDEX `WCS_NO_3`(`WCS_NO`) USING BTREE,
  INDEX `WCS_NO_4`(`WCS_NO`) USING BTREE,
  INDEX `TASKID_1_FK`(`TASK_UID_1`) USING BTREE,
  INDEX `TASKID_2_FK`(`TASK_UID_2`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = 'WCS任务指令总控' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_command_master
-- ----------------------------
INSERT INTO `wcs_command_master` VALUES ('1908141610', 'FRT01', '1907311', '1907312', '3', '2019-08-14 16:11:12', '2019-08-14 17:06:01');
INSERT INTO `wcs_command_master` VALUES ('I190820110522', '', 'Test0820110043', NULL, '1', '2019-08-20 11:05:23', NULL);
INSERT INTO `wcs_command_master` VALUES ('I190829102122', 'FRT03', 'NW190829102101', 'NW190829102337', '2', '2019-08-29 10:21:22', '2019-08-29 10:23:39');
INSERT INTO `wcs_command_master` VALUES ('I190909165121', 'FRT01', '1907312', NULL, '1', '2019-09-09 16:51:21', NULL);

-- ----------------------------
-- Table structure for wcs_config_device
-- ----------------------------
DROP TABLE IF EXISTS `wcs_config_device`;
CREATE TABLE `wcs_config_device`  (
  `DEVICE` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '设备',
  `IP` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT 'IP地址',
  `PORT` int(11) NOT NULL COMMENT '端口',
  `FLAG` varchar(2) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '状态(N:未知；Y:可用；U:已用一个辊台；L:锁定)',
  `LOCK_WCS_NO` varchar(15) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '锁定的清单号',
  `TYPE` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '类别',
  `AREA` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '区域',
  `REMARK` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '备注说明',
  `CREATION_TIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `UPDATE_TIME` timestamp(0) NULL DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`DEVICE`) USING BTREE,
  UNIQUE INDEX `IP_UNIQUE`(`IP`) USING BTREE,
  UNIQUE INDEX `PORT_UNIQUE`(`PORT`) USING BTREE,
  UNIQUE INDEX `DEVICE_UNIQUE`(`DEVICE`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '设备资讯   ' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_config_device
-- ----------------------------
INSERT INTO `wcs_config_device` VALUES ('ABC01', '127.0.0.81', 6001, 'L', '1908141610', 'ABC', 'B01', '行车', '2019-07-04 09:58:36', NULL);
INSERT INTO `wcs_config_device` VALUES ('ABC02', '127.0.0.82', 6002, 'Y', NULL, 'ABC', 'B01', '行车', '2019-07-04 09:58:57', NULL);
INSERT INTO `wcs_config_device` VALUES ('ARF01', '127.0.0.41', 5001, 'Y', NULL, 'ARF', 'B01', '摆渡车', '2019-07-04 09:54:44', NULL);
INSERT INTO `wcs_config_device` VALUES ('ARF02', '127.0.0.42', 5002, 'Y', NULL, 'ARF', 'B01', '摆渡车', '2019-07-04 09:55:05', NULL);
INSERT INTO `wcs_config_device` VALUES ('FRT01', '127.0.0.11', 4001, 'Y', NULL, 'FRT', 'B01', '固定辊台', '2019-07-04 09:50:33', NULL);
INSERT INTO `wcs_config_device` VALUES ('FRT02', '127.0.0.12', 4002, 'Y', NULL, 'FRT', 'B01', '固定辊台', '2019-07-04 09:49:16', NULL);
INSERT INTO `wcs_config_device` VALUES ('FRT03', '127.0.0.13', 4003, 'U', NULL, 'FRT', 'B01', '固定辊台', '2019-07-04 09:51:39', NULL);
INSERT INTO `wcs_config_device` VALUES ('FRT99', '127.0.0.111', 9001, 'Y', NULL, 'FRT', 'A01', '固定辊台', '2019-07-04 09:50:33', '2019-09-05 14:46:34');
INSERT INTO `wcs_config_device` VALUES ('RGV01', '127.0.0.61', 3001, 'Y', NULL, 'RGV', 'B01', '运输车', '2019-07-04 09:55:31', '2019-09-05 11:05:42');
INSERT INTO `wcs_config_device` VALUES ('RGV02', '127.0.0.62', 3002, 'Y', NULL, 'RGV', 'B01', '运输车', '2019-07-04 09:55:46', NULL);

-- ----------------------------
-- Table structure for wcs_config_loc
-- ----------------------------
DROP TABLE IF EXISTS `wcs_config_loc`;
CREATE TABLE `wcs_config_loc`  (
  `ID` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `WMS_LOC` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT 'WMS回馈位置',
  `FRT_LOC` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '固定辊台位置',
  `AGV_LOC` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT 'AGV定位站点',
  `ARF_LOC` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '摆渡车定位',
  `RGV_LOC_1` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '运输车辊台1[内]定位',
  `RGV_LOC_2` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '运输车辊台2[外]定位',
  `ABC_LOC_TRACK` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '行车轨道定位',
  `ABC_LOC_STOCK` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '行车库存定位',
  `CREATION_TIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 9 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '依据WMS回馈位置定义各设备目的点位  ' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_config_loc
-- ----------------------------
INSERT INTO `wcs_config_loc` VALUES (1, 'A01', 'FRT99', NULL, NULL, NULL, NULL, NULL, NULL, '2019-08-02 10:03:53');
INSERT INTO `wcs_config_loc` VALUES (2, 'B01', 'FRT01', NULL, '2', NULL, NULL, NULL, NULL, '2019-07-05 15:11:30');
INSERT INTO `wcs_config_loc` VALUES (3, 'B01', 'FRT02', NULL, '3', NULL, NULL, NULL, NULL, '2019-07-05 14:54:02');
INSERT INTO `wcs_config_loc` VALUES (4, 'B01', 'FRT03', NULL, '4', NULL, NULL, NULL, NULL, '2019-07-05 14:54:15');
INSERT INTO `wcs_config_loc` VALUES (5, 'C001-001-001', NULL, NULL, NULL, '002', '001', '001-004-001', '001-001-001', '2019-07-05 15:12:36');
INSERT INTO `wcs_config_loc` VALUES (6, 'C001-001-002', NULL, NULL, NULL, '002', '001', '001-004-001', '001-001-002', '2019-07-05 15:13:01');

-- ----------------------------
-- Table structure for wcs_function_log
-- ----------------------------
DROP TABLE IF EXISTS `wcs_function_log`;
CREATE TABLE `wcs_function_log`  (
  `ID` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `FUNCTION_NAME` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '方法名',
  `REMARK` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '说明',
  `WCS_NO` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT 'WCS单号',
  `ITEM_ID` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '项目ID',
  `RESULT` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '结果（OK / NG）',
  `MESSAGE` varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '讯息',
  `CREATION_TIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 366 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for wcs_task_backup
-- ----------------------------
DROP TABLE IF EXISTS `wcs_task_backup`;
CREATE TABLE `wcs_task_backup`  (
  `ID` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `WCS_NO` varchar(15) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT 'WCS单号',
  `TASK_UID` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '任务UID',
  `FRT` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '固定辊台',
  `TASK_TYPE` varchar(2) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '任务类型(\n0:AGV运输；1:入库；\n2:出库；\n3:移仓；\n4:盘点)',
  `BARCODE` varchar(30) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '货物码',
  `W_S_LOC` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '来源货位',
  `W_D_LOC` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '目标货位',
  `BACKUP_TIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '备份时间',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 5 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for wcs_task_info
-- ----------------------------
DROP TABLE IF EXISTS `wcs_task_info`;
CREATE TABLE `wcs_task_info`  (
  `TASK_UID` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '任务UID',
  `TASK_TYPE` varchar(2) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '任务类型(\n0:AGV运输；1:入库；\n2:出库；\n3:移仓；\n4:盘点)',
  `BARCODE` varchar(30) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '货物码',
  `W_S_LOC` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '来源货位',
  `W_D_LOC` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '目标货位',
  `SITE` varchar(2) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL DEFAULT 'N' COMMENT '任务状态(\nN:未执行；W:任务中；\nY:完成；\nX:失效)',
  `CREATION_TIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `UPDATE_TIME` timestamp(0) NULL DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`TASK_UID`) USING BTREE,
  UNIQUE INDEX `TASK_UID_UNIQUE`(`TASK_UID`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = 'WCS任务资讯  ' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_task_info
-- ----------------------------
INSERT INTO `wcs_task_info` VALUES ('1907311', '1', '1907311', 'B01', 'C001-001-001', 'W', '2019-07-31 16:48:54', '2019-08-14 17:06:01');
INSERT INTO `wcs_task_info` VALUES ('1907312', '1', '1907312', 'FRT01', 'CC001-001-002', 'W', '2019-07-31 16:49:28', '2019-09-09 16:51:21');
INSERT INTO `wcs_task_info` VALUES ('NW190829102020', '2', '', 'C012-001-001', 'FRT03', 'N', '2019-08-29 10:20:20', NULL);
INSERT INTO `wcs_task_info` VALUES ('NW190829102101', '1', '123321123321', 'FRT03', 'C012-002-002', 'N', '2019-08-29 10:21:01', '2019-08-29 10:21:22');
INSERT INTO `wcs_task_info` VALUES ('NW190829102337', '1', '12332113', 'FRT03', 'C012-002-001', 'N', '2019-08-29 10:23:37', '2019-08-29 10:23:39');
INSERT INTO `wcs_task_info` VALUES ('Test0820110043', '1', '5555', '', 'C005-005-005', 'N', '2019-08-20 11:01:11', '2019-08-20 11:05:14');

-- ----------------------------
-- Table structure for wcs_task_item
-- ----------------------------
DROP TABLE IF EXISTS `wcs_task_item`;
CREATE TABLE `wcs_task_item`  (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `WCS_NO` varchar(15) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT 'WCS单号',
  `ITEM_ID` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '项目ID',
  `DEVICE` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '设备',
  `LOC_FROM` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '启动位置',
  `LOC_TO` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '目的位置',
  `STATUS` varchar(2) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL DEFAULT 'N' COMMENT '任务状态(N:不可执行；Q:请求执行；W:任务中；R:交接中；E:异常；\nY:完成；\nX:失效)',
  `CREATION_TIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `UPDATE_TIME` timestamp(0) NULL DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 37 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = 'WCS指令资讯  ' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_task_item
-- ----------------------------
INSERT INTO `wcs_task_item` VALUES (35, '1908141610', '012', NULL, NULL, '2', 'N', '2019-08-14 17:06:01', NULL);
INSERT INTO `wcs_task_item` VALUES (36, '1908141610', '022', NULL, NULL, '001', 'N', '2019-08-14 17:06:01', NULL);
INSERT INTO `wcs_task_item` VALUES (37, '1908141610', '031', 'ABC01', '0-0-0', '001-004-001', 'Q', '2019-08-14 17:06:01', '2019-09-05 14:45:29');

-- ----------------------------
-- View structure for wcs_command_v
-- ----------------------------
DROP VIEW IF EXISTS `wcs_command_v`;
CREATE ALGORITHM = UNDEFINED DEFINER = `root`@`localhost` SQL SECURITY DEFINER VIEW `wcs_command_v` AS select `a`.`WCS_NO` AS `WCS_NO`,`a`.`FRT` AS `FRT`,`a`.`STEP` AS `STEP`,`b`.`TASK_TYPE` AS `TASK_TYPE`,`a`.`CREATION_TIME` AS `CREATION_TIME`,`a`.`TASK_UID_1` AS `TASK_UID_1`,`b`.`W_S_LOC` AS `LOC_FROM_1`,`b`.`W_D_LOC` AS `LOC_TO_1`,`b`.`SITE` AS `SITE_1`,`a`.`TASK_UID_2` AS `TASK_UID_2`,`c`.`W_S_LOC` AS `LOC_FROM_2`,`c`.`W_D_LOC` AS `LOC_TO_2`,`c`.`SITE` AS `SITE_2` from ((`wcs_command_master` `a` left join `wcs_task_info` `b` on((`a`.`TASK_UID_1` = `b`.`TASK_UID`))) left join `wcs_task_info` `c` on((`a`.`TASK_UID_2` = `c`.`TASK_UID`)));

-- ----------------------------
-- Procedure structure for DELETE_DATA
-- ----------------------------
DROP PROCEDURE IF EXISTS `DELETE_DATA`;
delimiter ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `DELETE_DATA`()
BEGIN
	/*仅保留 30 天的备份任务数据*/
	delete from wcs_task_backup where DATEDIFF(CURRENT_DATE,BACKUP_TIME) >= 30;
	/*仅保留 7 天的程序异常数据*/
	delete from wcs_function_log where DATEDIFF(CURRENT_DATE,CREATION_TIME) >= 7;
END
;;
delimiter ;

-- ----------------------------
-- Event structure for DELETE_DATA_EVENT
-- ----------------------------
DROP EVENT IF EXISTS `DELETE_DATA_EVENT`;
delimiter ;;
CREATE DEFINER = `root`@`localhost` EVENT `DELETE_DATA_EVENT`
ON SCHEDULE
EVERY '1' DAY STARTS '2019-09-12 00:00:00'
COMMENT '每日一次删除过期无效的历史数据'
DO CALL DELETE_DATA()
;;
delimiter ;

SET FOREIGN_KEY_CHECKS = 1;
