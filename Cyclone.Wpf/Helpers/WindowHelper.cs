using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Helpers;

public static class WindowHelper
{
    #region IsDragEnabled

    public static readonly DependencyProperty IsDragEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsDragEnabled",
            typeof(bool),
            typeof(WindowHelper),
            new PropertyMetadata(false, OnIsDragEnabledChanged));

    private static void OnIsDragEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            if ((bool)e.NewValue)
            {
                element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            }
            else
            {
                element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
            }
        }
    }

    private static void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is UIElement element)
        {
            Window window = Window.GetWindow(element);
            window?.DragMove();
        }
    }

    public static bool GetIsDragEnabled(UIElement element)
    {
        return (bool)element.GetValue(IsDragEnabledProperty);
    }

    public static void SetIsDragEnabled(UIElement element, bool value)
    {
        element.SetValue(IsDragEnabledProperty, value);
    }

    #endregion IsDragEnabled

    #region IsDoubleClickMaximizeEnabled

    public static readonly DependencyProperty IsDoubleClickMaximizeEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsDoubleClickMaximizeEnabled",
            typeof(bool),
            typeof(WindowHelper),
            new PropertyMetadata(false, OnIsDoubleClickMaximizeEnabledChanged));

    private static void OnIsDoubleClickMaximizeEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            if ((bool)e.NewValue)
            {
                element.MouseLeftButtonDown += Element_MouseLeftButtonDoubleClick;
            }
            else
            {
                element.MouseLeftButtonDown -= Element_MouseLeftButtonDoubleClick;
            }
        }
    }

    private static void Element_MouseLeftButtonDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is UIElement element && e.ClickCount == 2)
        {
            ToggleWindowState(element);
        }
    }

    private static void ToggleWindowState(UIElement element)
    {
        Window window = Window.GetWindow(element);
        if (window != null)
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
    }

    public static bool GetIsDoubleClickMaximizeEnabled(UIElement element)
    {
        return (bool)element.GetValue(IsDoubleClickMaximizeEnabledProperty);
    }

    public static void SetIsDoubleClickMaximizeEnabled(UIElement element, bool value)
    {
        element.SetValue(IsDoubleClickMaximizeEnabledProperty, value);
    }

    #endregion IsDoubleClickMaximizeEnabled

    #region IsShowSystemMenuOnRightClick

    public static readonly DependencyProperty IsShowSystemMenuOnRightClickProperty =
        DependencyProperty.RegisterAttached(
            "IsShowSystemMenuOnRightClick",
            typeof(bool),
            typeof(WindowHelper),
            new PropertyMetadata(false, OnIsShowSystemMenuOnRightClickChanged));

    private static void OnIsShowSystemMenuOnRightClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            if ((bool)e.NewValue)
            {
                element.MouseRightButtonUp += Element_MouseRightButtonUp;
            }
            else
            {
                element.MouseRightButtonUp -= Element_MouseRightButtonUp;
            }
        }
    }

    private static void Element_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is UIElement element)
        {
            Window window = Window.GetWindow(element);
            if (window != null)
            {
                var point = element.PointToScreen(e.GetPosition(element));
                ShowSystemMenu(window, (int)point.X, (int)point.Y);
            }
        }
    }

    private static void ShowSystemMenu(Window window, int x, int y)
    {
        IntPtr handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;
        IntPtr systemMenu = GetSystemMenu(handle, false);
        if (systemMenu != IntPtr.Zero)
        {
            int cmd = TrackPopupMenu(systemMenu, 0x100, x, y, 0, handle, IntPtr.Zero);
            if (cmd > 0)
            {
                SendMessage(handle, 0x112, (IntPtr)cmd, IntPtr.Zero);
            }
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("user32.dll")]
    private static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    public static bool GetIsShowSystemMenuOnRightClick(UIElement element)
    {
        return (bool)element.GetValue(IsShowSystemMenuOnRightClickProperty);
    }

    public static void SetIsShowSystemMenuOnRightClick(UIElement element, bool value)
    {
        element.SetValue(IsShowSystemMenuOnRightClickProperty, value);
    }

    #endregion IsShowSystemMenuOnRightClick

    #region IsHideOnCloseEnabled

    public static readonly DependencyProperty IsHideOnCloseEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsHideOnCloseEnabled",
            typeof(bool),
            typeof(WindowHelper),
            new PropertyMetadata(false, OnIsHideOnCloseEnabledChanged));

    private static void OnIsHideOnCloseEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Window window)
        {
            if ((bool)e.NewValue)
            {
                window.Closing += Window_Closing;
            }
            else
            {
                window.Closing -= Window_Closing;
            }
        }
    }

    private static void Window_Closing(object sender, CancelEventArgs e)
    {
        if (sender is Window window)
        {
            e.Cancel = true;
            window.Hide();
        }
    }

    public static bool GetIsHideOnCloseEnabled(Window window)
    {
        return (bool)window.GetValue(IsHideOnCloseEnabledProperty);
    }

    public static void SetIsHideOnCloseEnabled(Window window, bool value)
    {
        window.SetValue(IsHideOnCloseEnabledProperty, value);
    }

    #endregion IsHideOnCloseEnabled

    #region IsClosing

    /// <summary>
    /// 绑定需要使用Mode=OneWayToSource，否则窗口关闭事件会导致绑定失效
    /// </summary>
    public static readonly DependencyProperty IsClosingProperty =
        DependencyProperty.RegisterAttached(
            "IsClosing",
            typeof(bool),
            typeof(WindowHelper),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty IsClosingMonitorEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsClosingMonitorEnabled",
            typeof(bool),
            typeof(WindowHelper),
            new PropertyMetadata(false, OnIsClosingMonitorEnabledChanged));

    private static void OnIsClosingMonitorEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Window window)
        {
            if ((bool)e.NewValue)
            {
                window.Closing += Window_ClosingTracker;
            }
            else
            {
                window.Closing -= Window_ClosingTracker;
                window.SetValue(IsClosingProperty, false);
            }
        }
    }

    private static void Window_ClosingTracker(object sender, CancelEventArgs e)
    {
        if (sender is Window window)
        {
            window.SetValue(IsClosingProperty, true);
        }
    }

    public static bool GetIsClosing(Window window)
    {
        return (bool)window.GetValue(IsClosingProperty);
    }

    public static void SetIsClosing(Window window, bool value)
    {
        throw new InvalidOperationException("IsClosing is a read-only property.");
    }

    public static bool GetIsClosingMonitorEnabled(Window window)
    {
        return (bool)window.GetValue(IsClosingMonitorEnabledProperty);
    }

    public static void SetIsClosingMonitorEnabled(Window window, bool value)
    {
        window.SetValue(IsClosingMonitorEnabledProperty, value);
    }

    #endregion IsClosing
}