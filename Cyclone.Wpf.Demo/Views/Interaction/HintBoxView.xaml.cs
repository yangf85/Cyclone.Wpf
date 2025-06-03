using CommunityToolkit.Mvvm.ComponentModel;
using Cyclone.Wpf.Demo.Helper;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views
{
    /// <summary>
    /// HintBoxView.xaml 的交互逻辑 - MVVM 模式
    /// </summary>
    public partial class HintBoxView : UserControl
    {
        public HintBoxView()
        {
            InitializeComponent();
            DataContext = new HintBoxViewModel();
        }
    }

    public partial class HintBoxViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial ObservableCollection<FakerData> People1 { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<FakerData> People2 { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<FakerData> People3 { get; set; }

        [ObservableProperty]
        public partial FakerData? SelectedPerson { get; set; }

        [ObservableProperty]
        public partial FakerData? HighlightedPerson { get; set; }

        [ObservableProperty]
        public partial FakerData? GroupedPerson { get; set; }

        public HintBoxViewModel()
        {
            People1 = new ObservableCollection<FakerData>(FakerDataHelper.GenerateFakerDataCollection(100));
            People2 = new ObservableCollection<FakerData>(FakerDataHelper.GenerateFakerDataCollection(100));
            People3 = new ObservableCollection<FakerData>(FakerDataHelper.GenerateFakerDataCollection(100));
        }
    }
}