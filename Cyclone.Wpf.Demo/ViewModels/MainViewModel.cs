﻿using CommunityToolkit.Mvvm.ComponentModel;
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
    public MainViewModel()
    {
        CurrentView = new ButtonView();
    }

    [ObservableProperty]
    public partial SideMenuViewModel SideMenu { get; set; } = new SideMenuViewModel();

    [ObservableProperty]
    public partial object CurrentView { get; set; } = new object();

    [RelayCommand]
    private void SwitchView(object item)
    {
        // 处理传入的参数，统一为SideMenuItemViewModel
        if (item is not SideMenuItemViewModel menuItem) { return; }

        CurrentView = menuItem.Header switch
        {
            "Button" => new ButtonView(),
            "Input" => new InputView(),
            "Menu" => new MenuView(),
            "TreeView" => new TreeControlView(),
            "CascadePicker" => new CascadePickerView(),
            "Loading" => new LoadingView(),
            "DataGrid" => new DataGridView(),
            "DateTime" => new DateView(),
            "Range" => new RangeView(),
            "TabControl" => new TabControlView(),
            "FluidTabControl" => new FluidTabControlView(),
            "ComboBox" => new ComboBoxView(),
            "ListView" => new CollectionView(),
            "ListBox" => new ListBoxView(),
            "Notification" => new MessageBoxView(),
            "TransferBox" => new TransferBoxView(),
            "HintBox" => new HintBoxView(),
            "CyclicPanel" => new CyclicPanelView(),
            "SpacingUniformGrid" => new SpacingUniformGridView(),
            "SpacingStackPanel" => new SpacingStackPanelView(),
            "FisheyePanel" => new FisheyePanelView(),
            "WaterfallPanel" => new WaterfallPanelView(),
            "TilePanel" => new TilePanelView(),
            "TransitionBox" => new TransitionBoxView(),
            "Form" => new FormView(),
            "Expander" => new ExpanderView(),
            "Carousel" => new CarouselView(),
            "Drawer" => new DrawerView(),
            "FilterBox" => new FilterBoxView(),
            "IconBox" => new IconBoxView(),
            "Stepper" => new StepperView(),
            "Breadcrumb" => new BreadcrumbBarView(),
            "ColorPicker" => new ColorPickerView(),
            "CountDown" => new CountDownView(),
            "LcdDisplayer" => new LcdDisplayerView(),
            "EnumSelector" => new EnumSelectorView(),
            "SectionHeader" => new SectionHeaderView(),
            "Test" => new TestView(),
            _ => null,
        };
    }
}

public partial class SideMenuViewModel : ObservableObject
{
    public SideMenuViewModel()
    {
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Interaction",
            Icon = "\xe60f",
            Items =
            [
                new SideMenuItemViewModel
                {
                    Header="Button",
                    Icon= "\xe605",
                },
                new SideMenuItemViewModel
                {
                    Header="Input",
                    Icon= "\xe603",
                },
                new SideMenuItemViewModel
                {
                    Header="Range",
                    Icon= "\xe605",
                },
                 new SideMenuItemViewModel
                {
                    Header="ComboBox",
                    Icon= "\xe665",
                },

                new SideMenuItemViewModel
                {
                    Header="HintBox",
                    Icon= "\xe606",
                },
                new SideMenuItemViewModel
                {
                    Header="DateTime",
                    Icon= "\xe604",
                },
                new SideMenuItemViewModel
                {
                    Header="CascadePicker",
                    Icon= "\xe78a",
                },
                  new SideMenuItemViewModel
                {
                    Header="EnumSelector",
                    Icon= "\xe71a",
                },
                new SideMenuItemViewModel
                {
                    Header="FilterBox",
                    Icon= "\xe74c",
                },
                new SideMenuItemViewModel
                {
                    Header="Form",
                    Icon= "\xe60b",
                },
                new SideMenuItemViewModel
                {
                    Header="SectionHeader",
                    Icon= "\xe888",
                },
                new SideMenuItemViewModel
                {
                    Header="ColorPicker",
                    Icon= "\xe8fb",
                },

            ]
        });
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Collection",
            Icon = "\xe6d5",
            Items =
            [

                new SideMenuItemViewModel
                {
                    Header="ListBox",
                    Icon= "\xe627",
                },
                new SideMenuItemViewModel
                {
                    Header="ListView",
                    Icon= "\xe6a6",
                },
                new SideMenuItemViewModel
                {
                    Header="DataGrid",
                    Icon= "\xe751",
                },
                new SideMenuItemViewModel
                {
                    Header="Carousel",
                    Icon= "\xe6d2",
                },

            ]
        });
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Navigation",
            Icon = "\xe81a",
            Items =
            [
                new SideMenuItemViewModel
                {
                    Header="TabControl",
                    Icon= "\xe602",
                },
                 new SideMenuItemViewModel
                {
                    Header="FluidTabControl",
                    Icon= "\xe609",
                },
                new SideMenuItemViewModel
                {
                    Header="TransitionBox",
                    Icon= "\xe64a",
                },
                new SideMenuItemViewModel
                {
                    Header="TransferBox",
                    Icon= "\xe642",
                },
                new SideMenuItemViewModel
                {
                    Header="Expander",
                    Icon= "\xe6dd",
                },
                new SideMenuItemViewModel
                {
                    Header="Drawer",
                    Icon= "\xe650",
                },
                 new SideMenuItemViewModel
                {
                    Header="Stepper",
                    Icon= "\xe756",
                },
                new SideMenuItemViewModel
                {
                    Header="Breadcrumb",
                    Icon= "\xe8d4",
                },

            ]
        });

        Items.Add(new SideMenuItemViewModel
        {
            Header = "Hierarchy",
            Icon = "\xe817",
            Items =
            [
                new SideMenuItemViewModel
                {
                    Header="Menu",
                    Icon= "\xe633",
                },
                new SideMenuItemViewModel
                {
                    Header="TreeView",
                    Icon= "\xe970",
                },
            ]
        });

        Items.Add(new SideMenuItemViewModel
        {
            Header = "Notification",
            Icon = "\xe60e",
        });
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Panel",
            Icon = "\xe614",
            Items =
            [
                new SideMenuItemViewModel
                {
                    Header="CyclicPanel",
                    Icon= "\xe863",
                },
                new SideMenuItemViewModel
                {
                    Header="SpacingUniformGrid",
                    Icon= "\xe608",
                },
                new SideMenuItemViewModel
                {
                    Header="SpacingStackPanel",
                    Icon= "\xe668",
                },
                 new SideMenuItemViewModel
                {
                    Header="FisheyePanel",
                    Icon= "\xe60a",
                },
                 new SideMenuItemViewModel
                {
                    Header="WaterfallPanel",
                    Icon= "\xe607",
                },
                 new SideMenuItemViewModel
                {
                    Header="TilePanel",
                    Icon= "\xe62b",
                },

            ]
        });
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Other",
            Icon = "\xe61a",
            Items =
            [
                new SideMenuItemViewModel
                {
                    Header = "Loading",
                    Icon = "\xe891",
                },
                new SideMenuItemViewModel
                {
                    Header = "CountDown",
                    Icon = "\xe661",
                },
               new SideMenuItemViewModel
               {
                   Header="LcdDisplayer",
                   Icon = "\xe699",
               },
               new SideMenuItemViewModel
               {
                   Header="IconBox",
                   Icon = "\xe617",
               }
            ]
        });
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Test",
            Icon = "\xe629",
        });
    }

    [ObservableProperty]
    public partial ObservableCollection<SideMenuItemViewModel> Items { get; set; } = [];
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