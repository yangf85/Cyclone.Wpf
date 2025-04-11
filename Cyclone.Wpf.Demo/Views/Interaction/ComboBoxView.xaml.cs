using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Demo.Helper;
using System;
using System.Collections;
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
    /// ComboBoxView.xaml 的交互逻辑
    /// </summary>
    public partial class ComboBoxView : UserControl
    {
        public ComboBoxView()
        {
            InitializeComponent();
            DataContext = new ComboBoxViewModel();
        }
    }

    public partial class ComboBoxViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial ObservableCollection<FakerData> Data { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<FakerData> SelectedItems { get; set; } = [];

        public ComboBoxViewModel()
        {
            Data = new ObservableCollection<FakerData>(FakerDataHelper.GenerateFakerDataCollection(10));
        }

        [RelayCommand]
        void SwitchItem(FakerData item)
        {
            if (item != null)
            {
                MessageBox.Show($"{item.FirstName} {item.LastName}");
            }
        }

        [RelayCommand]
        void ItemSelected(ObservableCollection<FakerData> items)
        {
            var current = SelectedItems;
            MessageBox.Show("Item Selected");
        }
    }
}