using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Demo.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclone.Wpf.Demo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    public partial SideMenuViewModel SideMenu { get; set; } = new SideMenuViewModel();

    [ObservableProperty]
    public partial object CurrentView { get; set; } = new object();

    [RelayCommand]
    void SwitchView(SideMenuItemViewModel item)
    {
        if (item == null) { return; }

        CurrentView = item.Header switch
        {
            "Button" => new ButtonView(),
            "Text" => new TextView(),
            "Nesting" => new NestingView(),
            "Selector" => new SelectorView(),
            "Loading" => new LoadingView(),
            "DataGrid" => new DataGridView(),
            "Date" => new DateView(),
            "Range" => new RangeView(),
            "TabControl" => new TabControlView(),
            _ => new object(),
        };
    }
}

public partial class SideMenuViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<SideMenuItemViewModel> Items { get; set; } = [];

    public SideMenuViewModel()
    {
        Items.Add(new SideMenuItemViewModel()
        {
            Header = "Interaction",
            Icon = "\xe60f",
            Items =
            [
                new SideMenuItemViewModel()
                {
                    Header="Button",
                    Icon= "\xe605",
                },
                new SideMenuItemViewModel()
                {
                    Header="Text",
                    Icon= "\xe603",
                },
                new SideMenuItemViewModel()
                {
                    Header="Range",
                    Icon= "\xe605",
                },
            ]
        });
        Items.Add(new SideMenuItemViewModel()
        {
            Header = "Collection",
            Icon = "\xe6d5",
            Items =
            [
                new SideMenuItemViewModel()
                {
                    Header="ListBox",
                    Icon= "\xe6d5",
                },
                new SideMenuItemViewModel()
                {
                    Header="ListView",
                    Icon= "\xe6d5",
                },
                new SideMenuItemViewModel()
                {
                    Header="DataGrid",
                    Icon= "\xe6d5",
                },
            ]
        });
        Items.Add(new SideMenuItemViewModel()
        {
            Header = "Navigation",
            Icon = "\xe81a",
            Items =
            [
                new SideMenuItemViewModel()
                {
                    Header="TabControl",
                    Icon= "\xe81a",
                },
            ]
        });
        Items.Add(new SideMenuItemViewModel()
        {
            Header = "Nesting",
            Icon = "\xe7a3",
        });
        Items.Add(new SideMenuItemViewModel()
        {
            Header = "Selector",
            Icon = "\xe78a",
            Items =
            [
                new SideMenuItemViewModel()
                {
                    Header="Date",
                    Icon= "\xe78a",
                },
            ]
        });
        Items.Add(new SideMenuItemViewModel()
        {
            Header = "Loading",
            Icon = "\xe891",
        });
    }
}

public partial class SideMenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Header { get; set; }

    [ObservableProperty]
    public partial string Icon { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<SideMenuItemViewModel> Items { get; set; } = [];
}