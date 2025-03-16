using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = PART_StartThumb, Type = typeof(Thumb))]
[TemplatePart(Name = PART_EndThumb, Type = typeof(Thumb))]
[TemplatePart(Name = PART_StartRegion, Type = typeof(RepeatButton))]
[TemplatePart(Name = PART_MiddleRegion, Type = typeof(RepeatButton))]
[TemplatePart(Name = PART_EndRegion, Type = typeof(RepeatButton))]
public class RangeSlider : Control
{
    private const string PART_StartThumb = nameof(PART_StartThumb);

    private const string PART_EndThumb = nameof(PART_EndThumb);

    private const string PART_StartRegion = nameof(PART_StartRegion);

    private const string PART_MiddleRegion = nameof(PART_MiddleRegion);

    private const string PART_EndRegion = nameof(PART_EndRegion);

    private Thumb _StartThumb;

    private Thumb _EndThumb;

    private RepeatButton _StartRegion;

    private RepeatButton _MiddleRegion;

    private RepeatButton _EndRegion;

    static RangeSlider()
    {
        EventManager.RegisterClassHandler(typeof(RangeSlider), Thumb.DragStartedEvent, new DragStartedEventHandler(OnDragStartedEvent));
        EventManager.RegisterClassHandler(typeof(RangeSlider), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnThumbDragDelta));
        EventManager.RegisterClassHandler(typeof(RangeSlider), Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnDragCompletedEvent));
    }

    public RangeSlider()
    {
        Loaded += RangeSlider_Loaded;
        LowerValueChanged += RangeSlider_LowerValueChanged;
        UpperValueChanged += RangeSlider_UpperValueChanged;
    }



    private enum ThumbKind
    {
        Start,

        End,
    }

    #region TrackThickness
    public double TrackThickness
    {
        get => (double)GetValue(TrackThicknessProperty);
        set => SetValue(TrackThicknessProperty, value);
    }

    public static readonly DependencyProperty TrackThicknessProperty =
        DependencyProperty.Register(nameof(TrackThickness), typeof(double), typeof(RangeSlider), new PropertyMetadata(default(double)));

    #endregion

    #region InactiveTrackColor
    public Brush InactiveTrackColor
    {
        get => (Brush)GetValue(InactiveTrackColorProperty);
        set => SetValue(InactiveTrackColorProperty, value);
    }

    public static readonly DependencyProperty InactiveTrackColorProperty =
        DependencyProperty.Register(nameof(InactiveTrackColor), typeof(Brush), typeof(RangeSlider), new PropertyMetadata(default(Brush)));

    #endregion

    #region ActiveTrackColor
    public Brush ActiveTrackColor
    {
        get => (Brush)GetValue(ActiveTrackColorProperty);
        set => SetValue(ActiveTrackColorProperty, value);
    }

    public static readonly DependencyProperty ActiveTrackColorProperty =
        DependencyProperty.Register(nameof(ActiveTrackColor), typeof(Brush), typeof(RangeSlider), new PropertyMetadata(default(Brush)));

    #endregion

    #region Delay

    public static readonly DependencyProperty DelayProperty =
        DependencyProperty.Register(nameof(Delay), typeof(int), typeof(RangeSlider), new PropertyMetadata(500, OnDelayChanged, OnCoerceDelay));

    public int Delay
    {
        get => (int)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    private static void OnDelayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        if (slider._StartRegion != null)
        {
            slider._StartRegion.Delay = (int)e.NewValue;
        }
        if (slider._MiddleRegion != null)
        {
            slider._MiddleRegion.Delay = (int)e.NewValue;
        }
        if (slider._EndRegion != null)
        {
            slider._EndRegion.Delay = (int)e.NewValue;
        }
    }

    private static object OnCoerceDelay(DependencyObject d, object baseValue)
    {
        var num = (int)baseValue;
        if (num < 0)
        {
            throw new ArgumentException("delay must be >=0");
        }
        return baseValue;
    }

    #region Interval

    public static readonly DependencyProperty IntervalProperty =
        DependencyProperty.Register(nameof(Interval), typeof(int), typeof(RangeSlider), new PropertyMetadata(30, OnIntervalChanged, OnCoerceInterval));

    public int Interval
    {
        get => (int)GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }

    private static void OnIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        if (slider._StartRegion != null)
        {
            slider._StartRegion.Interval = (int)e.NewValue;
        }
        if (slider._MiddleRegion != null)
        {
            slider._MiddleRegion.Interval = (int)e.NewValue;
        }
        if (slider._EndRegion != null)
        {
            slider._EndRegion.Interval = (int)e.NewValue;
        }
    }

    private static object OnCoerceInterval(DependencyObject d, object baseValue)
    {
        var num = (int)baseValue;
        if (num <= 0)
        {
            throw new ArgumentException("delay must be > 0");
        }
        return baseValue;
    }

    #endregion Interval

    #endregion Delay

    #region Step

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(nameof(Step), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(OnStepChanged, OnCoerceStep));

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    private static object OnCoerceStep(DependencyObject d, object baseValue)
    {
        var num = (double)baseValue;
        if (num <= 0)
        {
            throw new ArgumentException("step must >0");
        }
        return num;
    }

    private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    #endregion Step

    #region Maximum

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(100d, FrameworkPropertyMetadataOptions.AffectsMeasure, OnMaximumChanged, OnCoerceMaximum));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    private static object OnCoerceMaximum(DependencyObject d, object baseValue)
    {
        var slider = (RangeSlider)d;
        var num = (double)baseValue;
        if (num < slider.Minimum)
        {
            throw new ArgumentException("maximum must be >= minimum ");
        }
        if (num < slider.UpperValue)
        {
            throw new ArgumentException("maximum must be >= minimum ");
        }
        return baseValue;
    }

    private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    #endregion Maximum

    #region Minimum

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure, OnMinimumChanged, OnCoerceMinimum));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    private static object OnCoerceMinimum(DependencyObject d, object baseValue)
    {
        var slider = (RangeSlider)d;
        var num = (double)baseValue;
        if (num > slider.Maximum)
        {
            throw new ArgumentException("minimum must be <= maximum");
        }
        return baseValue;
    }

    private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    #endregion Minimum

    #region LowerValue

    public static readonly DependencyProperty LowerValueProperty =
        DependencyProperty.Register(nameof(LowerValue), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(25d, FrameworkPropertyMetadataOptions.AffectsArrange, OnLowerValueChanged, OnCoerceLowerValue));

    public double LowerValue
    {
        get => (double)GetValue(LowerValueProperty);
        set => SetValue(LowerValueProperty, value);
    }

    private static object OnCoerceLowerValue(DependencyObject d, object baseValue)
    {
        var slider = (RangeSlider)d;
        var num = (double)baseValue;
        if (num > slider.UpperValue)
        {
            num = slider.UpperValue;
        }

        return num;
    }

    private static void OnLowerValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        var args = new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, LowerValueChangedEvent);
        slider.RaiseEvent(args);
    }

    #endregion LowerValue

    #region AutoToolTipPlacement
    public AutoToolTipPlacement AutoToolTipPlacement
    {
        get => (AutoToolTipPlacement)GetValue(AutoToolTipPlacementProperty);
        set => SetValue(AutoToolTipPlacementProperty, value);
    }

    public static readonly DependencyProperty AutoToolTipPlacementProperty =
        DependencyProperty.Register(nameof(AutoToolTipPlacement), typeof(AutoToolTipPlacement), typeof(RangeSlider), new PropertyMetadata(default(AutoToolTipPlacement)));

    #endregion

    #region AutoToolTipPrecision
    public int AutoToolTipPrecision
    {
        get => (int)GetValue(AutoToolTipPrecisionProperty);
        set => SetValue(AutoToolTipPrecisionProperty, value);
    }

    public static readonly DependencyProperty AutoToolTipPrecisionProperty =
        DependencyProperty.Register(nameof(AutoToolTipPrecision), typeof(int), typeof(RangeSlider), new PropertyMetadata(default(int)));

    #endregion

    #region UpperValue

    public static readonly DependencyProperty UpperValueProperty =
        DependencyProperty.Register(nameof(UpperValue), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(75d, FrameworkPropertyMetadataOptions.AffectsArrange, OnUpperValueChanged, OnCoerceUpperValue));

    public double UpperValue
    {
        get => (double)GetValue(UpperValueProperty);
        set => SetValue(UpperValueProperty, value);
    }

    

    private static object OnCoerceUpperValue(DependencyObject d, object baseValue)
    {
        var slider = (RangeSlider)d;

        var num = (double)baseValue;

        if (num < slider.LowerValue)
        {
            num = slider.LowerValue;
        }

        return num;
    }

    private static void OnUpperValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        var args = new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, UpperValueChangedEvent);
        slider.RaiseEvent(args);
    }

    #endregion UpperValue

    #region Orientation

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(RangeSlider), new PropertyMetadata(default(Orientation)));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    #endregion Orientation

    #region TickPlacement

    public static readonly DependencyProperty TickPlacementProperty =
        DependencyProperty.Register(nameof(TickPlacement), typeof(TickPlacement), typeof(RangeSlider), new PropertyMetadata(default(TickPlacement)));

    public TickPlacement TickPlacement
    {
        get => (TickPlacement)GetValue(TickPlacementProperty);
        set => SetValue(TickPlacementProperty, value);
    }

    #endregion TickPlacement

    #region IsMoveToPoint

    public static readonly DependencyProperty IsMoveToPointProperty =
        DependencyProperty.Register(nameof(IsMoveToPoint), typeof(bool), typeof(RangeSlider), new PropertyMetadata(true));

    public bool IsMoveToPoint
    {
        get => (bool)GetValue(IsMoveToPointProperty);
        set => SetValue(IsMoveToPointProperty, value);
    }

    #endregion IsMoveToPoint

    #region IsSnapToStep

    public static readonly DependencyProperty IsSnapToStepProperty =
        DependencyProperty.Register(nameof(IsSnapToStep), typeof(bool), typeof(RangeSlider), new PropertyMetadata(true,OnIsSnapToStep));

    public bool IsSnapToStep
    {
        get => (bool)GetValue(IsSnapToStepProperty);
        set => SetValue(IsSnapToStepProperty, value);
    }

    private static void OnIsSnapToStep(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    #endregion IsSnapToStep

    #region Override
    private ToolTip _startThumbToolTip;
    private ToolTip _endThumbToolTip;
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _StartThumb = GetTemplateChild(PART_StartThumb) as Thumb;
        _EndThumb = GetTemplateChild(PART_EndThumb) as Thumb;
        _StartRegion = GetTemplateChild(PART_StartRegion) as RepeatButton;
        _MiddleRegion = GetTemplateChild(PART_MiddleRegion) as RepeatButton;
        _EndRegion = GetTemplateChild(PART_EndRegion) as RepeatButton;

        _startThumbToolTip = _StartThumb?.ToolTip as ToolTip;
        _endThumbToolTip = _EndThumb?.ToolTip as ToolTip;

        
        if (_StartThumb != null)
        {
            ToolTipService.SetInitialShowDelay(_StartThumb, 0);
            ToolTipService.SetBetweenShowDelay(_StartThumb, 0);
        }

        if (_EndThumb != null)
        {
            
            ToolTipService.SetInitialShowDelay(_EndThumb, 0);
            ToolTipService.SetBetweenShowDelay(_EndThumb, 0);
        }

        if (_StartRegion != null)
        {
            _StartRegion.Click -= StartRegion_Click;
            _StartRegion.Click += StartRegion_Click;
        }
        if (_MiddleRegion != null)
        {
            _MiddleRegion.PreviewMouseLeftButtonDown -= MiddleRegion_PreviewMouseLeftButtonDown;
            _MiddleRegion.PreviewMouseLeftButtonDown += MiddleRegion_PreviewMouseLeftButtonDown;
            _MiddleRegion.PreviewMouseRightButtonDown -= MiddleRegion_PreviewMouseRightButtonDown;
            _MiddleRegion.PreviewMouseRightButtonDown += MiddleRegion_PreviewMouseRightButtonDown;
        }
        if (_EndRegion != null)
        {
            _EndRegion.Click -= EndRegion_Click;
            _EndRegion.Click += EndRegion_Click;
        }
    }

   

    #endregion Override

    #region LowerValueChanged

    public static readonly RoutedEvent LowerValueChangedEvent =
        EventManager.RegisterRoutedEvent("LowerValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(RangeSlider));

    public event RoutedPropertyChangedEventHandler<double> LowerValueChanged
    {
        add { AddHandler(LowerValueChangedEvent, value); }
        remove { RemoveHandler(LowerValueChangedEvent, value); }
    }

    #endregion LowerValueChanged

    #region UpperValueChanged

    public static readonly RoutedEvent UpperValueChangedEvent =
        EventManager.RegisterRoutedEvent("UpperValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(RangeSlider));

    public event RoutedPropertyChangedEventHandler<double> UpperValueChanged
    {
        add { AddHandler(UpperValueChangedEvent, value); }
        remove { RemoveHandler(UpperValueChangedEvent, value); }
    }

    #endregion UpperValueChanged

    #region HandleDragEvent

    private static void OnDragStartedEvent(object sender, DragStartedEventArgs e)
    {
        if (sender is RangeSlider rs)
        {
            rs.OnDragStartedEvent(e);
        }
    }

    private static void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (sender is RangeSlider rs)
        {
            rs.OnThumbDragDelta(e);
        }
    }

    private static void OnDragCompletedEvent(object sender, DragCompletedEventArgs e)
    {
        if (sender is RangeSlider rs)
        {
            rs.OnDragCompletedEvent(e);
        }
    }

    private void OnDragStartedEvent(DragStartedEventArgs e)
    {
        var thumb = e.OriginalSource as Thumb;
        if (thumb == _StartThumb && _startThumbToolTip != null)
        {
            _startThumbToolTip.IsOpen = true;
        }
        else if (thumb == _EndThumb && _endThumbToolTip != null)
        {
            _endThumbToolTip.IsOpen = true;
        }
    }

    private void OnThumbDragDelta(DragDeltaEventArgs e)
    {
        if (!CanUpdate())
        {
            return;
        }
        var offset = 0d;

        if (Orientation == Orientation.Horizontal)
        {
            offset = e.HorizontalChange / ActualWidth * Math.Abs(Maximum - Minimum);
        }
        else
        {
            offset = e.VerticalChange / ActualHeight * Math.Abs(Maximum - Minimum);
        }

        var thumb = e.OriginalSource as Thumb;

        if (thumb == _StartThumb)
        {
            var newLowerValue = LowerValue + offset;
            // 关键修复：在赋值前应用Step对齐
            if (IsSnapToStep)
            {
                newLowerValue = RoundToNearest(newLowerValue, Step);
            }
            LowerValue = Math.Max(Minimum, Math.Min(newLowerValue, UpperValue));

            UpdateThumbPosition(ThumbKind.Start);
            UpdateToolTip(_startThumbToolTip, LowerValue, thumb);
        }
        else if (thumb == _EndThumb)
        {
            var newUpperValue = UpperValue + offset;
            // 关键修复：在赋值前应用Step对齐
            if (IsSnapToStep)
            {
                newUpperValue = RoundToNearest(newUpperValue, Step);
            }
            UpperValue = Math.Min(Maximum, Math.Max(newUpperValue, LowerValue));

            UpdateThumbPosition(ThumbKind.End);
            UpdateToolTip(_endThumbToolTip, UpperValue, thumb);
        }
    }


    private void UpdateToolTip(ToolTip toolTip, double value, Thumb thumb)
    {
        if (toolTip == null || thumb == null) return;

        // 格式化数值精度
        toolTip.Content = value.ToString($"F{AutoToolTipPrecision}");

        // 设置工具提示位置
        toolTip.Placement = GetToolTipPlacement();
        toolTip.PlacementTarget = thumb;

        // 强制更新ToolTip位置（新增的关键代码）
        if (Orientation == Orientation.Horizontal)
        {
            toolTip.VerticalOffset += 0.001;
            toolTip.VerticalOffset -= 0.001;
        }
        else
        {
            toolTip.HorizontalOffset += 0.001;
            toolTip.HorizontalOffset -= 0.001;
        }
    }


    private PlacementMode GetToolTipPlacement()
    {
        if (Orientation == Orientation.Horizontal)
        {
            return AutoToolTipPlacement switch
            {
                AutoToolTipPlacement.TopLeft => PlacementMode.Top,
                AutoToolTipPlacement.BottomRight => PlacementMode.Bottom,
                _ => PlacementMode.Top
            };
        }
        else
        {
            return AutoToolTipPlacement switch
            {
                AutoToolTipPlacement.TopLeft => PlacementMode.Left,
                AutoToolTipPlacement.BottomRight => PlacementMode.Right,
                _ => PlacementMode.Left
            };
        }
    }

    private void OnDragCompletedEvent(DragCompletedEventArgs e)
    {
        var thumb = e.OriginalSource as Thumb;
        if (thumb == _StartThumb && _startThumbToolTip != null)
        {
            _startThumbToolTip.IsOpen = false;
        }
        else if (thumb == _EndThumb && _endThumbToolTip != null)
        {
            _endThumbToolTip.IsOpen = false;
        }
    }

    #endregion HandleDragEvent

    #region Private

    private bool CanUpdate()
    {
        return _StartThumb != null && _EndThumb != null && _StartRegion != null && _MiddleRegion != null && _EndRegion != null;
    }

    private double RoundToNearest(double value, double step)
    {
        //对数字按照指定的步距处理
        var digt = step.ToString(CultureInfo.InvariantCulture).IndexOf(".");
        if (digt < 0)
        {
            digt = 0;
        }

        var n = Math.Round(Math.Round(value / step) * step, digt);
        return n;
    }

    private void UpdateThumbPosition(ThumbKind thumbKind)
    {
        if (!CanUpdate())
        {
            return;
        }
        var total = CalculateTotalSize(Orientation);

        var scale = 0d;

        if (IsSnapToStep)
        {
            LowerValue = RoundToNearest(LowerValue, Step);
            UpperValue = RoundToNearest(UpperValue, Step);
        }

        if (Orientation == Orientation.Horizontal)
        {
            switch (thumbKind)
            {
                case ThumbKind.Start:
                    scale = MapValueToRange(LowerValue, (Minimum, Maximum), (0d, 1d));
                    _StartRegion.Width = scale * total;
                    break;

                case ThumbKind.End:

                    scale = MapValueToRange(Maximum - UpperValue + Minimum, (Minimum, Maximum), (0d, 1d));
                    _EndRegion.Width = scale * total;
                    break;

                default:
                    break;
            }
        }
        else
        {
            switch (thumbKind)
            {
                case ThumbKind.Start:
                    scale = MapValueToRange(LowerValue, (Minimum, Maximum), (0d, 1d));
                    _StartRegion.Height = scale * total;
                    break;

                case ThumbKind.End:

                    scale = MapValueToRange(Maximum - UpperValue + Minimum, (Minimum, Maximum), (0d, 1d));
                    _EndRegion.Height = scale * total;
                    break;

                default:
                    break;
            }
        }
    }

    private double MapValueToRange(double value, (double Min, double Max) original, (double Min, double Max) target)
    {
        //计算value在原始范围original中的相对位置
        var num = (value - original.Min) / (original.Max - original.Min);

        //返回根据num计算value在目标范围target中的对应值
        return target.Min + num * (target.Max - target.Min);
    }

    private double CalculateTotalSize(Orientation orientation, bool isAll = false)
    {
        switch (orientation)
        {
            case Orientation.Horizontal:
                if (isAll)
                {
                    return _StartRegion.ActualWidth + _StartThumb.ActualWidth + _MiddleRegion.ActualWidth + _EndThumb.ActualWidth + _EndRegion.ActualWidth;
                }
                else
                {
                    return _StartRegion.ActualWidth + _MiddleRegion.ActualWidth + _EndRegion.ActualWidth;
                }

            case Orientation.Vertical:
                if (isAll)
                {
                    return _StartRegion.ActualHeight + _StartThumb.ActualHeight + _MiddleRegion.ActualHeight + _EndThumb.ActualHeight + _EndRegion.ActualHeight;
                }
                else
                {
                    return _StartRegion.ActualHeight + _MiddleRegion.ActualHeight + _EndRegion.ActualHeight;
                }

            default:
                throw new NotImplementedException();
        }
    }

    private void RangeSlider_LowerValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateThumbPosition(ThumbKind.Start);
    }

    private void RangeSlider_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateThumbPosition(ThumbKind.Start);
        UpdateThumbPosition(ThumbKind.End);
    }

    private void RangeSlider_UpperValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateThumbPosition(ThumbKind.End);
    }

    private void StartRegion_Click(object sender, RoutedEventArgs e)
    {
        UpdateValueFromPoint(Mouse.GetPosition(this), ThumbKind.Start, MouseButton.Middle);
    }

    private void UpdateValueFromPoint(Point pt, ThumbKind thumbKind, MouseButton button)
    {
        var total = CalculateTotalSize(Orientation, true);
        switch (thumbKind)
        {
            case ThumbKind.Start:
                if (IsMoveToPoint)
                {
                    LowerValue = Minimum + pt.X / total * (Maximum - Minimum);
                }
                else
                {
                    if (button == MouseButton.Left)
                    {
                        LowerValue += Step;
                    }
                    else
                    {
                        LowerValue -= Step;
                    }
                }
                break;

            case ThumbKind.End:
                if (IsMoveToPoint)
                {
                    UpperValue = Minimum + pt.X / total * (Maximum - Minimum);
                }
                else
                {
                    if (button == MouseButton.Right)
                    {
                        UpperValue -= Step;
                    }
                    else
                    {
                        UpperValue += Step;
                    }
                }
                break;

            default:
                break;
        }

        UpdateThumbPosition(thumbKind);
    }

    private void EndRegion_Click(object sender, RoutedEventArgs e)
    {
        UpdateValueFromPoint(Mouse.GetPosition(this), ThumbKind.End, MouseButton.Middle);
    }

    private void MiddleRegion_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        UpdateValueFromPoint(e.GetPosition(this), ThumbKind.Start, MouseButton.Left);
    }

    private void MiddleRegion_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        UpdateValueFromPoint(e.GetPosition(this), ThumbKind.End, MouseButton.Right);
    }

    #endregion Private
}