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
        var content = new AlertDefaultMessage()
        {
            DataContext = message,
        };

        self.Option.ButtonType = AlertButton.Ok;
        return self.Show(content, title);
    }

    /// <summary>
    /// 显示带有确定按钮的信息警告
    /// </summary>
    public static bool? Information(this IAlertService self, string message, string title = "信息")
    {
        var content = new AlertInformationMessage()
        {
            DataContext = message,
        };

        self.Option.ButtonType = AlertButton.OkCancel;
        return self.Show(content, title);
    }

    /// <summary>
    /// 显示带有确定按钮的成功警告
    /// </summary>
    public static bool? Success(this IAlertService self, string message, string title = "成功")
    {
        var content = new AlertSuccessMessage()
        {
            DataContext = message,
        };

        self.Option.ButtonType = AlertButton.OkCancel;
        return self.Show(content, title);
    }

    /// <summary>
    /// 显示带有确定按钮的警告信息
    /// </summary>
    public static bool? Warning(this IAlertService self, string message, string title = "警告")
    {
        var content = new AlertWarningMessage()
        {
            DataContext = message,
        };

        self.Option.ButtonType = AlertButton.OkCancel;
        return self.Show(content, title);
    }

    /// <summary>
    /// 显示带有确定按钮的错误警告
    /// </summary>
    public static bool? Error(this IAlertService self, string message, string title = "错误")
    {
        var content = new AlertErrorMessage()
        {
            DataContext = message,
        };

        self.Option.ButtonType = AlertButton.OkCancel;
        return self.Show(content, title);
    }

    /// <summary>
    /// 显示带有确定和取消按钮的确认警告
    /// </summary>
    public static bool? Question(this IAlertService self, string message, string title = "疑问")
    {
        var content = new AlertQuestionMessage()
        {
            DataContext = message,
        };

        self.Option.ButtonType = AlertButton.OkCancel;
        return self.Show(content, title);
    }
}