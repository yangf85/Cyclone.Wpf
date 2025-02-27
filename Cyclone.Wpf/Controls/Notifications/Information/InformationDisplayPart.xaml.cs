using System.Windows;
using ToastNotifications.Core;

namespace Cyclone.Wpf.Controls.Notifications;

/// <summary>
/// Interaction logic for InformationDisplayPart.xaml
/// </summary>
public partial class InformationDisplayPart : NotificationDisplayPart
{
    public InformationDisplayPart(InformationMessage information, MessageOptions options)
    {
        InitializeComponent();
        Bind(information);
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        Notification.Close();
    }
}