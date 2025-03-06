using CommunityToolkit.Mvvm.ComponentModel;
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
    /// HintBoxView.xaml 的交互逻辑
    /// </summary>
    public partial class HintBoxView : UserControl
    {
        public HintBoxView()
        {
            InitializeComponent();
            DataContext= new HintBoxViewModel();
        }

        private void HintBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = e.AddedItems;
        }
    }

    public partial class HintBoxViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial ObservableCollection<FakerData> Data { get; set; } = [];

        public HintBoxViewModel()
        {
            Data =new ObservableCollection<FakerData>( FakerDataHelper.GenerateFakerDataCollection(100));
        }
    }
}
