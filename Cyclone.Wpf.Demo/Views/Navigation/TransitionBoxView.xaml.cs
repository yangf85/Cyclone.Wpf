using Cyclone.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Cyclone.Wpf.Demo.Views.Navigation
{
    /// <summary>
    /// TransitionBoxView.xaml 的交互逻辑
    /// </summary>
    public partial class TransitionBoxView : UserControl
    {
        public TransitionBoxView()
        {
            InitializeComponent();
            UpdateDuration();
        }

        private void Page1_Checked(object sender, RoutedEventArgs e)
        {
            MyTransitionBox.Content = Resources["Page1"];
        }

        private void Page2_Checked(object sender, RoutedEventArgs e)
        {
            MyTransitionBox.Content = Resources["Page2"];
        }

        private void Page3_Checked(object sender, RoutedEventArgs e)
        {
            MyTransitionBox.Content = Resources["Page3"];
        }

        private void TransitionSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TransitionSelector.SelectedItem is ComboBoxItem item && item.Tag is ITransition transition)
            {
                MyTransitionBox.Transition = transition;
            }
        }

        private void DurationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateDuration();
        }

        private void UpdateDuration()
        {
            // 更新过渡动画持续时间
            MyTransitionBox.TransitionDuration = new Duration(TimeSpan.FromMilliseconds(DurationSlider.Value));
        }
    }
}