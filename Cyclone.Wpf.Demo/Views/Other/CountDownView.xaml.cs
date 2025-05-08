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
    /// CountDown.xaml 的交互逻辑
    /// </summary>
    public partial class CountDownView : UserControl
    {
        public CountDownView()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            countdown.Start();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            countdown.Pause();
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            countdown.Resume();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            countdown.Reset();
        }

        private void Countdown_CountdownCompleted(object sender, System.EventArgs e)
        {
            MessageBox.Show("倒计时完成！", "提示");
        }
    }
}