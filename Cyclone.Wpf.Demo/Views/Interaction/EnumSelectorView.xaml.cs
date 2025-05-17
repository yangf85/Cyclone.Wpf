using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// EnumSelectorView.xaml 的交互逻辑
/// </summary>
public partial class EnumSelectorView : UserControl
{
    public EnumSelectorView()
    {
        InitializeComponent();
        DataContext = new EnumSelectorViewModel();
    }
}

public partial class EnumSelectorViewModel : ObservableObject
{
    [ObservableProperty]
    public partial WeekDays WeekDays { get; set; } = WeekDays.Sunday | WeekDays.Monday;

    [ObservableProperty]
    public partial VipLevel VipLevel { get; set; } = VipLevel.Silver;

    [RelayCommand]
    private void ShowFlagEnum()
    {
        NotificationService.Instance.Information("当前选择的星期: " + WeekDays.ToString());
    }

    [RelayCommand]
    private void ShowEnum()
    {
        NotificationService.Instance.Information("当前选择的会员等级: " + VipLevel.ToString());
    }
}

[Flags]
public enum WeekDays
{
    [Description("无")]
    None = 0,

    [Description("星期一")]
    Monday = 1,

    [Description("星期二")]
    Tuesday = 2,

    [Description("星期三")]
    Wednesday = 4,

    [Description("星期四")]
    Thursday = 8,

    [Description("星期五")]
    Friday = 16,

    [Description("星期六")]
    Saturday = 32,

    [Description("星期天")]
    Sunday = 64,

    [Description("工作日")]
    WorkDay = Monday | Tuesday | Wednesday | Thursday | Friday,

    [Description("周末")]
    Weekend = Saturday | Sunday,

    [Description("所有")]
    All = Saturday | Sunday | Friday | Thursday | Wednesday | Tuesday | Monday
}

public enum VipLevel
{
    [Description("无")]
    None = 0,

    [Description("青铜")]
    Bronze = 1,

    [Description("白银")]
    Silver = 2,

    [Description("黄金")]
    Gold = 3,

    [Description("铂金")]
    Diamond = 4
}