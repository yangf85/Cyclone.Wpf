using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 提供对Windows原生API的封装，用于窗口操作和事件监控
/// </summary>
internal static class WindowsNativeService
{
    #region 结构体定义

    /// <summary>
    /// Windows RECT 结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion 结构体定义

    #region 常量定义

    // ShowWindow函数的常量参数
    public const int SW_SHOW = 5;             // 显示窗口

    public const int SW_SHOWNA = 8;           // 显示窗口但不激活

    // 窗口样式常量
    public const int GWL_EXSTYLE = -20;               // 扩展窗口样式

    public const int WS_EX_NOACTIVATE = 0x08000000;   // 窗口不激活
    public const int WS_EX_TRANSPARENT = 0x00000020;  // 透明窗口（点击穿透）
    public const int HWND_TOPMOST = -1;               // 置顶窗口
    public const int HWND_NOTOPMOST = -2;             // 取消置顶

    // SetWindowPos标志常量
    public const uint SWP_NOSIZE = 0x0001;        // 保持当前大小

    public const uint SWP_NOMOVE = 0x0002;        // 保持当前位置
    public const uint SWP_NOACTIVATE = 0x0010;    // 不激活窗口
    public const uint SWP_SHOWWINDOW = 0x0040;    // 显示窗口

    // 窗口消息常量
    public const int WM_MOVE = 0x0003;            // 窗口移动消息

    public const int WM_SIZE = 0x0005;            // 窗口大小改变消息

    // 窗口事件常量
    public const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;  // 窗口位置变化事件

    public const uint EVENT_OBJECT_REORDER = 0x8004;         // 窗口Z序变化事件
    public const uint WINEVENT_OUTOFCONTEXT = 0x0000;        // 事件钩子标志

    #endregion 常量定义

    #region 委托定义

    /// <summary>
    /// 窗口事件监控委托
    /// </summary>
    public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
        IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    #endregion 委托定义

    #region Window API导入

    /// <summary>
    /// 获取窗口矩形
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    /// <summary>
    /// 判断句柄是否为有效窗口
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool IsWindow(IntPtr hWnd);

    /// <summary>
    /// 获取前台窗口句柄
    /// </summary>
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    /// <summary>
    /// 设置前台窗口
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>
    /// 将窗口置于顶层
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool BringWindowToTop(IntPtr hWnd);

    /// <summary>
    /// 判断窗口是否可见
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    /// <summary>
    /// 启用或禁用窗口
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    /// <summary>
    /// 显示窗口
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    /// <summary>
    /// 获取窗口样式
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    /// <summary>
    /// 设置窗口样式
    /// </summary>
    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    /// <summary>
    /// 设置窗口位置和Z顺序
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    /// <summary>
    /// 设置窗口事件钩子
    /// </summary>
    [DllImport("user32.dll")]
    public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax,
        IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
        uint idThread, uint dwFlags);

    /// <summary>
    /// 移除窗口事件钩子
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    #endregion Window API导入

    #region 实用方法

    /// <summary>
    /// 判断窗口句柄是否有效
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否有效</returns>
    public static bool IsValidWindow(IntPtr windowHandle)
    {
        return windowHandle != IntPtr.Zero && IsWindow(windowHandle);
    }

    /// <summary>
    /// 设置窗口为无激活样式
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    public static void SetWindowNoActivate(IntPtr windowHandle)
    {
        if (IsValidWindow(windowHandle))
        {
            var extendedStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
            SetWindowLong(windowHandle, GWL_EXSTYLE, extendedStyle | WS_EX_NOACTIVATE);
        }
    }

    /// <summary>
    /// 将窗口设置为置顶不激活
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    public static void SetWindowTopMostNoActivate(IntPtr windowHandle)
    {
        if (IsValidWindow(windowHandle))
        {
            SetWindowPos(windowHandle, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }
    }

    /// <summary>
    /// 将窗口设置为置顶并显示
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    public static void SetWindowTopMostAndShow(IntPtr windowHandle)
    {
        if (IsValidWindow(windowHandle))
        {
            SetWindowPos(windowHandle, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }
    }

    /// <summary>
    /// 显示窗口但不激活
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    public static void ShowWindowNoActivate(IntPtr windowHandle)
    {
        if (IsValidWindow(windowHandle))
        {
            ShowWindow(windowHandle, SW_SHOWNA);
        }
    }

    /// <summary>
    /// 激活窗口并置于前台
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    public static void ActivateAndBringToFront(IntPtr windowHandle)
    {
        if (IsValidWindow(windowHandle))
        {
            BringWindowToTop(windowHandle);
            SetForegroundWindow(windowHandle);
        }
    }

    /// <summary>
    /// 获取窗口矩形（转换为WPF Rect）
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>窗口矩形，如果获取失败则返回空值</returns>
    public static Rect? GetWindowRectAsWpfRect(IntPtr windowHandle)
    {
        if (IsValidWindow(windowHandle))
        {
            if (GetWindowRect(windowHandle, out RECT rect))
            {
                // 转换为WPF Rect
                return ConvertRectToWpfUnit(rect);
            }
        }
        return null;
    }

    /// <summary>
    /// 将RECT结构转换为WPF Rect（设备无关单位）
    /// </summary>
    /// <param name="rect">原始RECT结构</param>
    /// <returns>转换后的WPF Rect</returns>
    public static Rect ConvertRectToWpfUnit(RECT rect)
    {
        Point topLeft = ConvertPixelToWpfUnit(rect.Left, rect.Top);
        Point bottomRight = ConvertPixelToWpfUnit(rect.Right, rect.Bottom);

        return new Rect(topLeft, bottomRight);
    }

    /// <summary>
    /// 将物理像素坐标转换为WPF设备无关单位
    /// </summary>
    /// <param name="x">像素X坐标</param>
    /// <param name="y">像素Y坐标</param>
    /// <returns>WPF坐标点</returns>
    public static Point ConvertPixelToWpfUnit(int x, int y)
    {
        Matrix transformToDevice = GetDpiScale();
        double dpiX = transformToDevice.M11;
        double dpiY = transformToDevice.M22;

        // 转换坐标
        return new Point(x / dpiX, y / dpiY);
    }

    /// <summary>
    /// 获取当前DPI缩放比例
    /// </summary>
    /// <returns>DPI缩放矩阵</returns>
    public static Matrix GetDpiScale()
    {
        var source = PresentationSource.FromVisual(Application.Current.MainWindow);
        if (source?.CompositionTarget != null)
        {
            return source.CompositionTarget.TransformToDevice;
        }
        return Matrix.Identity; // 如果无法确定，则返回1:1比例
    }

    #endregion 实用方法
}