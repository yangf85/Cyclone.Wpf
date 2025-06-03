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
        public partial ObservableCollection<FakerData> People { get; set; }

        [ObservableProperty]
        public partial FakerData? SelectedPerson { get; set; }

        [ObservableProperty]
        public partial FakerData? HighlightedPerson { get; set; }

        [ObservableProperty]
        public partial FakerData? SelectedGroupedPerson { get; set; }

        [ObservableProperty]
        public partial string BasicResult { get; set; } = "Selected: None";

        [ObservableProperty]
        public partial string GroupedResult { get; set; } = "Selected: None";

        partial void OnSelectedPersonChanged(FakerData? value)
        {
            BasicResult = value != null
                ? $"Selected: {value.FullName}"
                : "Selected: None";
        }

        partial void OnSelectedGroupedPersonChanged(FakerData? value)
        {
            GroupedResult = value != null
                ? $"Selected: {value.FullName} from {value.City}"
                : "Selected: None";
        }

        public HintBoxViewModel()
        {
            People = new ObservableCollection<FakerData>(
                FakerDataHelper.GenerateFakerDataCollection(100)
            );
        }
    }
}