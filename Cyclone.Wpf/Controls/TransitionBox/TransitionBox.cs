using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

/// <summary>
/// 过渡动画接口
/// </summary>
public interface ITransition
{
    /// <summary>
    /// 创建从旧内容到新内容的过渡动画
    /// </summary>
    /// <param name="oldContent">旧内容</param>
    /// <param name="newContent">新内容</param>
    /// <returns>动画时间轴</returns>
    Storyboard CreateTransition(FrameworkElement oldContent, FrameworkElement newContent);
}

/// <summary>
/// 内容切换控件，允许在切换内容时播放动画
/// </summary>
[ContentProperty("Content")]
public class TransitionBox : ContentControl
{
    #region 私有字段

    private Grid _rootGrid;
    private ContentPresenter _oldContentPresenter;
    private ContentPresenter _newContentPresenter;
    private bool _isAnimating = false;
    private object _pendingContent = null;

    #endregion 私有字段

    #region 构造函数

    public TransitionBox()
    {
        this.DefaultStyleKey = typeof(TransitionBox);
        Loaded += TransitionBox_Loaded;
    }

    static TransitionBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TransitionBox),
            new FrameworkPropertyMetadata(typeof(TransitionBox)));
    }

    #endregion 构造函数

    #region 依赖属性

    /// <summary>
    /// 过渡动画
    /// </summary>
    public static readonly DependencyProperty TransitionProperty =
        DependencyProperty.Register("Transition", typeof(ITransition), typeof(TransitionBox),
            new PropertyMetadata(null));

    /// <summary>
    /// 过渡动画
    /// </summary>
    public ITransition Transition
    {
        get { return (ITransition)GetValue(TransitionProperty); }
        set { SetValue(TransitionProperty, value); }
    }

    /// <summary>
    /// 过渡动画持续时间
    /// </summary>
    public static readonly DependencyProperty TransitionDurationProperty =
        DependencyProperty.Register("TransitionDuration", typeof(Duration), typeof(TransitionBox),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(300))));

    /// <summary>
    /// 过渡动画持续时间
    /// </summary>
    public Duration TransitionDuration
    {
        get { return (Duration)GetValue(TransitionDurationProperty); }
        set { SetValue(TransitionDurationProperty, value); }
    }

    #endregion 依赖属性

    #region 重写方法

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        InitializeControl();
    }

    /// <summary>
    /// 当内容改变时触发
    /// </summary>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        // 如果还没有初始化完成，保存内容等待初始化后处理
        if (_rootGrid == null)
        {
            _pendingContent = newContent;
            return;
        }

        PerformTransition(oldContent, newContent);
    }

    #endregion 重写方法

    #region 私有方法

    private void TransitionBox_Loaded(object sender, RoutedEventArgs e)
    {
        InitializeControl();

        // 如果有挂起的内容，现在处理它
        if (_pendingContent != null)
        {
            _newContentPresenter.Content = _pendingContent;
            _pendingContent = null;
        }
        else if (Content != null)
        {
            _newContentPresenter.Content = Content;
        }
    }

    private void InitializeControl()
    {
        if (_rootGrid != null)
            return;

        // 创建控件视觉树
        _rootGrid = new Grid();
        _oldContentPresenter = new ContentPresenter { Opacity = 0 };
        _newContentPresenter = new ContentPresenter { Opacity = 1 };

        _rootGrid.Children.Add(_oldContentPresenter);
        _rootGrid.Children.Add(_newContentPresenter);

        // 设置为内容
        SetCurrentValue(ContentProperty, null); // 清除当前内容
        base.Content = _rootGrid;

        Debug.WriteLine("TransitionBox初始化完成");
    }

    private void PerformTransition(object oldContent, object newContent)
    {
        // 如果正在动画中，等待动画完成
        if (_isAnimating || _rootGrid == null)
        {
            _pendingContent = newContent;
            return;
        }

        // 忽略空内容或控件本身作为内容
        if (newContent == null || newContent == _rootGrid)
            return;

        Debug.WriteLine($"开始内容转换: {oldContent} -> {newContent}");

        // 交换新旧内容
        ContentPresenter temp = _oldContentPresenter;
        _oldContentPresenter = _newContentPresenter;
        _newContentPresenter = temp;

        // 设置新内容
        _newContentPresenter.Content = newContent;

        // 确保内容可见性正确
        _oldContentPresenter.Opacity = 1;
        _oldContentPresenter.Visibility = Visibility.Visible;
        _newContentPresenter.Opacity = 0;
        _newContentPresenter.Visibility = Visibility.Visible;

        // 应用转场动画
        if (Transition != null && _oldContentPresenter.Content != null)
        {
            try
            {
                _isAnimating = true;
                Storyboard storyboard = Transition.CreateTransition(_oldContentPresenter, _newContentPresenter);

                if (storyboard != null)
                {
                    Debug.WriteLine("开始播放过渡动画");

                    // 确保动画完成后的清理
                    storyboard.Completed += (s, e) =>
                    {
                        _isAnimating = false;

                        // 确保状态正确
                        _oldContentPresenter.Opacity = 0;
                        _newContentPresenter.Opacity = 1;

                        // 处理挂起的内容更新
                        if (_pendingContent != null)
                        {
                            object pendingContent = _pendingContent;
                            _pendingContent = null;
                            PerformTransition(_newContentPresenter.Content, pendingContent);
                        }

                        Debug.WriteLine("过渡动画完成");
                    };

                    storyboard.Begin(_rootGrid, true);
                }
                else
                {
                    // 动画创建失败，直接显示新内容
                    _oldContentPresenter.Opacity = 0;
                    _newContentPresenter.Opacity = 1;
                    _isAnimating = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"转场动画异常: {ex.Message}");
                _oldContentPresenter.Opacity = 0;
                _newContentPresenter.Opacity = 1;
                _isAnimating = false;
            }
        }
        else
        {
            // 没有动画，直接切换
            _oldContentPresenter.Opacity = 0;
            _newContentPresenter.Opacity = 1;
        }
    }

    #endregion 私有方法
}

/// <summary>
/// 淡入淡出过渡动画
/// </summary>
public class FadeTransition : ITransition
{
    public Storyboard CreateTransition(FrameworkElement oldContent, FrameworkElement newContent)
    {
        Storyboard storyboard = new Storyboard();

        // 旧内容淡出
        DoubleAnimation fadeOutAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300))
        };
        Storyboard.SetTarget(fadeOutAnimation, oldContent);
        Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(fadeOutAnimation);

        // 新内容淡入
        DoubleAnimation fadeInAnimation = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300))
        };
        Storyboard.SetTarget(fadeInAnimation, newContent);
        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(fadeInAnimation);

        return storyboard;
    }
}

/// <summary>
/// 滑动过渡动画
/// </summary>
public class SlideTransition : ITransition
{
    public enum SlideDirection
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }

    public SlideDirection Direction { get; set; } = SlideDirection.RightToLeft;

    public Storyboard CreateTransition(FrameworkElement oldContent, FrameworkElement newContent)
    {
        Storyboard storyboard = new Storyboard();

        // 确保新内容可见
        newContent.Opacity = 1;

        // 准备变换
        TranslateTransform oldTransform = oldContent.RenderTransform as TranslateTransform;
        TranslateTransform newTransform = newContent.RenderTransform as TranslateTransform;

        if (oldTransform == null)
        {
            oldTransform = new TranslateTransform();
            oldContent.RenderTransform = oldTransform;
        }

        if (newTransform == null)
        {
            newTransform = new TranslateTransform();
            newContent.RenderTransform = newTransform;
        }

        // 设置中心点
        oldContent.RenderTransformOrigin = new Point(0.5, 0.5);
        newContent.RenderTransformOrigin = new Point(0.5, 0.5);

        // 根据方向设置动画属性
        PropertyPath translatePropertyPath;
        double startValue, endValue;

        switch (Direction)
        {
            case SlideDirection.LeftToRight:
                translatePropertyPath = new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)");
                startValue = -300;
                endValue = 300;
                break;

            case SlideDirection.RightToLeft:
                translatePropertyPath = new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)");
                startValue = 300;
                endValue = -300;
                break;

            case SlideDirection.TopToBottom:
                translatePropertyPath = new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)");
                startValue = -300;
                endValue = 300;
                break;

            case SlideDirection.BottomToTop:
                translatePropertyPath = new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)");
                startValue = 300;
                endValue = -300;
                break;

            default:
                translatePropertyPath = new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)");
                startValue = 300;
                endValue = -300;
                break;
        }

        // 设置初始位置
        if (Direction == SlideDirection.LeftToRight || Direction == SlideDirection.RightToLeft)
        {
            newTransform.X = startValue;
            oldTransform.X = 0;
        }
        else
        {
            newTransform.Y = startValue;
            oldTransform.Y = 0;
        }

        // 新内容进入动画
        DoubleAnimation newContentAnimation = new DoubleAnimation
        {
            From = startValue,
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(400)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(newContentAnimation, newContent);
        Storyboard.SetTargetProperty(newContentAnimation, translatePropertyPath);
        storyboard.Children.Add(newContentAnimation);

        // 旧内容离开动画
        DoubleAnimation oldContentAnimation = new DoubleAnimation
        {
            From = 0,
            To = endValue,
            Duration = new Duration(TimeSpan.FromMilliseconds(400)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(oldContentAnimation, oldContent);
        Storyboard.SetTargetProperty(oldContentAnimation, translatePropertyPath);
        storyboard.Children.Add(oldContentAnimation);

        return storyboard;
    }
}