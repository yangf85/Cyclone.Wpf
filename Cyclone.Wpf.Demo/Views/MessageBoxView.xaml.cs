using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using Cyclone.Wpf.Demo.Helper;
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
using System.Windows.Markup;
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
        public static DataTemplate Faker { get; set; }

        public MessageBoxView()
        {
            InitializeComponent();
            DataContext = new MessageBoxViewModel();
            Loaded += (s, e) =>
            {
                Faker = (DataTemplate)Resources["FakerData"];
            };
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
            var service = Cyclone.Wpf.Controls.AlertService.Instance;
            service.SetOwner(App.Current.MainWindow);
            var button = (Button)sender;
            switch (button.Content)
            {
                case "Message":
                    service.Messgae($"通知消息");
                    break;

                case "Success":
                    service.Success("成功");
                    break;

                case "Infomation":
                    service.Information("信息");
                    break;

                case "Warning":
                    service.Warning("警告");
                    break;

                case "Error":
                    service.Error("错误");
                    break;

                case "Question":
                    service.Question("问题");
                    break;
            }
        }
    }

    public partial class MessageBoxViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial FakerData Data { get; set; } = new FakerData();

        [RelayCommand]
        void ShowForm()
        {
            var service = new Cyclone.Wpf.Controls.AlertService();
            service.SetOwner(App.Current.MainWindow);
            service.Option.Title = "Faker";
            service.Option.ButtonType = AlertButton.YesNo;
            service.Option.AlertButtonHorizontalAlignment = HorizontalAlignment.Right;
            service.Option.Width = 600;
            service.Option.Height = 400;
            var result = service.Show(new FakerForm()
            {
                DataContext = Data,
            });

            var mm = Data;
        }

        public MessageBoxViewModel()
        {
        }
    }
}