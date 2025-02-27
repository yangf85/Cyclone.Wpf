using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastNotifications;
using ToastNotifications.Core;

namespace Cyclone.Wpf.Controls;

public static class NotificationExtension
{
    public static void Error(this Notifier notifier, string message, MessageOptions options = null)
    {
        notifier.Notify(() => new ErrorMessage(message));
    }

    public static void Warning(this Notifier notifier, string message, MessageOptions options = null)
    {
        notifier.Notify(() => new WarningMessage(message));
    }

    public static void Success(this Notifier notifier, string message, MessageOptions options = null)
    {
        notifier.Notify(() => new SuccessMessage(message));
    }

    public static void Information(this Notifier notifier, string message, MessageOptions options = null)
    {
        notifier.Notify(() => new InformationMessage(message, options));
    }
}