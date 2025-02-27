using System.Windows.Controls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ToastNotifications.Core;
using ToastNotifications;
using ToastNotifications.Lifetime.Clear;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using System.Windows;
using Cyclone.Wpf.Controls;


namespace Cyclone.Wpf.Demo.ViewModels;

public interface INotificationService
{
    void ShowSuccess(string message, MessageOptions options=null);
    void ShowError(string message, MessageOptions options = null);
    void ShowWarning(string message, MessageOptions options = null);
    void ShowInfo(string message, MessageOptions options = null);
    void ShowCustom(string message, MessageOptions options = null);
    
}

public class NotificationService : INotificationService
{
    public static NotificationService Instance { get; } = new NotificationService();


    Notifier _notifier;

    private NotificationService()
    {
        _notifier = new Notifier(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.BottomRight,
                offsetX: 25,
                offsetY: 100);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(6),
                maximumNotificationCount: MaximumNotificationCount.FromCount(6));

            cfg.Dispatcher = Application.Current.Dispatcher;

            cfg.DisplayOptions.TopMost = false;
            cfg.DisplayOptions.Width = 250;
        });

        _notifier.ClearMessages(new ClearAll());
    }

    public void ShowCustom(string message, MessageOptions options = null)
    {
        return;
    }

    public void ShowError(string message, MessageOptions options = null)
    {
        _notifier.Notify(() => new ErrorMessage(message, options));
    }

    public void ShowInfo(string message, MessageOptions options = null)
    {
        _notifier.Notify(() => new InformationMessage(message, options));
        
    }

    public void ShowSuccess(string message, MessageOptions options = null)
    {
        _notifier.Notify(() => new SuccessMessage(message, options));
    }

    public void ShowWarning(string message, MessageOptions options = null)
    {
        _notifier.Notify(() => new WarningMessage(message, options));
    }
}