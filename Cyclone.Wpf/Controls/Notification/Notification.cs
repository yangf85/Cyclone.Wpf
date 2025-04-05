using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

#region 核心通知类型

/// <summary>
/// 通知类型枚举
/// </summary>
public enum NotificationType
{
    Information,
    Success,
    Warning,
    Error,
    Custom
}

/// <summary>
/// 通知选项类
/// </summary>
public class NotificationOptions
{
    /// <summary>
    /// 显示持续时间
    /// </summary>
    public TimeSpan DisplayDuration { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 最大不透明度
    /// </summary>
    public double MaxOpacity { get; set; } = 0.9;

    /// <summary>
    /// 位置
    /// </summary>
    public Position Position { get; set; } = Position.BottomRight;

    /// <summary>
    /// X轴偏移量
    /// </summary>
    public double OffsetX { get; set; } = 10;

    /// <summary>
    /// Y轴偏移量
    /// </summary>
    public double OffsetY { get; set; } = 10;

    /// <summary>
    /// 通知之间的间隙
    /// </summary>
    public double NotificationGap { get; set; } = 5;

    /// <summary>
    /// 是否允许多个通知
    /// </summary>
    public bool AllowMultiple { get; set; } = true;

    /// <summary>
    /// 通知宽度
    /// </summary>
    public double Width { get; set; } = 300;

    /// <summary>
    /// 最大高度
    /// </summary>
    public double MaxHeight { get; set; } = 100;
}

/// <summary>
/// 通知位置枚举
/// </summary>
public enum Position
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

#endregion 核心通知类型

#region 通知服务

/// <summary>
/// 通知服务类
/// </summary>
public class NotificationService : IDisposable
{
    private readonly NotificationOptions _options;
    private readonly List<NotificationWindow> _activeNotifications = new List<NotificationWindow>();
    private IntPtr _ownerHandle;
    private readonly DispatcherTimer _cleanupTimer;
    private DispatcherTimer _positionCheckTimer;

    // 用于处理窗口句柄的本地方法
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    // 单例模式的静态实例
    private static NotificationService _instance;

    /// <summary>
    /// 获取通知服务的单例实例
    /// </summary>
    public static NotificationService Instance => _instance ??= new NotificationService();

    /// <summary>
    /// 使用默认选项初始化通知服务
    /// </summary>
    public NotificationService()
        : this(new NotificationOptions())
    {
    }

    /// <summary>
    /// 使用自定义选项初始化通知服务
    /// </summary>
    public NotificationService(NotificationOptions options)
    {
        _options = options;
        _cleanupTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _cleanupTimer.Tick += CleanupTimerOnTick;
        _cleanupTimer.Start();
    }

    /// <summary>
    /// 设置WPF窗口作为通知的所有者
    /// </summary>
    public void SetOwner(Window owner)
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));

        var handle = new WindowInteropHelper(owner).Handle;
        SetOwnerInternal(handle);

        // 添加WPF特定的事件处理程序
        owner.LocationChanged += (sender, args) => RepositionActiveNotifications();
        owner.SizeChanged += (sender, args) => RepositionActiveNotifications();
        owner.StateChanged += (sender, args) => RepositionActiveNotifications();
        owner.Closed += (sender, args) => Dispose();
    }

    /// <summary>
    /// 设置非WPF窗口句柄作为通知的所有者
    /// </summary>
    public void SetOwner(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
            throw new ArgumentException("窗口句柄不能为零", nameof(windowHandle));

        if (!IsWindow(windowHandle))
            throw new ArgumentException("无效的窗口句柄", nameof(windowHandle));

        SetOwnerInternal(windowHandle);
    }

    /// <summary>
    /// 将当前前台窗口设置为所有者
    /// </summary>
    public void SetOwnerToForegroundWindow()
    {
        IntPtr foregroundHandle = GetForegroundWindow();
        if (foregroundHandle != IntPtr.Zero)
        {
            SetOwnerInternal(foregroundHandle);
        }
        else
        {
            throw new InvalidOperationException("无法获取前台窗口句柄");
        }
    }

    // 内部方法，用于设置所有者句柄和跟踪
    private void SetOwnerInternal(IntPtr handle)
    {
        // 清理现有计时器
        if (_positionCheckTimer != null)
        {
            _positionCheckTimer.Stop();
            _positionCheckTimer = null;
        }

        _ownerHandle = handle;

        // 设置窗口位置跟踪
        _positionCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };

        RECT lastKnownPosition;
        GetWindowRect(_ownerHandle, out lastKnownPosition);

        _positionCheckTimer.Tick += (sender, args) =>
        {
            if (!IsWindow(_ownerHandle))
            {
                // 窗口句柄不再有效，清理资源
                _positionCheckTimer.Stop();
                Dispose();
                return;
            }

            RECT currentPosition;
            if (GetWindowRect(_ownerHandle, out currentPosition))
            {
                if (lastKnownPosition.Left != currentPosition.Left ||
                    lastKnownPosition.Top != currentPosition.Top ||
                    lastKnownPosition.Right != currentPosition.Right ||
                    lastKnownPosition.Bottom != currentPosition.Bottom)
                {
                    lastKnownPosition = currentPosition;
                    RepositionActiveNotifications();
                }
            }
        };

        _positionCheckTimer.Start();
    }

    /// <summary>
    /// 显示指定消息和类型的通知
    /// </summary>
    public void ShowNotification(string message, NotificationType type)
    {
        if (_ownerHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("在显示通知之前必须设置所有者窗口。");
        }

        ShowNotificationInternal(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(5)
        }, type);
    }

    /// <summary>
    /// 显示带有自定义内容的通知
    /// </summary>
    public void ShowNotification(UIElement content, NotificationType type)
    {
        if (_ownerHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("在显示通知之前必须设置所有者窗口。");
        }

        ShowNotificationInternal(content, type);
    }

    /// <summary>
    /// 显示带有自定义内容和背景的通知
    /// </summary>
    public void ShowNotification(UIElement content, Brush background)
    {
        if (_ownerHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("在显示通知之前必须设置所有者窗口。");
        }

        var options = new NotificationOptions
        {
            DisplayDuration = _options.DisplayDuration,
            MaxOpacity = _options.MaxOpacity,
            Position = _options.Position,
            OffsetX = _options.OffsetX,
            OffsetY = _options.OffsetY,
            NotificationGap = _options.NotificationGap,
            AllowMultiple = _options.AllowMultiple,
            Width = _options.Width,
            MaxHeight = _options.MaxHeight,
        };

        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            var notification = new NotificationWindow();
            AddNotification(notification);
        }));
    }

    private void ShowNotificationInternal(UIElement content, NotificationType type)
    {
        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            var notification = new NotificationWindow();
            AddNotification(notification);
        }));
    }

    // 将通知添加到活动列表并定位它
    private void AddNotification(NotificationWindow notification)
    {
        if (!_options.AllowMultiple && _activeNotifications.Count > 0)
        {
            // 如果不允许多个通知，移除现有通知
            foreach (var existingNotification in _activeNotifications.ToList())
            {
                existingNotification.Close();
            }
            _activeNotifications.Clear();
        }

        notification.Closed += (sender, args) => RemoveNotification(notification);
        _activeNotifications.Add(notification);
        PositionNotification(notification);
        notification.Show();
    }

    // 从活动列表中移除通知
    private void RemoveNotification(NotificationWindow notification)
    {
        _activeNotifications.Remove(notification);
        RepositionActiveNotifications();
    }

    // 根据现有通知定位新通知
    private void PositionNotification(NotificationWindow notification)
    {
        if (!IsWindow(_ownerHandle))
        {
            // 如果无法获取所有者窗口位置，使用屏幕中心
            notification.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            return;
        }

        RECT ownerRect;
        if (!GetWindowRect(_ownerHandle, out ownerRect))
        {
            // 如果无法获取所有者窗口位置，使用屏幕中心
            notification.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            return;
        }

        double left = 0;
        double startY = 0;
        bool growDown = true;

        // 根据选定的位置确定起始位置
        switch (_options.Position)
        {
            case Position.TopLeft:
                left = ownerRect.Left + _options.OffsetX;
                startY = ownerRect.Top + _options.OffsetY;
                growDown = true;
                break;

            case Position.TopRight:
                left = ownerRect.Right - _options.Width - _options.OffsetX;
                startY = ownerRect.Top + _options.OffsetY;
                growDown = true;
                break;

            case Position.BottomLeft:
                left = ownerRect.Left + _options.OffsetX;
                startY = ownerRect.Bottom - _options.OffsetY;
                growDown = false;
                break;

            case Position.BottomRight:
                left = ownerRect.Right - _options.Width - _options.OffsetX;
                startY = ownerRect.Bottom - _options.OffsetY;
                growDown = false;
                break;
        }

        notification.Left = left;
        ArrangeNotificationsVertically(notification, startY, growDown);
    }

    // 垂直排列通知的辅助方法
    private void ArrangeNotificationsVertically(NotificationWindow notification, double startY, bool growDown)
    {
        var activeNotifications = _activeNotifications
            .Where(n => n != notification)
            .OrderBy(n => growDown ? n.Top : -n.Top)
            .ToList();

        if (activeNotifications.Count == 0)
        {
            // 第一个通知
            notification.Top = growDown ? startY : startY - notification.Height;
            return;
        }

        if (growDown)
        {
            // 从顶部开始向下堆叠
            var lastNotification = activeNotifications.LastOrDefault();
            notification.Top = lastNotification.Top + lastNotification.ActualHeight + _options.NotificationGap;
        }
        else
        {
            // 从底部开始向上堆叠
            var lastNotification = activeNotifications.LastOrDefault();
            notification.Top = lastNotification.Top - notification.Height - _options.NotificationGap;
        }
    }

    // 重新定位所有活动通知
    private void RepositionActiveNotifications()
    {
        if (_activeNotifications.Count == 0)
            return;

        if (!IsWindow(_ownerHandle))
            return;

        RECT ownerRect;
        if (!GetWindowRect(_ownerHandle, out ownerRect))
            return;

        double left = 0;
        double startY = 0;
        bool growDown = true;

        // 根据选项确定起始位置
        switch (_options.Position)
        {
            case Position.TopLeft:
                left = ownerRect.Left + _options.OffsetX;
                startY = ownerRect.Top + _options.OffsetY;
                growDown = true;
                break;

            case Position.TopRight:
                left = ownerRect.Right - _options.Width - _options.OffsetX;
                startY = ownerRect.Top + _options.OffsetY;
                growDown = true;
                break;

            case Position.BottomLeft:
                left = ownerRect.Left + _options.OffsetX;
                startY = ownerRect.Bottom - _options.OffsetY;
                growDown = false;
                break;

            case Position.BottomRight:
                left = ownerRect.Right - _options.Width - _options.OffsetX;
                startY = ownerRect.Bottom - _options.OffsetY;
                growDown = false;
                break;
        }

        // 按位置排序通知
        var sortedNotifications = _activeNotifications
            .OrderBy(n => growDown ? n.Top : -n.Top)
            .ToList();

        // 重新定位每个通知
        double currentY = startY;
        foreach (var notification in sortedNotifications)
        {
            notification.Left = left;

            if (growDown)
            {
                notification.Top = currentY;
                currentY += notification.ActualHeight + _options.NotificationGap;
            }
            else
            {
                notification.Top = currentY - notification.Height;
                currentY -= notification.Height + _options.NotificationGap;
            }
        }
    }

    // 清理过期通知
    private void CleanupTimerOnTick(object sender, EventArgs e)
    {
        foreach (var notification in _activeNotifications.ToList())
        {
        }
    }

    /// <summary>
    /// 释放通知服务并清除所有活动通知
    /// </summary>
    public void Dispose()
    {
        _cleanupTimer?.Stop();
        _positionCheckTimer?.Stop();

        foreach (var notification in _activeNotifications.ToList())
        {
            notification.Close();
        }
        _activeNotifications.Clear();
        _instance = null;
    }
}

#endregion 通知服务



#region 扩展方法

/// <summary>
/// 通知服务扩展方法
/// </summary>
public static class NotificationServiceExtensions
{
    /// <summary>
    /// 显示信息通知
    /// </summary>
    public static void ShowInformation(this NotificationService service, string message)
    {
        service.ShowNotification(message, NotificationType.Information);
    }

    /// <summary>
    /// 显示带有自定义内容的信息通知
    /// </summary>
    public static void ShowInformation(this NotificationService service, UIElement content)
    {
        service.ShowNotification(content, NotificationType.Information);
    }

    /// <summary>
    /// 显示成功通知
    /// </summary>
    public static void ShowSuccess(this NotificationService service, string message)
    {
        service.ShowNotification(message, NotificationType.Success);
    }

    /// <summary>
    /// 显示带有自定义内容的成功通知
    /// </summary>
    public static void ShowSuccess(this NotificationService service, UIElement content)
    {
        service.ShowNotification(content, NotificationType.Success);
    }

    /// <summary>
    /// 显示警告通知
    /// </summary>
    public static void ShowWarning(this NotificationService service, string message)
    {
        service.ShowNotification(message, NotificationType.Warning);
    }

    /// <summary>
    /// 显示带有自定义内容的警告通知
    /// </summary>
    public static void ShowWarning(this NotificationService service, UIElement content)
    {
        service.ShowNotification(content, NotificationType.Warning);
    }

    /// <summary>
    /// 显示错误通知
    /// </summary>
    public static void ShowError(this NotificationService service, string message)
    {
        service.ShowNotification(message, NotificationType.Error);
    }

    /// <summary>
    /// 显示带有自定义内容的错误通知
    /// </summary>
    public static void ShowError(this NotificationService service, UIElement content)
    {
        service.ShowNotification(content, NotificationType.Error);
    }
}

#endregion 扩展方法