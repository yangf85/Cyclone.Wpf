using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// AlertService的扩展方法
/// </summary>
public static class AlertServiceExtension
{
    public static bool? Messgae(this IAlertService self, string message, string title = "提示")
    {
        var content = new AlertMessageContent()
        {
            DataContext = message,
        };

        var service = AlertService.Instance;
        service.Option.ButtonType = AlertButton.Yes;
        return service.Show(content, title);
    }

    /// <summary>
    /// 显示带有确定按钮的信息警告
    /// </summary>
    public static bool? Information(this IAlertService self, string message, string title = "信息")
    {
        var content = new AlertInformationContent()
        {
            DataContext = message,
        };
        var service = AlertService.Instance;
        service.Option.ButtonType = AlertButton.YesNo;
        return service.Show(content, title);
    }

    /// <summary>
    /// 显示带有确定按钮的成功警告
    /// </summary>
    public static bool? Success(this IAlertService self, string message, string title = "成功")
    {
        var content = new AlertSuccessContent()
        {
            DataContext = message,
        };

        var service = AlertService.Instance;
        service.Option.ButtonType = AlertButton.YesNo;
        return service.Show(content, title);
    }

    /// <summary>
    /// 显示带有确定按钮的警告信息
    /// </summary>
    public static bool? Warning(this IAlertService self, string message, string title = "警告")
    {
        var content = new AlertWarningContent()
        {
            DataContext = message,
        };
        var service = AlertService.Instance;
        service.Option.ButtonType = AlertButton.YesNo;
        return service.Show(content, title);
    }

    /// <summary>
    /// 显示带有确定按钮的错误警告
    /// </summary>
    public static bool? Error(this IAlertService self, string message, string title = "错误")
    {
        var content = new AlertWarningContent()
        {
            DataContext = message,
        };
        var service = new AlertService();
        service.Option.ButtonType = AlertButton.YesNo;
        return service.Show(content, title);
    }

    /// <summary>
    /// 显示带有确定和取消按钮的确认警告
    /// </summary>
    public static bool? Question(this IAlertService self, string message, string title = "疑问")
    {
        var content = new AlertQuestionContent()
        {
            DataContext = message,
        };
        var service = AlertService.Instance;
        service.Option.ButtonType = AlertButton.YesNo;
        return service.Show(content, title);
    }
}