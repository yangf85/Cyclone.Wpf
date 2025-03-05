using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;


public enum RunningDirection
{

    RightToLeft,

    LeftToRight,


    BottomToTop,

    TopToBottom
}


public enum RunningLoopMode
{
    Repeat,
    Reverse,
}



[TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
public class RunningBlock : ContentControl
{
    private Canvas _canvas;
   
    static RunningBlock()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RunningBlock), new FrameworkPropertyMetadata(typeof(RunningBlock)));        // 重写默认样式
    }

    #region Spacing
   

    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing),typeof(double),typeof(RunningBlock), new PropertyMetadata(double.NaN, OnSpacingPropertyChanged));
    
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    private static void OnSpacingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((RunningBlock)d).UpdatePosition();
    }
    #endregion

    #region Speed

    public static readonly DependencyProperty SpeedProperty =
        DependencyProperty.Register(nameof(Speed), typeof(double), typeof(RunningBlock), new PropertyMetadata(120d, OnSpeedPropertyChanged));


    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    private static void OnSpeedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((RunningBlock)d).UpdatePosition();
    }
    #endregion


    #region Direction
    public static readonly DependencyProperty DirectionProperty =
        DependencyProperty.Register(nameof(Direction), typeof(RunningDirection), typeof(RunningBlock), new PropertyMetadata(RunningDirection.RightToLeft, OnDirectionPropertyChanged));
   

    public RunningDirection Direction
    {
        get => (RunningDirection)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    private static void OnDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((RunningBlock)d).UpdatePosition();
    }
    #endregion

    #region IsRunning
    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    public static readonly DependencyProperty IsRunningProperty =
        DependencyProperty.Register(nameof(IsRunning), typeof(bool), typeof(RunningBlock), new PropertyMetadata(default(bool)));

    #endregion

    #region Private

    void UpdatePosition()
    {
    }

    #endregion

}