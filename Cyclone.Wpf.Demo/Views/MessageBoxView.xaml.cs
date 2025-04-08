using Cyclone.Wpf.Controls;
using Cyclone.Wpf.Demo.ViewModels;
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
    /// NotificationView.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBoxView : UserControl
    {
        public MessageBoxView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var instance = Cyclone.Wpf.Controls.NotificationService.Instance;
            instance.SetOwner(App.Current.MainWindow);

            var button = (Button)sender;
            switch (button.Content)
            {
                case "Default":
                    instance.Show($"通知消息");
                    break;

                case "Success":
                    instance.Success("成功");
                    break;

                case "Infomation":
                    instance.Information("信息");
                    break;

                case "Warning":
                    instance.Warning("警告");
                    break;

                case "Error":
                    instance.Error("错误");
                    break;
            }
        }

        private void Alert_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hello World");
        }
    }
}