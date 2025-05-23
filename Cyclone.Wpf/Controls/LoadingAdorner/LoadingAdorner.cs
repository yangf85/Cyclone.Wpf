using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

public enum LoadingAttachedPosition
{
    Top,
    Bottom,
    Left,
    Right
}

public class LoadingAdorner : Adorner, IDisposable
{
    private static readonly Dictionary<FrameworkElement, LoadingAdorner> _adornerCache = new();
    private readonly FrameworkElement _owner;
    private readonly VisualCollection _children;
    private readonly Border _maskBorder;
    private readonly ContentControl _loadingContent;
    private readonly DispatcherTimer _delayTimer;
    private bool _isDisposed;

    #region Visual Tree Override

    protected override int VisualChildrenCount => _children.Count;

    protected override Visual GetVisualChild(int index) => _children[index];

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_isDisposed) return finalSize;

        var ownerSize = new Size(_owner.ActualWidth, _owner.ActualHeight);
        _maskBorder.Arrange(new Rect(ownerSize));
        return finalSize;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        if (_isDisposed) return Size.Empty;

        _maskBorder.Measure(constraint);
        return base.MeasureOverride(constraint);
    }

    #endregion Visual Tree Override

    public LoadingAdorner(FrameworkElement owner, object content, TimeSpan? showDelay = null) : base(owner)
    {
        _owner = owner;
        _owner.SizeChanged += OnOwnerSizeChanged;

        // 创建加载内容
        _loadingContent = new ContentControl
        {
            Content = content,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Focusable = false
        };

        // 创建遮罩边框
        _maskBorder = new Border
        {
            Background = GetMaskBackground(owner),
            Child = _loadingContent,
            Focusable = false
        };

        _children = new VisualCollection(this) { _maskBorder };

        // 延迟显示支持
        if (showDelay.HasValue && showDelay.Value > TimeSpan.Zero)
        {
            _maskBorder.Opacity = 0;
            _delayTimer = new DispatcherTimer
            {
                Interval = showDelay.Value
            };
            _delayTimer.Tick += (s, e) =>
            {
                _delayTimer.Stop();
                ShowWithAnimation();
            };
            _delayTimer.Start();
        }
        else
        {
            ShowWithAnimation();
        }
    }

    private void OnOwnerSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!_isDisposed)
        {
            InvalidateVisual();
        }
    }

    private void ShowWithAnimation()
    {
        if (_isDisposed) return;

        // 设置初始状态
        var scaleTransform = new ScaleTransform(0.8, 0.8);
        _loadingContent.RenderTransform = scaleTransform;
        _loadingContent.RenderTransformOrigin = new Point(0.5, 0.5);
        _maskBorder.Opacity = 0;

        // 创建入场动画
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var scaleAnimation = new DoubleAnimation(0.8, 1, TimeSpan.FromMilliseconds(300))
        {
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
        };

        _maskBorder.BeginAnimation(OpacityProperty, fadeIn);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
    }

    public void HideWithAnimation(Action onCompleted = null)
    {
        if (_isDisposed) return;

        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        fadeOut.Completed += (s, e) => onCompleted?.Invoke();
        _maskBorder.BeginAnimation(OpacityProperty, fadeOut);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _delayTimer?.Stop();
        _owner.SizeChanged -= OnOwnerSizeChanged;

        if (_loadingContent.Content is IDisposable disposableContent)
        {
            disposableContent.Dispose();
        }

        _loadingContent.Content = null;
        _children.Clear();
    }

    #region Attached Properties

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.RegisterAttached(
            "IsLoading",
            typeof(bool),
            typeof(LoadingAdorner),
            new PropertyMetadata(false, OnIsLoadingChanged));

    public static readonly DependencyProperty LoadingContentProperty =
        DependencyProperty.RegisterAttached(
            "LoadingContent",
            typeof(object),
            typeof(LoadingAdorner));

    public static readonly DependencyProperty MaskBackgroundProperty =
        DependencyProperty.RegisterAttached(
            "MaskBackground",
            typeof(Brush),
            typeof(LoadingAdorner),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(128, 0, 0, 0))));

    public static readonly DependencyProperty ShowDelayProperty =
        DependencyProperty.RegisterAttached(
            "ShowDelay",
            typeof(TimeSpan),
            typeof(LoadingAdorner),
            new PropertyMetadata(TimeSpan.Zero));

    public static readonly DependencyProperty AttachedContentProperty =
        DependencyProperty.RegisterAttached(
            "AttachedContent",
            typeof(object),
            typeof(LoadingAdorner));

    public static readonly DependencyProperty AttachedPositionProperty =
        DependencyProperty.RegisterAttached(
            "AttachedPosition",
            typeof(LoadingAttachedPosition),
            typeof(LoadingAdorner),
            new PropertyMetadata(LoadingAttachedPosition.Bottom));

    // Getters and Setters
    public static bool GetIsLoading(DependencyObject obj) => (bool)obj.GetValue(IsLoadingProperty);

    public static void SetIsLoading(DependencyObject obj, bool value) => obj.SetValue(IsLoadingProperty, value);

    public static object GetLoadingContent(DependencyObject obj) => obj.GetValue(LoadingContentProperty);

    public static void SetLoadingContent(DependencyObject obj, object value) => obj.SetValue(LoadingContentProperty, value);

    public static Brush GetMaskBackground(DependencyObject obj) => (Brush)obj.GetValue(MaskBackgroundProperty);

    public static void SetMaskBackground(DependencyObject obj, Brush value) => obj.SetValue(MaskBackgroundProperty, value);

    public static TimeSpan GetShowDelay(DependencyObject obj) => (TimeSpan)obj.GetValue(ShowDelayProperty);

    public static void SetShowDelay(DependencyObject obj, TimeSpan value) => obj.SetValue(ShowDelayProperty, value);

    public static object GetAttachedContent(DependencyObject obj) => obj.GetValue(AttachedContentProperty);

    public static void SetAttachedContent(DependencyObject obj, object value) => obj.SetValue(AttachedContentProperty, value);

    public static LoadingAttachedPosition GetAttachedPosition(DependencyObject obj) => (LoadingAttachedPosition)obj.GetValue(AttachedPositionProperty);

    public static void SetAttachedPosition(DependencyObject obj, LoadingAttachedPosition value) => obj.SetValue(AttachedPositionProperty, value);

    #endregion Attached Properties

    #region Private Methods

    private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element) return;

        var isLoading = (bool)e.NewValue;

        if (element.IsLoaded)
        {
            SetLoadingState(element, isLoading);
        }
        else
        {
            element.Loaded -= OnElementLoaded;
            element.Loaded += OnElementLoaded;
        }
    }

    private static void OnElementLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.Loaded -= OnElementLoaded;
            SetLoadingState(element, GetIsLoading(element));
        }
    }

    private static void SetLoadingState(FrameworkElement element, bool isLoading)
    {
        if (isLoading)
        {
            ShowLoading(element);
        }
        else
        {
            HideLoading(element);
        }
    }

    private static void ShowLoading(FrameworkElement element)
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(element);
        if (adornerLayer == null)
        {
            throw new InvalidOperationException("未找到装饰层，请确保元素在可视化树中。");
        }

        // 使用缓存避免重复添加
        if (_adornerCache.ContainsKey(element)) return;

        // 获取加载内容
        var loadingContent = CreateLoadingContent(element);
        var showDelay = GetShowDelay(element);

        var adorner = new LoadingAdorner(element, loadingContent, showDelay);
        _adornerCache[element] = adorner;
        adornerLayer.Add(adorner);
    }

    private static void HideLoading(FrameworkElement element)
    {
        if (!_adornerCache.TryGetValue(element, out var adorner)) return;

        var adornerLayer = AdornerLayer.GetAdornerLayer(element);
        if (adornerLayer == null) return;

        adorner.HideWithAnimation(() =>
        {
            adorner.Dispose();
            adornerLayer.Remove(adorner);
            _adornerCache.Remove(element);
        });
    }

    private static object CreateLoadingContent(FrameworkElement element)
    {
        var loadingContent = GetLoadingContent(element);
        var attachedContent = GetAttachedContent(element);
        var attachedPosition = GetAttachedPosition(element);

        // 如果同时设置了主加载内容和附加内容，按位置组合显示
        if (loadingContent != null && attachedContent != null)
        {
            return CreateCombinedContent(loadingContent, attachedContent, attachedPosition);
        }

        // 如果只设置了主加载内容
        if (loadingContent != null)
        {
            return loadingContent;
        }

        // 如果只设置了附加内容或都没设置，使用默认样式
        return CreateDefaultLoadingContent(element);
    }

    private static object CreateCombinedContent(object loadingContent, object attachedContent, LoadingAttachedPosition position)
    {
        var loadingControl = new ContentControl
        {
            Content = loadingContent,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var attachedControl = new ContentControl
        {
            Content = attachedContent,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        return position switch
        {
            LoadingAttachedPosition.Top => new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    ApplyMargin(attachedControl, new Thickness(0, 0, 0, 10)),
                    loadingControl
                }
            },
            LoadingAttachedPosition.Bottom => new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    ApplyMargin(loadingControl, new Thickness(0, 0, 0, 10)),
                    attachedControl
                }
            },
            LoadingAttachedPosition.Left => new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    ApplyMargin(attachedControl, new Thickness(0, 0, 10, 0)),
                    loadingControl
                }
            },
            LoadingAttachedPosition.Right => new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    ApplyMargin(loadingControl, new Thickness(0, 0, 10, 0)),
                    attachedControl
                }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(position))
        };
    }

    private static ContentControl ApplyMargin(ContentControl control, Thickness margin)
    {
        control.Margin = margin;
        return control;
    }

    private static object CreateDefaultLoadingContent(FrameworkElement element)
    {
        var attachedContent = GetAttachedContent(element);
        var attachedPosition = GetAttachedPosition(element);

        if (attachedContent != null)
        {
            // 使用默认的 LoadingPulse 和附加内容组合
            return CreateCombinedContent(new LoadingPulse(), attachedContent, attachedPosition);
        }

        return new LoadingPulse();
    }

    #endregion Private Methods
}