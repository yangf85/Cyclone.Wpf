using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Demo.Views;

public partial class TabControlView : UserControl
{
    public TabControlView()
    {
        InitializeComponent();
        DataContext = new TabControlViewModel();
    }
}

/// <summary>
/// TabItem 的 ViewModel
/// </summary>
public partial class TabItemViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Header { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; }

    [ObservableProperty]
    public partial Brush IconColor { get; set; }
}

/// <summary>
/// 主 ViewModel
/// </summary>
public partial class TabControlViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<TabItemViewModel> TabItems { get; set; }

    [ObservableProperty]
    public partial TabItemViewModel? SelectedTab { get; set; }

    private void InitializeTabItems()
    {
        TabItems = new ObservableCollection<TabItemViewModel>
        {
            new TabItemViewModel
            {
                Header = "用户列表",
                Title = "用户管理系统",
                Description = "这里显示所有注册用户的信息，包括基本资料和联系方式。",
                IconColor = new SolidColorBrush(Colors.Blue)
            },
            new TabItemViewModel
            {
                Header = "数据统计",
                Title = "数据分析面板",
                Description = "展示系统中的各项数据统计和分析结果。",
                IconColor = new SolidColorBrush(Colors.Green)
            },
            new TabItemViewModel
            {
                Header = "系统设置",
                Title = "系统配置中心",
                Description = "管理系统的各项配置参数和选项。",
                IconColor = new SolidColorBrush(Colors.Orange)
            },
            new TabItemViewModel
            {
                Header = "日志记录",
                Title = "操作日志查看",
                Description = "查看系统操作日志和用户活动记录。",
                IconColor = new SolidColorBrush(Colors.Red)
            }
        };

        SelectedTab = TabItems[0];
    }

    public TabControlViewModel()
    {
        InitializeTabItems();
    }
}