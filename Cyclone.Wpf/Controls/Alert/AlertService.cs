using System;
using System.Windows;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 警告框服务实现类
/// </summary>
public class AlertService : IAlertService, IDisposable
{
    // 使用非readonly的静态字段，允许重新分配
    private static Lazy<AlertService> _lazyInstance;

    // 确保实例重置线程安全的锁对象
    private static readonly object _instanceLock = new object();

    // 使用原子操作控制对象状态
    private int _isDisposed;

    // 自定义Dispatcher，用于在非WPF环境下工作
    private Dispatcher _customDispatcher;

    /// <summary>
    /// 静态构造函数，初始化单例实例
    /// </summary>
    static AlertService()
    {
        _lazyInstance = new Lazy<AlertService>(() =>
            new AlertService(), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// 获取AlertService的单例实例
    /// </summary>
    public static AlertService Instance
    {
        get
        {
            lock (_instanceLock)
            {
                return _lazyInstance.Value;
            }
        }
    }

    public AlertOption Option
    {
        get;
        private set;
    }

    /// <summary>
    /// 重置警告服务单例实例
    /// 用于服务处置后重新开始使用的情况
    /// </summary>
    public static void ResetInstance()
    {
        lock (_instanceLock)
        {
            _lazyInstance = new Lazy<AlertService>(() =>
                new AlertService(), LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }

    private Window _ownerWindow;               // 拥有者WPF窗口
    private IntPtr _ownerHandle;               // 拥有者窗口句柄
    private Window _maskWindow;                // 蒙版窗口
    private AlertWindow _currentAlertWindow;   // 当前活动的警告窗口

    // 非WPF窗口的事件钩子句柄
    private IntPtr _winEventHook = IntPtr.Zero;

    // 窗口事件委托（保持引用以防止被垃圾回收）
    private WindowsNativeService.WinEventDelegate _winEventProc;

    /// <summary>
    /// 使用默认选项初始化AlertService的新实例
    /// </summary>
    public AlertService() : this(new AlertOption())
    {
    }

    /// <summary>
    /// 使用自定义选项初始化AlertService的新实例
    /// </summary>
    public AlertService(AlertOption option)
    {
        Option = option ?? throw new ArgumentNullException(nameof(option));

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
    /// 同步在Dispatcher上执行操作并返回结果
    /// </summary>
    private T InvokeOnDispatcherWithResult<T>(Func<T> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var dispatcher = GetDispatcher();

        if (dispatcher == null || dispatcher.CheckAccess())
        {
            // 如果无法获取任何Dispatcher或已在UI线程上，直接执行
            return func();
        }
        else
        {
            // 需要在UI线程上执行并等待结果
            return (T)dispatcher.Invoke(func);
        }
    }

    /// <summary>
    /// 将WPF窗口设置为警告框的所有者
    /// </summary>
    public void SetOwner(Window owner)
    {
        if (owner == null)
        {
            throw new ArgumentNullException(nameof(owner));
        }

        // 检查是否已被处置
        ThrowIfDisposed();

        // 清理任何已存在的事件钩子
        UnhookEvents();

        _ownerWindow = owner;
        _ownerHandle = new WindowInteropHelper(owner).Handle;

        // 添加窗口关闭事件，确保在Owner关闭时释放资源
        owner.Closed += (sender, args) => Dispose();
    }

    /// <summary>
    /// 将非WPF窗口句柄设置为警告框的所有者
    /// </summary>
    public void SetOwner(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(windowHandle), "无效的窗口句柄");
        }

        // 检查是否已被处置
        ThrowIfDisposed();

        if (!WindowsNativeService.IsWindow(windowHandle))
        {
            throw new ArgumentException("句柄不是一个窗口", nameof(windowHandle));
        }

        // 清理任何已存在的事件钩子
        UnhookEvents();

        _ownerWindow = null;
        _ownerHandle = windowHandle;
    }

    /// <summary>
    /// 将当前前台窗口设置为所有者
    /// </summary>
    public void SetOwnerToForegroundWindow()
    {
        // 检查是否已被处置
        ThrowIfDisposed();

        IntPtr foregroundHandle = WindowsNativeService.GetForegroundWindow();
        if (foregroundHandle != IntPtr.Zero && WindowsNativeService.IsWindow(foregroundHandle))
        {
            // 清理任何已存在的事件钩子
            UnhookEvents();

            _ownerWindow = null;
            _ownerHandle = foregroundHandle;
        }
    }

    /// <summary>
    /// 取消任何活动的窗口事件钩子
    /// </summary>
    private void UnhookEvents()
    {
        try
        {
            if (_winEventHook != IntPtr.Zero)
            {
                WindowsNativeService.UnhookWinEvent(_winEventHook);
                _winEventHook = IntPtr.Zero;
                _winEventProc = null;
            }
        }
        catch (Exception)
        {
            // 忽略可能的异常
        }
    }

    /// <summary>
    /// 创建警告窗口实例
    /// </summary>
    /// <param name="content">窗口内容</param>
    /// <param name="title">窗口标题</param>
    /// <returns>创建的AlertWindow实例</returns>
    private AlertWindow CreateAlertWindow(object content, string title)
    {
        // 检查是否已被处置
        ThrowIfDisposed();

        var window = new AlertWindow
        {
            Width = Option.Width,
            Height = Option.Height,
            ButtonType = Option.ButtonType,
            Title = title ?? Option.Title,
            Icon = Option.Icon,
            CaptionHeight = Option.CaptionHeight,
            CaptionBackground = Option.CaptionBackground,
            TitleForeground = Option.TitleForeground,
            AlertButtonGroupBackground = Option.AlertButtonGroupBackground,
            AlertButtonGroupHeight = Option.AlertButtonGroupHeight,
            AlertButtonGroupHorizontalAlignment = Option.AlertButtonHorizontalAlignment,
            OkButtonText = Option.OkButtonText,
            CancelButtonText = Option.CancelButtonText,
            Content = content,
            Topmost = false,
            ShowInTaskbar = false,
        };

        return window;
    }

    /// <summary>
    /// 为所有者窗口创建蒙版窗口
    /// </summary>
    /// <returns>创建的蒙版窗口或null</returns>
    private Window CreateMaskWindow()
    {
        // 检查是否已被处置
        ThrowIfDisposed();

        if (_ownerWindow == null && _ownerHandle == IntPtr.Zero)
        {
            return null; // 没有所有者，不创建蒙版
        }

        // 获取所有者窗口的位置和尺寸
        Rect ownerRect;

        if (_ownerWindow != null)
        {
            // 使用WPF窗口
            ownerRect = new Rect(_ownerWindow.Left, _ownerWindow.Top, _ownerWindow.Width, _ownerWindow.Height);
        }
        else
        {
            // 使用句柄窗口
            var wpfRect = WindowsNativeService.GetWindowRectAsWpfRect(_ownerHandle);
            if (wpfRect.HasValue)
            {
                ownerRect = wpfRect.Value;
            }
            else
            {
                return null; // 无法获取窗口尺寸
            }
        }

        try
        {
            // 创建蒙版窗口
            var maskWindow = new Window
            {
                Width = ownerRect.Width,
                Height = ownerRect.Height,
                Left = ownerRect.Left,
                Top = ownerRect.Top,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                WindowStartupLocation = WindowStartupLocation.Manual,
                // 修改：设置为接收点击事件但不获取焦点
                IsHitTestVisible = false,
                // 修改：允许获取焦点以便正确处理Z顺序
                Focusable = true,
                // 设置为不是最上层，让Alert窗口可以在其上方
                Topmost = false,
            };

            // 添加矩形蒙版
            var rectangle = new Rectangle
            {
                Width = ownerRect.Width,
                Height = ownerRect.Height,
                Fill = Option.MaskBrush ?? new SolidColorBrush(Color.FromArgb(128, 0, 0, 0))
            };

            maskWindow.Content = rectangle;

            // 如果有WPF Owner，设置蒙版窗口的Owner，确保Z顺序和焦点行为正确
            if (_ownerWindow != null)
            {
                maskWindow.Owner = _ownerWindow;
            }

            // 添加事件处理，确保点击蒙版时不会让蒙版获得焦点
            maskWindow.PreviewMouseDown += (sender, e) =>
            {
                e.Handled = true;
            };

            return maskWindow;
        }
        catch (Exception ex)
        {
            // 记录异常但不中断流程
            System.Diagnostics.Debug.WriteLine($"创建蒙版窗口时出错: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 计算窗口中心位置
    /// </summary>
    /// <param name="window">要定位的窗口</param>
    private void PositionWindowInCenter(Window window)
    {
        // 检查是否已被处置
        ThrowIfDisposed();

        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        try
        {
            // 如果有WPF窗口作为拥有者，使用WPF的标准居中逻辑
            if (_ownerWindow != null)
            {
                window.Owner = _ownerWindow;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                return;
            }

            // 如果有非WPF窗口作为拥有者，使用专用的Alert窗口定位器
            if (WindowsNativeService.IsValidWindow(_ownerHandle))
            {
                // 尝试使用AlertWindowPositioner在拥有者窗口中心定位警告窗口
                if (AlertWindowPositioner.CenterAlertInOwner(window, _ownerHandle))
                {
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            // 记录异常但不中断流程
            System.Diagnostics.Debug.WriteLine($"定位警告窗口时出错: {ex.Message}");
        }

        // 如果以上方法都失败，则默认使用屏幕中心
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    /// <summary>
    /// 从句柄窗口更新蒙版窗口位置和大小
    /// </summary>
    private void UpdateMaskPositionFromHandle()
    {
        try
        {
            if (_currentAlertWindow != null && _currentAlertWindow.IsLoaded)
            {
                _currentAlertWindow.Visibility = Visibility.Visible;
                _currentAlertWindow.Activate();
            }

            if (_maskWindow != null && WindowsNativeService.IsValidWindow(_ownerHandle))
            {
                var wpfRect = WindowsNativeService.GetWindowRectAsWpfRect(_ownerHandle);
                if (wpfRect.HasValue)
                {
                    _maskWindow.Left = wpfRect.Value.Left;
                    _maskWindow.Top = wpfRect.Value.Top;
                    _maskWindow.Width = wpfRect.Value.Width;
                    _maskWindow.Height = wpfRect.Value.Height;
                }
            }
        }
        catch (Exception)
        {
            // 忽略可能的异常
        }
    }

    /// <summary>
    /// 用于监控非WPF窗口变化的窗口事件回调
    /// </summary>
    private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        try
        {
            // 只处理我们拥有者窗口的事件
            if (hwnd == _ownerHandle)
            {
                InvokeOnDispatcher(() =>
                {
                    UpdateMaskPositionFromHandle();
                });
            }
        }
        catch (Exception)
        {
            // 忽略可能的异常
        }
    }

    /// <summary>
    /// 为非WPF窗口设置窗口事件监控
    /// </summary>
    private void SetupNonWpfWindowMonitoring()
    {
        // 检查是否已被处置
        ThrowIfDisposed();

        try
        {
            // 只有在有非WPF所有者且尚未设置钩子时才设置
            if (_ownerHandle != IntPtr.Zero && _winEventHook == IntPtr.Zero && _ownerWindow == null)
            {
                // 创建委托（保持引用以防止垃圾回收）
                _winEventProc = new WindowsNativeService.WinEventDelegate(WinEventProc);

                // 为窗口位置变化设置事件钩子
                _winEventHook = WindowsNativeService.SetWinEventHook(
                    WindowsNativeService.EVENT_OBJECT_LOCATIONCHANGE,
                    WindowsNativeService.EVENT_OBJECT_LOCATIONCHANGE,
                    IntPtr.Zero,
                    _winEventProc,
                    0,
                    0,
                    WindowsNativeService.WINEVENT_OUTOFCONTEXT);
            }
        }
        catch (Exception)
        {
            // 忽略可能的异常
        }
    }

    /// <summary>
    /// 激活所有者窗口并将其置于前台
    /// </summary>
    private void ActivateOwnerWindow()
    {
        try
        {
            // 如果有WPF窗口作为Owner
            if (_ownerWindow != null)
            {
                // 确保窗口可见
                if (_ownerWindow.Visibility == Visibility.Visible)
                {
                    // 激活WPF窗口
                    _ownerWindow.Activate();

                    // 获取并使用窗口句柄来确保窗口位于前台
                    IntPtr ownerHandle = new WindowInteropHelper(_ownerWindow).Handle;
                    WindowsNativeService.ActivateAndBringToFront(ownerHandle);
                }
            }
            // 如果只有Owner句柄
            else if (WindowsNativeService.IsValidWindow(_ownerHandle))
            {
                // 使用Win32 API设置窗口到前台
                WindowsNativeService.ActivateAndBringToFront(_ownerHandle);
            }
        }
        catch (Exception ex)
        {
            // 记录异常但不阻止程序继续运行
            System.Diagnostics.Debug.WriteLine($"激活Owner窗口时出错: {ex.Message}");
        }
    }

    #region 实现IAlertService接口

    /// <summary>
    /// 显示模态警告框并返回对话框结果
    /// </summary>
    /// <param name="content">窗口内容</param>
    /// <param name="title">窗口标题</param>
    /// <returns>对话框结果</returns>
    public bool? Show(object content, string title = null)
    {
        // 检查是否已被处置
        ThrowIfDisposed();

        // 使用自定义的Dispatcher调用方法执行
        return InvokeOnDispatcherWithResult(() =>
        {
            return ShowDialogInternal(content, title);
        });
    }

    /// <summary>
    /// 显示带验证回调的警告框
    /// </summary>
    /// <param name="content">窗口内容</param>
    /// <param name="validation">验证回调函数</param>
    /// <param name="title">窗口标题</param>
    public void ShowWithValidation(object content, Func<bool> validation, string title = null)
    {
        // 检查是否已被处置
        ThrowIfDisposed();

        if (validation == null)
        {
            throw new ArgumentNullException(nameof(validation), "验证回调函数不能为空");
        }

        // 使用自定义的Dispatcher调用方法执行
        InvokeOnDispatcher(() =>
        {
            ShowDialogWithValidationInternal(content, validation, title);
        });
    }

    /// <summary>
    /// 内部方法，用于显示对话框
    /// </summary>
    /// <param name="content">窗口内容</param>
    /// <param name="title">窗口标题</param>
    /// <returns>对话框结果</returns>
    private bool? ShowDialogInternal(object content, string title)
    {
        // 检查是否已被处置
        ThrowIfDisposed();

        try
        {
            // 如果启用了蒙版并且有所有者，显示蒙版
            if (Option.IsShowMask && (_ownerWindow != null || _ownerHandle != IntPtr.Zero))
            {
                _maskWindow = CreateMaskWindow();
                // 显示蒙版窗口但不激活它
                _maskWindow?.Show();
            }

            var window = CreateAlertWindow(content, title);
            // 添加蒙版窗口作为警告窗口的关闭回调
            window.Closed += (sender, e) =>
            {
                try
                {
                    // 确保蒙版窗口关闭
                    if (_maskWindow != null && _maskWindow.IsLoaded)
                    {
                        _maskWindow.Close();
                    }

                    // 清理事件钩子
                    UnhookEvents();
                }
                catch
                {
                    // 忽略关闭时的异常
                }
            };

            _currentAlertWindow = window;
            PositionWindowInCenter(window);
            window.Owner = _maskWindow;
            bool? result = null;
            try
            {
                // 显示模态对话框
                result = window.ShowDialog();

                return result;
            }
            finally
            {
                _currentAlertWindow = null;

                // 关闭蒙版窗口
                if (_maskWindow != null)
                {
                    try
                    {
                        _maskWindow.Close();
                    }
                    catch
                    {
                        // 忽略关闭时可能发生的异常
                    }
                    _maskWindow = null;
                }

                // 清理事件钩子
                UnhookEvents();

                // 在弹窗关闭后激活Owner窗口以确保它保持在前台
                ActivateOwnerWindow();
            }
        }
        catch (Exception ex)
        {
            // 记录任何显示过程中的异常
            System.Diagnostics.Debug.WriteLine($"显示警告对话框时出错: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 内部方法，用于显示带验证的对话框
    /// </summary>
    /// <param name="content">窗口内容</param>
    /// <param name="validation">验证回调函数</param>
    /// <param name="title">窗口标题</param>
    private void ShowDialogWithValidationInternal(object content, Func<bool> validation, string title)
    {
        // 检查是否已被处置
        ThrowIfDisposed();

        try
        {
            // 如果启用了蒙版并且有所有者，显示蒙版
            if (Option.IsShowMask && (_ownerWindow != null || _ownerHandle != IntPtr.Zero))
            {
                _maskWindow = CreateMaskWindow();
                _maskWindow?.Show();
            }

            // 创建普通的AlertWindow，然后设置验证回调
            var window = CreateAlertWindow(content, title);
            window.ValidationCallback = validation; // 设置验证回调

            // 添加蒙版窗口关闭回调
            window.Closed += (sender, e) =>
            {
                try
                {
                    // 确保蒙版窗口关闭
                    if (_maskWindow != null && _maskWindow.IsLoaded)
                    {
                        _maskWindow.Close();
                    }

                    // 清理事件钩子
                    UnhookEvents();
                }
                catch
                {
                    // 忽略关闭时的异常
                }
            };

            _currentAlertWindow = window;
            PositionWindowInCenter(window);
            window.Owner = _maskWindow;

            try
            {
                // 显示模态对话框
                window.ShowDialog();
            }
            finally
            {
                _currentAlertWindow = null;

                // 关闭蒙版窗口
                if (_maskWindow != null)
                {
                    try
                    {
                        _maskWindow.Close();
                    }
                    catch
                    {
                        // 忽略关闭时可能发生的异常
                    }
                    _maskWindow = null;
                }

                // 清理事件钩子
                UnhookEvents();

                // 在弹窗关闭后激活Owner窗口
                ActivateOwnerWindow();
            }
        }
        catch (Exception ex)
        {
            // 记录任何显示过程中的异常
            System.Diagnostics.Debug.WriteLine($"显示验证警告对话框时出错: {ex.Message}");
        }
    }

    #endregion 实现IAlertService接口

    #region 实现IDisposable接口

    // 使用无锁方式检查是否已处置
    private void ThrowIfDisposed()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 1)
        {
            throw new ObjectDisposedException(nameof(AlertService));
        }
    }

    /// <summary>
    /// 处置警告服务并清除所有活动资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放警告服务使用的所有资源
    /// </summary>
    /// <param name="disposing">是否为显式释放</param>
    protected virtual void Dispose(bool disposing)
    {
        // 使用原子操作设置处置标志
        if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
        {
            if (disposing)
            {
                // 清理非托管资源
                UnhookEvents();

                // 使用安全的Dispatcher调用清理UI资源
                InvokeOnDispatcher(() =>
                {
                    try
                    {
                        // 关闭警告窗口
                        if (_currentAlertWindow != null)
                        {
                            _currentAlertWindow.Close();
                            _currentAlertWindow = null;
                        }

                        // 关闭蒙版窗口
                        if (_maskWindow != null)
                        {
                            _maskWindow.Close();
                            _maskWindow = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        // 记录任何清理过程中的异常，但不阻止释放继续进行
                        System.Diagnostics.Debug.WriteLine($"释放AlertService资源时出错: {ex.Message}");
                    }
                });

                // 释放引用
                _ownerWindow = null;
                _ownerHandle = IntPtr.Zero;
                _customDispatcher = null;
            }
        }
    }

    /// <summary>
    /// 析构函数，确保非托管资源释放
    /// </summary>
    ~AlertService()
    {
        Dispose(false);
    }

    #endregion 实现IDisposable接口
}