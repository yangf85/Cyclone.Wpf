using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 简洁的屏幕取色器控件
    /// </summary>
    public class ColorEyedropper : Control
    {
        #region Win32 API
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("gdi32.dll")]
        private static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }
        #endregion

        #region 私有字段
        private bool _isPickingColor = false;
        private Window _overlayWindow;
        #endregion

        #region 依赖属性
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(
                "SelectedColor",
                typeof(Color),
                typeof(ColorEyedropper),
                new FrameworkPropertyMetadata(
                    Colors.Transparent,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }
        #endregion

        #region 命令
        public static readonly RoutedCommand PickColorCommand = new RoutedCommand("PickColor", typeof(ColorEyedropper));
        #endregion

        #region 事件
        // 颜色变更事件
        public static readonly RoutedEvent ColorChangedEvent = EventManager.RegisterRoutedEvent(
            "ColorChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ColorEyedropper));

        public event RoutedEventHandler ColorChanged
        {
            add { AddHandler(ColorChangedEvent, value); }
            remove { RemoveHandler(ColorChangedEvent, value); }
        }
        #endregion

        #region 构造函数
        static ColorEyedropper()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ColorEyedropper),
                new FrameworkPropertyMetadata(typeof(ColorEyedropper)));
        }

        public ColorEyedropper()
        {
            // 注册命令
            CommandBindings.Add(new CommandBinding(PickColorCommand, PickColorCommandExecuted));

            // 添加卸载事件处理程序
            Unloaded += (s, e) => StopColorPicking();
        }
        #endregion

        #region 命令执行
        private void PickColorCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            StartColorPicking();
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 开始屏幕取色
        /// </summary>
        public void StartColorPicking()
        {
            if (_isPickingColor)
                return;

            _isPickingColor = true;

            try
            {
                // 创建全屏透明窗口以捕获鼠标事件
                _overlayWindow = new Window
                {
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    AllowsTransparency = true,
                    Background = Brushes.Transparent,
                    ShowInTaskbar = false,
                    Topmost = true,
                    Width = SystemParameters.PrimaryScreenWidth,
                    Height = SystemParameters.PrimaryScreenHeight,
                    Left = 0,
                    Top = 0,
                    WindowState = WindowState.Maximized,
                    Cursor = Cursors.Cross // 使用十字光标提示用户正在取色
                };

                // 注册事件
                _overlayWindow.MouseLeftButtonDown += OverlayWindow_MouseLeftButtonDown;
                _overlayWindow.KeyDown += OverlayWindow_KeyDown;

                // 显示窗口
                _overlayWindow.Show();
                _overlayWindow.Focus();
            }
            catch (Exception ex)
            {
                _isPickingColor = false;
                MessageBox.Show($"启动屏幕取色时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 停止屏幕取色
        /// </summary>
        public void StopColorPicking()
        {
            if (!_isPickingColor)
                return;

            _isPickingColor = false;

            if (_overlayWindow != null)
            {
                // 移除事件处理程序
                _overlayWindow.MouseLeftButtonDown -= OverlayWindow_MouseLeftButtonDown;
                _overlayWindow.KeyDown -= OverlayWindow_KeyDown;

                // 关闭窗口
                if (_overlayWindow.IsLoaded)
                {
                    _overlayWindow.Close();
                }
                _overlayWindow = null;
            }
        }
        #endregion

        #region 私有方法
        private Color GetColorAt(int x, int y)
        {
            IntPtr desktopDC = IntPtr.Zero;
            try
            {
                desktopDC = GetDC(IntPtr.Zero);
                uint pixel = GetPixel(desktopDC, x, y);

                byte r = (byte)(pixel & 0x000000FF);
                byte g = (byte)((pixel & 0x0000FF00) >> 8);
                byte b = (byte)((pixel & 0x00FF0000) >> 16);

                return Color.FromRgb(r, g, b);
            }
            finally
            {
                // 确保释放DC，避免GDI资源泄漏
                if (desktopDC != IntPtr.Zero)
                {
                    ReleaseDC(IntPtr.Zero, desktopDC);
                }
            }
        }
        #endregion

        #region 事件处理
        private void OverlayWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // 获取当前鼠标位置
                if (GetCursorPos(out POINT cursorPos))
                {
                    // 获取鼠标位置的颜色
                    Color color = GetColorAt(cursorPos.X, cursorPos.Y);

                    // 更新选中颜色
                    SelectedColor = color;

                    // 触发颜色变更事件
                    RaiseEvent(new RoutedEventArgs(ColorChangedEvent));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取屏幕颜色时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 无论成功与否，都停止取色
                StopColorPicking();
            }
        }

        private void OverlayWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // ESC键取消取色
                StopColorPicking();
            }
        }
        #endregion
    }
}