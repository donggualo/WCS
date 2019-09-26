using ModuleManager;
using ModuleManager.WCS;
using Panuon.UI.Silver;
using PubResourceManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskManager;

namespace WindowManager
{
    /// <summary>
    /// W_WcsTaskData.xaml 的交互逻辑
    /// </summary>
    public partial class W_TaskData : UserControl, ITabWin
    {
        TaskLogic _TASK;
        public W_TaskData()
        {
            InitializeComponent();
            RefreshData();
            _TASK = new TaskLogic();
        }
        /// <summary>
        /// 关闭窗口的时候执行释放的动作
        /// </summary>
        public void Close()
        {

        }
        // 设置时间格式
        private void DataGrid_TimeFormat(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
            {
                (e.Column as DataGridTextColumn).IsReadOnly = true;
                (e.Column as DataGridTextColumn).Binding.StringFormat = "yyyy/MM/dd HH:mm:ss";
            }
        }

        DataTable dt;
        String sql;

        /// <summary>
        /// 刷新清单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public void RefreshData()
        {
            try
            {
                // 清空数据
                dt = null;
                DGcommand.ItemsSource = null;
                DGitem.ItemsSource = null;

                sql = @"select WCS_NO WCS清单号,FRT 固定辊台,CREATION_TIME 创建时间,
             (case when STEP = '1' then '生成单号' 
                   when STEP = '2' then '请求执行' 
                   when STEP = '3' then '执行中' 
                   when STEP = '4' then '结束' else '' end) 清单状态,
			 (case when TASK_TYPE = '0' then 'AGV运输' 
                   when TASK_TYPE = '1' then '入库' 
                   when TASK_TYPE = '2' then '出库' 
                   when TASK_TYPE = '3' then '移仓' 
                   when TASK_TYPE = '4' then '盘点' else '' end) 作业类型,
			 TASK_UID_1 货物①任务号,LOC_FROM_1 货物①来源,LOC_TO_1 货物①目的,
			 (case when SITE_1 = 'N' then '未执行'
				   when SITE_1 = 'W' then '任务中'
				   when SITE_1 = 'Y' then '完成'
				   when SITE_1 = 'X' then '失效' else '' end) 货物①任务状态,
			 TASK_UID_2 货物②任务号,LOC_FROM_2 货物②来源,LOC_TO_2 货物②目的,
			 (case when SITE_2 = 'N' then '未执行'
				   when SITE_2 = 'W' then '任务中'
				   when SITE_2 = 'Y' then '完成'
				   when SITE_2 = 'X' then '失效' else '' end) 货物②任务状态 from wcs_command_v";
                // 获取数据
                dt = DataControl._mMySql.SelectAll(sql);
                DGcommand.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string wcsNO;

        /// <summary>
        /// 获取明细
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DGcommand_DoubleClick(object sender, System.EventArgs e)
        {
            try
            {
                // 获取清单号
                if (DGcommand.SelectedItem == null)
                {
                    return;
                }

                wcsNO = (DGcommand.SelectedItem as DataRowView).Row[0].ToString();

                GetDGitemInfo();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetDGitemInfo()
        {
            try
            {
                // 清空数据
                dt = null;
                DGitem.ItemsSource = null;

                sql = String.Format(@"select WCS_NO WCS清单号,ITEM_ID 作业类型,DEVICE 绑定设备号,LOC_FROM 来源,LOC_TO 目的,
			 (case when STATUS = 'N' then '不可执行'
			       when STATUS = 'Q' then '请求执行'
				   when STATUS = 'W' then '任务中'
				   when STATUS = 'R' then '交接中'
				   when STATUS = 'E' then '出现异常'
				   when STATUS = 'Y' then '完成任务'
				   when STATUS = 'X' then '失效' else '' end) 作业状态,CREATION_TIME 创建时间,ID
             from wcs_task_item where WCS_NO = '{0}'", wcsNO);
                // 获取数据
                dt = DataControl._mMySql.SelectAll(sql);
                DGitem.ItemsSource = dt.DefaultView;

                // 中文释义
                foreach (DataRowView dr in DGitem.ItemsSource)
                {
                    dr.Row[1] = ItemId.GetItemIdName(dr.Row[1].ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region 清单操作

        /// <summary>
        /// 生成清单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateCMD_Click(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 更新清单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateCMD_Click(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 结束清单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteCMD_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取清单号
                if (DGcommand.SelectedItem == null)
                {
                    return;
                }
                string wcs = (DGcommand.SelectedItem as DataRowView).Row[0].ToString();

                MessageBoxResult result = MessageBoxX.Show("确认结束此清单【" + wcs + "】？！", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                //更新WCS COMMAND状态——结束
                DataControl._mTaskTools.UpdateCommand(wcs, CommandStep.结束);
                // 备份任务数据
                DataControl._mTaskTools.BackupTask(wcs);
                // 解锁对应清单所有设备数据状态
                DataControl._mTaskTools.DeviceUnLock(wcs);

                RefreshData();
            }
            catch (Exception ex)
            {
                Notice.Show("结束失败： " + ex.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 逻辑执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Logic_Click(object sender, EventArgs e)
        {
            try
            {
                _TASK.Run_TaskContinued();
                _TASK.Run_InInitial();
                _TASK.Run_LinkDevice();
                _TASK.Run_OutFollow();
            }
            catch (Exception ex)
            {
                Notice.Show("执行失败： " + ex.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 分配设备
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DevPortion_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取清单号
                if (DGcommand.SelectedItem == null)
                {
                    return;
                }
                string wcs = (DGcommand.SelectedItem as DataRowView).Row[0].ToString();

                // 获取待分配设备任务
                String sql = String.Format(@"select * from WCS_TASK_ITEM where DEVICE is null and STATUS = '{1}' and WCS_NO = '{0}' order by CREATION_TIME", wcs, ItemStatus.不可执行);
                DataTable dtitem = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtitem))
                {
                    return;
                }
                List<WCS_TASK_ITEM> itemList = dtitem.ToDataList<WCS_TASK_ITEM>();
                // 遍历分配设备
                foreach (WCS_TASK_ITEM item in itemList)
                {
                    _TASK.ReadDevice(item);
                }
            }
            catch (Exception ex)
            {
                Notice.Show("分配失败： " + ex.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 生成指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendOrder_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取清单号
                if (DGcommand.SelectedItem == null)
                {
                    return;
                }
                string wcs = (DGcommand.SelectedItem as DataRowView).Row[0].ToString();

                // 获取 请求执行 的任务对应的 ITEM 资讯
                String sql = String.Format(@"select * from WCS_TASK_ITEM where STATUS in ('{1}','{2}') and WCS_NO = '{0}' order by CREATION_TIME", wcs, ItemStatus.请求执行, ItemStatus.任务中);
                DataTable dtitem = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtitem))
                {
                    return;
                }
                List<WCS_TASK_ITEM> itemList = dtitem.ToDataList<WCS_TASK_ITEM>();
                // 遍历处理任务
                foreach (WCS_TASK_ITEM item in itemList)
                {
                    _TASK.CreateAndAddTaskList(item);
                }
            }
            catch (Exception ex)
            {
                Notice.Show("生成失败： " + ex.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region 任务操作

        /// <summary>
        /// 生成任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(wcsNO))
                {
                    return;
                }
                W_TaskData_Task _task = new W_TaskData_Task(wcsNO, "", 0);
                _task.ShowDialog();

                GetDGitemInfo();
            }
            catch (Exception ex)
            {
                Notice.Show("生成失败： " + ex.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 更新任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DGitem.SelectedItem == null)
                {
                    return;
                }
                string id = (DGitem.SelectedItem as DataRowView)["ID"].ToString();
                W_TaskData_Task _task = new W_TaskData_Task(wcsNO, id, 1);
                _task.ShowDialog();

                GetDGitemInfo();
            }
            catch (Exception ex)
            {
                Notice.Show("更新失败： " + ex.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DGitem.SelectedItem == null)
                {
                    return;
                }

                MessageBoxResult result = MessageBoxX.Show("确认删除此任务？！", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                string id = (DGitem.SelectedItem as DataRowView)["ID"].ToString();
                DataControl._mMySql.ExcuteSql(string.Format(@"delete from wcs_task_item where id = '{0}'", id));

                DataControl._mTaskControler.IDeletTask(id);

                GetDGitemInfo();
            }
            catch (Exception ex)
            {
                Notice.Show("删除失败： " + ex.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        #endregion

    }
}
