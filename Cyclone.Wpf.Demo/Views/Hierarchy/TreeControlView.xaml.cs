using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using Cyclone.Wpf.Demo.Helper;
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
    /// TreeControlView.xaml 的交互逻辑
    /// </summary>
    public partial class TreeControlView : UserControl
    {
        public TreeControlView()
        {
            InitializeComponent();
            DataContext = new TreeControlViewModel();
        }
    }

    public partial class TreeControlViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial TreeViewModel TreeViewModel { get; set; } = new TreeViewModel();
    }

    public partial class TreeViewModel : ObservableObject
    {
        public TreeViewModel()
        {
        }

        [ObservableProperty]
        public partial City City { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<City> Cities { get; set; }

        [ObservableProperty]
        public partial string Text { get; set; } = "NewYork";

        [RelayCommand]
        private void GetCity()
        {
            if (City != null)
            {
                System.Windows.MessageBox.Show($"选择的城市为：{City}");
            }
        }
    }
}