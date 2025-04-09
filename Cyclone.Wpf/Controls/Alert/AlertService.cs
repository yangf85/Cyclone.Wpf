using System;
using System.Windows;
using System.Threading;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

public interface IAlertService
{
    bool? Show(object content, DataTemplate template, string title = null);
}

public class AlertService : IAlertService
{
    private readonly AlertOption _option;

    #region Native Windows API

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

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion Native Windows API

    // 静态单例实例
    private static readonly Lazy<AlertService> _lazyInstance =
        new Lazy<AlertService>(() => new AlertService(), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// 获取AlertService的单例实例
    /// </summary>
    public static AlertService Instance => _lazyInstance.Value;

    private Window _ownerWindow;
    private IntPtr _ownerHandle;
    private static ResourceDictionary _dict;
    private Window _maskWindow;

    static AlertService()
    {
        _dict = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Cyclone.Wpf;component/Styles/Alert.xaml", UriKind.Absolute)
        };
    }

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
            throw new ArgumentNullException(nameof(windowHandle), "Invalid WindowHandle");
        }

        if (!IsWindow(windowHandle))
        {
            throw new ArgumentException("Handle is not a Window", nameof(windowHandle));
        }

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
            _ownerWindow = null;
            _ownerHandle = foregroundHandle;
        }
    }

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
            ContentTemplate = template ?? _dict["Alert.Default.DataTemplate"] as DataTemplate
        };

        return window;
    }

    /// <summary>
    /// 为所有者窗口创建蒙版窗口
    /// </summary>
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
            RECT rect;
            if (GetWindowRect(_ownerHandle, out rect))
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
            // 确保蒙版窗口不接收焦点
            Focusable = false,
            // 设置Z顺序较低
            Topmost = false
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

        return maskWindow;
    }

    /// <summary>
    /// 计算窗口中心位置
    /// </summary>
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

                // 计算中心点
                double ownerCenterX = wpfRect.Left + (wpfRect.Width / 2);
                double ownerCenterY = wpfRect.Top + (wpfRect.Height / 2);

                // 计算警告窗口左上角位置
                window.Left = ownerCenterX - (window.Width / 2);
                window.Top = ownerCenterY - (window.Height / 2);

                // 确保窗口在屏幕边界内
                EnsureWindowInScreenBounds(window);
                return;
            }
        }

        // 如果没有Owner或者Owner无效，使用屏幕中心
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    /// <summary>
    /// 确保窗口在屏幕边界内
    /// </summary>
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

    #region Implementation IAlertService

    /// <summary>
    /// 显示模态警告框并返回对话框结果
    /// </summary>
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

    private bool? ShowDialogInternal(object content, DataTemplate template, string title)
    {
        var window = CreateAlertWindow(content, template, title);
        PositionWindowInCenter(window);

        // 存储弹窗显示前的当前活动窗口句柄
        IntPtr previousActiveWindow = GetForegroundWindow();

        // 如果启用了蒙版并且有所有者，显示蒙版
        if (_option.IsShowMask && (_ownerWindow != null || _ownerHandle != IntPtr.Zero))
        {
            _maskWindow = CreateMaskWindow();
            if (_maskWindow != null)
            {
                // 显示蒙版窗口
                _maskWindow.Show();

                // 确保Alert窗口在蒙版之上，但不使用Topmost属性
                // window.Topmost = true; // 移除这一行以避免Z顺序问题

                // 设置Alert窗口的Owner为蒙版窗口
                if (_ownerWindow != null)
                {
                    // 保留原始的Owner关系
                    window.Owner = _ownerWindow;
                }
                else
                {
                    // 使用蒙版窗口作为Owner，确保Z-order正确
                    // 注意：这可能会导致窗口关闭行为变化
                    window.Owner = _maskWindow;
                }
            }
        }

        bool? result = null;
        try
        {
            // 显示模态对话框
            result = window.ShowDialog();
            return result;
        }
        finally
        {
            // 关闭蒙版窗口
            if (_maskWindow != null)
            {
                _maskWindow.Close();
                _maskWindow = null;
            }

            // 在弹窗关闭后激活Owner窗口以确保它保持在前台
            ActivateOwnerWindow();
        }
    }

    #endregion Implementation IAlertService
}