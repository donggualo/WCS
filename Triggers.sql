

select * from information_schema.`TRIGGERS` where TRIGGER_name ='update_command_T';							

-- wcs_task_info 触发器【 update_command_T ，AFTER , 更新 】
BEGIN
	IF new.SITE = 'Y' THEN
		SET @WCS = (SELECT wcs_command_master.WCS_NO FROM wcs_command_master 
							   WHERE wcs_command_master.TASK_UID_1 = old.TASK_UID OR wcs_command_master.TASK_UID_2 = old.TASK_UID);
	  SET @SITE1 = (SELECT wcs_command_v.SITE_1 FROM wcs_command_v WHERE wcs_command_v.WCS_NO = @WCS);
		SET @SITE2 = (SELECT wcs_command_v.SITE_2 FROM wcs_command_v WHERE wcs_command_v.WCS_NO = @WCS);
		IF @SITE1 = 'Y' AND (@SITE2 IS NULL OR @SITE2 = 'Y') THEN
			UPDATE wcs_command_master SET wcs_command_master.STEP = '4' WHERE wcs_command_master.WCS_NO = @WCS;
		END IF;
	END IF;
END