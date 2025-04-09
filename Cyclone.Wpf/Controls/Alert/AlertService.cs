using System;
using System.Windows;
using System.Threading;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 弹出警告框服务接口
/// </summary>
public interface IAlertService
{
    bool? Show(object content, DataTemplate template, string title = null);
}

/// <summary>
/// 警告框服务实现类
/// </summary>
public class AlertService : IAlertService
{
    private readonly AlertOption _option;

    #region 原生Windows API

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    // ShowWindow函数的常量参数
    private const int SW_SHOW = 5;       // 显示窗口

    private const int SW_SHOWNA = 8;     // 显示窗口但不激活

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    // 窗口样式常量
    private const int GWL_EXSTYLE = -20;               // 扩展窗口样式

    private const int WS_EX_NOACTIVATE = 0x08000000;   // 窗口不激活
    private const int WS_EX_TRANSPARENT = 0x00000020;  // 透明窗口（点击穿透）
    private const int HWND_TOPMOST = -1;               // 置顶窗口
    private const int HWND_NOTOPMOST = -2;             // 取消置顶

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    // SetWindowPos标志常量
    private const uint SWP_NOSIZE = 0x0001;        // 保持当前大小

    private const uint SWP_NOMOVE = 0x0002;        // 保持当前位置
    private const uint SWP_NOACTIVATE = 0x0010;    // 不激活窗口
    private const uint SWP_SHOWWINDOW = 0x0040;    // 显示窗口

    // 添加Windows钩子常量和函数，用于窗口消息监控
    private const int WM_MOVE = 0x0003;            // 窗口移动消息

    private const int WM_SIZE = 0x0005;            // 窗口大小改变消息

    [DllImport("user32.dll")]
    private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax,
        IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
        uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    // 窗口事件常量
    private const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;  // 窗口位置变化事件

    private const uint EVENT_OBJECT_REORDER = 0x8004;         // 窗口Z序变化事件
    private const uint WINEVENT_OUTOFCONTEXT = 0x0000;        // 事件钩子标志

    // 窗口事件监控委托
    private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
        IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion 原生Windows API

    // 静态单例实例
    private static readonly Lazy<AlertService> _lazyInstance =
        new Lazy<AlertService>(() => new AlertService(), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// 获取AlertService的单例实例
    /// </summary>
    public static AlertService Instance => _lazyInstance.Value;

    private Window _ownerWindow;               // 拥有者WPF窗口
    private IntPtr _ownerHandle;               // 拥有者窗口句柄
    private static ResourceDictionary _dict;   // 资源字典
    private Window _maskWindow;                // 蒙版窗口
    private AlertWindow _currentAlertWindow;   // 当前活动的警告窗口

    // 非WPF窗口的事件钩子句柄
    private IntPtr _winEventHook = IntPtr.Zero;

    // 窗口事件委托（保持引用以防止被垃圾回收）
    private WinEventDelegate _winEventProc;

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
        _dict = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Cyclone.Wpf;component/Styles/Alert.xaml", UriKind.Absolute)
        };
        _option = option ?? throw new ArgumentNullException(nameof(option));
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

        // 清理任何已存在的事件钩子
        UnhookEvents();

        _ownerWindow = owner;
        _ownerHandle = new WindowInteropHelper(owner).Handle;
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

        if (!IsWindow(windowHandle))
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
        IntPtr foregroundHandle = GetForegroundWindow();
        if (foregroundHandle != IntPtr.Zero && IsWindow(foregroundHandle))
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
        if (_winEventHook != IntPtr.Zero)
        {
            UnhookWinEvent(_winEventHook);
            _winEventHook = IntPtr.Zero;
            _winEventProc = null;
        }
    }

    /// <summary>
    /// 创建警告窗口实例
    /// </summary>
    /// <param name="content">窗口内容</param>
    /// <param name="template">内容模板</param>
    /// <param name="title">窗口标题</param>
    /// <returns>创建的AlertWindow实例</returns>
    private AlertWindow CreateAlertWindow(object content, DataTemplate template, string title)
    {
        var window = new AlertWindow
        {
            Width = _option.Width,
            Height = _option.Height,
            ButtonType = _option.ButtonType,
            Title = title ?? _option.Title,
            Icon = _option.Icon,
            CaptionHeight = _option.CaptionHeight,
            CaptionBackground = _option.CaptionBackground,
            TitleForeground = _option.TitleForeground,
            AlertButtonGroupBackground = _option.AlertButtonGroupBackground,
            AlertButtonGroupHeight = _option.AlertButtonGroupHeight,
            OkButtonText = _option.OkButtonText,
            CancelButtonText = _option.CancelButtonText,
            Content = content,
            ContentTemplate = template ?? _dict["Alert.Default.DataTemplate"] as DataTemplate,
            // 不设置为始终最前以允许用户切换窗口
            Topmost = false,
            // 在任务栏显示，使最小化后可以还原
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
            if (GetWindowRect(_ownerHandle, out RECT rect))
            {
                var wpfRect = ConvertRectToWpfUnit(rect);
                ownerRect = wpfRect;
            }
            else
            {
                return null; // 无法获取窗口尺寸
            }
        }

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
            IsHitTestVisible = true,
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
            Fill = _option.MaskBrush ?? new SolidColorBrush(Color.FromArgb(128, 0, 0, 0))
        };

        maskWindow.Content = rectangle;

        // 如果有WPF Owner，设置蒙版窗口的Owner，确保Z顺序和焦点行为正确
        if (_ownerWindow != null)
        {
            maskWindow.Owner = _ownerWindow;
        }

        // 添加事件处理，确保点击蒙版时不会让蒙版获得焦点
        maskWindow.MouseDown += (sender, e) =>
        {
            // 点击蒙版时确保Alert窗口保持在前台
            if (_currentAlertWindow != null)
            {
                // 将警告窗口置于前台
                _currentAlertWindow.Activate();
                BringWindowToTop(new WindowInteropHelper(_currentAlertWindow).Handle);
                SetForegroundWindow(new WindowInteropHelper(_currentAlertWindow).Handle);
            }
            e.Handled = true;
        };

        // 添加窗口加载事件，设置窗口为无法激活
        maskWindow.Loaded += (sender, e) =>
        {
            var hwnd = new WindowInteropHelper(maskWindow).Handle;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_NOACTIVATE);
        };

        return maskWindow;
    }

    /// <summary>
    /// 计算窗口中心位置
    /// </summary>
    /// <param name="window">要定位的窗口</param>
    private void PositionWindowInCenter(Window window)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        // 如果有拥有者窗口，使用它作为中心点
        if (_ownerWindow != null)
        {
            window.Owner = _ownerWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            return;
        }

        // 如果有有效的句柄，使用句柄窗口位置计算中心点
        if (_ownerHandle != IntPtr.Zero && IsWindow(_ownerHandle))
        {
            RECT ownerRect;
            if (GetWindowRect(_ownerHandle, out ownerRect))
            {
                // 转换为WPF单位
                var wpfRect = ConvertRectToWpfUnit(ownerRect);

                // 计算窗口的尺寸
                double ownerWidth = wpfRect.Width;
                double ownerHeight = wpfRect.Height;

                // 计算中心点
                double ownerCenterX = wpfRect.Left + (ownerWidth / 2);
                double ownerCenterY = wpfRect.Top + (ownerHeight / 2);

                // 计算警告窗口左上角位置
                window.Left = ownerCenterX - (window.Width / 2);
                window.Top = ownerCenterY - (window.Height / 2);

                // 确保窗口在屏幕边界内
                EnsureWindowInScreenBounds(window);

                // 设置窗口启动位置为手动，以使用我们计算的位置
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                return;
            }
        }

        // 如果没有Owner或者Owner无效，使用屏幕中心
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    /// <summary>
    /// 确保窗口在屏幕边界内
    /// </summary>
    /// <param name="window">要检查的窗口</param>
    private void EnsureWindowInScreenBounds(Window window)
    {
        var screenBounds = SystemParameters.WorkArea;

        // 检查并调整左边界
        if (window.Left < screenBounds.Left)
        {
            window.Left = screenBounds.Left;
        }
        else if (window.Left + window.Width > screenBounds.Right)
        {
            window.Left = screenBounds.Right - window.Width;
        }

        // 检查并调整上边界
        if (window.Top < screenBounds.Top)
        {
            window.Top = screenBounds.Top;
        }
        else if (window.Top + window.Height > screenBounds.Bottom)
        {
            window.Top = screenBounds.Bottom - window.Height;
        }
    }

    #region DPI缩放

    /// <summary>
    /// 获取当前DPI缩放比例
    /// </summary>
    /// <returns>DPI缩放矩阵</returns>
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
    /// <param name="x">像素X坐标</param>
    /// <param name="y">像素Y坐标</param>
    /// <returns>WPF坐标点</returns>
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
    /// <param name="rect">原始RECT结构</param>
    /// <returns>转换后的WPF Rect</returns>
    private Rect ConvertRectToWpfUnit(RECT rect)
    {
        Point topLeft = ConvertPixelToWpfUnit(rect.Left, rect.Top);
        Point bottomRight = ConvertPixelToWpfUnit(rect.Right, rect.Bottom);

        return new Rect(topLeft, bottomRight);
    }

    #endregion DPI缩放

    /// <summary>
    /// 更新蒙版窗口位置
    /// </summary>
    private void UpdateMaskPosition()
    {
        if (_maskWindow != null && _ownerWindow != null && _maskWindow.IsLoaded)
        {
            _maskWindow.Left = _ownerWindow.Left;
            _maskWindow.Top = _ownerWindow.Top;
        }
    }

    /// <summary>
    /// 更新蒙版窗口大小
    /// </summary>
    private void UpdateMaskSize()
    {
        if (_maskWindow != null && _ownerWindow != null && _maskWindow.IsLoaded)
        {
            _maskWindow.Width = _ownerWindow.Width;
            _maskWindow.Height = _ownerWindow.Height;
        }
    }

    /// <summary>
    /// 从句柄窗口更新蒙版窗口位置和大小
    /// </summary>
    private void UpdateMaskPositionFromHandle()
    {
        if (_maskWindow != null && _ownerHandle != IntPtr.Zero && IsWindow(_ownerHandle))
        {
            RECT rect;
            if (GetWindowRect(_ownerHandle, out rect))
            {
                var wpfRect = ConvertRectToWpfUnit(rect);

                _maskWindow.Left = wpfRect.Left;
                _maskWindow.Top = wpfRect.Top;
                _maskWindow.Width = wpfRect.Width;
                _maskWindow.Height = wpfRect.Height;
            }
        }
    }

    /// <summary>
    /// 用于监控非WPF窗口变化的窗口事件回调
    /// </summary>
    /// <param name="hWinEventHook">事件钩子句柄</param>
    /// <param name="eventType">事件类型</param>
    /// <param name="hwnd">窗口句柄</param>
    /// <param name="idObject">对象ID</param>
    /// <param name="idChild">子项ID</param>
    /// <param name="dwEventThread">事件线程ID</param>
    /// <param name="dwmsEventTime">事件时间戳</param>
    private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        // 只处理我们拥有者窗口的事件
        if (hwnd == _ownerHandle)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateMaskPositionFromHandle();
            }));
        }
    }

    /// <summary>
    /// 为非WPF窗口设置窗口事件监控
    /// </summary>
    private void SetupNonWpfWindowMonitoring()
    {
        // 只有在有非WPF所有者且尚未设置钩子时才设置
        if (_ownerHandle != IntPtr.Zero && _winEventHook == IntPtr.Zero && _ownerWindow == null)
        {
            // 创建委托（保持引用以防止垃圾回收）
            _winEventProc = new WinEventDelegate(WinEventProc);

            // 为窗口位置变化设置事件钩子
            _winEventHook = SetWinEventHook(EVENT_OBJECT_LOCATIONCHANGE, EVENT_OBJECT_LOCATIONCHANGE, IntPtr.Zero, _winEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);
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
                    BringWindowToTop(ownerHandle);
                    SetForegroundWindow(ownerHandle);
                }
            }
            // 如果只有Owner句柄
            else if (_ownerHandle != IntPtr.Zero && IsWindow(_ownerHandle))
            {
                // 使用Win32 API设置窗口到前台
                BringWindowToTop(_ownerHandle);
                SetForegroundWindow(_ownerHandle);
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
    /// <param name="template">内容模板</param>
    /// <param name="title">窗口标题</param>
    /// <returns>对话框结果</returns>
    public bool? Show(object content, DataTemplate template = null, string title = null)
    {
        // 确保在UI线程上执行
        if (Application.Current.Dispatcher.CheckAccess())
        {
            return ShowDialogInternal(content, template, title);
        }
        else
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                return ShowDialogInternal(content, template, title);
            });
        }
    }

    /// <summary>
    /// 内部方法，用于显示对话框
    /// </summary>
    /// <param name="content">窗口内容</param>
    /// <param name="template">内容模板</param>
    /// <param name="title">窗口标题</param>
    /// <returns>对话框结果</returns>
    private bool? ShowDialogInternal(object content, DataTemplate template, string title)
    {
        var window = CreateAlertWindow(content, template, title);
        _currentAlertWindow = window;
        PositionWindowInCenter(window);

        // 存储弹窗显示前的当前活动窗口句柄
        IntPtr previousActiveWindow = GetForegroundWindow();

        // 不禁用Owner窗口，允许用户与Owner窗口交互
        // 移除了EnableWindow(false)的调用，允许用户正常操作Owner窗口

        // 如果启用了蒙版并且有所有者，显示蒙版
        if (_option.IsShowMask && (_ownerWindow != null || _ownerHandle != IntPtr.Zero))
        {
            _maskWindow = CreateMaskWindow();
            if (_maskWindow != null)
            {
                // 显示蒙版窗口但不激活它
                _maskWindow.Show();

                // 使用Win32 API确保蒙版窗口显示但不激活
                var maskHandle = new WindowInteropHelper(_maskWindow).Handle;
                ShowWindow(maskHandle, SW_SHOWNA);

                // 添加位置和尺寸同步
                if (_ownerWindow != null)
                {
                    // 对于WPF窗口，添加位置变化监听器
                    _ownerWindow.LocationChanged += (s, e) => UpdateMaskPosition();
                    _ownerWindow.SizeChanged += (s, e) => UpdateMaskSize();
                }
                else if (_ownerHandle != IntPtr.Zero)
                {
                    // 对于句柄窗口，设置窗口事件监控
                    SetupNonWpfWindowMonitoring();
                }

                // 设置Alert窗口的Z顺序在蒙版之上
                if (_ownerWindow != null)
                {
                    // 保留原始的Owner关系
                    window.Owner = _ownerWindow;
                }

                // 添加蒙版窗口作为警告窗口的关闭回调
                window.Closed += (sender, e) =>
                {
                    // 确保蒙版窗口关闭
                    if (_maskWindow != null && _maskWindow.IsLoaded)
                    {
                        _maskWindow.Close();
                    }

                    // 清理事件钩子
                    UnhookEvents();
                };

                // 添加窗口状态变化监听
                window.StateChanged += (sender, e) =>
                {
                    if (window.WindowState == WindowState.Minimized)
                    {
                        // 如果警告窗口被最小化，也隐藏蒙版窗口
                        if (_maskWindow != null && _maskWindow.IsLoaded)
                        {
                            _maskWindow.Visibility = Visibility.Hidden;
                        }
                    }
                    else
                    {
                        // 如果警告窗口恢复，也显示蒙版窗口
                        if (_maskWindow != null && _maskWindow.IsLoaded)
                        {
                            _maskWindow.Visibility = Visibility.Visible;

                            // 更新蒙版位置和大小
                            if (_ownerWindow != null)
                            {
                                UpdateMaskPosition();
                                UpdateMaskSize();
                            }
                            else if (_ownerHandle != IntPtr.Zero)
                            {
                                UpdateMaskPositionFromHandle();
                            }
                        }
                    }
                };
            }
        }

        bool? result = null;
        try
        {
            // 添加窗口焦点监控，确保Alert窗口始终在最上层
            window.Loaded += (sender, e) =>
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                // 主动设置窗口为顶层
                SetWindowPos(hwnd, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            };

            window.Deactivated += (sender, e) =>
            {
                // 当窗口失去焦点时，不强制重新激活
                // 但确保窗口保持在合适的Z顺序位置
                window.Dispatcher.InvokeAsync(() =>
                {
                    if (window.IsLoaded)
                    {
                        var hwnd = new WindowInteropHelper(window).Handle;
                        // 使用HWND_TOPMOST保持在最上层，但不强制激活
                        SetWindowPos(hwnd, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                    }
                });
            };

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
                _maskWindow.Close();
                _maskWindow = null;
            }

            // 清理事件钩子
            UnhookEvents();

            // 在弹窗关闭后激活Owner窗口以确保它保持在前台
            ActivateOwnerWindow();
        }
    }

    #endregion 实现IAlertService接口
}