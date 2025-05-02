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
    /// ColorPickerView.xaml 的交互逻辑
    /// </summary>
    public partial class ColorPickerView : UserControl
    {
        public ColorPickerView()
        {
            InitializeComponent();
            DataContext = new ColorPickerViewModel();
        }
    }

    public partial class ColorPickerViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial double Alpha { get; set; }

        [ObservableProperty]
        public partial double Bright { get; set; }

        [ObservableProperty]
        public partial double Saturation { get; set; }
    }
}