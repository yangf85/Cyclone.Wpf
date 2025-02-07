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

    public static bool GetIsDragEnabled(UIElement element)
    {
        return (bool)element.GetValue(IsDragEnabledProperty);
    }

    public static void SetIsDragEnabled(UIElement element, bool value)
    {
        element.SetValue(IsDragEnabledProperty, value);
    }

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

    #endregion IsDragEnabled

    #region IsDoubleClickMaximizeEnabled

    public static readonly DependencyProperty IsDoubleClickMaximizeEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsDoubleClickMaximizeEnabled",
            typeof(bool),
            typeof(WindowHelper),
            new PropertyMetadata(false, OnIsDoubleClickMaximizeEnabledChanged));

    public static bool GetIsDoubleClickMaximizeEnabled(UIElement element)
    {
        return (bool)element.GetValue(IsDoubleClickMaximizeEnabledProperty);
    }

    public static void SetIsDoubleClickMaximizeEnabled(UIElement element, bool value)
    {
        element.SetValue(IsDoubleClickMaximizeEnabledProperty, value);
    }

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

    #endregion IsDoubleClickMaximizeEnabled

    #region IsShowSystemMenuOnRightClick

    public static readonly DependencyProperty IsShowSystemMenuOnRightClickProperty =
        DependencyProperty.RegisterAttached(
            "IsShowSystemMenuOnRightClick",
            typeof(bool),
            typeof(WindowHelper),
            new PropertyMetadata(false, OnIsShowSystemMenuOnRightClickChanged));

    public static bool GetIsShowSystemMenuOnRightClick(UIElement element)
    {
        return (bool)element.GetValue(IsShowSystemMenuOnRightClickProperty);
    }

    public static void SetIsShowSystemMenuOnRightClick(UIElement element, bool value)
    {
        element.SetValue(IsShowSystemMenuOnRightClickProperty, value);
    }

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

    #endregion IsShowSystemMenuOnRightClick
}