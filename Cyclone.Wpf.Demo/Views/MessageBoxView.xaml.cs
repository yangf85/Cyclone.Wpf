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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var instance = Cyclone.Wpf.Controls.NotificationService.Instance;
            instance.SetOwner(App.Current.MainWindow);

            var button = (Button)sender;
            switch (button.Content)
            {
                case "Default":
                    instance.Message($"通知消息");
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
                    service.Messgae("这是一条很长很长的通知消息，用于测试文本换行效果是否正常工作。当文本内容超过容器宽度时，应该能够自动换行显示，而不是被截断或者溢出容器边界。");
                    break;

                case "Success":
                    service.Success("操作成功！您的数据已经保存到服务器，系统已自动生成备份文件并发送确认邮件到您的注册邮箱。请注意查收并妥善保管相关凭证。");
                    break;

                case "Infomation":
                    service.Information("系统信息");
                    break;

                case "Warning":
                    service.Warning("警告：检测到您的账户在异地登录，如非本人操作请立即修改密码并启用二次验证。系统将在30分钟后自动锁定账户以保护您的数据安全。请及时处理以免影响正常使用。");
                    break;

                case "Error":
                    service.Error("错误代码：0x80070057 - 参数不正确。无法连接到远程服务器，请检查网络连接状态、防火墙设置以及代理配置。如果问题持续存在，请联系系统管理员获取技术支持。详细错误日志已保存至 C:\\Logs\\error.log");
                    break;

                case "Question":
                    service.Question("您确定要执行此操作吗？此操作将永久删除所有选中的文件和文件夹，并且无法撤销恢复。建议在执行前先备份重要数据。是否继续？Are you absolutely sure you want to proceed with this irreversible action?");
                    break;
            }
        }

        public MessageBoxView()
        {
            InitializeComponent();
            DataContext = new MessageBoxViewModel();
        }
    }

    public partial class MessageBoxViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial FakerData Data { get; set; } = new FakerData();

        async Task<bool> Validate()
        {
            await Task.Delay(1000);
            if (Data.HasErrors)
            {
                var errors = Data.GetErrors();
                NotificationService.Instance.Error(string.Join("\n", errors.Select(x => x.ErrorMessage)));
                return false;
            }
            else
            {
                NotificationService.Instance.Success("提交成功");
                return true;
            }
        }

        [RelayCommand]
        async Task AysncShow()
        {
            var service = new Cyclone.Wpf.Controls.AlertService();
            service.SetOwner(App.Current.MainWindow);
            service.Option.Title = "Faker";
            service.Option.ButtonType = AlertButton.OkCancel;
            service.Option.AlertButtonGroupHorizontalAlignment = HorizontalAlignment.Right;
            await service.ShowAsync(new FakerForm() { DataContext = Data }, Validate);
        }

        [RelayCommand]
        async Task AsyncShowWithParameter(object parameter)
        {
            var service = new Cyclone.Wpf.Controls.AlertService();
            service.SetOwner(App.Current.MainWindow);
            service.Option.Title = "Faker";
            service.Option.ButtonType = AlertButton.OkCancel;
            service.Option.AlertButtonGroupHorizontalAlignment = HorizontalAlignment.Right;
            await service.ShowAsync<object>(new FakerForm() { DataContext = Data }, ShowWithParameterAsync, parameter);
        }

        async Task<bool> ShowWithParameterAsync(object parameter)
        {
            if (parameter is null)
            {
                NotificationService.Instance.Error("参数为空");
                return false;
            }
            else
            {
                await Task.Delay(1000);
                NotificationService.Instance.Success($"参数为：{parameter}");
                return true;
            }
        }

        [RelayCommand]
        private void Show()
        {
            var service = new Cyclone.Wpf.Controls.AlertService();
            service.SetOwner(App.Current.MainWindow);
            service.Option.Title = "Faker";
            service.Option.ButtonType = AlertButton.OkCancel;
            service.Option.AlertButtonGroupHorizontalAlignment = HorizontalAlignment.Right;
            service.Show(new FakerForm()
            {
                DataContext = Data,
            },
            () =>
            {
                if (Data.HasErrors)
                {
                    var errors = Data.GetErrors();
                    NotificationService.Instance.Error(string.Join("\n", errors.Select(x => x.ErrorMessage)));
                    return false;
                }
                else
                {
                    NotificationService.Instance.Success("提交成功");
                    return true;
                }
            });
        }

        public MessageBoxViewModel()
        {
        }
    }
}