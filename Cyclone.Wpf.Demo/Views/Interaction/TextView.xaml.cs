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
    /// TextView.xaml 的交互逻辑
    /// </summary>
    public partial class TextView : UserControl
    {
        public TextView()
        {
            InitializeComponent();

            DataContext= new TextViewModel();
        }
    }

    public partial class TextViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Text { get; set; } = "Hello World";

        [ObservableProperty]
        public partial double Number { get; set; } = 1800d;
    }
}
