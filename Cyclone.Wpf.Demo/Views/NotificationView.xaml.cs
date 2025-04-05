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
using NotificationService = Cyclone.Wpf.Demo.ViewModels.NotificationService;

namespace Cyclone.Wpf.Demo.Views
{
    /// <summary>
    /// NotificationView.xaml 的交互逻辑
    /// </summary>
    public partial class NotificationView : UserControl
    {
        public NotificationView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            switch (button.Content)
            {
                case "Success":
                    NotificationService.Instance.ShowSuccess("成功");
                    break;

                case "Info":
                    NotificationService.Instance.ShowInfo("信息");
                    break;

                case "Warning":
                    NotificationService.Instance.ShowWarning("警告");
                    break;

                case "Error":
                    NotificationService.Instance.ShowError("错误");
                    break;

                case "Custom":

                    break;
            }
        }

        private void Button_Custom(object sender, RoutedEventArgs e)
        {
            var window = new NotificationWindow();
            window.Show();
        }
    }
}