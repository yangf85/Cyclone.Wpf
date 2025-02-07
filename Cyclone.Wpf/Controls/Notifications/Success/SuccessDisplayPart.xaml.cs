using System.Windows;
using ToastNotifications.Core;

namespace Cyclone.Wpf.Controls.Notifications;

/// <summary>
/// Interaction logic for SuccessDisplayPart.xaml
/// </summary>
public partial class SuccessDisplayPart : NotificationDisplayPart
{
    public SuccessDisplayPart(SuccessMessage success)
    {
        InitializeComponent();

        Bind(success);
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        Notification.Close();
    }
}