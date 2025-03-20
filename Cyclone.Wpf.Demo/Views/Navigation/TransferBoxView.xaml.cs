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
    /// TransferBoxView.xaml 的交互逻辑
    /// </summary>
    public partial class TransferBoxView : UserControl
    {
        public TransferBoxView()
        {
            InitializeComponent();
            DataContext= new TransferBoxViewModel();
        }
    }

    public partial class TransferBoxViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial ObservableCollection<FakerData> SourceData { get; set; } = [];

        [ObservableProperty]
        public partial ObservableCollection<FakerData> TargetData { get; set; } = [];


        public TransferBoxViewModel()
        {
            SourceData =new ObservableCollection<FakerData>( FakerDataHelper.GenerateFakerDataCollection(10));
        }
    }
}
