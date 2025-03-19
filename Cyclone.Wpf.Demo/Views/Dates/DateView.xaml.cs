using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// DateView.xaml 的交互逻辑
/// </summary>
public partial class DateView : UserControl
{
    public DateView()
    {
        InitializeComponent();
        DataContext = new DateViewModel();
    }
}

public partial class DateViewModel : ObservableObject
{
    [ObservableProperty]
    public partial DateTime? Date { get; set; } = DateTime.Now;

    [ObservableProperty]
    public partial ObservableCollection<DateTime> BlockoutDates { get; set; } = [];

    [ObservableProperty]
    public partial DateTime Start { get; set; } = DateTime.Now;

    [ObservableProperty]
    public partial DateTime End { get; set; } = DateTime.Now.AddDays(5);

    public DateViewModel()
    {
        BlockoutDates.Add(DateTime.Now.AddDays(1));
        BlockoutDates.Add(DateTime.Now.AddDays(2));
        BlockoutDates.Add(DateTime.Now.AddDays(3));
    }

    [RelayCommand]
    private void AddBlockoutDate()
    {
        BlockoutDates =
        [
            DateTime.Now.AddDays(4),
            DateTime.Now.AddDays(5),
            DateTime.Now.AddDays(6),
            DateTime.Now.AddDays(7),
        ];
    }

    [RelayCommand]
    private void ShowStartAndEndDate()
    {
        MessageBox.Show($"Start: {Start}\nEnd: {End}");
    }
}