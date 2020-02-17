using System.Windows;
using Module;
using Panuon.UI.Silver;

using ADS = WcsManager.Administartor;

namespace TestWim
{
    /// <summary>
    /// TaskTest.xaml 的交互逻辑
    /// </summary>
    public partial class TaskTest : Window
    {
        public ADS admin;
        public TaskTest(ADS ads)
        {
            InitializeComponent();
            admin = ads;

            CBfrt.Items.Add("FRT01");
            CBfrt.Items.Add("FRT02");
        }

        private void BTNtemp_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TBtaskid.Text) || string.IsNullOrEmpty(TBbarcode.Text) ||
                string.IsNullOrEmpty(TBfromloc.Text) || string.IsNullOrEmpty(TBtoloc.Text) ||
                string.IsNullOrEmpty(CBfrt.Text))
            {
                Notice.Show("需补全信息！", "错误", 3, MessageBoxIcon.Error);
                return;
            }

            //admin.AddFrtTempJob(new WmsTask()
            //{
            //    frt = CBfrt.Text,
            //    taskuid = TBtaskid.Text,
            //    tasktype = TaskTypeEnum.入库,
            //    taskstatus = WmsStatus.init,
            //    barcode = TBbarcode.Text,
            //    takesite = TBfromloc.Text,
            //    givesite = TBtoloc.Text,
            //    site = new JobSite()
            //}, CBfrt.Text);

            admin.AddAwcTempJob(new WmsTask()
            {
                dev = "",
                taskuid = TBtaskid.Text,
                tasktype = TaskTypeEnum.出库,
                taskstatus = WmsTaskStatus.init,
                barcode = TBbarcode.Text,
                takesite = TBtoloc.Text,
                givesite = TBfromloc.Text,
                site = new JobSite()
            });

            Notice.Show("OK！", "完成", 3, MessageBoxIcon.Success);
        }

        private void CHBdojob_Click(object sender, RoutedEventArgs e)
        {
            //admin.AlterDoJobIn((bool)CHBdojob.IsChecked);
            admin.AlterDoJobOut((bool)CHBdojob.IsChecked);
            Notice.Show("OK！", "完成", 3, MessageBoxIcon.Success);
        }

        private void CHBdotask_Click(object sender, RoutedEventArgs e)
        {
            admin.AlterDoTask((bool)CHBdotask.IsChecked);
            Notice.Show("OK！", "完成", 3, MessageBoxIcon.Success);
        }

    }
}
