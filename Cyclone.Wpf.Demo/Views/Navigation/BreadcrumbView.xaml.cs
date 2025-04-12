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
    /// BreadcrumbView.xaml 的交互逻辑
    /// </summary>
    public partial class BreadcrumbView : UserControl
    {
        public BreadcrumbView()
        {
            InitializeComponent();
        }
    }

    public partial class BreadcrumbViewModel : ObservableObject
    {
    }

    public partial class BreadcrumbItemViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Text { get; set; } = "NewYork";
    }
}