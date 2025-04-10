using System.Windows;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 通知服务扩展方法
/// </summary>
public static class NotificationServiceExtension
{
    public static void Message(this INotificationService self, string message)
    {
        var content = new NotificationDefaultMessage()
        {
            DataContext = message,
        };

        self.Show(content);
    }

    public static void Information(this INotificationService self, string message)
    {
        var content = new NotificationInformationMessage()
        {
            DataContext = message,
        };

        self.Show(content);
    }

    public static void Success(this INotificationService service, string message)
    {
        var content = new NotificationSuccessMessage()
        {
            DataContext = message,
        };

        service.Show(content);
    }

    public static void Warning(this INotificationService service, string message)
    {
        var content = new NotificationWarningMessage()
        {
            DataContext = message,
        };

        service.Show(content);
    }

    public static void Error(this INotificationService service, string message)
    {
        var content = new NotificationErrorMessage()
        {
            DataContext = message,
        };

        service.Show(content);
    }
}