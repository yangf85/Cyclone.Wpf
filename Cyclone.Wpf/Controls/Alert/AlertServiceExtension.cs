using System;
using System.Windows;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// AlertService的扩展方法
/// </summary>
public static class AlertServiceExtension
{
    /// <summary>
    /// 显示带有确定按钮的信息警告
    /// </summary>
    public static bool Information(this IAlertService self, string message, string title = "信息")
    {
        return false;
    }

    /// <summary>
    /// 显示带有确定按钮的成功警告
    /// </summary>
    public static bool Success(this IAlertService self, string message, string title = "成功")
    {
        return false;
    }

    /// <summary>
    /// 显示带有确定按钮的警告信息
    /// </summary>
    public static bool Warning(this IAlertService self, string message, string title = "警告")
    {
        return false;
    }

    /// <summary>
    /// 显示带有确定按钮的错误警告
    /// </summary>
    public static bool Error(this IAlertService self, string message, string title = "错误")
    {
        return false;
    }

    /// <summary>
    /// 显示带有确定和取消按钮的确认警告
    /// </summary>
    public static bool Confirm(this IAlertService self, string message, string title = "确认")
    {
        return false;
    }

    /// <summary>
    /// 显示带有确定和取消按钮的确认警告
    /// </summary>
    public static bool Question(this IAlertService self, string message, string title = "确认")
    {
        return false;
    }
}