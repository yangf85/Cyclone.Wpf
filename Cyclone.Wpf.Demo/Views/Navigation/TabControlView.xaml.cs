using CommunityToolkit.Mvvm.ComponentModel;
using Cyclone.Wpf.Demo.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Demo.Views
{
    /// <summary>
    /// TabControlView.xaml 的交互逻辑
    /// </summary>
    public partial class TabControlView : UserControl
    {
        public TabControlView()
        {
            InitializeComponent();
            DataContext = new TabControlViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("按钮被点击了！");
        }
    }

    /// <summary>
    /// TabItem的ViewModel
    /// </summary>
    public partial class TabItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private string header;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private Brush backgroundColor;

        [ObservableProperty]
        private Brush iconColor;

        [ObservableProperty]
        private ObservableCollection<FakerData> fakerDataList;

        public TabItemViewModel()
        {
            FakerDataList = new ObservableCollection<FakerData>();
        }
    }

    /// <summary>
    /// 主ViewModel
    /// </summary>
    public partial class TabControlViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<TabItemViewModel> tabItems;

        [ObservableProperty]
        private TabItemViewModel selectedTab;

        public TabControlViewModel()
        {
            InitializeTabItems();
        }

        private void InitializeTabItems()
        {
            TabItems = new ObservableCollection<TabItemViewModel>();

            // 创建第一个Tab
            var tab1 = new TabItemViewModel
            {
                Header = "用户列表",
                Title = "用户管理系统",
                Description = "这里显示所有注册用户的信息，包括基本资料和联系方式。",
                BackgroundColor = Brushes.Transparent,
                IconColor = new SolidColorBrush(Colors.Blue),
                FakerDataList = new ObservableCollection<FakerData>(
                    FakerDataHelper.GenerateFakerDataCollection(5))
            };

            // 创建第二个Tab
            var tab2 = new TabItemViewModel
            {
                Header = "数据统计",
                Title = "数据分析面板",
                Description = "展示系统中的各项数据统计和分析结果。",
                BackgroundColor = Brushes.Transparent,
                IconColor = new SolidColorBrush(Colors.Green),
                FakerDataList = new ObservableCollection<FakerData>(
                    FakerDataHelper.GenerateFakerDataCollection(8))
            };

            // 创建第三个Tab
            var tab3 = new TabItemViewModel
            {
                Header = "系统设置",
                Title = "系统配置中心",
                Description = "管理系统的各项配置参数和选项。",
                BackgroundColor = Brushes.Transparent,
                IconColor = new SolidColorBrush(Colors.Orange),
                FakerDataList = new ObservableCollection<FakerData>(
                    FakerDataHelper.GenerateFakerDataCollection(3))
            };

            // 创建第四个Tab
            var tab4 = new TabItemViewModel
            {
                Header = "日志记录",
                Title = "操作日志查看",
                Description = "查看系统操作日志和用户活动记录。",
                BackgroundColor = Brushes.Transparent,
                IconColor = new SolidColorBrush(Colors.Red),
                FakerDataList = new ObservableCollection<FakerData>(
                    FakerDataHelper.GenerateFakerDataCollection(10))
            };

            // 添加到集合
            TabItems.Add(tab1);
            TabItems.Add(tab2);
            TabItems.Add(tab3);
            TabItems.Add(tab4);

            // 设置默认选中第一个
            SelectedTab = tab1;
        }
    }
}