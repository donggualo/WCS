using Caliburn.Micro;
using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WCS_phase1.Hlpers;

namespace WCS_phase1.WCSWindow
{
    /// <summary>
    /// W_NdcAgv.xaml 的交互逻辑
    /// </summary>
    public partial class W_NdcAgv : UserControl
    {

        #region Property
        public IList<DataGridTestModel> TestDataList { get; set; }
        private int countItem = 0;
        #endregion

        public W_NdcAgv()
        {
            InitializeComponent();

            TestDataList = new List<DataGridTestModel>()
            {
                new DataGridTestModel(){ Name = "Chris", IsEnabled = true, Score = 98,  },
                new DataGridTestModel(){ Name = "Judy", Score = 100,  },
                new DataGridTestModel(){ Name = "Jack", IsEnabled = true, Score = 100,  },
                new DataGridTestModel(){ Name = "Mario", IsEnabled = true, Score = 100,  },
                new DataGridTestModel(){ Name = "Chris", IsEnabled = true, Score = 98,  },
                new DataGridTestModel(){ Name = "Judy", Score = 100,  },
                new DataGridTestModel(){ Name = "Jack", IsEnabled = true, Score = 100,  },
                new DataGridTestModel(){ Name = "Mario", IsEnabled = true, Score = 100,  },
                new DataGridTestModel(){ Name = "Chris", IsEnabled = true, Score = 98,  },
                new DataGridTestModel(){ Name = "Judy", Score = 100,  },
                new DataGridTestModel(){ Name = "Jack", IsEnabled = true, Score = 100,  },
                new DataGridTestModel(){ Name = "Mario", IsEnabled = true, Score = 100,  },
            };
            DgCustom.ItemsSource = TestDataList;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            TestDataList.Add(new DataGridTestModel() { Name = "Item:"+ countItem, IsEnabled = true, Score = countItem++, });
            DgCustom.Items.Refresh();
        }

        
    }


    public class DataGridTestModel
    {
        [DataGridColumn("名称", false)]
        public string Name { get; set; }

        [DataGridColumn("分数", false)]
        public int Score { get; set; }

        [DataGridColumn("已启用", false)]
        public bool IsEnabled { get; set; }

        //[DataGridColumn(true)]
        //public object CustomData { get; set; }

    }
}
