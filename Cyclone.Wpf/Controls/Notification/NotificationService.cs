using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Collections.Concurrent;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 处理通知窗口的定位
/// </summary>
internal class NotificationWindowPositioner
{
    #region HandleWindowIntPtr

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

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

        if (!IsWindow(windowHandle))
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

        RECT ownerRect;
        var screenBounds = System.Windows.SystemParameters.WorkArea;

        if (useScreen || !IsWindow(ownerHandle) || !GetWindowRect(ownerHandle, out ownerRect))
        {
            // 使用屏幕作为定位参考
            ownerRect = new RECT
            {
                Left = (int)screenBounds.Left,
                Top = (int)screenBounds.Top,
                Right = (int)screenBounds.Right,
                Bottom = (int)screenBounds.Bottom
            };
        }
        else
        {
            // 将窗口坐标转换为WPF单位
            var wpfRect = ConvertRectToWpfUnit(ownerRect);
            ownerRect = new RECT
            {
                Left = (int)wpfRect.Left,
                Top = (int)wpfRect.Top,
                Right = (int)wpfRect.Right,
                Bottom = (int)wpfRect.Bottom
            };
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
        AnimationDirection animDirection;

        switch (_option.Position)
        {
            case NotificationPosition.TopLeft:
            case NotificationPosition.BottomLeft:
                animDirection = AnimationDirection.FromLeft;
                break;

            case NotificationPosition.TopRight:
            case NotificationPosition.BottomRight:
            default:
                animDirection = AnimationDirection.FromRight;
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

    #region DPI Scaling

    /// <summary>
    /// 获取当前DPI缩放比例
    /// </summary>
    private Matrix GetDpiScale()
    {
        var source = PresentationSource.FromVisual(Application.Current.MainWindow);
        if (source?.CompositionTarget != null)
        {
            return source.CompositionTarget.TransformToDevice;
        }
        return Matrix.Identity; // 如果无法确定，则返回1:1比例
    }

    /// <summary>
    /// 将物理像素坐标转换为WPF设备无关单位
    /// </summary>
    private Point ConvertPixelToWpfUnit(int x, int y)
    {
        Matrix transformToDevice = GetDpiScale();
        double dpiX = transformToDevice.M11;
        double dpiY = transformToDevice.M22;

        // 转换坐标
        return new Point(x / dpiX, y / dpiY);
    }

    /// <summary>
    /// 将RECT结构转换为WPF Rect（设备无关单位）
    /// </summary>
    private Rect ConvertRectToWpfUnit(RECT rect)
    {
        Point topLeft = ConvertPixelToWpfUnit(rect.Left, rect.Top);
        Point bottomRight = ConvertPixelToWpfUnit(rect.Right, rect.Bottom);

        return new Rect(topLeft, bottomRight);
    }

    #endregion DPI Scaling
}

public interface INotificationService
{
    void Show(object content, DataTemplate template, string title = null);
}

public class NotificationService : INotificationService, IDisposable
{
    private readonly NotificationOption _option;

    // 使用线程安全的字典，避免显式锁
    // 使用线程安全的字典存储活动窗口，并用创建时间跟踪添加顺序
    private readonly ConcurrentDictionary<NotificationWindow, DateTime> _activeWindows = [];

    private IntPtr _ownerHandle;
    private static ResourceDictionary _dict;
    private readonly NotificationWindowPositioner _windowPositioner;

    // 使用原子操作控制对象状态
    private int _isDisposed;

    // 允许重置单例的静态字段 - 非readonly
    private static Lazy<NotificationService> _lazyInstance =
        new Lazy<NotificationService>(() => new NotificationService(), LazyThreadSafetyMode.ExecutionAndPublication);

    // 确保实例重置线程安全的锁对象
    private static readonly object _instanceLock = new object();

    static NotificationService()
    {
        _dict = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Cyclone.Wpf;component/Styles/Notification.xaml", UriKind.Absolute)
        };
    }

    #region Native Windows API

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    #endregion Native Windows API

    /// <summary>
    /// 获取通知服务的单例实例
    /// </summary>
    public static NotificationService Instance => _lazyInstance.Value;

    /// <summary>
    /// 重置通知服务单例实例
    /// 用于服务处置后重新开始使用的情况
    /// </summary>
    public static void ResetInstance()
    {
        lock (_instanceLock)
        {
            _lazyInstance = new Lazy<NotificationService>(() =>
                new NotificationService(), LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }

    /// <summary>
    /// 使用默认选项初始化通知服务
    /// </summary>
    public NotificationService() : this(new NotificationOption())
    {
    }

    /// <summary>
    /// 使用自定义选项初始化通知服务
    /// </summary>
    public NotificationService(NotificationOption option)
    {
        _option = option;
        _windowPositioner = new NotificationWindowPositioner(option);
    }

    /// <summary>
    /// 将WPF窗口设置为通知的所有者
    /// </summary>
    public void SetOwner(Window owner)
    {
        if (owner == null)
        {
            throw new ArgumentNullException(nameof(owner));
        }

        // 检查是否已被处置
        ThrowIfDisposed();

        var handle = new WindowInteropHelper(owner).Handle;
        SetOwnerInternal(handle);

        // 添加WPF特定的事件处理程序
        owner.LocationChanged += (sender, args) => RepositionActiveWindows();
        owner.SizeChanged += (sender, args) => RepositionActiveWindows();
        owner.StateChanged += (sender, args) => RepositionActiveWindows();
        owner.Closed += (sender, args) => Dispose();
    }

    /// <summary>
    /// 将非WPF窗口句柄设置为通知的所有者
    /// </summary>
    public void SetOwner(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(windowHandle), "Invalid WindowHandle");
        }

        // 检查是否已被处置
        ThrowIfDisposed();

        if (!IsWindow(windowHandle))
        {
            throw new ArgumentException("Handle is not a Window", nameof(windowHandle));
        }

        SetOwnerInternal(windowHandle);
    }

    /// <summary>
    /// 将当前前台窗口设置为所有者
    /// </summary>
    public void SetOwnerToForegroundWindow()
    {
        // 检查是否已被处置
        ThrowIfDisposed();

        IntPtr foregroundHandle = GetForegroundWindow();
        if (foregroundHandle != IntPtr.Zero)
        {
            SetOwnerInternal(foregroundHandle);
        }
        else
        {
            // 如果无法获取前台窗口，则使用屏幕进行定位
            _windowPositioner.UseScreenPositioning();
        }
    }

    // 设置所有者句柄的内部方法
    private void SetOwnerInternal(IntPtr handle)
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 1) return;

        // 确保句柄有效
        if (IsWindow(handle))
        {
            _ownerHandle = handle;
            _windowPositioner.SetOwner(handle);

            // 确保在UI线程上重新定位窗口
            if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(RepositionActiveWindows));
            }
            else
            {
                RepositionActiveWindows();
            }
        }
    }

    #region Implementation INotificationService

    private void InternalShow(object content, DataTemplate template, string title)
    {
        // 检查是否已被处置
        ThrowIfDisposed();

        // 我们需要在UI线程上创建窗口
        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 1) return;

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

    public void Show(object content, DataTemplate template = null, string title = null)
    {
        // 检查是否已被处置
        ThrowIfDisposed();

        template ??= _dict["Notification.Default.DataTemplate"] as DataTemplate;
        InternalShow(content, template, title);
    }

    #endregion Implementation INotificationService

    private void AddWindow(NotificationWindow window)
    {
        // 检查是否超过最大窗口数
        while (_activeWindows.Count >= _option.MaxCount)
        {
            // 找到最早添加的窗口（按添加时间排序）
            var orderedWindows = _activeWindows.OrderBy(pair => pair.Value).ToList();
            if (orderedWindows.Count > 0)
            {
                var oldest = orderedWindows[0].Key;
                if (_activeWindows.TryRemove(oldest, out _))
                {
                    oldest.Close();
                }
                else
                {
                    // 如果移除失败，跳出循环避免死循环
                    break;
                }
            }
            else
            {
                // 如果没有窗口，跳出循环
                break;
            }
        }

        // 添加到字典，记录添加时间用于排序
        _activeWindows.TryAdd(window, DateTime.Now);
        window.Closed += (sender, args) => RemoveWindow(window);

        // 设置初始窗口属性
        _windowPositioner.SetAnimationDirection(window);

        // 重新定位所有窗口
        RepositionActiveWindows();

        window.Show();
    }

    private void RemoveWindow(NotificationWindow window)
    {
        // 从字典中移除窗口
        _activeWindows.TryRemove(window, out _);

        RepositionActiveWindows();
    }

    private void RepositionActiveWindows()
    {
        // 确保在UI线程上执行位置更新
        if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
        {
            // 如果不在UI线程，则分发到UI线程执行
            Application.Current.Dispatcher.BeginInvoke(new Action(RepositionActiveWindows));
            return;
        }

        // 获取当前窗口快照并按添加时间排序
        List<NotificationWindow> windowsSnapshot;

        // 根据位置决定排序方向
        bool isTopPosition = _option.Position == NotificationPosition.TopLeft ||
                             _option.Position == NotificationPosition.TopRight;

        if (isTopPosition)
        {
            // 顶部位置：最新的窗口在底部（按时间升序排列）
            windowsSnapshot = _activeWindows.OrderBy(pair => pair.Value)
                                          .Select(pair => pair.Key)
                                          .ToList();
        }
        else
        {
            // 底部位置：最新的窗口在顶部（按时间降序排列）
            windowsSnapshot = _activeWindows.OrderByDescending(pair => pair.Value)
                                          .Select(pair => pair.Key)
                                          .ToList();
        }

        // 使用排序后的快照进行重新定位
        _windowPositioner.PositionWindows(windowsSnapshot);
    }

    internal void UpdateOption(Action<NotificationOption> action)
    {
        // 检查是否已被处置
        ThrowIfDisposed();

        action?.Invoke(_option);
    }

    #region Implementation IDisposable

    // 使用无锁方式检查是否已处置
    private void ThrowIfDisposed()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 1)
        {
            throw new ObjectDisposedException(nameof(NotificationService));
        }
    }

    /// <summary>
    /// 处理通知服务并清除所有活动通知
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        // 使用原子操作设置处置标志
        if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
        {
            if (disposing)
            {
                // 在UI线程上关闭所有窗口
                if (Application.Current != null && Application.Current.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 获取当前所有窗口
                        var windowsToClose = _activeWindows.Keys.ToArray();

                        // 关闭所有窗口
                        foreach (var notification in windowsToClose)
                        {
                            notification.Close();
                        }

                        // 清空字典
                        _activeWindows.Clear();
                    });
                }

                // 重置单例实例 - 使用线程安全的公共方法
                ResetInstance();
            }
        }
    }

    ~NotificationService()
    {
        Dispose(false);
    }

    #endregion Implementation IDisposable
}