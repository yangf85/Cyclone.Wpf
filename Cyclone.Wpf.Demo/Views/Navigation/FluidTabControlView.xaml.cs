using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
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
        }
    }

    public partial class FluidTabControlViewModel : ObservableObject
    {
    }

    public partial class FluidTabItemViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Header { get; set; }
    }
}