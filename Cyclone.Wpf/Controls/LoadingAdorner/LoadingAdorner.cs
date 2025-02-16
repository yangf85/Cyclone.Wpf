using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;
using Cyclone.Wpf.Helpers;
using Cyclone.Wpf.Styles;

namespace Cyclone.Wpf.Controls;

public class LoadingAdorner : Adorner, IDisposable
{
    private FrameworkElement _owner;

    private VisualCollection _child;

    private Border _border;

    private ContentControl _content;

    #region Override

    protected override int VisualChildrenCount
    {
        get
        {
            return _child == null ? 0 : _child.Count;
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _border?.Arrange(new Rect(new Point(0, 0), new Size(_owner.ActualWidth, _owner.ActualHeight)));
        return finalSize;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        _border?.Arrange(new Rect(new Point(0, 0), new Size(_owner.ActualWidth, _owner.ActualHeight)));
        return base.MeasureOverride(constraint);
    }

    protected override Visual GetVisualChild(int index)
    {
        return _child?[index];
    }

    #endregion Override

    public LoadingAdorner(FrameworkElement owner, object content, Action completed = null) : base(owner)
    {
        _owner = owner;
        _owner.SizeChanged += (sender, e) => InvalidateVisual();

        _content = new ContentControl
        {
            Content = content,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _border = new Border
        {
            Background = GetMaskBackground(owner),
            Child = _content
        };
        _child = new VisualCollection(this) { _border };

        var storyboard = new Storyboard();
        if (completed != null)
            storyboard.Completed += (sender, e) => completed();
        AddDoubleAnimationUsingKeyFrames(storyboard, "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)", _content);
        AddDoubleAnimationUsingKeyFrames(storyboard, "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)", _content);
        AddRenderTransform(_content);
        AddTrigger(_content, storyboard);
    }

    #region Implement IDisposable

    public void Dispose()
    {
        _content.Content = null;
    }

    #endregion Implement IDisposable

    #region Private

    private void AddRenderTransform(FrameworkElement element)
    {
        var group = new TransformGroup();
        group.Children.Add(new ScaleTransform() { ScaleX = 0, ScaleY = 0 });
        group.Children.Add(new SkewTransform());
        group.Children.Add(new RotateTransform());
        group.Children.Add(new TranslateTransform());

        element.RenderTransform = group;
        element.RenderTransformOrigin = new Point(0.5, 0.5);
    }

    private void AddTrigger(FrameworkElement target, Storyboard storyboard)
    {
        var trigger = new EventTrigger { RoutedEvent = LoadedEvent };
        var beginStoryboard = new BeginStoryboard() { Storyboard = storyboard };
        trigger.Actions.Add(beginStoryboard);
        target.Triggers.Add(trigger);
    }

    private void AddDoubleAnimationUsingKeyFrames(Storyboard storyboard, string property, FrameworkElement target)
    {
        var daukf = new DoubleAnimationUsingKeyFrames();
        Storyboard.SetTargetProperty(daukf, new PropertyPath(property));
        Storyboard.SetTarget(daukf, target);

        var edkf = new EasingDoubleKeyFrame
        {
            KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
            Value = 0
        };
        daukf.KeyFrames.Add(edkf);

        var quinticEase = new QuinticEase { EasingMode = EasingMode.EaseOut };
        edkf = new EasingDoubleKeyFrame
        {
            KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.6)),
            Value = 1,
            EasingFunction = quinticEase
        };

        daukf.KeyFrames.Add(edkf);
        storyboard.Children.Add(daukf);
    }

    #endregion Private

    #region IsLoading

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.RegisterAttached("IsLoading", typeof(bool), typeof(LoadingAdorner), new PropertyMetadata(IsLoadingPropertyChangedCallback));

    public static bool GetIsLoading(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsLoadingProperty);
    }

    public static void SetIsLoading(DependencyObject obj, bool value)
    {
        obj.SetValue(IsLoadingProperty, value);
    }

    public static void Loading(FrameworkElement element, bool isOpen = true)
    {
        if (element == null) return;
        //移除遮罩
        if (!isOpen)
        {
            ClearAdorners(element);
            return;
        }

        //获取遮罩元素
        var LoadingContent = GetLoadingContent(element);
        if (LoadingContent == null)
        {
            LoadingContent = element.TryFindResource("LoadingContent");
            LoadingContent ??= new NormalLoading()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        //获取装饰层
        var adornerLayer = AdornerLayer.GetAdornerLayer(element) ?? throw new Exception("未找到装饰层。");

        //添加遮罩层
        if (LoadingContent is FrameworkElement maskElement)
        {
            if (maskElement.Parent != null)
            {
                var type = LoadingContent.GetType();
                var properties = type.GetProperties().Where(p => p.CanWrite).ToArray();
                var obj = Activator.CreateInstance(type);
                foreach (var property in properties)
                {
                    if (property.Name == "Content" || property.Name == "Child" || property.Name == "Children")
                        continue;
                    property.SetValue(obj, property.GetValue(LoadingContent));
                }
                LoadingContent = obj;
            }
        }
        adornerLayer.Add(new LoadingAdorner(element, LoadingContent));
    }

    private static void IsLoadingPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement control)
        {
            if (!control.IsLoaded)
            {
                control.Loaded -= Control_Loaded;
                control.Loaded += Control_Loaded;
            }
            else
            {
                Loading(control, (bool)e.NewValue);
            }
        }
    }

    private static void Control_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement control)
        {
            Loading(control);
        }
    }

    private static void ClearAdorners(FrameworkElement element)
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(element);
        if (adornerLayer != null)
        {
            Adorner[] adorners = adornerLayer.GetAdorners(element);
            if (adorners != null)
            {
                for (int i = 0; i < adorners.Length; i++)
                {
                    if (adorners[i] is IDisposable disposable)
                        disposable.Dispose();
                    adornerLayer.Remove(adorners[i]);
                }
            }
        }
    }

    #endregion IsLoading

    #region LoadingContent

    public static readonly DependencyProperty LoadingContentProperty =
        DependencyProperty.RegisterAttached("LoadingContent", typeof(object), typeof(LoadingAdorner));

    public static object GetLoadingContent(DependencyObject obj)
    {
        return obj.GetValue(LoadingContentProperty);
    }

    public static void SetLoadingContent(DependencyObject obj, object value)
    {
        obj.SetValue(LoadingContentProperty, value);
    }

    #endregion LoadingContent

    #region MaskBackground

    public static readonly DependencyProperty MaskBackgroundProperty =
                DependencyProperty.RegisterAttached("MaskBackground", typeof(Brush), typeof(LoadingAdorner), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B2000000"))));

    public static Brush GetMaskBackground(DependencyObject obj) => (Brush)obj.GetValue(MaskBackgroundProperty);

    public static void SetMaskBackground(DependencyObject obj, Brush value) => obj.SetValue(MaskBackgroundProperty, value);

    #endregion MaskBackground
}