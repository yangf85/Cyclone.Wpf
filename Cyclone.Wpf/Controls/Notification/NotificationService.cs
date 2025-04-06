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

public class NotificationOption
{
    /// <summary>
    /// 显示持续时间
    /// </summary>
    public TimeSpan DisplayDuration
    {
        get => field;
        set
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Duration must be >0");
            }
            field = value;
        }
    } = TimeSpan.FromMilliseconds(2400);

    /// <summary>
    /// 位置
    /// </summary>
    public Position Position { get; set; } = Position.BottomRight;

    /// <summary>
    /// X轴偏移量
    /// </summary>
    public double OffsetX { get; set; } = 0;

    /// <summary>
    /// Y轴偏移量
    /// </summary>
    public double OffsetY { get; set; } = 0;

    /// <summary>
    /// 通知之间的间隙
    /// </summary>
    public double Spacing { get; set; } = 5;

    /// <summary>
    /// 最多显示多少个通知
    /// </summary>
    public int MaxCount
    {
        get => field;
        set
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "MaxCount must be >0");
            }
            field = value;
        }
    } = 5;

    /// <summary>
    /// 通知宽度
    /// </summary>
    public double MaxWidth { get; set; } = 240;

    /// <summary>
    /// 最大高度
    /// </summary>
    public double MaxHeight { get; set; } = 75;

    /// <summary>
    /// 是否显示关闭按钮
    /// </summary>
    public bool IsShowCloseButton { get; set; } = true;
}

public enum Position
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public interface INotificationService
{
    void Show(object content, DataTemplate template, string title = null);
}

public class NotificationService : INotificationService, IDisposable
{
    private readonly NotificationOption _option;
    private readonly List<NotificationWindow> _activeWindows = [];
    private IntPtr _ownerHandle;
    private bool _useScreenForPositioning = false;

    #region HandleWindowIntPtr

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

    #endregion HandleWindowIntPtr

    // 单例模式的静态实例
    private static NotificationService _instance;

    /// <summary>
    /// 获取通知服务的单例实例
    /// </summary>
    public static NotificationService Instance => _instance ??= new NotificationService();

    /// <summary>
    /// 使用默认选项初始化通知服务
    /// </summary>
    public NotificationService() : this(new NotificationOption())
    {
        // 默认使用屏幕作为定位基准
        _useScreenForPositioning = true;
    }

    /// <summary>
    /// 使用自定义选项初始化通知服务
    /// </summary>
    public NotificationService(NotificationOption option)
    {
        _option = option;
        // 默认使用屏幕作为定位基准
        _useScreenForPositioning = true;
    }

    /// <summary>
    /// 设置WPF窗口作为通知的所有者
    /// </summary>
    public void SetOwner(Window owner)
    {
        if (owner == null)
        {
            throw new ArgumentNullException(nameof(owner));
        }

        var handle = new WindowInteropHelper(owner).Handle;
        SetOwnerInternal(handle);
        _useScreenForPositioning = false;

        // 添加WPF特定的事件处理程序
        owner.LocationChanged += (sender, args) => RepositionActiveWindows();
        owner.SizeChanged += (sender, args) => RepositionActiveWindows();
        owner.StateChanged += (sender, args) => RepositionActiveWindows();
        owner.Closed += (sender, args) => Dispose();
    }

    /// <summary>
    /// 设置非WPF窗口句柄作为通知的所有者
    /// </summary>
    public void SetOwner(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(windowHandle), "Invalid WindowHandle");
        }

        if (!IsWindow(windowHandle))
        {
            throw new ArgumentException("Handle is not a Window", nameof(windowHandle));
        }

        SetOwnerInternal(windowHandle);
        _useScreenForPositioning = false;
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
            _useScreenForPositioning = false;
        }
        else
        {
            // 如果无法获取前台窗口，则使用屏幕作为定位基准
            _useScreenForPositioning = true;
        }
    }

    /// <summary>
    /// 使用整个屏幕作为定位基准
    /// </summary>
    public void UseScreenForPositioning()
    {
        _useScreenForPositioning = true;
        _ownerHandle = IntPtr.Zero;
        RepositionActiveWindows();
    }

    // 内部方法，用于设置所有者句柄和跟踪
    private void SetOwnerInternal(IntPtr handle)
    {
        _ownerHandle = handle;

        GetWindowRect(_ownerHandle, out RECT lastKnownPosition);

        if (!IsWindow(_ownerHandle))
        {
            _useScreenForPositioning = true;
            return;
        }

        if (GetWindowRect(_ownerHandle, out RECT currentPosition))
        {
            if (lastKnownPosition.Left != currentPosition.Left ||
                lastKnownPosition.Top != currentPosition.Top ||
                lastKnownPosition.Right != currentPosition.Right ||
                lastKnownPosition.Bottom != currentPosition.Bottom)
            {
                lastKnownPosition = currentPosition;
                RepositionActiveWindows();
            }
        }
    }

    #region Implementation  INotificationService

    public void Show(object content, DataTemplate template, string title = null)
    {
        // 不再需要检查 _ownerHandle，因为我们可以使用屏幕作为定位基准
        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            var window = new NotificationWindow();
            window.Title = title ?? string.Empty;
            window.Content = content;
            window.ContentTemplate = template;
            window.Width = _option.MaxWidth;
            window.Height = _option.MaxHeight;
            window.IsShowCloseButton = _option.IsShowCloseButton;
            window.DisplayDuration = _option.DisplayDuration;
            AddWindow(window);
        }));
    }

    #endregion Implementation  INotificationService

    private void AddWindow(NotificationWindow window)
    {
        if (_activeWindows.Count >= _option.MaxCount)
        {
            var oldest = _activeWindows.First();
            oldest.Close();
        }

        window.Closed += (sender, args) => RemoveWindow(window);
        _activeWindows.Add(window);

        // 设置初始位置属性
        SetInitialWindowProperties(window);

        // 重新排列所有窗口
        RepositionActiveWindows();

        window.Show();
    }

    private void SetInitialWindowProperties(NotificationWindow window)
    {
        // 设置窗口的动画方向
        AnimationDirection animDirection;

        switch (_option.Position)
        {
            case Position.TopLeft:
            case Position.BottomLeft:
                animDirection = AnimationDirection.FromLeft;
                break;

            case Position.TopRight:
            case Position.BottomRight:
            default:
                animDirection = AnimationDirection.FromRight;
                break;
        }

        window.AnimationDirection = animDirection;

        // 设置初始宽高
        if (window.ActualWidth == 0)
        {
            window.Width = _option.MaxWidth;
        }

        if (window.ActualHeight == 0)
        {
            window.Height = _option.MaxHeight;
        }
    }

    private void RemoveWindow(NotificationWindow window)
    {
        _activeWindows.Remove(window);
        RepositionActiveWindows();
    }

    internal void UpdateOption(Action<NotificationOption> action)
    {
        action?.Invoke(_option);
    }

    #region Positioning

    private void RepositionActiveWindows()
    {
        if (_activeWindows.Count == 0) { return; }

        RECT ownerRect;
        var screenBounds = System.Windows.SystemParameters.WorkArea;

        if (_useScreenForPositioning || !IsWindow(_ownerHandle) || !GetWindowRect(_ownerHandle, out ownerRect))
        {
            // 使用屏幕作为定位基准
            ownerRect = new RECT
            {
                Left = (int)screenBounds.Left,
                Top = (int)screenBounds.Top,
                Right = (int)screenBounds.Right,
                Bottom = (int)screenBounds.Bottom
            };
        }

        double baseLeft = 0;
        double baseTop = 0;
        bool isTop = false;  // 是否是顶部定位

        // 计算基准位置
        switch (_option.Position)
        {
            case Position.TopLeft:
                baseLeft = ownerRect.Left + _option.OffsetX;
                baseTop = ownerRect.Top + _option.OffsetY;
                isTop = true;
                break;

            case Position.TopRight:
                baseLeft = ownerRect.Right - _option.MaxWidth - _option.OffsetX;
                baseTop = ownerRect.Top + _option.OffsetY;
                isTop = true;
                break;

            case Position.BottomLeft:
                baseLeft = ownerRect.Left + _option.OffsetX;
                baseTop = ownerRect.Bottom - _option.OffsetY;
                isTop = false;
                break;

            case Position.BottomRight:
                baseLeft = ownerRect.Right - _option.MaxWidth - _option.OffsetX;
                baseTop = ownerRect.Bottom - _option.OffsetY;
                isTop = false;
                break;
        }

        // 确保左侧位置在屏幕范围内
        if (baseLeft + _option.MaxWidth > screenBounds.Right)
        {
            baseLeft = screenBounds.Right - _option.MaxWidth;
        }

        if (baseLeft < screenBounds.Left)
        {
            baseLeft = screenBounds.Left;
        }

        // 调整顶部位置确保在屏幕内
        if (isTop && baseTop < screenBounds.Top)
        {
            baseTop = screenBounds.Top;
        }
        else if (!isTop && baseTop > screenBounds.Bottom)
        {
            baseTop = screenBounds.Bottom;
        }

        // 重新排序窗口
        // 顶部位置：按照添加时间降序排列（新窗口在顶部）
        // 底部位置：按照添加时间升序排列（新窗口在底部）
        List<NotificationWindow> orderedWindows;

        if (isTop)
        {
            // 最新的窗口在最上面（数组开头）
            orderedWindows = _activeWindows.OrderByDescending(w => _activeWindows.IndexOf(w)).ToList();
        }
        else
        {
            // 最新的窗口在最下面（数组末尾）
            orderedWindows = _activeWindows.OrderBy(w => _activeWindows.IndexOf(w)).ToList();
        }

        // 依次定位窗口
        double currentPosition = baseTop;

        foreach (var window in orderedWindows)
        {
            // 获取窗口实际高度
            double windowHeight = window.ActualHeight > 0 ? window.ActualHeight : _option.MaxHeight;

            // 设置水平位置
            window.Left = baseLeft;

            if (isTop)
            {
                // 顶部定位：从上往下增长
                window.Top = currentPosition;
                currentPosition += windowHeight + _option.Spacing;

                // 确保不超出屏幕底部
                if (window.Top + windowHeight > screenBounds.Bottom)
                {
                    window.Top = screenBounds.Bottom - windowHeight;
                }
            }
            else
            {
                // 底部定位：从下往上增长
                // 计算窗口顶部位置
                window.Top = currentPosition - windowHeight;
                currentPosition = window.Top - _option.Spacing;

                // 确保不超出屏幕顶部
                if (window.Top < screenBounds.Top)
                {
                    window.Top = screenBounds.Top;
                }
            }
        }
    }

    #endregion Positioning

    #region Implementation  IDisposable

    /// <summary>
    /// 释放通知服务并清除所有活动通知
    /// </summary>
    public void Dispose()
    {
        foreach (var notification in _activeWindows.ToList())
        {
            notification.Close();
        }
        _activeWindows.Clear();
        _instance = null;
    }

    #endregion Implementation  IDisposable
}

/// <summary>
/// 通知服务扩展方法
/// </summary>
public static class NotificationServiceExtension
{
    private static ResourceDictionary _dict;

    static NotificationServiceExtension()
    {
        _dict = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Cyclone.Wpf;component/Styles/Notification.xaml", UriKind.Absolute)
        };
    }

    public static void Information(this INotificationService self, string message)
    {
        var template = _dict["Notification.Information.DataTemplate"] as DataTemplate;
        self.Show(message, template);
    }

    public static void Success(this INotificationService service, string message)
    {
    }

    public static void ShowWarning(this INotificationService service, string message)
    {
    }

    public static void ShowError(this INotificationService service, string message)
    {
    }
}