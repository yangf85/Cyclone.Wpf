using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
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

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// SelectorView.xaml 的交互逻辑
/// </summary>
public partial class SelectorView : UserControl
{
    public SelectorView()
    {
        InitializeComponent();
        DataContext=new SelectorViewModel();
    }

    private void CascadePicker_SelectedChanged(object sender, RoutedEventArgs e)
    {
        var t = (CascadePicker)sender;
        var t1 = t.SelectedItem;
    }
}

public partial class SelectorViewModel : ObservableObject
{
    [ObservableProperty]
    public partial CascadePickerViewModel CascadePicker { get; set; } = new CascadePickerViewModel();
}

public partial class CascadePickerViewModel : ObservableObject
{
    public CascadePickerViewModel()
    {
        Cities =
        [
            new CityInfo
            {
                Name = "北京",
                ItemsSource = new List<CityInfo>
                    {
                        new CityInfo {Name = "西城区"}, new CityInfo {Name = "东城区"}, new CityInfo {Name = "海淀区"},
                        new CityInfo {Name = "朝阳区"}, new CityInfo {Name = "丰台区"}, new CityInfo {Name = "石景山区"}
                    }
            },
            new CityInfo
            {
                Name = "四川",
                ItemsSource = new List<CityInfo>
                    {
                        new CityInfo {Name = "成都市"},
                        new CityInfo
                        {
                            Name = "巴中市", ItemsSource = new List<CityInfo>
                            {
                                new CityInfo
                                {
                                    Name = "恩阳区"
                                },
                                new CityInfo
                                {
                                    Name = "南江县"
                                },
                                new CityInfo
                                {
                                    Name = "通江县"
                                }
                            }
                        }
                    }
            },
            new CityInfo
            {
                Name = "山东",
                ItemsSource = new List<CityInfo>
                    {
                        new CityInfo {Name = "青岛市"},
                        new CityInfo {Name = "烟台市"},
                        new CityInfo {Name = "威海市"},
                        new CityInfo {Name = "枣庄市"},
                        new CityInfo
                        {
                            Name = "潍坊市",
                            ItemsSource = new List<CityInfo>
                            {
                                new CityInfo
                                {
                                    Name = "青州市"
                                },
                                new CityInfo
                                {
                                    Name = "诸城市"
                                },
                                new CityInfo
                                {
                                    Name = "寿光市"
                                },
                                new CityInfo
                                {
                                    Name = "安丘市"
                                },
                                new CityInfo
                                {
                                    Name = "高密市"
                                },
                                new CityInfo
                                {
                                    Name = "昌邑市"
                                }
                            }
                        }
                    }
            },
        ];
    }

    [ObservableProperty]
    public partial string City { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<CityInfo> Cities { get; set; }
    

    [RelayCommand]
    private void GetCity()
    {
        if (string.IsNullOrWhiteSpace(City)) return;
        System.Windows.MessageBox.Show($"选择的城市为：{City}");
    }
}

public struct CityInfo
{
    public string Name { get; set; }
    public List<CityInfo> ItemsSource { get; set; }
}
