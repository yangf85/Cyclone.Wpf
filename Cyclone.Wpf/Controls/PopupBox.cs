using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Xml.Linq;
using System.Windows.Threading;
using System.Windows.Markup;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Data;



namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
[TemplatePart(Name = PART_ButtonContent, Type = typeof(ContentPresenter))]
[TemplatePart(Name = PART_PopupContent, Type = typeof(ContentPresenter))]
[TemplatePart(Name = PART_ToggleButton, Type = typeof(ToggleButton))]
[ContentProperty(nameof(PopupContent))]
public class PopupBox : Control
{
    static PopupBox()
    {
      
    }
  
    private const string PART_Popup = nameof(PART_Popup);

    private const string PART_ButtonContent = nameof(PART_ButtonContent);

    private const string PART_PopupContent = nameof(PART_PopupContent);

    private const string PART_ToggleButton = nameof(PART_ToggleButton);

    private Popup _popup;

    private ToggleButton _toggleButton;

    #region Content

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(PopupBox), new PropertyMetadata(default(object)));

    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    #endregion Content

    #region PopupContent

    public static readonly DependencyProperty PopupContentProperty =
        DependencyProperty.Register(nameof(PopupContent), typeof(object), typeof(PopupBox), new PropertyMetadata(default(object),OnPopupContentChanged));

    private static void OnPopupContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
       var box= (PopupBox)d;
        if(e.NewValue != null)
        {
            box.RemoveLogicalChild(e.OldValue);
            box.AddLogicalChild(e.NewValue);
        }
    }

    public object PopupContent
    {
        get => GetValue(PopupContentProperty);
        set => SetValue(PopupContentProperty, value);
    }

    #endregion PopupContent



    #region PopupContentBackground
    public Brush PopupContentBackground
    {
        get => (Brush)GetValue(PopupContentBackgroundProperty);
        set => SetValue(PopupContentBackgroundProperty, value);
    }

    public static readonly DependencyProperty PopupContentBackgroundProperty =
        DependencyProperty.Register(nameof(PopupContentBackground), typeof(Brush), typeof(PopupBox), new PropertyMetadata(SystemColors.WindowBrush));

    #endregion



    #region IsOpened
    public bool IsOpened
    {
        get => (bool)GetValue(IsOpenedProperty);
        set => SetValue(IsOpenedProperty, value);
    }

    public static readonly DependencyProperty IsOpenedProperty =
        DependencyProperty.Register(nameof(IsOpened), typeof(bool), typeof(PopupBox), new FrameworkPropertyMetadata(default(bool),FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion

    #region VerticalOffset

    public static readonly DependencyProperty VerticalOffsetProperty=Popup.VerticalOffsetProperty.AddOwner(typeof(PopupBox));

    public double VerticalOffset
    {
        get => (double)GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }

    #endregion VerticalOffset

    #region HorizontalOffset

    public static readonly DependencyProperty HorizontalOffsetProperty = Popup.HorizontalOffsetProperty.AddOwner(typeof(PopupBox));

    public double HorizontalOffset
    {
        get => (double)GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    #endregion HorizontalOffset

    #region PopupAnimation

    public static readonly DependencyProperty PopupAnimationProperty = Popup.PopupAnimationProperty.AddOwner(typeof(PopupBox));

    public PopupAnimation PopupAnimation
    {
        get => (PopupAnimation)GetValue(PopupAnimationProperty);
        set => SetValue(PopupAnimationProperty, value);
    }

    #endregion PopupAnimation

    #region Placement

    public static readonly DependencyProperty PlacementProperty = Popup.PlacementProperty.AddOwner(typeof(PopupBox));

    public PlacementMode Placement
    {
        get => (PlacementMode)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    #endregion Placement

    #region StaysOpen

    public static readonly DependencyProperty StaysOpenProperty = Popup.StaysOpenProperty.AddOwner(typeof(PopupBox));

    public bool StaysOpen
    {
        get => (bool)GetValue(StaysOpenProperty);
        set => SetValue(StaysOpenProperty, value);
    }

    #endregion StaysOpen



    public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
        "Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PopupBox));

    public static readonly RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent(
        "Opened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PopupBox));

    public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
        "Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PopupBox));

    #region Override

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _toggleButton = GetTemplateChild(PART_ToggleButton) as ToggleButton;
        if (_toggleButton != null)
        {
            _toggleButton.Click += (sender, e) =>
            {
                // 在这里引发 Click 事件
                RaiseEvent(new RoutedEventArgs(ClickEvent));
            };
        }
        _popup = GetTemplateChild(PART_Popup) as Popup;
        if (_popup != null)
        {

            if (_toggleButton != null)
            {
                //某些情况下，PopupBox 的 ToggleButton 可见性会改变，因此需要监听 IsVisibleChanged 事件
                _toggleButton.IsVisibleChanged += (s, e) =>
                {
                    if (!_toggleButton.IsVisible)
                    {
                        _popup.IsOpen = false;
                    }
                };
            }

            // 使用 PlacementTarget 绑定 DataContext
            Binding binding = new Binding
            {
                Path = new PropertyPath("DataContext"),
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            };

            // 设置 Popup 的 DataContext 绑定到其 PlacementTarget 的 DataContext
            _popup.SetBinding(Popup.DataContextProperty, new Binding
            {
                Source = _popup.PlacementTarget,
                Path = new PropertyPath("DataContext")
            });


            _popup.Opened += (sender, e) =>
            {
                // 在这里引发 Opened 事件
                RaiseEvent(new RoutedEventArgs(OpenedEvent));
            };
            _popup.Closed += (sender, e) =>
            {
                // 在这里引发 Closed 事件
                RaiseEvent(new RoutedEventArgs(ClosedEvent));
            };
        }
    }

    #endregion Override

    public event RoutedEventHandler Click
    {
        add { AddHandler(ClickEvent, value); }
        remove { RemoveHandler(ClickEvent, value); }
    }

    public event RoutedEventHandler Opened
    {
        add { AddHandler(OpenedEvent, value); }
        remove { RemoveHandler(OpenedEvent, value); }
    }

    // 定义 Closed 事件包装器
    public event RoutedEventHandler Closed
    {
        add { AddHandler(ClosedEvent, value); }
        remove { RemoveHandler(ClosedEvent, value); }
    }

    #region IsPositionUpdate

    public static readonly DependencyProperty IsPositionUpdateProperty =
                DependencyProperty.RegisterAttached("IsPositionUpdate", typeof(bool), typeof(Popup), new PropertyMetadata(IsPositionUpdateChanged));

    private static void IsPositionUpdateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue)
        {
            var popup = d as Popup;
            if (popup != null)
            {
                popup.Loaded -= UpdatePopupLocation;
                popup.Loaded += UpdatePopupLocation;
                popup.Opened -= UpdatePopupLocation;
                popup.Opened += UpdatePopupLocation;
                popup.Closed -= UpdatePopupLocation;
                popup.Closed += UpdatePopupLocation;
            }
        }
    }

    /// <summary>
    /// 更新窗口位置
    /// </summary>
    private static void UpdatePopupLocation(object sender, EventArgs e)
    {
        var popup = sender as Popup;
        if (popup == null) { return; }

        var parent = VisualTreeHelper.GetParent(popup);
        while (parent != null && parent as AdvancedWindow == null)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }
        var window = parent as AdvancedWindow;
        if (window != null)
        {
            window.LocationChanged += (s, arg) => UpdatePosition();
            window.SizeChanged += (s, arg) => UpdatePosition();
        }

        void UpdatePosition()
        {
            try
            {
                var method = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (popup.IsOpen && popup != null)
                {
                    method.Invoke(popup, null);
                }
            }
            catch
            {
                return;
            }
        }
    }

    public static bool GetIsPositionUpdate(Popup obj) => (bool)obj.GetValue(IsPositionUpdateProperty);

    public static void SetIsPositionUpdate(Popup obj, bool value) => obj.SetValue(IsPositionUpdateProperty, value);

    #endregion IsPositionUpdate

    #region IsTopmost

    private static class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

        [DllImport("user32", EntryPoint = "SetWindowPos")]
        internal static extern int SetWindowPos(nint hWnd, int hwndInsertAfter, int x, int y, int cx, int cy, int wFlags);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;

        public int Top;

        public int Right;

        public int Bottom;
    }

    public static readonly DependencyProperty IsTopmostProperty =
                DependencyProperty.RegisterAttached("IsTopmost", typeof(bool), typeof(FrameworkElement), new PropertyMetadata(true, OnIsTopmostChanged));

    private static void OnIsTopmostChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is Popup popup)
        {
            if (!(bool)e.NewValue)
            {
                popup.Opened += (s, arg) =>
                {
                    var hwnd = ((HwndSource)PresentationSource.FromVisual(popup.Child)).Handle;
                    if (NativeMethods.GetWindowRect(hwnd, out var rect))
                    {
                        // 如果IsTopmost为true（-1），则将窗口置顶
                        // 如果IsTopmost为false（-2），则将窗口置底
                        NativeMethods.SetWindowPos(hwnd, (bool)e.NewValue ? -1 : -2, rect.Left, rect.Top, (int)popup.Width, (int)popup.Height, 0);
                    }
                };
            }
        }
    }

    public static bool GetIsTopmost(Popup obj) => (bool)obj.GetValue(IsTopmostProperty);

    public static void SetIsTopmost(Popup obj, bool value) => obj.SetValue(IsTopmostProperty, value);

    #endregion IsTopmost
}