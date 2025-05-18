using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Collections.Concurrent;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

public interface INotificationService
{
    void Show(object content, string title = null);
}

public class NotificationService : INotificationService, IDisposable
{
    // 使用线程安全的字典，避免显式锁
    // 使用线程安全的字典存储活动窗口，并用创建时间跟踪添加顺序
    private readonly ConcurrentDictionary<NotificationWindow, DateTime> _activeWindows = [];

    private IntPtr _ownerHandle;
    private readonly NotificationWindowPositioner _windowPositioner;

    // 使用原子操作控制对象状态
    private int _isDisposed;

    // 自定义Dispatcher，用于在非WPF环境下工作
    private Dispatcher _customDispatcher;

    // 允许重置单例的静态字段 - 非readonly
    private static Lazy<NotificationService> _lazyInstance =
        new Lazy<NotificationService>(() => new NotificationService(), LazyThreadSafetyMode.ExecutionAndPublication);

    // 确保实例重置线程安全的锁对象
    private static readonly object _instanceLock = new object();

    /// <summary>
    /// 获取通知服务的单例实例
    /// </summary>
    public static NotificationService Instance => _lazyInstance.Value;

    private NotificationOption field = new NotificationOption();

    public NotificationOption Option
    {
        get => field;
        private set
        {
            field = value;
        }
    }

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
        Option = option;
        _windowPositioner = new NotificationWindowPositioner(option);

        // 确保Dispatcher已初始化
        GetDispatcher();
    }

    /// <summary>
    /// 获取可用的Dispatcher，确保在非WPF环境下也能正常工作
    /// </summary>
    private Dispatcher GetDispatcher()
    {
        // 如果已有自定义Dispatcher，直接返回
        if (_customDispatcher != null)
            return _customDispatcher;

        // 尝试获取Application.Current.Dispatcher
        if (Application.Current != null && Application.Current.Dispatcher != null)
            return Application.Current.Dispatcher;

        // 在非WPF环境下，创建并使用自己的Dispatcher线程
        _customDispatcher = Dispatcher.CurrentDispatcher;
        return _customDispatcher;
    }

    /// <summary>
    /// 检查是否需要在Dispatcher上调用并执行操作
    /// </summary>
    private void InvokeOnDispatcher(Action action)
    {
        if (action == null)
            return;

        var dispatcher = GetDispatcher();

        if (dispatcher == null)
        {
            // 如果无法获取任何Dispatcher，则直接执行操作
            action();
            return;
        }

        if (dispatcher.CheckAccess())
        {
            // 已在UI线程上，直接执行
            action();
        }
        else
        {
            // 需要在UI线程上执行
            dispatcher.BeginInvoke(action);
        }
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

        if (!WindowsNativeService.IsWindow(windowHandle))
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

        IntPtr foregroundHandle = WindowsNativeService.GetForegroundWindow();
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
        if (WindowsNativeService.IsValidWindow(handle))
        {
            _ownerHandle = handle;
            _windowPositioner.SetOwner(handle);

            // 确保在UI线程上重新定位窗口
            InvokeOnDispatcher(RepositionActiveWindows);
        }
    }

    #region Implementation INotificationService

    public void Show(object content, string title = null)
    {
        // 检查是否已被处置
        ThrowIfDisposed();

        // 在UI线程上创建窗口
        InvokeOnDispatcher(() =>
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 1) return;

            var window = new NotificationWindow();
            window.Title = title ?? string.Empty;
            window.Content = content;
            window.Width = Option.Width;
            window.Height = Option.Height;
            window.IsShowCloseButton = Option.IsShowCloseButton;
            window.DisplayDuration = Option.DisplayDuration;

            AddWindow(window);
        });
    }

    #endregion Implementation INotificationService

    private void AddWindow(NotificationWindow window)
    {
        // 检查是否超过最大窗口数
        while (_activeWindows.Count >= Option.MaxCount)
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
        // 使用自定义的线程调度方法
        InvokeOnDispatcher(() =>
        {
            // 获取当前窗口快照并按添加时间排序
            List<NotificationWindow> windowsSnapshot;

            // 根据位置决定排序方向
            bool isTopPosition = Option.Position == NotificationPosition.TopLeft ||
                                Option.Position == NotificationPosition.TopRight;

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
        });
    }

    internal void UpdateOption(Action<NotificationOption> action)
    {
        // 检查是否已被处置
        ThrowIfDisposed();

        action?.Invoke(Option);
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
                // 使用自定义的线程调度方法关闭所有窗口
                InvokeOnDispatcher(() =>
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

                    // 清理自定义Dispatcher
                    _customDispatcher = null;
                });

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