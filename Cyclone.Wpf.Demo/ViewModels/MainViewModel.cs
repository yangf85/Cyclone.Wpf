using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Demo.Views;
using Cyclone.Wpf.Demo.Views.Navigation;
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
    private void SwitchView(SideMenuItemViewModel item)
    {
        if (item == null) { return; }

        CurrentView = item.Header switch
        {
            "Button" => new ButtonView(),
            "Input" => new InputView(),
            "Nesting" => new NestingView(),
            "CascadePicker" => new CascadePickerView(),
            "Loading" => new LoadingView(),
            "DataGrid" => new DataGridView(),
            "Date" => new DateView(),
            "Range" => new RangeView(),
            "TabControl" => new TabControlView(),
            "FluidTabControl" => new FluidTabControlView(),
            "ComboBox" => new ComboBoxView(),
            "ListView" => new CollectionView(),
            "ListBox" => new ListBoxView(),
            "Notification" => new NotificationView(),
            "TransferBox" => new TransferBoxView(),
            "HintBox" => new HintBoxView(),
            "Panel" => new PanelView(),
            "TransitionBox" => new TransitionBoxView(),
            "Form" => new FormView(),
            "Expander" => new ExpanderView(),
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
                    Header="Input",
                    Icon= "\xe603",
                },
                new SideMenuItemViewModel()
                {
                    Header="Range",
                    Icon= "\xe605",
                },
                 new SideMenuItemViewModel()
                {
                    Header="ComboBox",
                    Icon= "\xe665",
                },

                new SideMenuItemViewModel()
                {
                    Header="HintBox",
                    Icon= "\xe606",
                },
                new SideMenuItemViewModel()
                {
                    Header="Date",
                    Icon= "\xe604",
                },
                new SideMenuItemViewModel()
                {
                    Header="CascadePicker",
                    Icon= "\xe78a",
                },
                 new SideMenuItemViewModel()
                {
                    Header="Form",
                    Icon= "\xe60b",
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
                    Icon= "\xe627",
                },
                new SideMenuItemViewModel()
                {
                    Header="ListView",
                    Icon= "\xe6a6",
                },
                new SideMenuItemViewModel()
                {
                    Header="DataGrid",
                    Icon= "\xe751",
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
                    Icon= "\xe602",
                },
                 new SideMenuItemViewModel()
                {
                    Header="FluidTabControl",
                    Icon= "\xe6c1",
                },
                new SideMenuItemViewModel()
                {
                    Header="TransitionBox",
                    Icon= "\xe600",
                },
                new SideMenuItemViewModel()
                {
                    Header="TransferBox",
                    Icon= "\xe642",
                },
                new SideMenuItemViewModel()
                {
                    Header="Expander",
                    Icon= "\xe6dd",
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
            Header = "Loading",
            Icon = "\xe891",
        });
        Items.Add(new SideMenuItemViewModel()
        {
            Header = "Notification",
            Icon = "\xe60e",
        });
        Items.Add(new SideMenuItemViewModel()
        {
            Header = "Other",
            Icon = "\xe61a",
            Items =
            [
                new SideMenuItemViewModel()
                {
                    Header="Panel",
                    Icon= "\xe614",
                },

            ]
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