using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Cyclone.Wpf.Controls;

internal static class SystemMenuManager
{
    [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
    private static extern IntPtr GetSystemMenu(IntPtr hwnd, int bRevert);

    [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
    private static extern int RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

    [DllImport("user32.dll", EntryPoint = "DrawMenuBar")]
    private static extern IntPtr DrawMenuBar(IntPtr hwnd);

    [DllImport("user32.dll", EntryPoint = "GetMenuItemCount")]
    private static extern int GetMenuItemCount(IntPtr hMenu);

    private const uint MF_BYPOSITION = 0x0400;

    public static void DisableCloseMenuItem(Window window)
    {
        if (window == null) return;

        window.SourceInitialized += (s, e) =>
        {
            var helper = new WindowInteropHelper(window);
            IntPtr hwnd = helper.Handle;
            IntPtr hMenu = GetSystemMenu(hwnd, 0);

            // 移除所有系统菜单项
            int itemCount = GetMenuItemCount(hMenu);
            for (int i = itemCount - 1; i >= 0; i--)
            {
                RemoveMenu(hMenu, (uint)i, MF_BYPOSITION);
            }

            DrawMenuBar(hwnd);
        };
    }
}