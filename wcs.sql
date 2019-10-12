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

 Date: 12/10/2019 13:55:56
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
INSERT INTO `wcs_agv_info` VALUES (1, 'T1190930145936', 'AGV01', 'FRT99', 'FRT01', 1, '2019-08-02 08:50:03', NULL);
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
-- Records of wcs_command_master
-- ----------------------------
INSERT INTO `wcs_command_master` VALUES ('I190928092054', 'FRT01', 'T1190928092054', '', '4', '2019-09-28 09:20:54', '2019-09-28 10:42:26');
INSERT INTO `wcs_command_master` VALUES ('I190929085547', 'FRT02', 'T1190929085547', '', '4', '2019-09-29 08:55:47', '2019-09-29 08:56:15');
INSERT INTO `wcs_command_master` VALUES ('I190929135628', 'FRT02', 'T1190929135628', 'T2190929135628', '4', '2019-09-29 13:56:28', '2019-09-29 13:58:11');
INSERT INTO `wcs_command_master` VALUES ('I190929145819', 'FRT03', 'T1190929145819', 'T2190929145819', '4', '2019-09-29 14:58:19', '2019-09-29 14:59:51');
INSERT INTO `wcs_command_master` VALUES ('I190929161634', 'FRT02', 'T1190929161634', 'T2190929161634', '4', '2019-09-29 16:16:35', '2019-09-30 10:14:03');
INSERT INTO `wcs_command_master` VALUES ('I190929172638', 'FRT02', 'T1190929172638', 'T2190929172638', '4', '2019-09-29 17:26:38', '2019-09-29 17:26:52');
INSERT INTO `wcs_command_master` VALUES ('O190928154215', 'FRT01', 'T1190928154215', '', '4', '2019-09-30 09:42:16', '2019-09-30 09:49:32');
INSERT INTO `wcs_command_master` VALUES ('O190930101849', 'FRT01', 'T1190930101849', '', '4', '2019-09-30 10:18:49', '2019-09-30 10:26:30');
INSERT INTO `wcs_command_master` VALUES ('O190930104904', 'FRT01', 'T1190930104904', 'T2190930104904', '4', '2019-09-30 10:49:04', '2019-09-30 10:53:21');
INSERT INTO `wcs_command_master` VALUES ('O190930110627', 'FRT03', 'T1190930110627', 'T2190930110627', '4', '2019-09-30 11:06:27', '2019-09-30 11:11:16');
INSERT INTO `wcs_command_master` VALUES ('O190930115336', 'FRT01', 'T1190930115336', '', '4', '2019-09-30 11:53:36', '2019-09-30 13:58:53');
INSERT INTO `wcs_command_master` VALUES ('O190930115357', 'FRT02', 'T1190930115357', '', '4', '2019-09-30 11:53:57', '2019-09-30 13:59:07');
INSERT INTO `wcs_command_master` VALUES ('O190930145846', 'FRT01', 'T1190930145846', 'T2190930145846', '4', '2019-09-30 14:58:46', '2019-09-30 15:00:17');
INSERT INTO `wcs_command_master` VALUES ('O190930145936', 'FRT03', 'T1190930145936', '', '4', '2019-09-30 14:59:36', '2019-09-30 15:00:23');

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
  UNIQUE INDEX `DEVICE_UNIQUE`(`DEVICE`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '设备资讯   ' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_config_device
-- ----------------------------
INSERT INTO `wcs_config_device` VALUES ('ARF01', '192.168.8.80', 2000, 'Y', NULL, 'ARF', 'B01', '摆渡车Test', '2019-07-04 09:54:44', '2019-09-23 14:49:14');
INSERT INTO `wcs_config_device` VALUES ('FRT01', '192.168.8.90', 2000, 'N', NULL, 'FRT', 'B01', 'Test1', '2019-09-23 16:52:50', NULL);
INSERT INTO `wcs_config_device` VALUES ('FRT02', '192.168.8.90', 2001, 'N', NULL, 'FRT', 'B01', 'Test2', '2019-09-23 17:06:49', NULL);
INSERT INTO `wcs_config_device` VALUES ('FRT03', '192.168.8.90', 2002, 'N', '', 'FRT', 'A01', 'Test3', '2019-09-23 17:06:55', NULL);
INSERT INTO `wcs_config_device` VALUES ('RGV01', '192.168.8.71', 2002, 'N', NULL, 'RGV', 'B01', '运输车Test', '2019-09-24 17:06:52', '2019-09-24 17:07:38');

-- ----------------------------
-- Table structure for wcs_config_loc
-- ----------------------------
DROP TABLE IF EXISTS `wcs_config_loc`;
CREATE TABLE `wcs_config_loc`  (
  `ID` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `WMS_LOC` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT 'WMS回馈位置',
  `FRT_LOC` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '固定辊台位置',
  `ARF_LOC` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '摆渡车定位',
  `RGV_LOC_1` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '运输车辊台1[内]定位',
  `RGV_LOC_2` varchar(5) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '运输车辊台2[外]定位',
  `ABC_LOC_TRACK` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '行车轨道定位',
  `ABC_LOC_STOCK` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '行车库存定位',
  `CREATION_TIME` timestamp(0) NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 12 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '依据WMS回馈位置定义各设备目的点位  ' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_config_loc
-- ----------------------------
INSERT INTO `wcs_config_loc` VALUES (1, 'A01', 'FRT99', '', '', '', '', '', '2019-10-12 11:29:26');
INSERT INTO `wcs_config_loc` VALUES (2, 'B01', 'FRT01', '2', '', '', '', '', '2019-10-12 11:29:26');
INSERT INTO `wcs_config_loc` VALUES (3, 'B01', 'FRT02', '3', '', '', '', '', '2019-10-12 11:29:26');
INSERT INTO `wcs_config_loc` VALUES (4, 'B01', 'FRT03', '4', '', '', '', '', '2019-10-12 11:29:26');
INSERT INTO `wcs_config_loc` VALUES (5, 'C001-001-001', '', '', '001', '002', '001-004-001', '001-001-001', '2019-10-12 11:29:26');
INSERT INTO `wcs_config_loc` VALUES (6, 'C001-001-002', '', '', '001', '002', '001-004-001', '001-001-002', '2019-10-12 11:29:26');
INSERT INTO `wcs_config_loc` VALUES (7, 'C200-001-001', '', '', '251', '252', '200-004-001', '200-001-001', '2019-10-12 11:29:26');
INSERT INTO `wcs_config_loc` VALUES (8, 'C200-001-002', '', '', '251', '252', '200-004-001', '200-001-002', '2019-10-12 11:29:26');

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
) ENGINE = InnoDB AUTO_INCREMENT = 384 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_function_log
-- ----------------------------
INSERT INTO `wcs_function_log` VALUES (378, 'DELETE_DATA()', '按时清除过期资讯', NULL, NULL, NULL, 'JOB[保留备份数据30天，程序异常记录7天]', '2019-10-09 00:00:00');
INSERT INTO `wcs_function_log` VALUES (379, 'DELETE_DATA()', '按时清除过期资讯', NULL, NULL, NULL, 'JOB[保留备份数据30天，程序异常记录7天]', '2019-10-10 00:00:01');
INSERT INTO `wcs_function_log` VALUES (380, 'DELETE_DATA()', '按时清除过期资讯', NULL, NULL, NULL, 'JOB[保留备份数据30天，程序异常记录7天]', '2019-10-11 00:00:00');
INSERT INTO `wcs_function_log` VALUES (381, 'LinkDevicesClient()', '连接网络设备', '', '', 'NG', 'System.NullReferenceException: 未将对象引用设置到对象的实例。\r\n   在 TaskManager.Functions.TaskTools.LinkDevicesClient() 位置 D:CodeWCSTaskManagerFunctionsTaskTools.cs:行号 48', '2019-10-11 09:44:31');
INSERT INTO `wcs_function_log` VALUES (382, 'LinkDevicesClient()', '连接网络设备', '', '', 'NG', 'System.NullReferenceException: 未将对象引用设置到对象的实例。\r\n   在 System.Collections.Generic.List`1.Enumerator.MoveNext()\r\n   在 TaskManager.Functions.TaskTools.LinkDevicesClient() 位置 D:CodeWCSTaskManagerFunctionsTaskTools.cs:行号 58', '2019-10-11 09:45:30');
INSERT INTO `wcs_function_log` VALUES (383, 'DELETE_DATA()', '按时清除过期资讯', NULL, NULL, NULL, 'JOB[保留备份数据30天，程序异常记录7天]', '2019-10-12 00:00:00');

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
INSERT INTO `wcs_ndc_site` VALUES ('FRT01', '2371', 'unloadarea');
INSERT INTO `wcs_ndc_site` VALUES ('FRT02', '2366', 'unloadsite');
INSERT INTO `wcs_ndc_site` VALUES ('FRT99', '1380', 'loadsite');

-- ----------------------------
-- Table structure for wcs_ndc_task
-- ----------------------------
DROP TABLE IF EXISTS `wcs_ndc_task`;
CREATE TABLE `wcs_ndc_task`  (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `TASKID` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `IKEY` int(10) NULL DEFAULT NULL,
  `ORDERINDEX` int(10) NULL DEFAULT NULL,
  `LOADSITE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `UNLOADSITE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `REDIRECTSITE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `NDCLOADSITE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `NDCUNLOADSITE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `NDCREDIRECTSITE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `HADLOAD` int(1) NULL DEFAULT NULL,
  `HADUNLOAD` int(1) NULL DEFAULT NULL,
  `CREATETIME` datetime(6) NULL DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

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
  `VALUE6` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_param
-- ----------------------------
INSERT INTO `wcs_param` VALUES (1, 'NDC_SERVER_IP', 'NDC服务IP', '10.9.30.120', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `wcs_param` VALUES (2, 'NDC_SERVER_PORT', 'NDC服务端口', '30001', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `wcs_param` VALUES (3, 'NDC_TASK_IKEY', '用于计算生成任务的IKEY值', '68', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `wcs_param` VALUES (4, 'WCS_HTTP_SERVER_PORT', 'WCS提供WMS的服务端口', '8080', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `wcs_param` VALUES (5, 'WMS_SERVER_URL', 'WMS服务地址', 'http://10.9.31.119:8081', NULL, NULL, NULL, NULL, NULL);

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
) ENGINE = InnoDB AUTO_INCREMENT = 22 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_task_backup
-- ----------------------------
INSERT INTO `wcs_task_backup` VALUES (5, 'I190926160501', 'T1190926160100', 'FRT01', '1', '2333', 'CCC', 'DDD', '2019-09-26 16:55:04');
INSERT INTO `wcs_task_backup` VALUES (6, 'I190926160501', 'T2190926160100', 'FRT01', '1', '2333', 'AAA', 'BBB', '2019-09-26 16:55:04');
INSERT INTO `wcs_task_backup` VALUES (8, 'I190926160501', 'T1190926160100', 'FRT02', '1', '2333', 'AAA', 'BBB', '2019-09-26 17:02:49');
INSERT INTO `wcs_task_backup` VALUES (9, 'I190926171824', 'T1190926171824', 'FRT02', '1', 'qqq', 'asdqwe', 'qweasd', '2019-09-26 17:19:01');
INSERT INTO `wcs_task_backup` VALUES (10, 'I190926171824', 'T2190926171824', 'FRT02', '1', 'www', 'dasaewq', 'dsaewq', '2019-09-26 17:19:01');
INSERT INTO `wcs_task_backup` VALUES (12, 'I190927094547', 'T1190927094547', 'FRT01', '1', 'qqq', 'B01', 'C001-001-001', '2019-09-27 11:13:34');
INSERT INTO `wcs_task_backup` VALUES (13, 'I190927094547', 'T2190927094547', 'FRT01', '1', 'qqq', 'B01', 'C001-001-002', '2019-09-27 11:13:34');
INSERT INTO `wcs_task_backup` VALUES (15, 'U190927111544', 'T1190927111544', 'FRT01', '2', 'qqq', 'C001-001-001', 'B01', '2019-09-27 11:18:12');
INSERT INTO `wcs_task_backup` VALUES (16, 'U190927111544', 'T2190927111544', 'FRT01', '2', 'qqq', 'C001-001-002', 'B01', '2019-09-27 11:18:12');
INSERT INTO `wcs_task_backup` VALUES (18, 'O190927111858', 'T1190927111858', 'FRT02', '2', 'QWEEWQ', 'C001-001-001', 'B01', '2019-09-27 11:19:40');
INSERT INTO `wcs_task_backup` VALUES (19, 'O190927111858', 'T2190927111858', 'FRT02', '2', 'QWEEWQ', 'C001-001-001', 'B01', '2019-09-27 11:19:40');
INSERT INTO `wcs_task_backup` VALUES (21, 'I190929082206', 'T1190929082206', 'FRT02', '1', 'IN-TEST-1-N-X', 'B01', 'C200-001-001', '2019-09-29 08:54:34');

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
-- Records of wcs_task_info
-- ----------------------------
INSERT INTO `wcs_task_info` VALUES ('T1190928092054', '1', 'IN-TEST-1-W-X', 'B01', 'C001-001-001', 'Y', '2019-09-28 09:20:54', '2019-09-28 15:06:10');
INSERT INTO `wcs_task_info` VALUES ('T1190928154215', '2', 'OUT-TEST-1-1-0', 'C001-001-002', 'B01', 'Y', '2019-09-30 09:42:15', '2019-09-30 10:17:15');
INSERT INTO `wcs_task_info` VALUES ('T1190929085547', '1', 'IN-TEST-1-N-X', 'B01', 'C200-001-001', 'Y', '2019-09-29 08:55:47', '2019-09-29 10:25:11');
INSERT INTO `wcs_task_info` VALUES ('T1190929135628', '1', 'IN-TEST-2-W-W', 'B01', 'C001-001-001', 'Y', '2019-09-29 13:56:28', '2019-09-29 14:37:57');
INSERT INTO `wcs_task_info` VALUES ('T1190929145819', '1', 'IN-TEST-2-N-N', 'B01', 'C200-001-001', 'Y', '2019-09-29 14:58:19', '2019-09-29 15:45:43');
INSERT INTO `wcs_task_info` VALUES ('T1190929161634', '1', 'IN-TEST-2-W-N', 'B01', 'C001-001-001', 'Y', '2019-09-29 16:16:34', '2019-09-29 17:15:47');
INSERT INTO `wcs_task_info` VALUES ('T1190929172638', '1', 'IN-TEST-2-N-W', 'B01', 'C200-001-001', 'Y', '2019-09-29 17:26:38', '2019-09-30 09:35:00');
INSERT INTO `wcs_task_info` VALUES ('T1190930101849', '2', 'OUT-TEST-1-0-1', 'C200-001-001', 'B01', 'Y', '2019-09-30 10:18:49', '2019-09-30 10:45:13');
INSERT INTO `wcs_task_info` VALUES ('T1190930104904', '2', 'OUT-TEST-2-2-0', 'C001-001-002', 'B01', 'Y', '2019-09-30 10:49:04', '2019-09-30 11:03:50');
INSERT INTO `wcs_task_info` VALUES ('T1190930110627', '2', 'OUT-TEST-2-0-2', 'C200-001-002', 'B01', 'Y', '2019-09-30 11:06:27', '2019-09-30 11:21:47');
INSERT INTO `wcs_task_info` VALUES ('T1190930115336', '2', 'OUT-TEST-2-1-1', 'C001-001-001', 'B01', 'Y', '2019-09-30 11:53:36', '2019-09-30 14:12:26');
INSERT INTO `wcs_task_info` VALUES ('T1190930115357', '2', 'OUT-TEST-2-1-1', 'C200-001-001', 'B01', 'Y', '2019-09-30 11:53:57', '2019-09-30 14:16:40');
INSERT INTO `wcs_task_info` VALUES ('T1190930145846', '2', 'OUT-TEST-3-2-1', 'C001-001-002', 'B01', 'Y', '2019-09-30 14:58:46', '2019-09-30 15:12:16');
INSERT INTO `wcs_task_info` VALUES ('T1190930145936', '2', 'OUT-TEST-3-2-1', 'C200-001-001', 'B01', 'Y', '2019-09-30 14:59:36', '2019-09-30 15:13:46');
INSERT INTO `wcs_task_info` VALUES ('T2190929135628', '1', 'IN-TEST-2-W-W', 'B01', 'C001-001-002', 'Y', '2019-09-29 13:56:28', '2019-09-29 14:44:21');
INSERT INTO `wcs_task_info` VALUES ('T2190929145819', '1', 'IN-TEST-2-N-N', 'B01', 'C200-001-002', 'Y', '2019-09-29 14:58:19', '2019-09-29 15:50:30');
INSERT INTO `wcs_task_info` VALUES ('T2190929161634', '1', 'IN-TEST-2-W-N', 'B01', 'C200-001-001', 'Y', '2019-09-29 16:16:35', '2019-09-30 10:15:33');
INSERT INTO `wcs_task_info` VALUES ('T2190929172638', '1', 'IN-TEST-2-N-W', 'B01', 'C001-001-001', 'Y', '2019-09-29 17:26:38', '2019-09-30 10:10:01');
INSERT INTO `wcs_task_info` VALUES ('T2190930104904', '2', 'OUT-TEST-2-2-0', 'C001-001-001', 'B01', 'Y', '2019-09-30 10:49:04', '2019-09-30 11:03:50');
INSERT INTO `wcs_task_info` VALUES ('T2190930110627', '2', 'OUT-TEST-2-0-2', 'C200-001-001', 'B01', 'Y', '2019-09-30 11:06:27', '2019-09-30 11:21:47');
INSERT INTO `wcs_task_info` VALUES ('T2190930145846', '2', 'OUT-TEST-3-2-1', 'C001-001-001', 'B01', 'Y', '2019-09-30 14:58:46', '2019-09-30 15:12:16');

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
) ENGINE = InnoDB AUTO_INCREMENT = 284 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = 'WCS指令资讯  ' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of wcs_task_item
-- ----------------------------
INSERT INTO `wcs_task_item` VALUES (7, 'I190928092054', '012', 'ARF01', '', '2', 'Y', '2019-09-28 10:42:26', '2019-09-28 13:58:44');
INSERT INTO `wcs_task_item` VALUES (8, 'I190928092054', '022', 'RGV01', '', '001', 'Y', '2019-09-28 10:42:26', '2019-09-28 14:49:36');
INSERT INTO `wcs_task_item` VALUES (9, 'I190928092054', '031', 'ABC01', '', '001-004-001', 'Y', '2019-09-28 10:42:26', '2019-09-28 14:57:07');
INSERT INTO `wcs_task_item` VALUES (10, 'I190928092054', '113', 'ARF01', 'FRT01', '', 'Y', '2019-09-28 13:58:32', NULL);
INSERT INTO `wcs_task_item` VALUES (11, 'I190928092054', '111', 'FRT01', '', 'ARF01', 'Y', '2019-09-28 13:58:34', NULL);
INSERT INTO `wcs_task_item` VALUES (12, 'I190928092054', '013', 'ARF01', '', '2', 'Y', '2019-09-28 14:46:47', '2019-09-28 14:49:30');
INSERT INTO `wcs_task_item` VALUES (13, 'I190928092054', '115', 'RGV01', 'ARF01', '', 'Y', '2019-09-28 14:49:27', NULL);
INSERT INTO `wcs_task_item` VALUES (14, 'I190928092054', '113', 'ARF01', '', 'RGV01', 'Y', '2019-09-28 14:49:29', NULL);
INSERT INTO `wcs_task_item` VALUES (15, 'I190928092054', '021', 'RGV01', '', '001', 'Y', '2019-09-28 14:54:31', '2019-09-28 14:57:08');
INSERT INTO `wcs_task_item` VALUES (16, 'I190928092054', '117', 'ABC01', '', '001-004-001', 'Y', '2019-09-28 14:57:05', NULL);
INSERT INTO `wcs_task_item` VALUES (17, 'I190928092054', '032', 'ABC01', '', '001-001-001', 'Y', '2019-09-28 14:59:23', '2019-09-28 15:01:33');
INSERT INTO `wcs_task_item` VALUES (18, 'I190928092054', '118', 'ABC01', '', '001-001-001', 'Y', '2019-09-28 15:01:31', NULL);
INSERT INTO `wcs_task_item` VALUES (19, 'O190928154215', '032', 'ABC01', '', '001-001-002', 'Y', '2019-09-28 15:48:20', '2019-09-28 15:57:50');
INSERT INTO `wcs_task_item` VALUES (20, 'O190928154215', '021', 'RGV01', '', '001', 'Y', '2019-09-28 15:49:30', '2019-09-28 16:05:32');
INSERT INTO `wcs_task_item` VALUES (21, 'O190928154215', '013', 'ARF01', '', '2', 'Y', '2019-09-28 15:49:31', '2019-09-28 16:10:48');
INSERT INTO `wcs_task_item` VALUES (22, 'O190928154215', '117', 'ABC01', '', '001-001-002', 'Y', '2019-09-28 15:57:48', NULL);
INSERT INTO `wcs_task_item` VALUES (23, 'O190928154215', '031', 'ABC01', '', '001-004-001', 'Y', '2019-09-28 16:03:06', '2019-09-28 16:05:31');
INSERT INTO `wcs_task_item` VALUES (24, 'O190928154215', '118', 'ABC01', '', '001-004-001', 'Y', '2019-09-28 16:05:28', NULL);
INSERT INTO `wcs_task_item` VALUES (25, 'O190928154215', '022', 'RGV01', '', '001', 'Y', '2019-09-28 16:09:12', '2019-09-28 16:10:48');
INSERT INTO `wcs_task_item` VALUES (26, 'O190928154215', '114', 'ARF01', 'RGV01', '', 'Y', '2019-09-28 16:10:35', NULL);
INSERT INTO `wcs_task_item` VALUES (27, 'O190928154215', '116', 'RGV01', '', 'ARF01', 'Y', '2019-09-28 16:10:47', NULL);
INSERT INTO `wcs_task_item` VALUES (28, 'O190928154215', '012', 'ARF01', '', '2', 'Y', '2019-09-28 16:13:43', '2019-09-28 16:16:23');
INSERT INTO `wcs_task_item` VALUES (31, 'O190928154215', '112', 'FRT01', 'ARF01', '', 'Y', '2019-09-28 16:16:23', NULL);
INSERT INTO `wcs_task_item` VALUES (32, 'O190928154215', '114', 'ARF01', '', 'FRT01', 'Y', '2019-09-28 16:16:23', NULL);
INSERT INTO `wcs_task_item` VALUES (45, 'I190929085547', '012', 'ARF01', '', '3', 'Y', '2019-09-29 08:56:14', '2019-09-29 08:57:07');
INSERT INTO `wcs_task_item` VALUES (46, 'I190929085547', '022', 'RGV01', '', '001', 'Y', '2019-09-29 08:56:15', '2019-09-29 08:57:46');
INSERT INTO `wcs_task_item` VALUES (47, 'I190929085547', '031', 'ABC02', '', '200-004-001', 'Y', '2019-09-29 08:56:15', '2019-09-29 10:21:25');
INSERT INTO `wcs_task_item` VALUES (48, 'I190929085547', '113', 'ARF01', 'FRT02', '', 'Y', '2019-09-29 08:57:06', NULL);
INSERT INTO `wcs_task_item` VALUES (49, 'I190929085547', '111', 'FRT02', '', 'ARF01', 'Y', '2019-09-29 08:57:07', NULL);
INSERT INTO `wcs_task_item` VALUES (50, 'I190929085547', '013', 'ARF01', '', '2', 'Y', '2019-09-29 08:57:30', '2019-09-29 08:57:46');
INSERT INTO `wcs_task_item` VALUES (51, 'I190929085547', '115', 'RGV01', 'ARF01', '', 'Y', '2019-09-29 08:57:46', NULL);
INSERT INTO `wcs_task_item` VALUES (52, 'I190929085547', '113', 'ARF01', '', 'RGV01', 'Y', '2019-09-29 08:57:46', NULL);
INSERT INTO `wcs_task_item` VALUES (53, 'I190929085547', '023', 'RGV02', '', '241', 'Y', '2019-09-29 08:58:18', '2019-09-29 09:00:14');
INSERT INTO `wcs_task_item` VALUES (54, 'I190929085547', '024', 'RGV01', '', '239', 'Y', '2019-09-29 08:58:18', '2019-09-29 09:00:14');
INSERT INTO `wcs_task_item` VALUES (55, 'I190929085547', '115', 'RGV02', 'RGV01', '', 'Y', '2019-09-29 09:00:03', NULL);
INSERT INTO `wcs_task_item` VALUES (56, 'I190929085547', '115', 'RGV01', '', 'RGV02', 'Y', '2019-09-29 09:00:14', NULL);
INSERT INTO `wcs_task_item` VALUES (57, 'I190929085547', '021', 'RGV02', '', '251', 'Y', '2019-09-29 10:18:10', '2019-09-29 10:21:25');
INSERT INTO `wcs_task_item` VALUES (58, 'I190929085547', '117', 'ABC02', '', '200-004-001', 'Y', '2019-09-29 10:21:25', NULL);
INSERT INTO `wcs_task_item` VALUES (59, 'I190929085547', '032', 'ABC02', '', '200-001-001', 'Y', '2019-09-29 10:24:18', '2019-09-29 10:24:56');
INSERT INTO `wcs_task_item` VALUES (60, 'I190929085547', '118', 'ABC02', '', '200-001-001', 'Y', '2019-09-29 10:24:56', NULL);
INSERT INTO `wcs_task_item` VALUES (61, 'I190929135628', '012', 'ARF01', '', '3', 'Y', '2019-09-29 13:58:11', '2019-09-29 14:17:56');
INSERT INTO `wcs_task_item` VALUES (62, 'I190929135628', '022', 'RGV01', '', '001', 'Y', '2019-09-29 13:58:11', '2019-09-29 14:20:39');
INSERT INTO `wcs_task_item` VALUES (63, 'I190929135628', '031', 'ABC01', '', '001-004-001', 'Y', '2019-09-29 13:58:11', '2019-09-29 14:32:23');
INSERT INTO `wcs_task_item` VALUES (64, 'I190929135628', '113', 'ARF01', 'FRT02', '', 'Y', '2019-09-29 14:17:55', NULL);
INSERT INTO `wcs_task_item` VALUES (65, 'I190929135628', '111', 'FRT02', '', 'ARF01', 'Y', '2019-09-29 14:17:56', NULL);
INSERT INTO `wcs_task_item` VALUES (66, 'I190929135628', '013', 'ARF01', '', '2', 'Y', '2019-09-29 14:19:25', '2019-09-29 14:20:39');
INSERT INTO `wcs_task_item` VALUES (67, 'I190929135628', '115', 'RGV01', 'ARF01', '', 'Y', '2019-09-29 14:20:39', NULL);
INSERT INTO `wcs_task_item` VALUES (68, 'I190929135628', '113', 'ARF01', '', 'RGV01', 'Y', '2019-09-29 14:20:39', NULL);
INSERT INTO `wcs_task_item` VALUES (69, 'I190929135628', '021', 'RGV01', '', '001', 'Y', '2019-09-29 14:24:03', '2019-09-29 14:32:23');
INSERT INTO `wcs_task_item` VALUES (70, 'I190929135628', '117', 'ABC01', '', '001-004-001', 'Y', '2019-09-29 14:32:23', NULL);
INSERT INTO `wcs_task_item` VALUES (71, 'I190929135628', '021', 'RGV01', '', '002', 'Y', '2019-09-29 14:34:33', '2019-09-29 14:40:46');
INSERT INTO `wcs_task_item` VALUES (72, 'I190929135628', '032', 'ABC01', '', '001-001-001', 'Y', '2019-09-29 14:34:50', '2019-09-29 14:36:45');
INSERT INTO `wcs_task_item` VALUES (73, 'I190929135628', '118', 'ABC01', '', '001-001-001', 'Y', '2019-09-29 14:36:45', NULL);
INSERT INTO `wcs_task_item` VALUES (74, 'I190929135628', '031', 'ABC01', '', '001-004-001', 'Y', '2019-09-29 14:38:38', '2019-09-29 14:40:46');
INSERT INTO `wcs_task_item` VALUES (75, 'I190929135628', '117', 'ABC01', '', '001-004-001', 'Y', '2019-09-29 14:40:46', NULL);
INSERT INTO `wcs_task_item` VALUES (76, 'I190929135628', '032', 'ABC01', '', '001-001-002', 'Y', '2019-09-29 14:42:40', '2019-09-29 14:43:54');
INSERT INTO `wcs_task_item` VALUES (77, 'I190929135628', '118', 'ABC01', '', '001-001-002', 'Y', '2019-09-29 14:43:54', NULL);
INSERT INTO `wcs_task_item` VALUES (78, 'I190929145819', '012', 'ARF01', '', '4', 'Y', '2019-09-29 14:59:51', '2019-09-29 15:02:13');
INSERT INTO `wcs_task_item` VALUES (79, 'I190929145819', '022', 'RGV01', '', '001', 'Y', '2019-09-29 14:59:51', '2019-09-29 15:06:17');
INSERT INTO `wcs_task_item` VALUES (80, 'I190929145819', '031', 'ABC02', '', '200-004-001', 'Y', '2019-09-29 14:59:51', '2019-09-29 15:15:10');
INSERT INTO `wcs_task_item` VALUES (81, 'I190929145819', '113', 'ARF01', 'FRT03', '', 'Y', '2019-09-29 15:02:13', NULL);
INSERT INTO `wcs_task_item` VALUES (82, 'I190929145819', '111', 'FRT03', '', 'ARF01', 'Y', '2019-09-29 15:02:13', NULL);
INSERT INTO `wcs_task_item` VALUES (83, 'I190929145819', '013', 'ARF01', '', '2', 'Y', '2019-09-29 15:05:24', '2019-09-29 15:06:17');
INSERT INTO `wcs_task_item` VALUES (84, 'I190929145819', '115', 'RGV01', 'ARF01', '', 'Y', '2019-09-29 15:06:17', NULL);
INSERT INTO `wcs_task_item` VALUES (85, 'I190929145819', '113', 'ARF01', '', 'RGV01', 'Y', '2019-09-29 15:06:17', NULL);
INSERT INTO `wcs_task_item` VALUES (86, 'I190929145819', '023', 'RGV02', '', '241', 'Y', '2019-09-29 15:07:56', '2019-09-29 15:12:21');
INSERT INTO `wcs_task_item` VALUES (87, 'I190929145819', '024', 'RGV01', '', '239', 'Y', '2019-09-29 15:07:56', '2019-09-29 15:12:21');
INSERT INTO `wcs_task_item` VALUES (88, 'I190929145819', '115', 'RGV02', 'RGV01', '', 'Y', '2019-09-29 15:12:21', NULL);
INSERT INTO `wcs_task_item` VALUES (89, 'I190929145819', '115', 'RGV01', '', 'RGV02', 'Y', '2019-09-29 15:12:21', NULL);
INSERT INTO `wcs_task_item` VALUES (90, 'I190929145819', '021', 'RGV02', '', '251', 'Y', '2019-09-29 15:14:24', '2019-09-29 15:15:10');
INSERT INTO `wcs_task_item` VALUES (91, 'I190929145819', '117', 'ABC02', '', '200-004-001', 'Y', '2019-09-29 15:15:10', NULL);
INSERT INTO `wcs_task_item` VALUES (92, 'I190929145819', '021', 'RGV02', '', '252', 'Y', '2019-09-29 15:24:38', '2019-09-29 15:47:10');
INSERT INTO `wcs_task_item` VALUES (93, 'I190929145819', '032', 'ABC02', '', '200-001-001', 'Y', '2019-09-29 15:28:50', '2019-09-29 15:29:24');
INSERT INTO `wcs_task_item` VALUES (94, 'I190929145819', '118', 'ABC02', '', '200-001-001', 'Y', '2019-09-29 15:29:24', NULL);
INSERT INTO `wcs_task_item` VALUES (96, 'I190929145819', '031', 'ABC02', '', '200-004-001', 'Y', '2019-09-29 15:46:17', '2019-09-29 15:47:10');
INSERT INTO `wcs_task_item` VALUES (97, 'I190929145819', '117', 'ABC02', '', '200-004-001', 'Y', '2019-09-29 15:47:10', NULL);
INSERT INTO `wcs_task_item` VALUES (98, 'I190929145819', '032', 'ABC02', '', '200-001-002', 'Y', '2019-09-29 15:49:47', '2019-09-29 15:50:10');
INSERT INTO `wcs_task_item` VALUES (99, 'I190929145819', '118', 'ABC02', '', '200-001-002', 'Y', '2019-09-29 15:50:10', NULL);
INSERT INTO `wcs_task_item` VALUES (100, 'I190929161634', '012', 'ARF01', '', '3', 'Y', '2019-09-29 16:18:05', '2019-09-29 16:33:45');
INSERT INTO `wcs_task_item` VALUES (101, 'I190929161634', '022', 'RGV01', '', '001', 'Y', '2019-09-29 16:18:05', '2019-09-29 16:47:56');
INSERT INTO `wcs_task_item` VALUES (102, 'I190929161634', '031', 'ABC01', '', '001-004-001', 'Y', '2019-09-29 16:18:05', '2019-09-29 16:51:46');
INSERT INTO `wcs_task_item` VALUES (103, 'I190929161634', '113', 'ARF01', 'FRT02', '', 'Y', '2019-09-29 16:33:45', NULL);
INSERT INTO `wcs_task_item` VALUES (104, 'I190929161634', '111', 'FRT02', '', 'ARF01', 'Y', '2019-09-29 16:33:45', NULL);
INSERT INTO `wcs_task_item` VALUES (105, 'I190929161634', '013', 'ARF01', '', '2', 'Y', '2019-09-29 16:47:38', '2019-09-29 16:47:56');
INSERT INTO `wcs_task_item` VALUES (106, 'I190929161634', '115', 'RGV01', 'ARF01', '', 'Y', '2019-09-29 16:47:56', NULL);
INSERT INTO `wcs_task_item` VALUES (107, 'I190929161634', '113', 'ARF01', '', 'RGV01', 'Y', '2019-09-29 16:47:56', NULL);
INSERT INTO `wcs_task_item` VALUES (108, 'I190929161634', '021', 'RGV01', '', '001', 'Y', '2019-09-29 16:51:10', '2019-09-29 16:51:46');
INSERT INTO `wcs_task_item` VALUES (109, 'I190929161634', '117', 'ABC01', '', '001-004-001', 'Y', '2019-09-29 16:51:46', NULL);
INSERT INTO `wcs_task_item` VALUES (110, 'I190929161634', '023', 'RGV02', '', '241', 'Y', '2019-09-29 16:53:07', '2019-09-29 17:16:32');
INSERT INTO `wcs_task_item` VALUES (111, 'I190929161634', '024', 'RGV01', '', '239', 'Y', '2019-09-29 16:53:07', '2019-09-29 17:16:32');
INSERT INTO `wcs_task_item` VALUES (112, 'I190929161634', '032', 'ABC01', '', '001-001-001', 'Y', '2019-09-29 16:53:07', '2019-09-29 17:11:48');
INSERT INTO `wcs_task_item` VALUES (113, 'I190929161634', '118', 'ABC01', '', '001-001-001', 'Y', '2019-09-29 17:11:48', NULL);
INSERT INTO `wcs_task_item` VALUES (115, 'I190929161634', '115', 'RGV02', 'RGV01', '', 'Y', '2019-09-29 17:16:32', NULL);
INSERT INTO `wcs_task_item` VALUES (116, 'I190929161634', '115', 'RGV01', '', 'RGV02', 'Y', '2019-09-29 17:16:32', NULL);
INSERT INTO `wcs_task_item` VALUES (121, 'I190929172638', '012', 'ARF01', '', '3', 'Y', '2019-09-29 17:26:52', '2019-09-29 17:34:39');
INSERT INTO `wcs_task_item` VALUES (122, 'I190929172638', '022', 'RGV01', '', '001', 'Y', '2019-09-29 17:26:52', '2019-09-29 17:40:55');
INSERT INTO `wcs_task_item` VALUES (123, 'I190929172638', '031', 'ABC01', '', '001-004-001', 'Y', '2019-09-29 17:26:52', '2019-09-29 17:41:29');
INSERT INTO `wcs_task_item` VALUES (124, 'I190929172638', '113', 'ARF01', 'FRT02', '', 'Y', '2019-09-29 17:34:39', NULL);
INSERT INTO `wcs_task_item` VALUES (125, 'I190929172638', '111', 'FRT02', '', 'ARF01', 'Y', '2019-09-29 17:34:39', NULL);
INSERT INTO `wcs_task_item` VALUES (126, 'I190929172638', '013', 'ARF01', '', '2', 'Y', '2019-09-29 17:40:40', '2019-09-29 17:40:55');
INSERT INTO `wcs_task_item` VALUES (127, 'I190929172638', '115', 'RGV01', 'ARF01', '', 'Y', '2019-09-29 17:40:55', NULL);
INSERT INTO `wcs_task_item` VALUES (128, 'I190929172638', '113', 'ARF01', '', 'RGV01', 'Y', '2019-09-29 17:40:55', NULL);
INSERT INTO `wcs_task_item` VALUES (129, 'I190929172638', '021', 'RGV01', '', '002', 'Y', '2019-09-29 17:41:15', '2019-09-29 17:41:29');
INSERT INTO `wcs_task_item` VALUES (130, 'I190929172638', '117', 'ABC01', '', '001-004-001', 'Y', '2019-09-29 17:41:29', NULL);
INSERT INTO `wcs_task_item` VALUES (135, 'I190929172638', '023', 'RGV02', '', '241', 'Y', '2019-09-29 17:45:23', '2019-09-29 17:46:51');
INSERT INTO `wcs_task_item` VALUES (136, 'I190929172638', '024', 'RGV01', '', '239', 'Y', '2019-09-29 17:45:23', '2019-09-29 17:46:51');
INSERT INTO `wcs_task_item` VALUES (137, 'I190929172638', '032', 'ABC01', '', '001-001-001', 'Y', '2019-09-29 17:45:23', '2019-09-29 17:45:53');
INSERT INTO `wcs_task_item` VALUES (138, 'I190929172638', '118', 'ABC01', '', '001-001-001', 'Y', '2019-09-29 17:45:53', NULL);
INSERT INTO `wcs_task_item` VALUES (139, 'I190929172638', '115', 'RGV02', 'RGV01', '', 'Y', '2019-09-29 17:46:51', NULL);
INSERT INTO `wcs_task_item` VALUES (140, 'I190929172638', '115', 'RGV01', '', 'RGV02', 'Y', '2019-09-29 17:46:51', NULL);
INSERT INTO `wcs_task_item` VALUES (141, 'I190929172638', '021', 'RGV02', '', '251', 'Y', '2019-09-29 17:47:16', '2019-09-30 10:09:16');
INSERT INTO `wcs_task_item` VALUES (147, 'I190929172638', '031', 'ABC02', '', 'C001-001-001', 'Y', '2019-09-30 10:08:53', '2019-09-30 10:09:16');
INSERT INTO `wcs_task_item` VALUES (148, 'I190929172638', '117', 'ABC02', '', 'C001-001-001', 'Y', '2019-09-30 10:09:16', NULL);
INSERT INTO `wcs_task_item` VALUES (149, 'I190929172638', '032', 'ABC02', '', '001-001-001', 'Y', '2019-09-30 10:09:41', '2019-09-30 10:09:52');
INSERT INTO `wcs_task_item` VALUES (150, 'I190929172638', '118', 'ABC02', '', '001-001-001', 'Y', '2019-09-30 10:09:52', NULL);
INSERT INTO `wcs_task_item` VALUES (151, 'I190929161634', '021', 'RGV02', '', '252', 'Y', '2019-09-30 10:12:36', '2019-09-30 10:14:49');
INSERT INTO `wcs_task_item` VALUES (154, 'I190929161634', '031', 'ABC02', '', 'C200-001-001', 'Y', '2019-09-30 10:14:35', '2019-09-30 10:14:49');
INSERT INTO `wcs_task_item` VALUES (155, 'I190929161634', '117', 'ABC02', '', 'C200-001-001', 'Y', '2019-09-30 10:14:49', NULL);
INSERT INTO `wcs_task_item` VALUES (156, 'I190929161634', '032', 'ABC02', '', '200-001-001', 'Y', '2019-09-30 10:15:06', '2019-09-30 10:15:24');
INSERT INTO `wcs_task_item` VALUES (157, 'I190929161634', '118', 'ABC02', '', '200-001-001', 'Y', '2019-09-30 10:15:24', NULL);
INSERT INTO `wcs_task_item` VALUES (158, 'O190930101849', '032', 'ABC02', '', '200-001-001', 'Y', '2019-09-30 10:26:30', '2019-09-30 10:28:07');
INSERT INTO `wcs_task_item` VALUES (159, 'O190930101849', '021', 'RGV02', '', '251', 'Y', '2019-09-30 10:26:30', '2019-09-30 10:29:44');
INSERT INTO `wcs_task_item` VALUES (160, 'O190930101849', '013', 'ARF01', '', '2', 'Y', '2019-09-30 10:26:30', '2019-09-30 10:44:23');
INSERT INTO `wcs_task_item` VALUES (161, 'O190930101849', '117', 'ABC02', '', '200-001-001', 'Y', '2019-09-30 10:28:06', NULL);
INSERT INTO `wcs_task_item` VALUES (162, 'O190930101849', '031', 'ABC02', '', '200-004-001', 'Y', '2019-09-30 10:29:11', '2019-09-30 10:29:44');
INSERT INTO `wcs_task_item` VALUES (163, 'O190930101849', '118', 'ABC02', '', '200-004-001', 'Y', '2019-09-30 10:29:43', NULL);
INSERT INTO `wcs_task_item` VALUES (165, 'O190930101849', '023', 'RGV02', '', '241', 'Y', '2019-09-30 10:33:08', '2019-09-30 10:36:29');
INSERT INTO `wcs_task_item` VALUES (166, 'O190930101849', '024', 'RGV01', '', '239', 'Y', '2019-09-30 10:33:08', '2019-09-30 10:36:29');
INSERT INTO `wcs_task_item` VALUES (167, 'O190930101849', '116', 'RGV01', 'RGV02', '', 'Y', '2019-09-30 10:36:29', NULL);
INSERT INTO `wcs_task_item` VALUES (168, 'O190930101849', '116', 'RGV02', '', 'RGV01', 'Y', '2019-09-30 10:36:29', NULL);
INSERT INTO `wcs_task_item` VALUES (169, 'O190930101849', '022', 'RGV01', '', '001', 'Y', '2019-09-30 10:36:45', '2019-09-30 10:44:23');
INSERT INTO `wcs_task_item` VALUES (170, 'O190930101849', '114', 'ARF01', 'RGV01', '', 'Y', '2019-09-30 10:44:23', NULL);
INSERT INTO `wcs_task_item` VALUES (171, 'O190930101849', '116', 'RGV01', '', 'ARF01', 'Y', '2019-09-30 10:44:23', NULL);
INSERT INTO `wcs_task_item` VALUES (172, 'O190930101849', '012', 'ARF01', '', '2', 'Y', '2019-09-30 10:44:39', '2019-09-30 10:44:56');
INSERT INTO `wcs_task_item` VALUES (173, 'O190930101849', '112', 'FRT01', 'ARF01', '', 'Y', '2019-09-30 10:44:56', NULL);
INSERT INTO `wcs_task_item` VALUES (174, 'O190930101849', '114', 'ARF01', '', 'FRT01', 'Y', '2019-09-30 10:44:56', NULL);
INSERT INTO `wcs_task_item` VALUES (175, 'O190930104904', '032', 'ABC01', '', '001-001-002', 'Y', '2019-09-30 10:53:21', '2019-09-30 10:54:02');
INSERT INTO `wcs_task_item` VALUES (176, 'O190930104904', '021', 'RGV01', '', '001', 'Y', '2019-09-30 10:53:21', '2019-09-30 10:54:33');
INSERT INTO `wcs_task_item` VALUES (177, 'O190930104904', '013', 'ARF01', '', '2', 'Y', '2019-09-30 10:53:21', '2019-09-30 11:02:54');
INSERT INTO `wcs_task_item` VALUES (178, 'O190930104904', '117', 'ABC01', '', '001-001-002', 'Y', '2019-09-30 10:54:02', NULL);
INSERT INTO `wcs_task_item` VALUES (179, 'O190930104904', '031', 'ABC01', '', '001-004-001', 'Y', '2019-09-30 10:54:22', '2019-09-30 10:54:33');
INSERT INTO `wcs_task_item` VALUES (180, 'O190930104904', '118', 'ABC01', '', '001-004-001', 'Y', '2019-09-30 10:54:33', NULL);
INSERT INTO `wcs_task_item` VALUES (181, 'O190930104904', '021', 'RGV01', '', '002', 'Y', '2019-09-30 10:59:22', '2019-09-30 11:01:04');
INSERT INTO `wcs_task_item` VALUES (182, 'O190930104904', '032', 'ABC01', '', '001-001-001', 'Y', '2019-09-30 10:59:23', '2019-09-30 11:00:22');
INSERT INTO `wcs_task_item` VALUES (183, 'O190930104904', '117', 'ABC01', '', '001-001-001', 'Y', '2019-09-30 11:00:22', NULL);
INSERT INTO `wcs_task_item` VALUES (184, 'O190930104904', '031', 'ABC01', '', '001-004-001', 'Y', '2019-09-30 11:00:49', '2019-09-30 11:01:04');
INSERT INTO `wcs_task_item` VALUES (185, 'O190930104904', '118', 'ABC01', '', '001-004-001', 'Y', '2019-09-30 11:01:04', NULL);
INSERT INTO `wcs_task_item` VALUES (186, 'O190930104904', '022', 'RGV01', '', '001', 'Y', '2019-09-30 11:02:39', '2019-09-30 11:02:54');
INSERT INTO `wcs_task_item` VALUES (187, 'O190930104904', '114', 'ARF01', 'RGV01', '', 'Y', '2019-09-30 11:02:54', NULL);
INSERT INTO `wcs_task_item` VALUES (188, 'O190930104904', '116', 'RGV01', '', 'ARF01', 'Y', '2019-09-30 11:02:54', NULL);
INSERT INTO `wcs_task_item` VALUES (189, 'O190930104904', '012', 'ARF01', '', '2', 'Y', '2019-09-30 11:03:11', '2019-09-30 11:03:21');
INSERT INTO `wcs_task_item` VALUES (190, 'O190930104904', '112', 'FRT01', 'ARF01', '', 'Y', '2019-09-30 11:03:21', NULL);
INSERT INTO `wcs_task_item` VALUES (191, 'O190930104904', '114', 'ARF01', '', 'FRT01', 'Y', '2019-09-30 11:03:21', NULL);
INSERT INTO `wcs_task_item` VALUES (192, 'O190930110627', '032', 'ABC02', '', '200-001-002', 'Y', '2019-09-30 11:11:16', '2019-09-30 11:12:14');
INSERT INTO `wcs_task_item` VALUES (193, 'O190930110627', '021', 'RGV02', '', '251', 'Y', '2019-09-30 11:11:16', '2019-09-30 11:12:39');
INSERT INTO `wcs_task_item` VALUES (194, 'O190930110627', '013', 'ARF02', '', '2', 'Y', '2019-09-30 11:11:16', '2019-09-30 11:20:50');
INSERT INTO `wcs_task_item` VALUES (195, 'O190930110627', '117', 'ABC02', '', '200-001-002', 'Y', '2019-09-30 11:12:14', NULL);
INSERT INTO `wcs_task_item` VALUES (196, 'O190930110627', '031', 'ABC02', '', '200-004-001', 'Y', '2019-09-30 11:12:28', '2019-09-30 11:12:39');
INSERT INTO `wcs_task_item` VALUES (197, 'O190930110627', '118', 'ABC02', '', '200-004-001', 'Y', '2019-09-30 11:12:39', NULL);
INSERT INTO `wcs_task_item` VALUES (198, 'O190930110627', '021', 'RGV02', '', '252', 'Y', '2019-09-30 11:15:32', '2019-09-30 11:17:19');
INSERT INTO `wcs_task_item` VALUES (199, 'O190930110627', '032', 'ABC02', '', '200-001-001', 'Y', '2019-09-30 11:15:33', '2019-09-30 11:16:06');
INSERT INTO `wcs_task_item` VALUES (200, 'O190930110627', '117', 'ABC02', '', '200-001-001', 'Y', '2019-09-30 11:16:05', NULL);
INSERT INTO `wcs_task_item` VALUES (201, 'O190930110627', '031', 'ABC02', '', '200-004-001', 'Y', '2019-09-30 11:16:52', '2019-09-30 11:17:19');
INSERT INTO `wcs_task_item` VALUES (202, 'O190930110627', '118', 'ABC02', '', '200-004-001', 'Y', '2019-09-30 11:17:19', NULL);
INSERT INTO `wcs_task_item` VALUES (203, 'O190930110627', '023', 'RGV02', '', '241', 'Y', '2019-09-30 11:18:51', '2019-09-30 11:20:25');
INSERT INTO `wcs_task_item` VALUES (204, 'O190930110627', '024', 'RGV01', '', '239', 'Y', '2019-09-30 11:18:52', '2019-09-30 11:20:25');
INSERT INTO `wcs_task_item` VALUES (205, 'O190930110627', '116', 'RGV01', 'RGV02', '', 'Y', '2019-09-30 11:20:25', NULL);
INSERT INTO `wcs_task_item` VALUES (206, 'O190930110627', '116', 'RGV02', '', 'RGV01', 'Y', '2019-09-30 11:20:25', NULL);
INSERT INTO `wcs_task_item` VALUES (207, 'O190930110627', '022', 'RGV01', '', '001', 'Y', '2019-09-30 11:20:41', '2019-09-30 11:20:50');
INSERT INTO `wcs_task_item` VALUES (208, 'O190930110627', '114', 'ARF02', 'RGV01', '', 'Y', '2019-09-30 11:20:50', NULL);
INSERT INTO `wcs_task_item` VALUES (209, 'O190930110627', '116', 'RGV01', '', 'ARF02', 'Y', '2019-09-30 11:20:50', NULL);
INSERT INTO `wcs_task_item` VALUES (210, 'O190930110627', '012', 'ARF02', '', '4', 'Y', '2019-09-30 11:21:12', '2019-09-30 11:21:26');
INSERT INTO `wcs_task_item` VALUES (211, 'O190930110627', '112', 'FRT03', 'ARF02', '', 'Y', '2019-09-30 11:21:26', NULL);
INSERT INTO `wcs_task_item` VALUES (212, 'O190930110627', '114', 'ARF02', '', 'FRT03', 'Y', '2019-09-30 11:21:26', NULL);
INSERT INTO `wcs_task_item` VALUES (222, 'O190930115336', '032', 'ABC01', '', '001-001-001', 'Y', '2019-09-30 13:58:53', '2019-09-30 14:04:05');
INSERT INTO `wcs_task_item` VALUES (223, 'O190930115336', '021', 'RGV01', '', '001', 'Y', '2019-09-30 13:58:53', '2019-09-30 14:07:30');
INSERT INTO `wcs_task_item` VALUES (224, 'O190930115336', '013', 'ARF01', '', '2', 'Y', '2019-09-30 13:58:53', '2019-09-30 14:10:54');
INSERT INTO `wcs_task_item` VALUES (225, 'O190930115357', '032', 'ABC02', '', '200-001-001', 'Y', '2019-09-30 13:59:07', '2019-09-30 14:04:06');
INSERT INTO `wcs_task_item` VALUES (226, 'O190930115357', '021', 'RGV02', '', '251', 'Y', '2019-09-30 13:59:07', '2019-09-30 14:07:31');
INSERT INTO `wcs_task_item` VALUES (227, 'O190930115357', '013', 'ARF02', '', '2', 'Y', '2019-09-30 13:59:07', '2019-09-30 14:16:01');
INSERT INTO `wcs_task_item` VALUES (228, 'O190930115336', '117', 'ABC01', '', '001-001-001', 'Y', '2019-09-30 14:04:05', NULL);
INSERT INTO `wcs_task_item` VALUES (229, 'O190930115357', '117', 'ABC02', '', '200-001-001', 'Y', '2019-09-30 14:04:06', NULL);
INSERT INTO `wcs_task_item` VALUES (230, 'O190930115336', '031', 'ABC01', '', '001-004-001', 'Y', '2019-09-30 14:05:47', '2019-09-30 14:07:30');
INSERT INTO `wcs_task_item` VALUES (231, 'O190930115357', '031', 'ABC02', '', '200-004-001', 'Y', '2019-09-30 14:06:12', '2019-09-30 14:07:31');
INSERT INTO `wcs_task_item` VALUES (232, 'O190930115336', '118', 'ABC01', '', '001-004-001', 'Y', '2019-09-30 14:07:30', NULL);
INSERT INTO `wcs_task_item` VALUES (233, 'O190930115357', '118', 'ABC02', '', '200-004-001', 'Y', '2019-09-30 14:07:31', NULL);
INSERT INTO `wcs_task_item` VALUES (234, 'O190930115336', '022', 'RGV01', '', '001', 'Y', '2019-09-30 14:08:58', '2019-09-30 14:10:54');
INSERT INTO `wcs_task_item` VALUES (236, 'O190930115357', '023', 'RGV02', '', '241', 'Y', '2019-09-30 14:10:35', '2019-09-30 14:15:25');
INSERT INTO `wcs_task_item` VALUES (237, 'O190930115357', '024', 'RGV01', '', '239', 'Y', '2019-09-30 14:10:35', '2019-09-30 14:15:25');
INSERT INTO `wcs_task_item` VALUES (238, 'O190930115336', '114', 'ARF01', 'RGV01', '', 'Y', '2019-09-30 14:10:53', NULL);
INSERT INTO `wcs_task_item` VALUES (239, 'O190930115336', '116', 'RGV01', '', 'ARF01', 'Y', '2019-09-30 14:10:54', NULL);
INSERT INTO `wcs_task_item` VALUES (240, 'O190930115336', '012', 'ARF01', '', '2', 'Y', '2019-09-30 14:11:32', '2019-09-30 14:11:49');
INSERT INTO `wcs_task_item` VALUES (241, 'O190930115336', '112', 'FRT01', 'ARF01', '', 'Y', '2019-09-30 14:11:49', NULL);
INSERT INTO `wcs_task_item` VALUES (242, 'O190930115336', '114', 'ARF01', '', 'FRT01', 'Y', '2019-09-30 14:11:49', NULL);
INSERT INTO `wcs_task_item` VALUES (243, 'O190930115357', '116', 'RGV01', 'RGV02', '', 'Y', '2019-09-30 14:15:25', NULL);
INSERT INTO `wcs_task_item` VALUES (244, 'O190930115357', '116', 'RGV02', '', 'RGV01', 'Y', '2019-09-30 14:15:25', NULL);
INSERT INTO `wcs_task_item` VALUES (245, 'O190930115357', '022', 'RGV01', '', '001', 'Y', '2019-09-30 14:15:45', '2019-09-30 14:16:01');
INSERT INTO `wcs_task_item` VALUES (246, 'O190930115357', '114', 'ARF02', 'RGV01', '', 'Y', '2019-09-30 14:16:01', NULL);
INSERT INTO `wcs_task_item` VALUES (247, 'O190930115357', '116', 'RGV01', '', 'ARF02', 'Y', '2019-09-30 14:16:01', NULL);
INSERT INTO `wcs_task_item` VALUES (248, 'O190930115357', '012', 'ARF02', '', '3', 'Y', '2019-09-30 14:16:14', '2019-09-30 14:16:26');
INSERT INTO `wcs_task_item` VALUES (249, 'O190930115357', '112', 'FRT02', 'ARF02', '', 'Y', '2019-09-30 14:16:26', NULL);
INSERT INTO `wcs_task_item` VALUES (250, 'O190930115357', '114', 'ARF02', '', 'FRT02', 'Y', '2019-09-30 14:16:26', NULL);
INSERT INTO `wcs_task_item` VALUES (251, 'O190930145846', '032', 'ABC01', '', '001-001-002', 'Y', '2019-09-30 15:00:17', '2019-09-30 15:03:21');
INSERT INTO `wcs_task_item` VALUES (252, 'O190930145846', '021', 'RGV01', '', '001', 'Y', '2019-09-30 15:00:17', '2019-09-30 15:06:04');
INSERT INTO `wcs_task_item` VALUES (253, 'O190930145846', '013', 'ARF01', '', '2', 'Y', '2019-09-30 15:00:17', '2019-09-30 15:11:11');
INSERT INTO `wcs_task_item` VALUES (254, 'O190930145936', '032', 'ABC02', '', '200-001-001', 'Y', '2019-09-30 15:00:23', '2019-09-30 15:03:22');
INSERT INTO `wcs_task_item` VALUES (255, 'O190930145936', '021', 'RGV02', '', '251', 'Y', '2019-09-30 15:00:23', '2019-09-30 15:06:05');
INSERT INTO `wcs_task_item` VALUES (256, 'O190930145936', '013', 'ARF02', '', '2', 'Y', '2019-09-30 15:00:23', '2019-09-30 15:13:11');
INSERT INTO `wcs_task_item` VALUES (257, 'O190930145846', '117', 'ABC01', '', '001-001-002', 'Y', '2019-09-30 15:03:21', NULL);
INSERT INTO `wcs_task_item` VALUES (258, 'O190930145936', '117', 'ABC02', '', '200-001-001', 'Y', '2019-09-30 15:03:22', NULL);
INSERT INTO `wcs_task_item` VALUES (259, 'O190930145846', '031', 'ABC01', '', '001-004-001', 'Y', '2019-09-30 15:05:17', '2019-09-30 15:06:04');
INSERT INTO `wcs_task_item` VALUES (260, 'O190930145936', '031', 'ABC02', '', '200-004-001', 'Y', '2019-09-30 15:05:17', '2019-09-30 15:06:04');
INSERT INTO `wcs_task_item` VALUES (261, 'O190930145846', '118', 'ABC01', '', '001-004-001', 'Y', '2019-09-30 15:06:04', NULL);
INSERT INTO `wcs_task_item` VALUES (262, 'O190930145936', '118', 'ABC02', '', '200-004-001', 'Y', '2019-09-30 15:06:04', NULL);
INSERT INTO `wcs_task_item` VALUES (263, 'O190930145936', '023', 'RGV02', '', '241', 'Y', '2019-09-30 15:07:48', '2019-09-30 15:12:42');
INSERT INTO `wcs_task_item` VALUES (264, 'O190930145936', '024', 'RGV01', '', '239', 'Y', '2019-09-30 15:07:49', '2019-09-30 15:12:42');
INSERT INTO `wcs_task_item` VALUES (265, 'O190930145846', '021', 'RGV01', '', '002', 'Y', '2019-09-30 15:09:15', '2019-09-30 15:10:08');
INSERT INTO `wcs_task_item` VALUES (266, 'O190930145846', '032', 'ABC01', '', '001-001-001', 'Y', '2019-09-30 15:09:15', '2019-09-30 15:09:39');
INSERT INTO `wcs_task_item` VALUES (267, 'O190930145846', '117', 'ABC01', '', '001-001-001', 'Y', '2019-09-30 15:09:39', NULL);
INSERT INTO `wcs_task_item` VALUES (268, 'O190930145846', '031', 'ABC01', '', '001-004-001', 'Y', '2019-09-30 15:09:52', '2019-09-30 15:10:08');
INSERT INTO `wcs_task_item` VALUES (269, 'O190930145846', '118', 'ABC01', '', '001-004-001', 'Y', '2019-09-30 15:10:08', NULL);
INSERT INTO `wcs_task_item` VALUES (270, 'O190930145846', '022', 'RGV01', '', '001', 'Y', '2019-09-30 15:10:59', '2019-09-30 15:11:11');
INSERT INTO `wcs_task_item` VALUES (271, 'O190930145846', '114', 'ARF01', 'RGV01', '', 'Y', '2019-09-30 15:11:11', NULL);
INSERT INTO `wcs_task_item` VALUES (272, 'O190930145846', '116', 'RGV01', '', 'ARF01', 'Y', '2019-09-30 15:11:11', NULL);
INSERT INTO `wcs_task_item` VALUES (273, 'O190930145846', '012', 'ARF01', '', '2', 'Y', '2019-09-30 15:11:44', '2019-09-30 15:11:56');
INSERT INTO `wcs_task_item` VALUES (274, 'O190930145846', '112', 'FRT01', 'ARF01', '', 'Y', '2019-09-30 15:11:56', NULL);
INSERT INTO `wcs_task_item` VALUES (275, 'O190930145846', '114', 'ARF01', '', 'FRT01', 'Y', '2019-09-30 15:11:56', NULL);
INSERT INTO `wcs_task_item` VALUES (276, 'O190930145936', '116', 'RGV01', 'RGV02', '', 'Y', '2019-09-30 15:12:42', NULL);
INSERT INTO `wcs_task_item` VALUES (277, 'O190930145936', '116', 'RGV02', '', 'RGV01', 'Y', '2019-09-30 15:12:42', NULL);
INSERT INTO `wcs_task_item` VALUES (278, 'O190930145936', '022', 'RGV01', '', '001', 'Y', '2019-09-30 15:12:57', '2019-09-30 15:13:11');
INSERT INTO `wcs_task_item` VALUES (279, 'O190930145936', '114', 'ARF02', 'RGV01', '', 'Y', '2019-09-30 15:13:11', NULL);
INSERT INTO `wcs_task_item` VALUES (280, 'O190930145936', '116', 'RGV01', '', 'ARF02', 'Y', '2019-09-30 15:13:11', NULL);
INSERT INTO `wcs_task_item` VALUES (281, 'O190930145936', '012', 'ARF02', '', '4', 'Y', '2019-09-30 15:13:24', '2019-09-30 15:13:34');
INSERT INTO `wcs_task_item` VALUES (282, 'O190930145936', '112', 'FRT03', 'ARF02', '', 'Y', '2019-09-30 15:13:34', NULL);
INSERT INTO `wcs_task_item` VALUES (283, 'O190930145936', '114', 'ARF02', '', 'FRT03', 'Y', '2019-09-30 15:13:34', NULL);

-- ----------------------------
-- View structure for wcs_command_v
-- ----------------------------
DROP VIEW IF EXISTS `wcs_command_v`;
CREATE ALGORITHM = UNDEFINED DEFINER = `root`@`localhost` SQL SECURITY DEFINER VIEW `wcs_command_v` AS select `a`.`WCS_NO` AS `WCS_NO`,`a`.`FRT` AS `FRT`,`a`.`STEP` AS `STEP`,`b`.`TASK_TYPE` AS `TASK_TYPE`,`a`.`CREATION_TIME` AS `CREATION_TIME`,`a`.`TASK_UID_1` AS `TASK_UID_1`,`b`.`BARCODE` AS `CODE_1`,`b`.`W_S_LOC` AS `LOC_FROM_1`,`b`.`W_D_LOC` AS `LOC_TO_1`,`b`.`SITE` AS `SITE_1`,`a`.`TASK_UID_2` AS `TASK_UID_2`,`c`.`BARCODE` AS `CODE_2`,`c`.`W_S_LOC` AS `LOC_FROM_2`,`c`.`W_D_LOC` AS `LOC_TO_2`,`c`.`SITE` AS `SITE_2` from ((`wcs_command_master` `a` left join `wcs_task_info` `b` on((`a`.`TASK_UID_1` = `b`.`TASK_UID`))) left join `wcs_task_info` `c` on((`a`.`TASK_UID_2` = `c`.`TASK_UID`)));

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
