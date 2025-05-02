using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices; // 添加这个引用来调用Win32 API

namespace Cyclone.Wpf.Controls;

///// <summary>
///// 屏幕颜色吸管工具
///// </summary>
//public class ColorEyedropper : Control
//{
//    private bool _isActive;
//    private Window _overlay;
//    private Image _preview;
//    private TextBlock _colorInfo;

//    static ColorEyedropper()
//    {
//        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorEyedropper),
//            new FrameworkPropertyMetadata(typeof(ColorEyedropper)));
//    }

//    #region IsActive

//    public bool IsActive
//    {
//        get => (bool)GetValue(IsActiveProperty);
//        set => SetValue(IsActiveProperty, value);
//    }

//    public static readonly DependencyProperty IsActiveProperty =
//        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(ColorEyedropper),
//        new PropertyMetadata(false, OnIsActiveChanged));

//    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        if (d is ColorEyedropper eyedropper)
//        {
//            bool isActive = (bool)e.NewValue;
//            if (isActive != eyedropper._isActive)
//            {
//                eyedropper._isActive = isActive;
//                if (isActive)
//                {
//                    eyedropper.StartColorPicking();
//                }
//                else
//                {
//                    eyedropper.StopColorPicking();
//                }
//            }
//        }
//    }

//    #endregion IsActive

//    #region SelectedColor

//    public Color SelectedColor
//    {
//        get => (Color)GetValue(SelectedColorProperty);
//        set => SetValue(SelectedColorProperty, value);
//    }

//    public static readonly DependencyProperty SelectedColorProperty =
//        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorEyedropper),
//        new PropertyMetadata(Colors.Transparent));

//    #endregion SelectedColor

//    #region PreviewSize

//    public double PreviewSize
//    {
//        get => (double)GetValue(PreviewSizeProperty);
//        set => SetValue(PreviewSizeProperty, value);
//    }

//    public static readonly DependencyProperty PreviewSizeProperty =
//        DependencyProperty.Register(nameof(PreviewSize), typeof(double), typeof(ColorEyedropper),
//        new PropertyMetadata(150.0));

//    #endregion PreviewSize

//    #region ZoomFactor

//    public double ZoomFactor
//    {
//        get => (double)GetValue(ZoomFactorProperty);
//        set => SetValue(ZoomFactorProperty, value);
//    }

//    public static readonly DependencyProperty ZoomFactorProperty =
//        DependencyProperty.Register(nameof(ZoomFactor), typeof(double), typeof(ColorEyedropper),
//        new PropertyMetadata(4.0));

//    #endregion ZoomFactor

//    #region Events

//    public event EventHandler<ColorSelectedEventArgs> ColorSelected;

//    #endregion Events

//    #region Commands

//    public ICommand PickColorCommand
//    {
//        get => (ICommand)GetValue(PickColorCommandProperty);
//        set => SetValue(PickColorCommandProperty, value);
//    }

//    public static readonly DependencyProperty PickColorCommandProperty =
//        DependencyProperty.Register(nameof(PickColorCommand), typeof(ICommand), typeof(ColorEyedropper));

//    #endregion Commands

//    public override void OnApplyTemplate()
//    {
//        base.OnApplyTemplate();

//        // 初始化命令
//        PickColorCommand = new RelayCommand(_ => ToggleActive());
//    }

//    private void ToggleActive()
//    {
//        IsActive = !IsActive;
//    }

//    private void StartColorPicking()
//    {
//        // 创建覆盖窗口
//        _overlay = new Window
//        {
//            WindowStyle = WindowStyle.None,
//            ResizeMode = ResizeMode.NoResize,
//            AllowsTransparency = true,
//            Background = Brushes.Transparent,
//            Topmost = true,
//            ShowInTaskbar = false,
//            Width = SystemParameters.PrimaryScreenWidth,
//            Height = SystemParameters.PrimaryScreenHeight,
//            Left = 0,
//            Top = 0,
//            Cursor = Cursors.Cross
//        };

//        // 创建预览UI
//        var grid = new Grid();

//        // 预览图像
//        _preview = new Image
//        {
//            Width = PreviewSize,
//            Height = PreviewSize,
//            Stretch = Stretch.Uniform
//        };

//        var previewBorder = new Border
//        {
//            Child = _preview,
//            BorderBrush = new SolidColorBrush(Colors.Black),
//            BorderThickness = new Thickness(1),
//            Width = PreviewSize + 2,
//            Height = PreviewSize + 2,
//            HorizontalAlignment = HorizontalAlignment.Left,
//            VerticalAlignment = VerticalAlignment.Top,
//            Margin = new Thickness(10)
//        };

//        // 颜色信息
//        _colorInfo = new TextBlock
//        {
//            Background = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
//            Padding = new Thickness(5),
//            HorizontalAlignment = HorizontalAlignment.Left,
//            VerticalAlignment = VerticalAlignment.Bottom,
//            Margin = new Thickness(10),
//            FontFamily = new FontFamily("Consolas")
//        };

//        grid.Children.Add(previewBorder);
//        grid.Children.Add(_colorInfo);

//        _overlay.Content = grid;

//        // 绑定事件
//        _overlay.MouseMove += Overlay_MouseMove;
//        _overlay.MouseLeftButtonDown += Overlay_MouseLeftButtonDown;
//        _overlay.KeyDown += Overlay_KeyDown;

//        // 显示窗口
//        _overlay.Show();
//        _overlay.Focus();
//    }

//    private void StopColorPicking()
//    {
//        if (_overlay != null)
//        {
//            _overlay.Close();
//            _overlay = null;
//            _preview = null;
//            _colorInfo = null;
//        }
//    }

//    private void Overlay_MouseMove(object sender, MouseEventArgs e)
//    {
//        Point position = e.GetPosition(null);

//        // 获取鼠标位置下的颜色
//        Color pixelColor = ScreenColorPicker.GetColorAt(position);

//        // 更新颜色信息
//        _colorInfo.Text = $"RGB: {pixelColor.R}, {pixelColor.G}, {pixelColor.B}\n" +
//                          $"HEX: #{pixelColor.R:X2}{pixelColor.G:X2}{pixelColor.B:X2}";

//        // 更新预览图像
//        UpdatePreview(position);
//    }

//    private void Overlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
//    {
//        Point position = e.GetPosition(null);

//        // 获取鼠标位置下的颜色
//        Color pixelColor = ScreenColorPicker.GetColorAt(position);

//        // 更新选中的颜色
//        SelectedColor = pixelColor;

//        // 触发颜色选择事件
//        ColorSelected?.Invoke(this, new ColorSelectedEventArgs(pixelColor));

//        // 关闭吸管工具
//        IsActive = false;

//        e.Handled = true;
//    }

//    private void Overlay_KeyDown(object sender, KeyEventArgs e)
//    {
//        if (e.Key == Key.Escape)
//        {
//            // 按ESC键取消拾色
//            IsActive = false;
//            e.Handled = true;
//        }
//    }

//    private void UpdatePreview(Point cursorPosition)
//    {
//        try
//        {
//            // 计算预览区域
//            int size = (int)(PreviewSize / ZoomFactor);
//            int startX = (int)cursorPosition.X - size / 2;
//            int startY = (int)cursorPosition.Y - size / 2;

//            // 创建一个可写位图
//            WriteableBitmap writeableBitmap = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgra32, null);

//            // 捕获屏幕区域
//            CaptureScreenRegion(writeableBitmap, startX, startY, size, size);

//            // 绘制十字线
//            int centerX = size / 2;
//            int centerY = size / 2;
//            DrawCrosshair(writeableBitmap, centerX, centerY);

//            _preview.Source = writeableBitmap;
//        }
//        catch (Exception ex)
//        {
//            // 忽略捕获屏幕时可能出现的错误
//            Console.WriteLine($"截图预览出错: {ex.Message}");
//        }
//    }

//    // 使用WPF的方式捕获屏幕区域
//    private void CaptureScreenRegion(WriteableBitmap writeableBitmap, int x, int y, int width, int height)
//    {
//        // 确保坐标不超出屏幕范围
//        x = Math.Max(0, x);
//        y = Math.Max(0, y);

//        // 创建屏幕DC
//        IntPtr screenDC = NativeMethods.GetDC(IntPtr.Zero);
//        IntPtr memDC = NativeMethods.CreateCompatibleDC(screenDC);
//        IntPtr hBitmap = NativeMethods.CreateCompatibleBitmap(screenDC, width, height);
//        IntPtr oldBitmap = NativeMethods.SelectObject(memDC, hBitmap);

//        // 复制屏幕内容到位图
//        NativeMethods.BitBlt(memDC, 0, 0, width, height, screenDC, x, y, NativeMethods.SRCCOPY);

//        // 获取位图数据
//        NativeMethods.BITMAPINFO bmi = new NativeMethods.BITMAPINFO();
//        bmi.bmiHeader.biSize = Marshal.SizeOf(typeof(NativeMethods.BITMAPINFOHEADER));
//        bmi.bmiHeader.biWidth = width;
//        bmi.bmiHeader.biHeight = -height; // 负值表示自上而下的DIB
//        bmi.bmiHeader.biPlanes = 1;
//        bmi.bmiHeader.biBitCount = 32;
//        bmi.bmiHeader.biCompression = 0; // BI_RGB

//        // 分配内存并获取像素数据
//        writeableBitmap.Lock();
//        NativeMethods.GetDIBits(memDC, hBitmap, 0, (uint)height,
//            writeableBitmap.BackBuffer, ref bmi, 0);

//        // 标记整个位图为脏区域
//        writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
//        writeableBitmap.Unlock();

//        // 清理资源
//        NativeMethods.SelectObject(memDC, oldBitmap);
//        NativeMethods.DeleteObject(hBitmap);
//        NativeMethods.DeleteDC(memDC);
//        NativeMethods.ReleaseDC(IntPtr.Zero, screenDC);
//    }

//    private void DrawCrosshair(WriteableBitmap bitmap, int x, int y)
//    {
//        try
//        {
//            // 绘制水平线
//            for (int i = 0; i < bitmap.PixelWidth; i++)
//            {
//                SetPixel(bitmap, i, y, Colors.Red);
//            }

//            // 绘制垂直线
//            for (int i = 0; i < bitmap.PixelHeight; i++)
//            {
//                SetPixel(bitmap, x, i, Colors.Red);
//            }
//        }
//        catch
//        {
//            // 忽略绘制错误
//        }
//    }

//    private void SetPixel(WriteableBitmap bitmap, int x, int y, Color color)
//    {
//        try
//        {
//            if (x >= 0 && x < bitmap.PixelWidth && y >= 0 && y < bitmap.PixelHeight)
//            {
//                bitmap.Lock();

//                unsafe
//                {
//                    IntPtr pBackBuffer = bitmap.BackBuffer;
//                    int stride = bitmap.BackBufferStride;

//                    byte* pBuffer = (byte*)pBackBuffer.ToPointer();
//                    int offset = y * stride + x * 4;

//                    pBuffer[offset] = color.B;
//                    pBuffer[offset + 1] = color.G;
//                    pBuffer[offset + 2] = color.R;
//                    pBuffer[offset + 3] = color.A;
//                }

//                bitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
//                bitmap.Unlock();
//            }
//        }
//        catch
//        {
//            // 忽略绘制错误
//        }
//    }
//}

///// <summary>
///// 颜色选择事件参数
///// </summary>
//public class ColorSelectedEventArgs : EventArgs
//{
//    public Color SelectedColor { get; }

//    public ColorSelectedEventArgs(Color selectedColor)
//    {
//        SelectedColor = selectedColor;
//    }
//}

///// <summary>
///// Win32 API访问类
///// </summary>
//internal static class NativeMethods
//{
//    [DllImport("gdi32.dll")]
//    public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
//        IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

//    [DllImport("gdi32.dll")]
//    public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

//    [DllImport("gdi32.dll")]
//    public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

//    [DllImport("gdi32.dll")]
//    public static extern bool DeleteDC(IntPtr hdc);

//    [DllImport("gdi32.dll")]
//    public static extern bool DeleteObject(IntPtr hObject);

//    [DllImport("gdi32.dll")]
//    public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

//    [DllImport("user32.dll")]
//    public static extern IntPtr GetDC(IntPtr hwnd);

//    [DllImport("user32.dll")]
//    public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

//    [DllImport("gdi32.dll")]
//    public static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines,
//        IntPtr lpvBits, ref BITMAPINFO lpbmi, uint uUsage);

//    [StructLayout(LayoutKind.Sequential)]
//    public struct BITMAPINFOHEADER
//    {
//        public int biSize;
//        public int biWidth;
//        public int biHeight;
//        public short biPlanes;
//        public short biBitCount;
//        public int biCompression;
//        public int biSizeImage;
//        public int biXPelsPerMeter;
//        public int biYPelsPerMeter;
//        public int biClrUsed;
//        public int biClrImportant;
//    }

//    [StructLayout(LayoutKind.Sequential)]
//    public struct BITMAPINFO
//    {
//        public BITMAPINFOHEADER bmiHeader;

//        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
//        public uint[] bmiColors;
//    }

//    [DllImport("gdi32.dll")]
//    public static extern int GetPixel(IntPtr hdc, int nXPos, int nYPos);

//    public const uint SRCCOPY = 0x00CC0020;
//}

///// <summary>
///// 屏幕颜色拾取静态工具类
///// </summary>
//internal static class ScreenColorPicker
//{
//    public static Color GetColorAt(Point position)
//    {
//        IntPtr desk = NativeMethods.GetDC(IntPtr.Zero);
//        int colorRef = NativeMethods.GetPixel(desk, (int)position.X, (int)position.Y);
//        NativeMethods.ReleaseDC(IntPtr.Zero, desk);

//        byte r = (byte)(colorRef & 0xFF);
//        byte g = (byte)((colorRef & 0xFF00) >> 8);
//        byte b = (byte)((colorRef & 0xFF0000) >> 16);

//        return Color.FromRgb(r, g, b);
//    }
//}