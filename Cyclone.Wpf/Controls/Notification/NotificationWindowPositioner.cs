using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 处理通知窗口的定位
/// </summary>
internal class NotificationWindowPositioner
{
    private readonly NotificationOption _option;
    private IntPtr _ownerHandle;
    private volatile bool _useScreenForPositioning = true;

    public NotificationWindowPositioner(NotificationOption option)
    {
        _option = option ?? throw new ArgumentNullException(nameof(option));
    }

    /// <summary>
    /// 设置用于定位通知的所有者句柄
    /// </summary>
    public void SetOwner(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(windowHandle), "Invalid WindowHandle");
        }

        if (!WindowsNativeService.IsWindow(windowHandle))
        {
            throw new ArgumentException("Handle is not a Window", nameof(windowHandle));
        }

        // 原子操作设置状态
        _ownerHandle = windowHandle;
        _useScreenForPositioning = false;
    }

    /// <summary>
    /// 重置为使用屏幕坐标进行定位
    /// </summary>
    public void UseScreenPositioning()
    {
        // 原子操作设置状态
        _useScreenForPositioning = true;
    }

    /// <summary>
    /// 根据当前设置定位所有通知窗口
    /// </summary>
    public void PositionWindows(IList<NotificationWindow> activeWindows)
    {
        if (activeWindows == null || activeWindows.Count == 0)
        {
            return;
        }

        // 创建只读状态副本，避免计算过程中状态变化
        bool useScreen = _useScreenForPositioning;
        IntPtr ownerHandle = _ownerHandle;

        WindowsNativeService.RECT ownerRect;
        var screenBounds = System.Windows.SystemParameters.WorkArea;

        if (useScreen || !WindowsNativeService.IsValidWindow(ownerHandle) ||
            !WindowsNativeService.GetWindowRect(ownerHandle, out ownerRect))
        {
            // 使用屏幕作为定位参考
            ownerRect = new WindowsNativeService.RECT
            {
                Left = (int)screenBounds.Left,
                Top = (int)screenBounds.Top,
                Right = (int)screenBounds.Right,
                Bottom = (int)screenBounds.Bottom
            };
        }
        else
        {
            // 窗口坐标已经通过WindowsNativeService.GetWindowRect转换为WPF单位
            var wpfRect = WindowsNativeService.GetWindowRectAsWpfRect(ownerHandle);
            if (wpfRect.HasValue)
            {
                ownerRect = new WindowsNativeService.RECT
                {
                    Left = (int)wpfRect.Value.Left,
                    Top = (int)wpfRect.Value.Top,
                    Right = (int)wpfRect.Value.Right,
                    Bottom = (int)wpfRect.Value.Bottom
                };
            }
        }

        double baseLeft = 0;
        double baseTop = 0;
        bool isTop = false;  // 是否从顶部定位

        // 计算基础位置
        switch (_option.Position)
        {
            case NotificationPosition.TopLeft:
                baseLeft = ownerRect.Left + _option.OffsetX;
                baseTop = ownerRect.Top + _option.OffsetY;
                isTop = true;
                break;

            case NotificationPosition.TopRight:
                baseLeft = ownerRect.Right - _option.MaxWidth - _option.OffsetX;
                baseTop = ownerRect.Top + _option.OffsetY;
                isTop = true;
                break;

            case NotificationPosition.BottomLeft:
                baseLeft = ownerRect.Left + _option.OffsetX;
                baseTop = ownerRect.Bottom - _option.OffsetY;
                isTop = false;
                break;

            case NotificationPosition.BottomRight:
                baseLeft = ownerRect.Right - _option.MaxWidth - _option.OffsetX;
                baseTop = ownerRect.Bottom - _option.OffsetY;
                isTop = false;
                break;
        }

        // 确保左侧位置在屏幕边界内
        if (baseLeft + _option.MaxWidth > screenBounds.Right)
        {
            baseLeft = screenBounds.Right - _option.MaxWidth;
        }

        if (baseLeft < screenBounds.Left)
        {
            baseLeft = screenBounds.Left;
        }

        // 调整顶部位置以确保它在屏幕上
        if (isTop && baseTop < screenBounds.Top)
        {
            baseTop = screenBounds.Top;
        }
        else if (!isTop && baseTop > screenBounds.Bottom)
        {
            baseTop = screenBounds.Bottom;
        }

        // 重新排序窗口
        List<NotificationWindow> orderedWindows;

        if (isTop)
        {
            // 最新的窗口在底部（数组开始）
            orderedWindows = activeWindows.OrderBy(w => activeWindows.IndexOf(w)).ToList();
        }
        else
        {
            // 最新的窗口在顶部（数组末尾）
            orderedWindows = activeWindows.OrderByDescending(w => activeWindows.IndexOf(w)).ToList();
        }

        // 按顺序定位窗口
        double currentPosition = baseTop;

        foreach (var window in orderedWindows)
        {
            // 获取实际窗口高度
            double windowHeight = window.ActualHeight > 0 ? window.ActualHeight : _option.MaxHeight;

            // 设置水平位置
            window.Left = baseLeft;

            if (isTop)
            {
                // 顶部定位：向下增长
                window.Top = currentPosition;
                currentPosition += windowHeight + _option.Spacing;

                // 确保不超过屏幕底部
                if (window.Top + windowHeight > screenBounds.Bottom)
                {
                    window.Top = screenBounds.Bottom - windowHeight;
                }
            }
            else
            {
                // 底部定位：向上增长
                // 计算窗口顶部位置
                window.Top = currentPosition - windowHeight;
                currentPosition = window.Top - _option.Spacing;

                // 确保不超过屏幕顶部
                if (window.Top < screenBounds.Top)
                {
                    window.Top = screenBounds.Top;
                }
            }
        }
    }

    /// <summary>
    /// 根据位置为通知窗口设置动画方向
    /// </summary>
    public void SetAnimationDirection(NotificationWindow window)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        // 设置窗口动画方向
        NotificationAnimationDirection animDirection;

        switch (_option.Position)
        {
            case NotificationPosition.TopLeft:
            case NotificationPosition.BottomLeft:
                animDirection = NotificationAnimationDirection.FromLeft;
                break;

            case NotificationPosition.TopRight:
            case NotificationPosition.BottomRight:
            default:
                animDirection = NotificationAnimationDirection.FromRight;
                break;
        }

        window.AnimationDirection = animDirection;

        // 如果需要，设置初始尺寸
        if (window.ActualWidth == 0)
        {
            window.Width = _option.MaxWidth;
        }

        if (window.ActualHeight == 0)
        {
            window.Height = _option.MaxHeight;
        }
    }
}