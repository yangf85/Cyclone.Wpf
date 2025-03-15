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
    /// SliderView.xaml 的交互逻辑
    /// </summary>
    public partial class RangeView : UserControl
    {
        public RangeView()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            CountdownControl.Start();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            CountdownControl.Reset();
        }

        private void CountdownControl_Completed(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("倒计时结束！");
        }
    }

    public partial class RangeViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial double Value { get; set; }
    }
}