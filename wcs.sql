/*
 Navicat Premium Data Transfer

 Source Server         : TEST
 Source Server Type    : MySQL
 Source Server Version : 80016
 Source Host           : localhost:3306
 Source Schema         : wcs

 Target Server Type    : MySQL
 Target Server Version : 80016
 File Encoding         : 65001

 Date: 15/11/2019 01:02:28
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
-- Table structure for wcs_command_master
-- ----------------------------
DROP TABLE IF EXISTS `wcs_command_master`;
CREATE TABLE `wcs_command_master`  (
  `WCS_NO` varchar(15) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT 'WCS单号',
  `FRT` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '固定辊台',
  `TASK_UID_1` varchar(36) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '任务UID_1',
  `TASK_UID_2` varchar(36) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '任务UID_2',
  `STEP` varchar(2) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL DEFAULT '1' COMMENT '步骤(1:组成单号；2:请求执行；3:执行中；4:结束)',
  `CREATION_TIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `UPDATE_TIME` timestamp(0) NULL DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`WCS_NO`) USING BTREE,
  INDEX `WCS_NO`(`WCS_NO`) USING BTREE,
  INDEX `TASKID_1_FK`(`TASK_UID_1`) USING BTREE,
  INDEX `TASKID_2_FK`(`TASK_UID_2`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = 'WCS任务指令总控' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for wcs_config_dev_gap
-- ----------------------------
DROP TABLE IF EXISTS `wcs_config_dev_gap`;
CREATE TABLE `wcs_config_dev_gap`  (
  `DEVICE` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '设备号',
  `TYPE` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '类别',
  `GAP_X` int(11) NULL DEFAULT 0 COMMENT 'X轴差',
  `GAP_Y` int(11) NULL DEFAULT 0 COMMENT 'Y轴差',
  `GAP_Z` int(11) NULL DEFAULT 0 COMMENT 'Z轴差',
  `CREATION_TIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`DEVICE`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = 'WCS设备偏差值' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_config_dev_gap
-- ----------------------------
INSERT INTO `wcs_config_dev_gap` VALUES ('AWC01', 'ABC', 0, 0, 0, '2019-10-19 10:24:37');
INSERT INTO `wcs_config_dev_gap` VALUES ('AWC02', 'ABC', 0, 20, 0, '2019-10-16 16:21:49');
INSERT INTO `wcs_config_dev_gap` VALUES ('RGV01', 'RGV', 0, 0, 0, '2019-10-21 15:14:46');
INSERT INTO `wcs_config_dev_gap` VALUES ('RGV02', 'RGV', -20, 0, 0, '2019-10-16 16:20:22');
INSERT INTO `wcs_config_dev_gap` VALUES ('RGV03', 'RGV', 0, 0, 0, '2019-10-16 16:20:33');

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
  `DUTY` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '职责(I:仅入库；O:仅出库；A:所有)',
  `REMARK` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '备注说明',
  `CREATION_TIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `UPDATE_TIME` timestamp(0) NULL DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`DEVICE`) USING BTREE,
  UNIQUE INDEX `DEVICE_UNIQUE`(`DEVICE`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '设备资讯   ' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_config_device
-- ----------------------------
INSERT INTO `wcs_config_device` VALUES ('ARF01', '192.168.8.80', 2000, 'Y', NULL, 'ARF', 'B01', 'I', '摆渡车1', '2019-07-04 09:54:44', '2019-10-23 09:06:55');
INSERT INTO `wcs_config_device` VALUES ('ARF02', '192.168.8.83', 2000, 'Y', NULL, 'ARF', 'B01', 'O', '摆渡车2', '2019-10-23 09:07:08', NULL);
INSERT INTO `wcs_config_device` VALUES ('AWC01', '192.168.8.40', 2000, 'Y', NULL, 'ABC', 'B01', 'A', '行车1', '2019-10-23 09:08:24', NULL);
INSERT INTO `wcs_config_device` VALUES ('AWC02', '192.168.8.48', 2000, 'Y', NULL, 'ABC', 'B01', 'A', '行车2', '2019-10-23 09:08:42', NULL);
INSERT INTO `wcs_config_device` VALUES ('FRT01', '192.168.8.90', 2000, 'Y', NULL, 'FRT', 'A01', 'I', '固定辊台1', '2019-09-23 16:52:50', '2019-10-29 09:22:37');
INSERT INTO `wcs_config_device` VALUES ('FRT02', '192.168.8.90', 2001, 'Y', '', 'FRT', 'A01', 'I', '固定辊台2', '2019-09-23 16:06:49', '2019-10-23 09:09:18');
INSERT INTO `wcs_config_device` VALUES ('FRT03', '192.168.8.90', 2002, 'Y', '', 'FRT', 'B01', 'O', '固定辊台3', '2019-09-23 17:06:55', '2019-10-27 08:51:38');
INSERT INTO `wcs_config_device` VALUES ('RGV01', '192.168.8.60', 2002, 'Y', NULL, 'RGV', 'B01', 'A', '运输车1', '2019-10-23 09:06:18', NULL);
INSERT INTO `wcs_config_device` VALUES ('RGV02', '192.168.8.66', 2002, 'Y', NULL, 'RGV', 'B01', 'A', '运输车2', '2019-10-23 09:06:32', NULL);
INSERT INTO `wcs_config_device` VALUES ('RGV03', '192.168.8.71', 2002, 'N', NULL, 'RGV', 'B01', 'A', '运输车3', '2019-09-24 17:06:52', '2019-10-23 09:05:59');

-- ----------------------------
-- Table structure for wcs_config_loc
-- ----------------------------
DROP TABLE IF EXISTS `wcs_config_loc`;
CREATE TABLE `wcs_config_loc`  (
  `ID` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `WMS_LOC` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT 'WMS回馈位置',
  `FRT_LOC` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '固定辊台位置',
  `ARF_LOC` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '摆渡车定位',
  `RGV_LOC_1` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '运输车辊台1[内]定位',
  `RGV_LOC_2` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '运输车辊台2[外]定位',
  `ABC_LOC_TRACK` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '行车轨道定位',
  `ABC_LOC_STOCK` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '行车库存定位',
  `CREATION_TIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 2789 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '依据WMS回馈位置定义各设备目的点位  ' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for wcs_function_log
-- ----------------------------
DROP TABLE IF EXISTS `wcs_function_log`;
CREATE TABLE `wcs_function_log`  (
  `ID` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `FUNCTION_NAME` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '方法名',
  `REMARK` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '说明',
  `WCS_NO` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT 'WCS单号',
  `ITEM_ID` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '项目ID',
  `RESULT` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '结果（OK / NG）',
  `MESSAGE` varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '讯息',
  `CREATION_TIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_function_log
-- ----------------------------
INSERT INTO `wcs_function_log` VALUES (3, 'DELETE_DATA()', '按时清除过期资讯', NULL, NULL, NULL, 'JOB[保留备份数据30天，程序异常记录7天]', '2019-11-14 00:00:00');
INSERT INTO `wcs_function_log` VALUES (4, 'DELETE_DATA()', '按时清除过期资讯', NULL, NULL, NULL, 'JOB[保留备份数据30天，程序异常记录7天]', '2019-11-15 00:00:00');

-- ----------------------------
-- Table structure for wcs_ndc_site
-- ----------------------------
DROP TABLE IF EXISTS `wcs_ndc_site`;
CREATE TABLE `wcs_ndc_site`  (
  `WCSSITE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `NDCSITE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `TYPE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`WCSSITE`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_ndc_site
-- ----------------------------
INSERT INTO `wcs_ndc_site` VALUES ('FRT01', '81', 'loadsite');
INSERT INTO `wcs_ndc_site` VALUES ('FRT02', '91', 'loadsite');
INSERT INTO `wcs_ndc_site` VALUES ('FRT03', '122', 'unloadsite');
INSERT INTO `wcs_ndc_site` VALUES ('FRT99', '102', 'unloadarea');

-- ----------------------------
-- Table structure for wcs_ndc_task
-- ----------------------------
DROP TABLE IF EXISTS `wcs_ndc_task`;
CREATE TABLE `wcs_ndc_task`  (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `TASKID` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `IKEY` int(10) NULL DEFAULT NULL,
  `NDCINDEX` int(10) NULL DEFAULT NULL,
  `CARRIERID` int(2) NULL DEFAULT NULL,
  `LOADSITE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `UNLOADSITE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `REDIRECTSITE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `NDCLOADSITE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `NDCUNLOADSITE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `NDCREDIRECTSITE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `HADLOAD` tinyint(4) NULL DEFAULT NULL,
  `HADUNLOAD` tinyint(4) NULL DEFAULT NULL,
  `PAUSE` tinyint(4) NULL DEFAULT NULL,
  `FINISH` tinyint(4) NULL DEFAULT NULL,
  `CREATETIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `UPDATE_TIME` datetime(0) NULL DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 23 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for wcs_ndc_task_temp
-- ----------------------------
DROP TABLE IF EXISTS `wcs_ndc_task_temp`;
CREATE TABLE `wcs_ndc_task_temp`  (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `NDCINDEX` int(11) NULL DEFAULT NULL,
  `IKEY` int(11) NULL DEFAULT NULL,
  `CARRIERID` int(11) NULL DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for wcs_param
-- ----------------------------
DROP TABLE IF EXISTS `wcs_param`;
CREATE TABLE `wcs_param`  (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `NAME` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `INFO` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `VALUE1` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `VALUE2` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `VALUE3` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `VALUE4` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `VALUE5` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `VALUE6` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 6 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_param
-- ----------------------------
INSERT INTO `wcs_param` VALUES (1, 'NDC_SERVER_IP', 'NDC服务IP', '192.168.8.120', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `wcs_param` VALUES (2, 'NDC_SERVER_PORT', 'NDC服务端口', '30001', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `wcs_param` VALUES (3, 'NDC_TASK_IKEY', '用于计算生成任务的IKEY值', '97', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `wcs_param` VALUES (4, 'WCS_HTTP_SERVER_PORT', 'WCS提供WMS的服务端口', '8080', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `wcs_param` VALUES (5, 'WMS_SERVER_URL', 'WMS服务地址', 'http://192.168.8.244:8081', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `wcs_param` VALUES (6, 'WCS_STOCK_PARTITION_RANGE', 'WCS库存划分范围（行车）', '1', '11300', '82550', NULL, NULL, 'B01');
INSERT INTO `wcs_param` VALUES (7, 'WCS_STOCK_PARTITION_RANGE', 'WCS库存划分范围（行车）', '2', '83800', '155050', NULL, NULL, 'B01');
INSERT INTO `wcs_param` VALUES (8, 'WCS_STOCK_PARTITION_RANGE', 'WCS库存划分范围（行车）', '3', '156300', '227550', NULL, NULL, 'B01');
INSERT INTO `wcs_param` VALUES (9, 'WCS_STOCK_PARTITION_RANGE', 'WCS库存划分范围（行车）', '4', '228800', '300050', NULL, NULL, 'B01');

-- ----------------------------
-- Table structure for wcs_task_backup
-- ----------------------------
DROP TABLE IF EXISTS `wcs_task_backup`;
CREATE TABLE `wcs_task_backup`  (
  `ID` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `WCS_NO` varchar(15) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT 'WCS单号',
  `TASK_UID` varchar(36) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '任务UID',
  `FRT` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '固定辊台',
  `TASK_TYPE` varchar(2) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '任务类型(\n0:AGV运输；1:入库；\n2:出库；\n3:移仓；\n4:盘点)',
  `BARCODE` varchar(30) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '货物码',
  `W_S_LOC` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '来源货位',
  `W_D_LOC` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '目标货位',
  `BACKUP_TIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '备份时间',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 56 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for wcs_task_info
-- ----------------------------
DROP TABLE IF EXISTS `wcs_task_info`;
CREATE TABLE `wcs_task_info`  (
  `TASK_UID` varchar(36) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '任务UID',
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
) ENGINE = InnoDB AUTO_INCREMENT = 335 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = 'WCS指令资讯  ' ROW_FORMAT = Dynamic;

-- ----------------------------
-- View structure for wcs_command_v
-- ----------------------------
DROP VIEW IF EXISTS `wcs_command_v`;
CREATE ALGORITHM = UNDEFINED DEFINER = `root`@`localhost` SQL SECURITY DEFINER VIEW `wcs_command_v` AS select `a`.`WCS_NO` AS `WCS_NO`,`a`.`FRT` AS `FRT`,`a`.`STEP` AS `STEP`,`b`.`TASK_TYPE` AS `TASK_TYPE`,`a`.`CREATION_TIME` AS `CREATION_TIME`,`a`.`TASK_UID_1` AS `TASK_UID_1`,`b`.`BARCODE` AS `CODE_1`,`b`.`W_S_LOC` AS `LOC_FROM_1`,`b`.`W_D_LOC` AS `LOC_TO_1`,`b`.`SITE` AS `SITE_1`,`a`.`TASK_UID_2` AS `TASK_UID_2`,`c`.`BARCODE` AS `CODE_2`,`c`.`W_S_LOC` AS `LOC_FROM_2`,`c`.`W_D_LOC` AS `LOC_TO_2`,`c`.`SITE` AS `SITE_2` from ((`wcs_command_master` `a` left join `wcs_task_info` `b` on((`a`.`TASK_UID_1` = `b`.`TASK_UID`))) left join `wcs_task_info` `c` on((`a`.`TASK_UID_2` = `c`.`TASK_UID`)));

-- ----------------------------
-- View structure for wcs_task_out_range_v
-- ----------------------------
DROP VIEW IF EXISTS `wcs_task_out_range_v`;
CREATE ALGORITHM = UNDEFINED DEFINER = `root`@`localhost` SQL SECURITY DEFINER VIEW `wcs_task_out_range_v` AS select `l`.`AREA` AS `AREA`,`t`.`TASK_UID` AS `TASK_UID`,`t`.`SITE` AS `SITE`,`l`.`WMS_LOC` AS `WMS_LOC`,`l`.`WMS_Z` AS `WMS_Z`,`l`.`STOCK_X` AS `STOCK_X`,`l`.`VALUE1` AS `P_RANGE`,`l`.`VALUE2` AS `RANGE_FROM`,`l`.`VALUE3` AS `RANGE_TO` from ((select distinct `wcs_task_info`.`TASK_UID` AS `TASK_UID`,`wcs_task_info`.`W_S_LOC` AS `W_S_LOC`,`wcs_task_info`.`SITE` AS `SITE` from `wcs_task_info` where (`wcs_task_info`.`TASK_TYPE` = '2')) `t` join (select `a`.`WMS_LOC` AS `WMS_LOC`,`a`.`STOCK_X` AS `STOCK_X`,`a`.`AREA` AS `AREA`,`a`.`WMS_Z` AS `WMS_Z`,`b`.`VALUE1` AS `VALUE1`,`b`.`VALUE2` AS `VALUE2`,`b`.`VALUE3` AS `VALUE3` from ((select `wcs_config_loc`.`WMS_LOC` AS `WMS_LOC`,substring_index(`wcs_config_loc`.`ABC_LOC_STOCK`,'-',1) AS `STOCK_X`,substring_index(`wcs_config_loc`.`WMS_LOC`,'-',1) AS `AREA`,substring_index(`wcs_config_loc`.`WMS_LOC`,'-',-(1)) AS `WMS_Z` from `wcs_config_loc`) `a` join (select `wcs_param`.`VALUE1` AS `VALUE1`,`wcs_param`.`VALUE2` AS `VALUE2`,`wcs_param`.`VALUE3` AS `VALUE3`,`wcs_param`.`VALUE6` AS `VALUE6` from `wcs_param` where (`wcs_param`.`NAME` = 'WCS_STOCK_PARTITION_RANGE')) `b`) where (((`b`.`VALUE2` + 0) <= (`a`.`STOCK_X` + 0)) and ((`b`.`VALUE3` + 0) >= (`a`.`STOCK_X` + 0)) and (`b`.`VALUE6` = `a`.`AREA`))) `l`) where (`t`.`W_S_LOC` = `l`.`WMS_LOC`) order by `l`.`STOCK_X`,`l`.`WMS_Z` desc;

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
	/*TEST*/
	insert into wcs_function_log(FUNCTION_NAME,REMARK,MESSAGE) VALUES('DELETE_DATA()','按时清除过期资讯','JOB[保留备份数据30天，程序异常记录7天]');
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
EVERY '1' DAY STARTS '2019-09-21 00:00:00'
COMMENT '每日一次删除过期无效的历史数据'
DO CALL DELETE_DATA()
;;
delimiter ;

SET FOREIGN_KEY_CHECKS = 1;
