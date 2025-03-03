using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
/// DataGridView.xaml 的交互逻辑
/// </summary>
public partial class DataGridView : UserControl
{
    public DataGridView()
    {
        InitializeComponent();
        DataContext = new DataGridViewModel();
    }
}

public partial class DataGridViewModel : ObservableObject, IPagination
{
    List<FakerData> _source;

    [ObservableProperty]
    public partial int PageIndex { get; set; } = 5;

    [ObservableProperty]
    public partial int PerPageCount { get; set; } = 10;

    [ObservableProperty]
    public partial int Total { get; set; } = 100;

    [ObservableProperty]
    public partial ObservableCollection<FakerData> DataGridData { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<FakerData> SelectedItems { get; set; } = [];

    public DataGridViewModel()
    {
        _source = FakerDataHelper.GenerateFakerDataCollection(100);
        DataGridData = [.. _source.Take(50)];
        Update();
    }

    void Update()
    {
        DataGridData = [.. _source.Skip((PageIndex - 1) * PerPageCount).Take(PerPageCount)];
    }

    [RelayCommand]
    void ShowSelectedItems()
    {
        if (SelectedItems == null || SelectedItems.Count == 0)
        {
            MessageBox.Show("No selected items");
        }
        else
        {
            MessageBox.Show($"{string.Join("\n", SelectedItems)}");
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(PageIndex):

            case nameof(PerPageCount):

            case nameof(Total):
                Update();
                break;

            default: return;
        }
    }
}