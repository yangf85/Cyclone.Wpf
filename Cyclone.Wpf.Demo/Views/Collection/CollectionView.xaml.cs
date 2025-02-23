using CommunityToolkit.Mvvm.ComponentModel;
using Cyclone.Wpf.Controls;
using Cyclone.Wpf.Demo.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// ListView.xaml 的交互逻辑
/// </summary>
public partial class CollectionView : UserControl
{
    public CollectionView()
    {
        InitializeComponent();
        DataContext = new CollectionViewModel();
    }
}

public partial class CollectionViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<FakerData> Data { get; set; } = [];

    public CollectionViewModel()
    {
        Data = [.. FakerDataHelper.GenerateFakerDataCollection(20)];
    }
}