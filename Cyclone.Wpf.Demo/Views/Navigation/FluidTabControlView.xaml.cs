using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

namespace Cyclone.Wpf.Demo.Views
{
    /// <summary>
    /// FluidTabControl.xaml 的交互逻辑
    /// </summary>
    public partial class FluidTabControlView : UserControl
    {
        public FluidTabControlView()
        {
            InitializeComponent();
            DataContext = new FluidTabControlViewModel();
        }
    }

    public partial class FluidTabControlViewModel : ObservableObject
    {
        [RelayCommand]
        void ShowSelected()
        {
            MessageBox.Show(SelectedItem?.ToString());
        }

        [ObservableProperty]
        public partial object SelectedItem { get; set; }

        [ObservableProperty]
        public partial FluidTabItemViewModel First { get; set; } =
            new FluidTabItemViewModel()
            {
                Header = "First",
                Content = "Content of First",
                Background = Brushes.Red
            };

        [ObservableProperty]
        public partial FluidTabItemViewModel Second { get; set; } =
            new FluidTabItemViewModel()
            {
                Header = "Second",
                Content = "Content of Second",
                Background = Brushes.Green
            };

        [ObservableProperty]
        public partial ObservableCollection<FluidTabItemViewModel> Items { get; set; }

        public FluidTabControlViewModel()
        {
            Items = new ObservableCollection<FluidTabItemViewModel>()
            {
                new FluidTabItemViewModel()
                {
                    Header="DataBinding Demo 1",
                    Content="This is the content of DataBinding Demo 1",
                    Background=Brushes.Blue
                },
                new FluidTabItemViewModel()
                {
                    Header="DataBinding Demo 2",
                    Content="This is the content of DataBinding Demo 2",
                    Background=Brushes.Yellow
                },
                new FluidTabItemViewModel()
                {
                    Header="DataBinding Demo 3",
                    Content="This is the content of DataBinding Demo 3",
                    Background=Brushes.Purple
                },
            };
        }
    }

    public partial class FluidTabItemViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Header { get; set; }

        [ObservableProperty]
        public partial object Content { get; set; }

        [ObservableProperty]
        public partial Brush Background { get; set; }
    }
}