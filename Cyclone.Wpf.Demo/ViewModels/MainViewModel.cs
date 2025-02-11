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

public partial class MainViewModel:ObservableObject
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
            "Selector"=> new SelectorView(),
            _ => new object(),
        };
    }

}


public partial class SideMenuViewModel:ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<SideMenuItemViewModel> Items { get; set; } = [];


    public SideMenuViewModel()
    {
        Items.Add(new SideMenuItemViewModel()
        {
            Header= "Interaction",
            Icon= "\xe60f",
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

            ]
        });
        Items.Add(new SideMenuItemViewModel()
        {
            Header="Items",
            Icon= "\xe6d5",
        });
        Items.Add(new SideMenuItemViewModel()
        {
            Header="Navigation",
            Icon= "\xe81a",
        });
        Items.Add(new SideMenuItemViewModel()
        {
            Header="Nesting",
            Icon= "\xe81a",
        });
        Items.Add(new SideMenuItemViewModel()
        {
            Header = "Selector",
            Icon = "\xe81a",
        });
    }
}

public partial class SideMenuItemViewModel:ObservableObject
{
    [ObservableProperty]
    public partial string Header { get; set; }

    [ObservableProperty]
    public partial string Icon { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<SideMenuItemViewModel> Items { get; set; } = [];

}