﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using Cyclone.Wpf.Demo.ViewModels;
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
public partial class CascadePickerView : UserControl
{
    public CascadePickerView()
    {
        InitializeComponent();
        DataContext = new CascadePickerViewModel();
    }
}

public partial class CascadePickerViewModel : ObservableObject
{
    [ObservableProperty]
    public partial CascadeItemViewModel CascadePicker { get; set; } = new CascadeItemViewModel();
}

public partial class CascadeItemViewModel : ObservableObject
{
    [ObservableProperty]
    public partial City City { get; set; }

    partial void OnCityChanged(City value)
    {
        NotificationService.Instance.Show($"选择的城市为：{value}");
    }

    [ObservableProperty]
    public partial ObservableCollection<City> Cities { get; set; }

    [ObservableProperty]
    public partial string Text { get; set; } = "NewYork";

    public CascadeItemViewModel()
    {
        Cities =
        [
            new City
            {
                NodePath = "北京",
                Cyties =
                    [
                        new City {NodePath = "西城区"}, new City {NodePath = "东城区"}, new City {NodePath = "海淀区"},
                        new City {NodePath = "朝阳区"}, new City {NodePath = "丰台区"}, new City {NodePath = "石景山区"}
                    ]
            },
            new City
            {
                NodePath = "四川",
                Cyties =
                    [
                        new City {NodePath = "成都市"},
                        new City
                        {
                            NodePath = "巴中市", Cyties =
                            [
                                new City
                                {
                                    NodePath = "恩阳区"
                                },
                                new City
                                {
                                    NodePath = "南江县"
                                },
                                new City
                                {
                                    NodePath = "通江县"
                                }
                            ]
                        }
                    ]
            },
            new City
            {
                NodePath = "山东",
                Cyties =
                    [
                        new City {NodePath = "青岛市"},
                        new City {NodePath = "烟台市"},
                        new City {NodePath = "威海市"},
                        new City {NodePath = "枣庄市"},
                        new City
                        {
                            NodePath = "潍坊市",
                            Cyties =
                            [
                                new City
                                {
                                    NodePath = "青州市"
                                },
                                new City
                                {
                                    NodePath = "诸城市"
                                },
                                new City
                                {
                                    NodePath = "寿光市"
                                },
                                new City
                                {
                                    NodePath = "安丘市"
                                },
                                new City
                                {
                                    NodePath = "高密市"
                                },
                                new City
                                {
                                    NodePath = "昌邑市"
                                }
                            ]
                        }
                    ]
            },
        ];
    }

    [RelayCommand]
    private void GetCity()
    {
        if (City != null)
        {
            System.Windows.MessageBox.Show($"选择的城市为：{City}");
        }
    }
}

public class City : ICascadeNode
{
    public string NodePath { get; set; } = string.Empty;

    public List<City> Cyties { get; set; } = [];

    public override string ToString()
    {
        return NodePath;
    }
}