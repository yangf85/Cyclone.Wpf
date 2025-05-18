using System;
using System.Windows;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 提供Alert窗口居中定位功能，使用WindowsNativeService类获取窗口信息
/// </summary>
internal static class AlertWindowPositioner
{
    /// <summary>
    /// 将警告窗口定位在拥有者窗口的中心
    /// </summary>
    /// <param name="alertWindow">警告窗口</param>
    /// <param name="ownerHandle">拥有者窗口句柄</param>
    /// <returns>是否成功定位</returns>
    public static bool CenterAlertInOwner(Window alertWindow, IntPtr ownerHandle)
    {
        if (alertWindow == null || !WindowsNativeService.IsValidWindow(ownerHandle))
            return false;

        try
        {
            // 获取拥有者窗口矩形
            var ownerRect = WindowsNativeService.GetWindowRectAsWpfRect(ownerHandle);
            if (!ownerRect.HasValue)
                return false;

            // 计算中心点
            double centerX = ownerRect.Value.Left + (ownerRect.Value.Width / 2);
            double centerY = ownerRect.Value.Top + (ownerRect.Value.Height / 2);

            // 定位警告窗口
            alertWindow.WindowStartupLocation = WindowStartupLocation.Manual;

            // 使用实际宽高或预设宽高
            double alertWidth = alertWindow.ActualWidth > 0 ? alertWindow.ActualWidth : alertWindow.Width;
            double alertHeight = alertWindow.ActualHeight > 0 ? alertWindow.ActualHeight : alertWindow.Height;

            alertWindow.Left = centerX - (alertWidth / 2);
            alertWindow.Top = centerY - (alertHeight / 2);

            // 确保警告窗口在屏幕边界内
            EnsureWindowInScreenBounds(alertWindow);

            // 如果是蒙版窗口的Owner，也更新蒙版窗口位置和大小
            if (alertWindow.Owner is Window maskWindow)
            {
                UpdateMaskWindow(maskWindow, ownerRect.Value);
            }

            // 记录窗口位置信息，用于调试
            System.Diagnostics.Debug.WriteLine($"Alert窗口位置: Left={alertWindow.Left}, Top={alertWindow.Top}, " +
                                             $"Width={alertWidth}, Height={alertHeight}");
            System.Diagnostics.Debug.WriteLine($"拥有者窗口: Left={ownerRect.Value.Left}, Top={ownerRect.Value.Top}, " +
                                             $"Width={ownerRect.Value.Width}, Height={ownerRect.Value.Height}");

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"居中窗口出错: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 更新蒙版窗口以匹配拥有者窗口
    /// </summary>
    private static void UpdateMaskWindow(Window maskWindow, Rect ownerRect)
    {
        if (maskWindow != null)
        {
            // 设置蒙版窗口的位置和大小与拥有者窗口一致
            maskWindow.Left = ownerRect.Left;
            maskWindow.Top = ownerRect.Top;
            maskWindow.Width = ownerRect.Width;
            maskWindow.Height = ownerRect.Height;

            // 确保蒙版窗口置顶
            maskWindow.Topmost = true;

            // 记录蒙版窗口位置信息
            System.Diagnostics.Debug.WriteLine($"蒙版窗口: Left={maskWindow.Left}, Top={maskWindow.Top}, " +
                                            $"Width={maskWindow.Width}, Height={maskWindow.Height}");
        }
    }

    /// <summary>
    /// 确保窗口在屏幕边界内
    /// </summary>
    private static void EnsureWindowInScreenBounds(Window window)
    {
        try
        {
            // 获取系统工作区
            var workArea = WindowsNativeService.GetSystemWorkArea();

            // 获取窗口的实际大小
            double windowWidth = window.ActualWidth > 0 ? window.ActualWidth : window.Width;
            double windowHeight = window.ActualHeight > 0 ? window.ActualHeight : window.Height;

            // 检查并调整左边界
            if (window.Left < workArea.Left)
            {
                window.Left = workArea.Left;
            }
            else if (window.Left + windowWidth > workArea.Right)
            {
                window.Left = workArea.Right - windowWidth;
            }

            // 检查并调整上边界
            if (window.Top < workArea.Top)
            {
                window.Top = workArea.Top;
            }
            else if (window.Top + windowHeight > workArea.Bottom)
            {
                window.Top = workArea.Bottom - windowHeight;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"调整窗口边界出错: {ex.Message}");
        }
    }
}