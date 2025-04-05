using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 内容变化时播放动画效果的控件
/// </summary>
[TemplatePart(Name = "PART_OldContentPresenter", Type = typeof(ContentPresenter))]
[TemplatePart(Name = "PART_NewContentPresenter", Type = typeof(ContentPresenter))]
public class TransitionBox : ContentControl
{
    private ContentPresenter _oldContentPresenter;
    private ContentPresenter _newContentPresenter;
    private object _oldContent;
    private bool _isTemplateApplied = false;

    #region 依赖属性

    public static readonly DependencyProperty TransitionProperty =
        DependencyProperty.Register(
            nameof(Transition),
            typeof(ITransition),
            typeof(TransitionBox),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置内容切换时使用的过渡效果
    /// </summary>
    public ITransition Transition
    {
        get => (ITransition)GetValue(TransitionProperty);
        set => SetValue(TransitionProperty, value);
    }

    public static readonly DependencyProperty TransitionDurationProperty =
        DependencyProperty.Register(
            nameof(TransitionDuration),
            typeof(Duration),
            typeof(TransitionBox),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(300))));

    /// <summary>
    /// 获取或设置过渡动画的持续时间
    /// </summary>
    public Duration TransitionDuration
    {
        get => (Duration)GetValue(TransitionDurationProperty);
        set => SetValue(TransitionDurationProperty, value);
    }

    #endregion 依赖属性

    static TransitionBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TransitionBox),
            new FrameworkPropertyMetadata(typeof(TransitionBox)));
    }

    public TransitionBox()
    {
        this.Loaded += OnLoaded;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 初始化控件时获取内容呈现器
        _oldContentPresenter = GetTemplateChild("PART_OldContentPresenter") as ContentPresenter;
        _newContentPresenter = GetTemplateChild("PART_NewContentPresenter") as ContentPresenter;

        if (_oldContentPresenter != null && _newContentPresenter != null)
        {
            // 初始设置
            _oldContentPresenter.Visibility = Visibility.Collapsed;
            _newContentPresenter.Visibility = Visibility.Visible;
            _newContentPresenter.Opacity = 1.0;

            // 初始显示内容
            _newContentPresenter.Content = Content;
            _isTemplateApplied = true;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // 确保内容正确显示
        if (_isTemplateApplied && _newContentPresenter != null)
        {
            _newContentPresenter.Content = Content;
        }
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        // 保存旧内容引用
        _oldContent = oldContent;

        // 如果控件尚未加载模板，则直接返回
        if (!_isTemplateApplied || _oldContentPresenter == null || _newContentPresenter == null)
        {
            return;
        }

        // 如果没有设置过渡效果，或者是初始内容设置，直接更新内容
        if (Transition == null || oldContent == null)
        {
            _newContentPresenter.Content = newContent;
            return;
        }

        // 准备新旧内容
        _oldContentPresenter.Content = oldContent;
        _oldContentPresenter.Visibility = Visibility.Visible;
        _oldContentPresenter.Opacity = 1.0;

        // 预先准备新内容，但先保持不可见
        _newContentPresenter.Content = newContent;
        _newContentPresenter.Visibility = Visibility.Visible;
        _newContentPresenter.Opacity = 0;

        // 强制一次布局更新，确保所有内容都被正确测量
        this.UpdateLayout();

        // 在布局更新后，使用Dispatcher延迟一帧开始动画
        Dispatcher.BeginInvoke(new Action(() =>
        {
            // 执行过渡动画
            Transition.StartTransition(this, _oldContentPresenter, _newContentPresenter, TransitionDuration);
        }), System.Windows.Threading.DispatcherPriority.Render);
    }
}

/// <summary>
/// 定义内容过渡效果的接口
/// </summary>
public interface ITransition
{
    /// <summary>
    /// 开始执行过渡动画
    /// </summary>
    /// <param name="transitionBox">过渡控件</param>
    /// <param name="oldContent">旧内容的呈现器</param>
    /// <param name="newContent">新内容的呈现器</param>
    /// <param name="duration">动画持续时间</param>
    void StartTransition(TransitionBox transitionBox, ContentPresenter oldContent, ContentPresenter newContent, Duration duration);
}

/// <summary>
/// 淡入淡出过渡效果
/// </summary>
public class FadeTransition : ITransition
{
    public void StartTransition(TransitionBox transitionBox, ContentPresenter oldContent, ContentPresenter newContent, Duration duration)
    {
        // 创建一个Storyboard来协调动画
        Storyboard storyboard = new Storyboard();

        // 为旧内容创建淡出动画
        DoubleAnimation oldAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = duration,
            FillBehavior = FillBehavior.HoldEnd
        };

        Storyboard.SetTarget(oldAnimation, oldContent);
        Storyboard.SetTargetProperty(oldAnimation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(oldAnimation);

        // 为新内容创建淡入动画
        DoubleAnimation newAnimation = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = duration,
            FillBehavior = FillBehavior.HoldEnd
        };

        Storyboard.SetTarget(newAnimation, newContent);
        Storyboard.SetTargetProperty(newAnimation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(newAnimation);

        // 设置动画完成的处理
        storyboard.Completed += (s, e) =>
        {
            // 隐藏旧内容
            oldContent.Visibility = Visibility.Collapsed;
            oldContent.Content = null;
        };

        // 开始播放动画
        storyboard.Begin();
    }
}

/// <summary>
/// 滑动过渡效果
/// </summary>
public class SlideTransition : ITransition
{
    /// <summary>
    /// 滑动方向
    /// </summary>
    public enum SlideDirection
    {
        /// <summary>从左向右滑动</summary>
        LeftToRight,

        /// <summary>从右向左滑动</summary>
        RightToLeft,

        /// <summary>从上向下滑动</summary>
        TopToBottom,

        /// <summary>从下向上滑动</summary>
        BottomToTop
    }

    /// <summary>
    /// 获取或设置滑动方向
    /// </summary>
    public SlideDirection Direction { get; set; } = SlideDirection.RightToLeft;

    public void StartTransition(TransitionBox transitionBox, ContentPresenter oldContent, ContentPresenter newContent, Duration duration)
    {
        // 设置变换
        oldContent.RenderTransform = new TranslateTransform();
        newContent.RenderTransform = new TranslateTransform();

        // 根据方向确定动画的起始值和结束值
        double fromX = 0, toX = 0, fromY = 0, toY = 0;

        switch (Direction)
        {
            case SlideDirection.LeftToRight:
                fromX = -transitionBox.ActualWidth;
                toX = transitionBox.ActualWidth;
                break;

            case SlideDirection.RightToLeft:
                fromX = transitionBox.ActualWidth;
                toX = -transitionBox.ActualWidth;
                break;

            case SlideDirection.TopToBottom:
                fromY = -transitionBox.ActualHeight;
                toY = transitionBox.ActualHeight;
                break;

            case SlideDirection.BottomToTop:
                fromY = transitionBox.ActualHeight;
                toY = -transitionBox.ActualHeight;
                break;
        }

        // 创建旧内容的滑出动画
        var oldXAnimation = new DoubleAnimation
        {
            To = toX,
            Duration = duration
        };

        var oldYAnimation = new DoubleAnimation
        {
            To = toY,
            Duration = duration
        };

        // 创建新内容的滑入动画
        var newXAnimation = new DoubleAnimation
        {
            From = fromX,
            To = 0,
            Duration = duration
        };

        var newYAnimation = new DoubleAnimation
        {
            From = fromY,
            To = 0,
            Duration = duration
        };

        // 设置新内容初始位置
        ((TranslateTransform)newContent.RenderTransform).X = fromX;
        ((TranslateTransform)newContent.RenderTransform).Y = fromY;
        newContent.Opacity = 1;

        // 创建完成时操作的Storyboard
        Storyboard storyboard = new Storyboard();
        storyboard.Completed += (s, e) =>
        {
            // 动画完成后，清空旧内容
            oldContent.Visibility = Visibility.Collapsed;
            oldContent.Content = null;
        };

        // 应用动画
        ((TranslateTransform)oldContent.RenderTransform).BeginAnimation(TranslateTransform.XProperty, oldXAnimation);
        ((TranslateTransform)oldContent.RenderTransform).BeginAnimation(TranslateTransform.YProperty, oldYAnimation);
        ((TranslateTransform)newContent.RenderTransform).BeginAnimation(TranslateTransform.XProperty, newXAnimation);
        ((TranslateTransform)newContent.RenderTransform).BeginAnimation(TranslateTransform.YProperty, newYAnimation);

        // 启动Storyboard以确保动画完成事件触发
        storyboard.Duration = duration;
        storyboard.Begin();
    }
}

/// <summary>
/// 缩放过渡效果
/// </summary>
public class ScaleTransition : ITransition
{
    /// <summary>
    /// 获取或设置新内容的初始缩放值
    /// </summary>
    public double FromScale { get; set; } = 0.5;

    public void StartTransition(TransitionBox transitionBox, ContentPresenter oldContent, ContentPresenter newContent, Duration duration)
    {
        // 创建缓动函数，使动画更平滑自然
        var easing = new PowerEase { Power = 2, EasingMode = EasingMode.EaseInOut };

        // 创建一个Storyboard来协调动画
        Storyboard storyboard = new Storyboard();

        // 首先确保新内容已完全布局，但先隐藏不显示
        newContent.Visibility = Visibility.Visible;
        newContent.Opacity = 0;

        // 使用父容器的大小设置变换组
        TransformGroup oldTransformGroup = new TransformGroup();
        TransformGroup newTransformGroup = new TransformGroup();

        // 添加初始ScaleTransform
        ScaleTransform oldScaleTransform = new ScaleTransform(1, 1);
        ScaleTransform newScaleTransform = new ScaleTransform(FromScale, FromScale);

        oldTransformGroup.Children.Add(oldScaleTransform);
        newTransformGroup.Children.Add(newScaleTransform);

        // 应用变换组
        oldContent.RenderTransform = oldTransformGroup;
        newContent.RenderTransform = newTransformGroup;

        // 设置变换中心点为内容中心
        oldContent.RenderTransformOrigin = new Point(0.5, 0.5);
        newContent.RenderTransformOrigin = new Point(0.5, 0.5);

        // 确保所有变换准备就绪，强制布局更新
        transitionBox.UpdateLayout();

        // 创建旧内容的缩小并淡出动画
        DoubleAnimation oldScaleXAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.8,  // 使旧内容只缩小一点点
            Duration = duration,
            EasingFunction = easing
        };

        DoubleAnimation oldScaleYAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.8,  // 使旧内容只缩小一点点
            Duration = duration,
            EasingFunction = easing
        };

        DoubleAnimation oldOpacityAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = duration,
            EasingFunction = easing
        };

        // 创建新内容的放大并淡入动画
        DoubleAnimation newOpacityAnimation = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = duration,
            EasingFunction = easing
        };

        // 通过两步动画处理放大效果，避免突然跳变
        // 第一步：先让新内容完全不可见，但在起始缩放位置
        // 目的：让新内容预先就位，但用户还看不到
        newContent.Opacity = 0;
        newScaleTransform.ScaleX = FromScale;
        newScaleTransform.ScaleY = FromScale;

        // 强制布局更新，确保所有属性生效
        transitionBox.UpdateLayout();

        // 第二步：现在开始淡入动画和缩放动画
        DoubleAnimation newScaleXAnimation = new DoubleAnimation
        {
            From = FromScale,
            To = 1.0,
            Duration = duration,
            EasingFunction = easing
        };

        DoubleAnimation newScaleYAnimation = new DoubleAnimation
        {
            From = FromScale,
            To = 1.0,
            Duration = duration,
            EasingFunction = easing
        };

        // 添加动画到各个目标
        Storyboard.SetTarget(oldScaleXAnimation, oldScaleTransform);
        Storyboard.SetTargetProperty(oldScaleXAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));

        Storyboard.SetTarget(oldScaleYAnimation, oldScaleTransform);
        Storyboard.SetTargetProperty(oldScaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));

        Storyboard.SetTarget(oldOpacityAnimation, oldContent);
        Storyboard.SetTargetProperty(oldOpacityAnimation, new PropertyPath(UIElement.OpacityProperty));

        Storyboard.SetTarget(newScaleXAnimation, newScaleTransform);
        Storyboard.SetTargetProperty(newScaleXAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));

        Storyboard.SetTarget(newScaleYAnimation, newScaleTransform);
        Storyboard.SetTargetProperty(newScaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));

        Storyboard.SetTarget(newOpacityAnimation, newContent);
        Storyboard.SetTargetProperty(newOpacityAnimation, new PropertyPath(UIElement.OpacityProperty));

        // 添加所有动画到Storyboard
        storyboard.Children.Add(oldScaleXAnimation);
        storyboard.Children.Add(oldScaleYAnimation);
        storyboard.Children.Add(oldOpacityAnimation);
        storyboard.Children.Add(newScaleXAnimation);
        storyboard.Children.Add(newScaleYAnimation);
        storyboard.Children.Add(newOpacityAnimation);

        // 设置动画完成时的回调
        storyboard.Completed += (s, e) =>
        {
            // 动画完成后，重置变换以确保内容完全撑满容器
            newContent.RenderTransform = null;
            newContent.RenderTransformOrigin = new Point(0, 0);

            // 确保新内容完全可见
            newContent.Opacity = 1.0;

            // 清空并隐藏旧内容
            oldContent.Visibility = Visibility.Collapsed;
            oldContent.Opacity = 0;
            oldContent.Content = null;
            oldContent.RenderTransform = null;
        };

        // 开始播放动画
        storyboard.Begin();
    }
}