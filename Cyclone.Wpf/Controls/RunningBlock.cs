using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;



public enum RunningDirection
{
    Horizontal,

    Vertical
}

public enum RunningLoopMode
{
    Repeat,

    Reverse
}

[TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
[TemplatePart(Name = "PART_Content", Type = typeof(ContentPresenter))]
public class RunningBlock : ContentControl
{

    #region IsRunning
    public static readonly DependencyProperty IsRunningProperty =
        DependencyProperty.Register(nameof(IsRunning), typeof(bool), typeof(RunningBlock),
            new FrameworkPropertyMetadata(true, OnIsRunningChanged));
    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }
    #endregion

    #region Duration
    public static readonly DependencyProperty DurationProperty =
      DependencyProperty.Register(nameof(Duration), typeof(Duration), typeof(RunningBlock),
          new FrameworkPropertyMetadata(new Duration(TimeSpan.FromSeconds(5)), OnDurationChanged));
    public Duration Duration
    {
        get => (Duration)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }
    #endregion

    #region LoopMode
    public static readonly DependencyProperty LoopModeProperty =
       DependencyProperty.Register(nameof(LoopMode), typeof(RunningLoopMode), typeof(RunningBlock),
           new FrameworkPropertyMetadata(RunningLoopMode.Repeat, OnLoopModeChanged));

    public RunningLoopMode LoopMode
    {
        get => (RunningLoopMode)GetValue(LoopModeProperty);
        set => SetValue(LoopModeProperty, value);
    }
    #endregion


    #region Direction

    public static readonly DependencyProperty DirectionProperty =
        DependencyProperty.Register(nameof(Direction), typeof(RunningDirection), typeof(RunningBlock),
            new FrameworkPropertyMetadata(RunningDirection.Horizontal, OnDirectionChanged));
    public RunningDirection Direction
    {
        get => (RunningDirection)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    #endregion

    #region Private Fields

    private Canvas _canvas;

    private ContentPresenter _content;

    private Storyboard _storyboard;

    #endregion Private Fields

    static RunningBlock()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RunningBlock),
            new FrameworkPropertyMetadata(typeof(RunningBlock)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _canvas = GetTemplateChild("PART_Canvas") as Canvas;
        _content = GetTemplateChild("PART_Content") as ContentPresenter;

        if (_content != null)
        {
            _content.RenderTransform = new TranslateTransform();
            _content.SizeChanged += OnContentSizeChanged;
        }

        UpdateAnimation();
    }

    #region Event Handlers

    private static void OnIsRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (RunningBlock)d;
        control.UpdateAnimation();
    }

    private static void OnDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((RunningBlock)d).UpdateAnimation();

    private static void OnLoopModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((RunningBlock)d).UpdateAnimation();

    private static void OnDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((RunningBlock)d).UpdateAnimation();

    private void OnContentSizeChanged(object sender, SizeChangedEventArgs e)
        => UpdateAnimation();

    #endregion Event Handlers

    #region Animation Control

    private void StartAnimation()
    {
        _storyboard?.Begin(this, true);
    }

    private void StopAnimation()
    {
        _storyboard?.Pause(this);
    }

    private void UpdateAnimation()
    {
        if (_canvas == null || _content == null) return;

        // 停止当前动画并清除Storyboard
        _storyboard?.Stop(this);
        _storyboard = new Storyboard();

        if (!IsRunning)
        {
            // 如果IsRunning为false，直接返回，不添加任何动画
            return;
        }

        // 计算动画值
        var (from, to) = CalculateAnimationValues();

        // 创建动画
        var animation = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = Duration,
            AutoReverse = LoopMode == RunningLoopMode.Reverse,
            RepeatBehavior = RepeatBehavior.Forever
        };

        // 设置目标属性
        var propertyPath = Direction == RunningDirection.Horizontal
            ? new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)")
            : new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)");

        Storyboard.SetTarget(animation, _content);
        Storyboard.SetTargetProperty(animation, propertyPath);

        // 添加动画到Storyboard并开始
        _storyboard.Children.Add(animation);
        _storyboard.Begin(this, true);
    }

    private (double from, double to) CalculateAnimationValues()
    {
        var containerSize = Direction == RunningDirection.Horizontal
            ? _canvas.ActualWidth
            : _canvas.ActualHeight;

        var contentSize = Direction == RunningDirection.Horizontal
            ? _content.ActualWidth
            : _content.ActualHeight;

        if (contentSize > containerSize)
        {
            // 内容比容器大：从右侧边缘开始，移动到左侧边缘结束
            return Direction switch
            {
                RunningDirection.Horizontal => (containerSize, containerSize - contentSize),
                RunningDirection.Vertical => (containerSize, containerSize - contentSize),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        else
        {
            // 内容比容器小：在容器内来回移动（始终可见）
            var from = 0.0; // 左边缘对齐容器左侧
            var to = containerSize - contentSize; // 右边缘对齐容器右侧
            return (from, to);
        }
    }

    #endregion Animation Control
}