using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclone.Wpf.Demo.ViewModels;

public partial class MainViewModel
{
}


public partial class SideMenuViewModel:ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<SideMenuItem> Items { get; set; } = [];


    public SideMenuViewModel()
    {
        
    }
}

public partial class SideMenuItem:ObservableObject
{
    [ObservableProperty]
    public partial string Header { get; set; }

    [ObservableProperty]
    public partial string Icon { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<SideMenuItem> Items { get; set; } = [];

}