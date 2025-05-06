using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = "PART_ExecuteButton", Type = typeof(Button))]
public class ColorEyedropper : Control
{
    #region SelectedColor

    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorEyedropper),
            new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

    public Color SelectedColor
    {
        get { return (Color)GetValue(SelectedColorProperty); }
        set { SetValue(SelectedColorProperty, value); }
    }

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    #endregion SelectedColor

    #region IsShowSelectedColor

    public bool IsShowSelectedColor
    {
        get => (bool)GetValue(IsShowSelectedColorProperty);
        set => SetValue(IsShowSelectedColorProperty, value);
    }

    public static readonly DependencyProperty IsShowSelectedColorProperty =
        DependencyProperty.Register(nameof(IsShowSelectedColor), typeof(bool), typeof(ColorEyedropper), new PropertyMetadata(true));

    #endregion IsShowSelectedColor

    #region PositionText

    public string PositionText
    {
        get => (string)GetValue(PositionTextProperty);
        set => SetValue(PositionTextProperty, value);
    }

    public static readonly DependencyProperty PositionTextProperty =
        DependencyProperty.Register(nameof(PositionText), typeof(string), typeof(ColorEyedropper), new PropertyMetadata("坐标"));

    #endregion PositionText

    #region ShortcutKeyText

    public string ShortcutKeyText
    {
        get => (string)GetValue(ShortcutKeyTextProperty);
        set => SetValue(ShortcutKeyTextProperty, value);
    }

    public static readonly DependencyProperty ShortcutKeyTextProperty =
        DependencyProperty.Register(nameof(ShortcutKeyText), typeof(string), typeof(ColorEyedropper), new PropertyMetadata("按 Shift 切换格式 | 按 C 复制颜色值并完成 | 按 Esc 取消"));

    #endregion ShortcutKeyText

    #region 路由命令

    /// <summary>
    /// 取色命令
    /// </summary>
    public static readonly RoutedCommand PickColorCommand = new RoutedCommand("PickColor", typeof(ColorEyedropper));

    #endregion 路由命令

    #region 路由事件

    /// <summary>
    /// 颜色选中事件
    /// </summary>
    public static readonly RoutedEvent ColorPickedEvent =
        EventManager.RegisterRoutedEvent("ColorPicked", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ColorEyedropper));

    /// <summary>
    /// 颜色选中事件处理器
    /// </summary>
    public event RoutedEventHandler ColorPicked
    {
        add { AddHandler(ColorPickedEvent, value); }
        remove { RemoveHandler(ColorPickedEvent, value); }
    }

    /// <summary>
    /// 颜色复制到剪贴板事件
    /// </summary>
    public static readonly RoutedEvent ColorCopiedEvent =
        EventManager.RegisterRoutedEvent("ColorCopied", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ColorEyedropper));

    /// <summary>
    /// 颜色复制到剪贴板事件处理器
    /// </summary>
    public event RoutedEventHandler ColorCopied
    {
        add { AddHandler(ColorCopiedEvent, value); }
        remove { RemoveHandler(ColorCopiedEvent, value); }
    }

    #endregion 路由事件

    // 取色窗口
    private ColorPickerOverlay _pickerOverlay;

    // 当前像素颜色
    private Color _currentColor;

    // 当前坐标
    private Point _currentPosition;

    // 是否使用RGB格式
    private bool _isRgbFormat = true;

    // 取色状态
    private bool _isPickingColor = false;

    // 桌面DC
    private IntPtr _desktopDC;

    // 预创建的委托，避免频繁创建
    private Action _updateAction;

    /// <summary>
    /// 静态构造函数
    /// </summary>
    static ColorEyedropper()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorEyedropper),
            new FrameworkPropertyMetadata(typeof(ColorEyedropper)));

        CommandManager.RegisterClassCommandBinding(typeof(ColorEyedropper), new CommandBinding(PickColorCommand, HandlePickColorCommand));
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    public ColorEyedropper()
    {
        // 预创建委托
        _updateAction = () => _pickerOverlay.UpdateColorInfo(_currentPosition, _currentColor, _isRgbFormat);
    }

    private static void HandlePickColorCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is ColorEyedropper eyedropper)
        {
            eyedropper.PickColor();
        }
    }

    private void PickColor()
    {
        if (_isPickingColor)
            return;

        try
        {
            _isPickingColor = true;

            // 获取桌面DC
            _desktopDC = GetDC(IntPtr.Zero);
            if (_desktopDC == IntPtr.Zero)
            {
                CleanUp();
                return;
            }

            // 创建取色覆盖层
            _pickerOverlay = new ColorPickerOverlay(this);

            // 注册事件
            _pickerOverlay.Closed += (s, e) => CleanUp();

            // 显示取色覆盖层
            _pickerOverlay.Show();
            _pickerOverlay.Activate();
            _pickerOverlay.Focus();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"取色失败: {ex.Message}");
            CleanUp();
        }
    }

    /// <summary>
    /// 处理鼠标移动事件
    /// </summary>
    public void OnMouseMove(Point position)
    {
        // 更新当前位置
        _currentPosition = position;

        // 获取当前颜色
        _currentColor = GetPixelColor((int)position.X, (int)position.Y);

        // 在UI线程上更新覆盖层 - 使用预创建的委托
        if (_pickerOverlay != null && _pickerOverlay.IsLoaded)
        {
            _pickerOverlay.Dispatcher.BeginInvoke(_updateAction, DispatcherPriority.Render);
        }
    }

    /// <summary>
    /// 完成颜色选择操作
    /// </summary>
    public void CompleteColorSelection()
    {
        SelectedColor = _currentColor;
        RaiseEvent(new RoutedEventArgs(ColorPickedEvent, this));
        CleanUp();
    }

    /// <summary>
    /// 复制当前颜色值到剪贴板并完成操作
    /// </summary>
    public void CopyColorAndComplete()
    {
        string text = _isRgbFormat
            ? $"RGB({_currentColor.R}, {_currentColor.G}, {_currentColor.B})"
            : $"#{_currentColor.R:X2}{_currentColor.G:X2}{_currentColor.B:X2}";

        // 设置选中的颜色
        SelectedColor = _currentColor;

        // 触发颜色选中事件
        RaiseEvent(new RoutedEventArgs(ColorPickedEvent, this));

        // 先清理资源和关闭窗口
        CleanUp();

        // 使用低优先级的后台操作设置剪贴板
        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        {
            try
            {
                // 使用DataObject更可靠
                DataObject dataObject = new DataObject();
                dataObject.SetData(DataFormats.Text, text);
                Clipboard.SetDataObject(dataObject, true);

                // 触发颜色复制事件
                RaiseEvent(new RoutedEventArgs(ColorCopiedEvent, this));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"设置剪贴板时出现错误: {ex.Message}");
            }
        }));
    }

    /// <summary>
    /// 切换颜色格式
    /// </summary>
    public void ToggleColorFormat()
    {
        _isRgbFormat = !_isRgbFormat;
    }

    /// <summary>
    /// 获取指定位置的颜色
    /// </summary>
    private Color GetPixelColor(int x, int y)
    {
        try
        {
            if (_desktopDC == IntPtr.Zero)
                return Colors.Black;

            int colorRef = GetPixel(_desktopDC, x, y);

            if (colorRef < 0)
                return Colors.Black;

            byte r = (byte)(colorRef & 0x000000FF);
            byte g = (byte)((colorRef & 0x0000FF00) >> 8);
            byte b = (byte)((colorRef & 0x00FF0000) >> 16);

            return Color.FromRgb(r, g, b);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"获取像素颜色时出错: {ex.Message}");
            return Colors.Black;
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    private void CleanUp()
    {
        // 释放DC
        if (_desktopDC != IntPtr.Zero)
        {
            ReleaseDC(IntPtr.Zero, _desktopDC);
            _desktopDC = IntPtr.Zero;
        }

        // 关闭覆盖层
        if (_pickerOverlay != null)
        {
            _pickerOverlay.Close();
            _pickerOverlay = null;
        }

        // 重置状态
        _isPickingColor = false;
    }

    #region 必要的Win32 API（仅保留取色相关API）

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int GetPixel(IntPtr hdc, int nXPos, int nYPos);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    #endregion 必要的Win32 API（仅保留取色相关API）

    /// <summary>
    /// 取色覆盖层 - 纯WPF实现，处理鼠标和键盘事件
    /// </summary>
    private class ColorPickerOverlay : Window
    {
        // 颜色信息面板
        private Border _colorInfoPanel;

        // 坐标文本
        private TextBlock _positionText;

        // 颜色文本
        private TextBlock _colorText;

        // 颜色示例
        private Border _colorSample;

        // DPI缩放比例
        private double _dpiScaleX = 1.0;

        private double _dpiScaleY = 1.0;

        // 对父控件的引用
        private ColorEyedropper _parent;

        // 预创建的 SolidColorBrush，重用以减少内存分配
        private SolidColorBrush _colorBrush;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ColorPickerOverlay(ColorEyedropper parent)
        {
            // 保存父控件引用
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            // 设置窗口属性
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            AllowsTransparency = true;
            Background = null;
            Topmost = true;
            ShowInTaskbar = false;
            Cursor = Cursors.Cross;
            WindowStartupLocation = WindowStartupLocation.Manual;
            IsHitTestVisible = true; // 确保窗口可以接收鼠标事件

            // 使用虚拟屏幕
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            // 创建主布局
            Grid mainGrid = new Grid();
            mainGrid.IsHitTestVisible = true; // 确保网格可以接收鼠标事件
            Content = mainGrid;

            // 添加一个完全透明但可以接收鼠标事件的覆盖层
            Rectangle transparentBackground = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(1, 255, 255, 255)), // 几乎完全透明，但不是0
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsHitTestVisible = true, // 关键属性：确保它可以接收鼠标事件
                Cursor = Cursors.Cross  // 直接在覆盖层上设置光标
            };
            mainGrid.Children.Add(transparentBackground);
            Panel.SetZIndex(transparentBackground, 0);

            // 创建颜色信息面板
            _colorInfoPanel = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(220, 30, 30, 30)),
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = false // 信息面板不应阻止鼠标事件
            };
            mainGrid.Children.Add(_colorInfoPanel);
            Panel.SetZIndex(_colorInfoPanel, 100); // 确保信息面板在最上层

            // 创建面板内容
            StackPanel infoPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0)
            };
            _colorInfoPanel.Child = infoPanel;

            // 创建坐标文本
            _positionText = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 0, 5)
            };
            infoPanel.Children.Add(_positionText);

            // 创建颜色面板
            StackPanel colorPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0)
            };
            infoPanel.Children.Add(colorPanel);

            // 创建颜色文本
            _colorText = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            colorPanel.Children.Add(_colorText);

            // 创建颜色示例
            _colorSample = new Border
            {
                Width = 20,
                Height = 20,
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(1)
            };
            colorPanel.Children.Add(_colorSample);

            // 创建帮助文本
            TextBlock shortcutKeyText = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.LightGray),
                FontSize = 11,
                Text = $"{_parent.ShortcutKeyText}",
                Margin = new Thickness(0, 4, 0, 0)
            };
            infoPanel.Children.Add(shortcutKeyText);

            // 预创建 SolidColorBrush，后续重用
            _colorBrush = new SolidColorBrush();
            _colorSample.Background = _colorBrush;

            // 重要：在加载时立即设置全局光标，而不是依靠鼠标移动事件
            Loaded += (s, e) =>
            {
                // 在窗口加载时直接设置全局光标
                Mouse.OverrideCursor = Cursors.Cross;
            };

            // 确保窗口关闭时恢复原来的光标
            Closed += (s, e) =>
            {
                Mouse.OverrideCursor = null;
            };

            // 注册鼠标事件
            PreviewMouseMove += ColorPickerOverlay_PreviewMouseMove;
            PreviewMouseLeftButtonDown += ColorPickerOverlay_PreviewMouseLeftButtonDown;

            // 注册键盘事件处理
            KeyDown += ColorPickerOverlay_KeyDown;
            PreviewKeyDown += ColorPickerOverlay_PreviewKeyDown;

            // 注册事件
            Loaded += ColorPickerOverlay_Loaded;
            IsVisibleChanged += ColorPickerOverlay_IsVisibleChanged;
        }

        /// <summary>
        /// 鼠标移动事件处理
        /// </summary>
        private void ColorPickerOverlay_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // 获取屏幕坐标
            Point position = PointToScreen(e.GetPosition(this));

            // 调用父控件的鼠标移动处理方法
            _parent.OnMouseMove(position);
        }

        /// <summary>
        /// 鼠标左键按下事件处理
        /// </summary>
        private void ColorPickerOverlay_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 完成颜色选择
            _parent.CompleteColorSelection();
            e.Handled = true;
        }

        /// <summary>
        /// 窗口加载事件
        /// </summary>
        private void ColorPickerOverlay_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取DPI缩放比例
            PresentationSource source = PresentationSource.FromVisual(this);
            if (source != null && source.CompositionTarget != null)
            {
                _dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
                _dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
            }

            // 确保窗口获取焦点
            Activate();
            Focus();
            Keyboard.Focus(this);

            Mouse.OverrideCursor = Cursors.Cross;
        }

        /// <summary>
        /// 可见性变化事件
        /// </summary>
        private void ColorPickerOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                // 窗口变为可见时，确保激活
                Activate();
                Focus();
                Keyboard.Focus(this);
                Mouse.OverrideCursor = Cursors.Cross;
            }
            else
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// 键盘按键事件处理
        /// </summary>
        private void ColorPickerOverlay_KeyDown(object sender, KeyEventArgs e)
        {
            ProcessKeyDown(e);
        }

        /// <summary>
        /// 键盘预览按键事件处理（确保事件优先被捕获）
        /// </summary>
        private void ColorPickerOverlay_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ProcessKeyDown(e);
        }

        /// <summary>
        /// 处理按键
        /// </summary>
        private void ProcessKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                _parent.ToggleColorFormat();
                UpdateColorInfo(
                    _parent._currentPosition,
                    _parent._currentColor,
                    _parent._isRgbFormat);
                e.Handled = true;
            }
            else if (e.Key == Key.C)
            {
                _parent.CopyColorAndComplete();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _parent.CleanUp();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 更新颜色信息
        /// </summary>
        public void UpdateColorInfo(Point position, Color color, bool isRgbFormat)
        {
            // 更新坐标文本
            _positionText.Text = $"{_parent.PositionText}: ({(int)position.X}, {(int)position.Y})";

            // 更新颜色文本
            _colorText.Text = isRgbFormat
                ? $"RGB: {color.R}, {color.G}, {color.B}"
                : $"HEX: #{color.R:X2}{color.G:X2}{color.B:X2}";

            // 重用现有的画刷，只更新颜色
            _colorBrush.Color = color;

            // 确保面板可见
            _colorInfoPanel.Visibility = Visibility.Visible;

            // 更新面板位置 - 考虑DPI缩放
            UpdatePanelPosition(position);

            // 确保光标样式正确
            Mouse.OverrideCursor = Cursors.Cross;
        }

        /// <summary>
        /// 更新面板位置
        /// </summary>
        private void UpdatePanelPosition(Point position)
        {
            // 调整位置时考虑DPI
            double screenX = position.X / _dpiScaleX;
            double screenY = position.Y / _dpiScaleY;

            // 确保面板尺寸更新
            _colorInfoPanel.UpdateLayout();

            // 面板位置
            double left = screenX + 20;
            double top = screenY + 20;

            // 确保不超出屏幕边界
            if (left + _colorInfoPanel.ActualWidth > SystemParameters.VirtualScreenWidth / _dpiScaleX)
            {
                left = screenX - _colorInfoPanel.ActualWidth - 5;
            }

            if (top + _colorInfoPanel.ActualHeight > SystemParameters.VirtualScreenHeight / _dpiScaleY)
            {
                top = screenY - _colorInfoPanel.ActualHeight - 5;
            }

            // 设置面板位置
            _colorInfoPanel.Margin = new Thickness(left, top, 0, 0);
        }
    }
}