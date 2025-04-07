using System.Windows;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 通知服务扩展方法
/// </summary>
public static class NotificationServiceExtension
{
    private static ResourceDictionary _dict;

    static NotificationServiceExtension()
    {
        _dict = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Cyclone.Wpf;component/Styles/Notification.xaml", UriKind.Absolute)
        };
    }

    public static void Information(this INotificationService self, string message)
    {
        var template = _dict["Notification.Information.DataTemplate"] as DataTemplate;
        self.Show(message, template);
    }

    public static void Success(this INotificationService service, string message)
    {
        var template = _dict["Notification.Success.DataTemplate"] as DataTemplate;
        service.Show(message, template);
    }

    public static void Warning(this INotificationService service, string message)
    {
        var template = _dict["Notification.Warning.DataTemplate"] as DataTemplate;
        service.Show(message, template);
    }

    public static void Error(this INotificationService service, string message)
    {
        var template = _dict["Notification.Error.DataTemplate"] as DataTemplate;
        service.Show(message, template);
    }
}