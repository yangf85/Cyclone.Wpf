using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
    #region Dependency Properties

    public static readonly DependencyProperty IsRunningProperty =
        DependencyProperty.Register(nameof(IsRunning), typeof(bool), typeof(RunningBlock),
            new FrameworkPropertyMetadata(true, OnRunningChanged));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(Duration), typeof(RunningBlock),
            new FrameworkPropertyMetadata(new Duration(TimeSpan.FromSeconds(5)), OnDurationChanged));

    public static readonly DependencyProperty LoopModeProperty =
        DependencyProperty.Register(nameof(LoopMode), typeof(RunningLoopMode), typeof(RunningBlock),
            new FrameworkPropertyMetadata(RunningLoopMode.Repeat, OnLoopModeChanged));

    public static readonly DependencyProperty DirectionProperty =
        DependencyProperty.Register(nameof(Direction), typeof(RunningDirection), typeof(RunningBlock),
            new FrameworkPropertyMetadata(RunningDirection.Horizontal, OnDirectionChanged));

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    public Duration Duration
    {
        get => (Duration)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public RunningLoopMode LoopMode
    {
        get => (RunningLoopMode)GetValue(LoopModeProperty);
        set => SetValue(LoopModeProperty, value);
    }

    public RunningDirection Direction
    {
        get => (RunningDirection)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    #endregion Dependency Properties

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

    private static void OnRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (RunningBlock)d;
        if ((bool)e.NewValue) control.StartAnimation();
        else control.StopAnimation();
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

        _storyboard?.Stop(this);
        _storyboard = new Storyboard();

        if (!IsRunning) return;
        if (_content.RenderTransform is not TranslateTransform) return;

        var (from, to) = CalculateAnimationValues();

        var animation = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = Duration,
            AutoReverse = LoopMode == RunningLoopMode.Reverse,
            RepeatBehavior = RepeatBehavior.Forever
        };

        var propertyPath = Direction == RunningDirection.Horizontal
            ? new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)")
            : new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)");

        Storyboard.SetTarget(animation, _content);
        Storyboard.SetTargetProperty(animation, propertyPath);
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

        // 修复逻辑：确保内容可以完全移入/移出可视区域
        return Direction switch
        {
            RunningDirection.Horizontal => contentSize > containerSize
                ? (containerSize, containerSize - contentSize)  // 长内容滚动
                : (containerSize, -contentSize),                // 短内容往返

            RunningDirection.Vertical => contentSize > containerSize
                ? (containerSize, containerSize - contentSize)  // 长内容滚动
                : (containerSize, -contentSize),                // 短内容往返

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    #endregion Animation Control
}