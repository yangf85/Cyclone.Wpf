using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

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