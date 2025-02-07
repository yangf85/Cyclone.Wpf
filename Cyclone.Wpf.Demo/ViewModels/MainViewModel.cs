using CommunityToolkit.Mvvm.ComponentModel;
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
}


public partial class SideMenuViewModel:ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<SideMenuItemViewModel> Items { get; set; } = [];


    public SideMenuViewModel()
    {
        Items.Add(new SideMenuItemViewModel()
        {
            Header="Input",
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
                    Header="TextBox",
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