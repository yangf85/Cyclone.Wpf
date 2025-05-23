using System;
using System.Windows;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 提供Alert窗口居中定位功能，使用WindowsNativeService类获取窗口信息
/// </summary>
internal static class AlertWindowPositioner
{
    /// <summary>
    /// 将警告窗口定位在拥有者窗口的中心（改进版）
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
            if (!WindowsNativeService.GetWindowRect(ownerHandle, out var rect))
                return false;

            // 获取窗口所在显示器的DPI
            var monitor = WindowsNativeService.MonitorFromWindow(ownerHandle, 0);
            uint dpiX = 96, dpiY = 96;

            if (monitor != IntPtr.Zero)
            {
                try
                {
                    WindowsNativeService.GetDpiForMonitor(monitor, 0, out dpiX, out dpiY);
                }
                catch
                {
                    // 使用默认DPI
                }
            }

            // 计算DPI缩放
            double scaleX = 96.0 / dpiX;
            double scaleY = 96.0 / dpiY;

            // 转换owner窗口坐标到WPF单位
            double ownerLeft = rect.Left * scaleX;
            double ownerTop = rect.Top * scaleY;
            double ownerWidth = (rect.Right - rect.Left) * scaleX;
            double ownerHeight = (rect.Bottom - rect.Top) * scaleY;

            // 确保Alert窗口已经测量过
            if (alertWindow.ActualWidth == 0 || alertWindow.ActualHeight == 0)
            {
                alertWindow.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                alertWindow.Arrange(new Rect(alertWindow.DesiredSize));
            }

            // 使用实际尺寸或期望尺寸
            double alertWidth = alertWindow.ActualWidth > 0 ? alertWindow.ActualWidth : alertWindow.DesiredSize.Width;
            double alertHeight = alertWindow.ActualHeight > 0 ? alertWindow.ActualHeight : alertWindow.DesiredSize.Height;

            // 如果仍然没有尺寸，使用默认值
            if (alertWidth <= 0) alertWidth = 400;
            if (alertHeight <= 0) alertHeight = 200;

            // 计算中心位置
            alertWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            alertWindow.Left = ownerLeft + (ownerWidth - alertWidth) / 2;
            alertWindow.Top = ownerTop + (ownerHeight - alertHeight) / 2;

            // 确保窗口在屏幕内
            EnsureWindowInScreenBounds(alertWindow);

            // 如果是蒙版窗口的Owner，也更新蒙版窗口位置和大小
            if (alertWindow.Owner is Window maskWindow)
            {
                UpdateMaskWindow(maskWindow, new Rect(ownerLeft, ownerTop, ownerWidth, ownerHeight));
            }

            // 调试信息
            System.Diagnostics.Debug.WriteLine($"DPI: {dpiX}x{dpiY}, Scale: {scaleX}x{scaleY}");
            System.Diagnostics.Debug.WriteLine($"Owner原始位置: {rect.Left},{rect.Top} 大小: {rect.Right - rect.Left}x{rect.Bottom - rect.Top}");
            System.Diagnostics.Debug.WriteLine($"Owner WPF位置: {ownerLeft},{ownerTop} 大小: {ownerWidth}x{ownerHeight}");
            System.Diagnostics.Debug.WriteLine($"Alert位置: {alertWindow.Left},{alertWindow.Top} 大小: {alertWidth}x{alertHeight}");

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