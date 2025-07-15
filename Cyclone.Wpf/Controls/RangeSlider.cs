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
    private ToolTip _startThumbToolTip;
    private ToolTip _endThumbToolTip;

    private bool _isInternalUpdate = false;

    static RangeSlider()
    {
        EventManager.RegisterClassHandler(typeof(RangeSlider), Thumb.DragStartedEvent, new DragStartedEventHandler(OnDragStartedEvent));
        EventManager.RegisterClassHandler(typeof(RangeSlider), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnThumbDragDelta));
        EventManager.RegisterClassHandler(typeof(RangeSlider), Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnDragCompletedEvent));
    }

    public RangeSlider()
    {
        Loaded += RangeSlider_Loaded;
    }

    private enum ThumbKind
    {
        Start,
        End,
    }

    #region TrackThickness

    public static readonly DependencyProperty TrackThicknessProperty =
        DependencyProperty.Register(nameof(TrackThickness), typeof(double), typeof(RangeSlider),
            new PropertyMetadata(5.0));

    public double TrackThickness
    {
        get => (double)GetValue(TrackThicknessProperty);
        set => SetValue(TrackThicknessProperty, value);
    }

    #endregion TrackThickness

    #region InactiveTrackColor

    public static readonly DependencyProperty InactiveTrackColorProperty =
        DependencyProperty.Register(nameof(InactiveTrackColor), typeof(Brush), typeof(RangeSlider),
            new PropertyMetadata(Brushes.LightGray));

    public Brush InactiveTrackColor
    {
        get => (Brush)GetValue(InactiveTrackColorProperty);
        set => SetValue(InactiveTrackColorProperty, value);
    }

    #endregion InactiveTrackColor

    #region ActiveTrackColor

    public static readonly DependencyProperty ActiveTrackColorProperty =
        DependencyProperty.Register(nameof(ActiveTrackColor), typeof(Brush), typeof(RangeSlider),
            new PropertyMetadata(Brushes.Blue));

    public Brush ActiveTrackColor
    {
        get => (Brush)GetValue(ActiveTrackColorProperty);
        set => SetValue(ActiveTrackColorProperty, value);
    }

    #endregion ActiveTrackColor

    #region Delay

    public static readonly DependencyProperty DelayProperty =
        DependencyProperty.Register(nameof(Delay), typeof(int), typeof(RangeSlider),
            new PropertyMetadata(500, OnDelayChanged, OnCoerceDelay));

    public int Delay
    {
        get => (int)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    private static void OnDelayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        var newValue = (int)e.NewValue;

        if (slider._StartRegion != null)
            slider._StartRegion.Delay = newValue;
        if (slider._MiddleRegion != null)
            slider._MiddleRegion.Delay = newValue;
        if (slider._EndRegion != null)
            slider._EndRegion.Delay = newValue;
    }

    private static object OnCoerceDelay(DependencyObject d, object baseValue)
    {
        var num = (int)baseValue;
        if (num < 0)
        {
            throw new ArgumentException("delay must be >= 0");
        }
        return baseValue;
    }

    #endregion Delay

    #region Interval

    public static readonly DependencyProperty IntervalProperty =
        DependencyProperty.Register(nameof(Interval), typeof(int), typeof(RangeSlider),
            new PropertyMetadata(30, OnIntervalChanged, OnCoerceInterval));

    public int Interval
    {
        get => (int)GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }

    private static void OnIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        var newValue = (int)e.NewValue;

        if (slider._StartRegion != null)
            slider._StartRegion.Interval = newValue;
        if (slider._MiddleRegion != null)
            slider._MiddleRegion.Interval = newValue;
        if (slider._EndRegion != null)
            slider._EndRegion.Interval = newValue;
    }

    private static object OnCoerceInterval(DependencyObject d, object baseValue)
    {
        var num = (int)baseValue;
        if (num <= 0)
        {
            throw new ArgumentException("interval must be > 0");
        }
        return baseValue;
    }

    #endregion Interval

    #region Step

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(nameof(Step), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(1.0, OnStepChanged, OnCoerceStep));

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
            throw new ArgumentException("step must be > 0");
        }
        return num;
    }

    private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        if (slider.IsSnapToStep)
        {
            slider.CoerceValueToStep();
        }
    }

    #endregion Step

    #region Maximum

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(100d, FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnMaximumChanged, OnCoerceMaximum));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    private static object OnCoerceMaximum(DependencyObject d, object baseValue)
    {
        var slider = (RangeSlider)d;
        var num = (double)baseValue;

        // 确保 Maximum >= Minimum
        return Math.Max(num, slider.Minimum);
    }

    private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;

        // 确保相关值在新的最大值范围内
        slider.CoerceValue(UpperValueProperty);
        slider.CoerceValue(LowerValueProperty);

        slider.UpdateLayout();
    }

    #endregion Maximum

    #region Minimum

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnMinimumChanged, OnCoerceMinimum));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    private static object OnCoerceMinimum(DependencyObject d, object baseValue)
    {
        var slider = (RangeSlider)d;
        var num = (double)baseValue;

        // 确保 Minimum <= Maximum
        return Math.Min(num, slider.Maximum);
    }

    private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;

        // 确保相关值在新的最小值范围内
        slider.CoerceValue(LowerValueProperty);
        slider.CoerceValue(UpperValueProperty);

        slider.UpdateLayout();
    }

    #endregion Minimum

    #region LowerValue

    public static readonly DependencyProperty LowerValueProperty =
        DependencyProperty.Register(nameof(LowerValue), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsArrange,
                OnLowerValueChanged, OnCoerceLowerValue));

    public double LowerValue
    {
        get => (double)GetValue(LowerValueProperty);
        set => SetValue(LowerValueProperty, value);
    }

    private static object OnCoerceLowerValue(DependencyObject d, object baseValue)
    {
        var slider = (RangeSlider)d;
        var num = (double)baseValue;

        // 确保 Minimum <= LowerValue <= UpperValue <= Maximum
        var minimum = slider.Minimum;
        var upperValue = slider.UpperValue;
        var maximum = slider.Maximum;

        return Math.Min(Math.Min(upperValue, maximum), Math.Max(num, minimum));
    }

    private static void OnLowerValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;

        if (!slider._isInternalUpdate)
        {
            slider.UpdateThumbPosition(ThumbKind.Start);
        }

        var args = new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, LowerValueChangedEvent);
        slider.RaiseEvent(args);
    }

    #endregion LowerValue

    #region UpperValue

    public static readonly DependencyProperty UpperValueProperty =
        DependencyProperty.Register(nameof(UpperValue), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(100d, FrameworkPropertyMetadataOptions.AffectsArrange,
                OnUpperValueChanged, OnCoerceUpperValue));

    public double UpperValue
    {
        get => (double)GetValue(UpperValueProperty);
        set => SetValue(UpperValueProperty, value);
    }

    private static object OnCoerceUpperValue(DependencyObject d, object baseValue)
    {
        var slider = (RangeSlider)d;
        var num = (double)baseValue;

        // 确保 Minimum <= LowerValue <= UpperValue <= Maximum
        var minimum = slider.Minimum;
        var lowerValue = slider.LowerValue;
        var maximum = slider.Maximum;

        return Math.Min(maximum, Math.Max(Math.Max(num, lowerValue), minimum));
    }

    private static void OnUpperValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;

        if (!slider._isInternalUpdate)
        {
            slider.UpdateThumbPosition(ThumbKind.End);
        }

        var args = new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, UpperValueChangedEvent);
        slider.RaiseEvent(args);
    }

    #endregion UpperValue

    #region AutoToolTipPlacement

    public static readonly DependencyProperty AutoToolTipPlacementProperty =
        DependencyProperty.Register(nameof(AutoToolTipPlacement), typeof(AutoToolTipPlacement), typeof(RangeSlider),
            new PropertyMetadata(AutoToolTipPlacement.TopLeft));

    public AutoToolTipPlacement AutoToolTipPlacement
    {
        get => (AutoToolTipPlacement)GetValue(AutoToolTipPlacementProperty);
        set => SetValue(AutoToolTipPlacementProperty, value);
    }

    #endregion AutoToolTipPlacement

    #region AutoToolTipPrecision

    public static readonly DependencyProperty AutoToolTipPrecisionProperty =
        DependencyProperty.Register(nameof(AutoToolTipPrecision), typeof(int), typeof(RangeSlider),
            new PropertyMetadata(1));

    public int AutoToolTipPrecision
    {
        get => (int)GetValue(AutoToolTipPrecisionProperty);
        set => SetValue(AutoToolTipPrecisionProperty, value);
    }

    #endregion AutoToolTipPrecision

    #region Orientation

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(RangeSlider),
            new PropertyMetadata(Orientation.Horizontal));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    #endregion Orientation

    #region TickPlacement

    public static readonly DependencyProperty TickPlacementProperty =
        DependencyProperty.Register(nameof(TickPlacement), typeof(TickPlacement), typeof(RangeSlider),
            new PropertyMetadata(TickPlacement.None));

    public TickPlacement TickPlacement
    {
        get => (TickPlacement)GetValue(TickPlacementProperty);
        set => SetValue(TickPlacementProperty, value);
    }

    #endregion TickPlacement

    #region IsMoveToPoint

    public static readonly DependencyProperty IsMoveToPointProperty =
        DependencyProperty.Register(nameof(IsMoveToPoint), typeof(bool), typeof(RangeSlider),
            new PropertyMetadata(true));

    public bool IsMoveToPoint
    {
        get => (bool)GetValue(IsMoveToPointProperty);
        set => SetValue(IsMoveToPointProperty, value);
    }

    #endregion IsMoveToPoint

    #region IsSnapToStep

    public static readonly DependencyProperty IsSnapToStepProperty =
        DependencyProperty.Register(nameof(IsSnapToStep), typeof(bool), typeof(RangeSlider),
            new PropertyMetadata(true, OnIsSnapToStepChanged));

    public bool IsSnapToStep
    {
        get => (bool)GetValue(IsSnapToStepProperty);
        set => SetValue(IsSnapToStepProperty, value);
    }

    private static void OnIsSnapToStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        if ((bool)e.NewValue)
        {
            slider.CoerceValueToStep();
        }
    }

    #endregion IsSnapToStep

    #region TickFrequency

    public static readonly DependencyProperty TickFrequencyProperty =
        DependencyProperty.Register(nameof(TickFrequency), typeof(double), typeof(RangeSlider),
            new PropertyMetadata(1.0));

    public double TickFrequency
    {
        get => (double)GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    #endregion TickFrequency

    #region Ticks

    public static readonly DependencyProperty TicksProperty =
        DependencyProperty.Register(nameof(Ticks), typeof(DoubleCollection), typeof(RangeSlider),
            new PropertyMetadata(null));

    public DoubleCollection Ticks
    {
        get => (DoubleCollection)GetValue(TicksProperty);
        set => SetValue(TicksProperty, value);
    }

    #endregion Ticks

    #region IsSnapToTick

    public static readonly DependencyProperty IsSnapToTickProperty =
        DependencyProperty.Register(nameof(IsSnapToTick), typeof(bool), typeof(RangeSlider),
            new PropertyMetadata(false, OnIsSnapToTickChanged));

    public bool IsSnapToTick
    {
        get => (bool)GetValue(IsSnapToTickProperty);
        set => SetValue(IsSnapToTickProperty, value);
    }

    private static void OnIsSnapToTickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        if ((bool)e.NewValue)
        {
            slider.CoerceValueToTick();
        }
    }

    // 新增：对齐到最近的刻度线
    private double SnapToTick(double value)
    {
        var tickValues = GetTickValues();
        if (tickValues == null || tickValues.Count == 0)
            return value;

        // 找到最近的刻度线
        var closestTick = tickValues[0];
        var minDistance = Math.Abs(value - closestTick);

        foreach (var tick in tickValues)
        {
            var distance = Math.Abs(value - tick);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTick = tick;
            }
        }

        return closestTick;
    }

    private List<double> GetTickValues()
    {
        var tickValues = new List<double>();

        // 如果设置了自定义刻度线
        if (Ticks != null && Ticks.Count > 0)
        {
            foreach (var tick in Ticks)
            {
                if (tick >= Minimum && tick <= Maximum)
                {
                    tickValues.Add(tick);
                }
            }
        }
        // 如果设置了刻度线频率
        else if (TickFrequency > 0)
        {
            var current = Minimum;
            while (current <= Maximum)
            {
                tickValues.Add(current);
                current += TickFrequency;
            }

            // 确保包含最大值
            if (Math.Abs(tickValues.LastOrDefault() - Maximum) > double.Epsilon)
            {
                tickValues.Add(Maximum);
            }
        }

        return tickValues.OrderBy(x => x).ToList();
    }

    // 新增：强制当前值对齐到刻度线
    private void CoerceValueToTick()
    {
        if (!IsSnapToTick) return;

        _isInternalUpdate = true;
        try
        {
            var newLowerValue = SnapToTick(LowerValue);
            var newUpperValue = SnapToTick(UpperValue);

            if (Math.Abs(LowerValue - newLowerValue) > double.Epsilon)
            {
                LowerValue = newLowerValue;
            }

            if (Math.Abs(UpperValue - newUpperValue) > double.Epsilon)
            {
                UpperValue = newUpperValue;
            }
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    #endregion IsSnapToTick

    #region IsDirectionReversed

    public static readonly DependencyProperty IsDirectionReversedProperty =
        DependencyProperty.Register(nameof(IsDirectionReversed), typeof(bool), typeof(RangeSlider),
            new PropertyMetadata(false));

    public bool IsDirectionReversed
    {
        get => (bool)GetValue(IsDirectionReversedProperty);
        set => SetValue(IsDirectionReversedProperty, value);
    }

    #endregion IsDirectionReversed

    #region Override

    private void UnbindEvents()
    {
        if (_StartRegion != null)
            _StartRegion.Click -= StartRegion_Click;
        if (_MiddleRegion != null)
        {
            _MiddleRegion.PreviewMouseLeftButtonDown -= MiddleRegion_PreviewMouseLeftButtonDown;
            _MiddleRegion.PreviewMouseRightButtonDown -= MiddleRegion_PreviewMouseRightButtonDown;
        }
        if (_EndRegion != null)
            _EndRegion.Click -= EndRegion_Click;
    }

    private void BindEvents()
    {
        if (_StartRegion != null)
            _StartRegion.Click += StartRegion_Click;
        if (_MiddleRegion != null)
        {
            _MiddleRegion.PreviewMouseLeftButtonDown += MiddleRegion_PreviewMouseLeftButtonDown;
            _MiddleRegion.PreviewMouseRightButtonDown += MiddleRegion_PreviewMouseRightButtonDown;
        }
        if (_EndRegion != null)
            _EndRegion.Click += EndRegion_Click;
    }

    private void SetupToolTips()
    {
        if (_StartThumb != null)
        {
            _startThumbToolTip = _StartThumb.ToolTip as ToolTip;
            if (_startThumbToolTip == null && AutoToolTipPlacement != AutoToolTipPlacement.None)
            {
                _startThumbToolTip = new ToolTip
                {
                    Placement = GetToolTipPlacement(),
                    PlacementTarget = _StartThumb
                };
                _StartThumb.ToolTip = _startThumbToolTip;
            }

            if (_startThumbToolTip != null)
            {
                ToolTipService.SetInitialShowDelay(_StartThumb, 0);
                ToolTipService.SetBetweenShowDelay(_StartThumb, 0);
            }
        }

        if (_EndThumb != null)
        {
            _endThumbToolTip = _EndThumb.ToolTip as ToolTip;
            if (_endThumbToolTip == null && AutoToolTipPlacement != AutoToolTipPlacement.None)
            {
                _endThumbToolTip = new ToolTip
                {
                    Placement = GetToolTipPlacement(),
                    PlacementTarget = _EndThumb
                };
                _EndThumb.ToolTip = _endThumbToolTip;
            }

            if (_endThumbToolTip != null)
            {
                ToolTipService.SetInitialShowDelay(_EndThumb, 0);
                ToolTipService.SetBetweenShowDelay(_EndThumb, 0);
            }
        }
    }

    private void ApplyCurrentSettings()
    {
        // 应用 Delay 和 Interval 设置
        if (_StartRegion != null)
        {
            _StartRegion.Delay = Delay;
            _StartRegion.Interval = Interval;
        }
        if (_MiddleRegion != null)
        {
            _MiddleRegion.Delay = Delay;
            _MiddleRegion.Interval = Interval;
        }
        if (_EndRegion != null)
        {
            _EndRegion.Delay = Delay;
            _EndRegion.Interval = Interval;
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 解除之前的事件绑定
        UnbindEvents();

        // 获取模板元素
        _StartThumb = GetTemplateChild(PART_StartThumb) as Thumb;
        _EndThumb = GetTemplateChild(PART_EndThumb) as Thumb;
        _StartRegion = GetTemplateChild(PART_StartRegion) as RepeatButton;
        _MiddleRegion = GetTemplateChild(PART_MiddleRegion) as RepeatButton;
        _EndRegion = GetTemplateChild(PART_EndRegion) as RepeatButton;

        // 设置工具提示
        SetupToolTips();

        // 绑定事件
        BindEvents();

        // 应用当前设置
        ApplyCurrentSettings();
    }

    #endregion Override

    #region Events

    public static readonly RoutedEvent LowerValueChangedEvent =
        EventManager.RegisterRoutedEvent("LowerValueChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>), typeof(RangeSlider));

    public static readonly RoutedEvent UpperValueChangedEvent =
        EventManager.RegisterRoutedEvent("UpperValueChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>), typeof(RangeSlider));

    public event RoutedPropertyChangedEventHandler<double> LowerValueChanged
    {
        add { AddHandler(LowerValueChangedEvent, value); }
        remove { RemoveHandler(LowerValueChangedEvent, value); }
    }

    public event RoutedPropertyChangedEventHandler<double> UpperValueChanged
    {
        add { AddHandler(UpperValueChangedEvent, value); }
        remove { RemoveHandler(UpperValueChangedEvent, value); }
    }

    #endregion Events

    #region Drag Events

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
            UpdateToolTipContent(_startThumbToolTip, LowerValue);
            _startThumbToolTip.IsOpen = true;
        }
        else if (thumb == _EndThumb && _endThumbToolTip != null)
        {
            UpdateToolTipContent(_endThumbToolTip, UpperValue);
            _endThumbToolTip.IsOpen = true;
        }
    }

    private void OnThumbDragDelta(DragDeltaEventArgs e)
    {
        if (!CanUpdate()) return;

        var offset = CalculateDragOffset(e);
        var thumb = e.OriginalSource as Thumb;

        _isInternalUpdate = true;
        try
        {
            if (thumb == _StartThumb)
            {
                var newLowerValue = LowerValue + offset;

                // 优先对齐刻度线，其次对齐步进
                if (IsSnapToTick)
                {
                    newLowerValue = SnapToTick(newLowerValue);
                }
                else if (IsSnapToStep)
                {
                    newLowerValue = RoundToNearest(newLowerValue, Step);
                }

                // 边界检查
                newLowerValue = Math.Max(Minimum, Math.Min(newLowerValue, Math.Min(UpperValue, Maximum)));

                if (Math.Abs(LowerValue - newLowerValue) > double.Epsilon)
                {
                    LowerValue = newLowerValue;
                    UpdateThumbPosition(ThumbKind.Start);
                }

                UpdateToolTipContent(_startThumbToolTip, LowerValue);
            }
            else if (thumb == _EndThumb)
            {
                var newUpperValue = UpperValue + offset;

                // 优先对齐刻度线，其次对齐步进
                if (IsSnapToTick)
                {
                    newUpperValue = SnapToTick(newUpperValue);
                }
                else if (IsSnapToStep)
                {
                    newUpperValue = RoundToNearest(newUpperValue, Step);
                }

                // 边界检查
                newUpperValue = Math.Min(Maximum, Math.Max(newUpperValue, Math.Max(LowerValue, Minimum)));

                if (Math.Abs(UpperValue - newUpperValue) > double.Epsilon)
                {
                    UpperValue = newUpperValue;
                    UpdateThumbPosition(ThumbKind.End);
                }

                UpdateToolTipContent(_endThumbToolTip, UpperValue);
            }
        }
        finally
        {
            _isInternalUpdate = false;
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

    private double CalculateDragOffset(DragDeltaEventArgs e)
    {
        var range = Math.Abs(Maximum - Minimum);

        if (Orientation == Orientation.Horizontal)
        {
            return e.HorizontalChange / ActualWidth * range;
        }
        else
        {
            // 垂直方向：向上为负，向下为正（从下到上递增）
            return -e.VerticalChange / ActualHeight * range;
        }
    }

    #endregion Drag Events

    #region Private Methods

    private bool CanUpdate()
    {
        return _StartThumb != null && _EndThumb != null &&
               _StartRegion != null && _MiddleRegion != null && _EndRegion != null &&
               ActualWidth > 0 && ActualHeight > 0;
    }

    private double RoundToNearest(double value, double step)
    {
        if (step <= 0) return value;

        try
        {
            var stepStr = step.ToString(CultureInfo.InvariantCulture);
            var decimalIndex = stepStr.IndexOf('.');
            var precision = decimalIndex < 0 ? 0 : stepStr.Length - decimalIndex - 1;

            var rounded = Math.Round(value / step) * step;
            return Math.Round(rounded, Math.Max(precision, 10));
        }
        catch
        {
            return value;
        }
    }

    private void CoerceValueToStep()
    {
        if (!IsSnapToStep || Step <= 0) return;

        _isInternalUpdate = true;
        try
        {
            var newLowerValue = RoundToNearest(LowerValue, Step);
            var newUpperValue = RoundToNearest(UpperValue, Step);

            if (Math.Abs(LowerValue - newLowerValue) > double.Epsilon)
            {
                LowerValue = newLowerValue;
            }

            if (Math.Abs(UpperValue - newUpperValue) > double.Epsilon)
            {
                UpperValue = newUpperValue;
            }
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    private void UpdateThumbPosition(ThumbKind thumbKind)
    {
        if (!CanUpdate()) return;

        var total = CalculateTotalSize(Orientation);
        if (total <= 0) return;

        if (Orientation == Orientation.Horizontal)
        {
            switch (thumbKind)
            {
                case ThumbKind.Start:
                    var startScale = MapValueToRange(LowerValue, (Minimum, Maximum), (0d, 1d));
                    _StartRegion.Width = startScale * total;
                    break;

                case ThumbKind.End:
                    var endScale = MapValueToRange(Maximum - UpperValue + Minimum, (Minimum, Maximum), (0d, 1d));
                    _EndRegion.Width = endScale * total;
                    break;
            }
        }
        else
        {
            switch (thumbKind)
            {
                case ThumbKind.Start:
                    var startScale = MapValueToRange(Maximum - LowerValue + Minimum, (Minimum, Maximum), (0d, 1d));
                    _StartRegion.Height = startScale * total;
                    break;

                case ThumbKind.End:
                    var endScale = MapValueToRange(UpperValue, (Minimum, Maximum), (0d, 1d));
                    _EndRegion.Height = endScale * total;
                    break;
            }
        }
    }

    private double MapValueToRange(double value, (double Min, double Max) original, (double Min, double Max) target)
    {
        if (Math.Abs(original.Max - original.Min) < double.Epsilon)
            return target.Min;

        var num = (value - original.Min) / (original.Max - original.Min);
        return target.Min + num * (target.Max - target.Min);
    }

    private double CalculateTotalSize(Orientation orientation, bool includeThumbSize = false)
    {
        switch (orientation)
        {
            case Orientation.Horizontal:
                if (includeThumbSize)
                {
                    return _StartRegion.ActualWidth + _StartThumb.ActualWidth +
                           _MiddleRegion.ActualWidth + _EndThumb.ActualWidth + _EndRegion.ActualWidth;
                }
                else
                {
                    return _StartRegion.ActualWidth + _MiddleRegion.ActualWidth + _EndRegion.ActualWidth;
                }

            case Orientation.Vertical:
                if (includeThumbSize)
                {
                    return _StartRegion.ActualHeight + _StartThumb.ActualHeight +
                           _MiddleRegion.ActualHeight + _EndThumb.ActualHeight + _EndRegion.ActualHeight;
                }
                else
                {
                    return _StartRegion.ActualHeight + _MiddleRegion.ActualHeight + _EndRegion.ActualHeight;
                }

            default:
                return 0;
        }
    }

    private void UpdateToolTipContent(ToolTip toolTip, double value)
    {
        if (toolTip == null) return;

        // 格式化数值精度
        toolTip.Content = value.ToString($"F{AutoToolTipPrecision}");

        // 设置工具提示位置
        toolTip.Placement = GetToolTipPlacement();

        // 强制更新ToolTip位置（恢复原有逻辑）
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

    private void UpdateValueFromPoint(Point pt, ThumbKind thumbKind, MouseButton button)
    {
        var total = CalculateTotalSize(Orientation, true);
        if (total <= 0) return;

        var newValue = 0d;

        if (Orientation == Orientation.Horizontal)
        {
            newValue = Minimum + pt.X / total * (Maximum - Minimum);
        }
        else
        {
            // 垂直方向：从下到上递增
            newValue = Minimum + (total - pt.Y) / total * (Maximum - Minimum);
        }

        // 应用对齐：优先刻度线，其次步进
        if (IsSnapToTick)
        {
            newValue = SnapToTick(newValue);
        }
        else if (IsSnapToStep && Step > 0)
        {
            newValue = RoundToNearest(newValue, Step);
        }

        _isInternalUpdate = true;
        try
        {
            switch (thumbKind)
            {
                case ThumbKind.Start:
                    if (IsMoveToPoint)
                    {
                        LowerValue = Math.Max(Minimum, Math.Min(newValue, UpperValue));
                    }
                    else
                    {
                        // 在非点击移动模式下，也应该考虑刻度线对齐
                        var stepValue = IsSnapToTick ? GetNearestTickStep(LowerValue, button == MouseButton.Left)
                                                     : (button == MouseButton.Left ? Step : -Step);
                        var targetValue = LowerValue + stepValue;
                        LowerValue = Math.Max(Minimum, Math.Min(targetValue, UpperValue));
                    }
                    UpdateThumbPosition(ThumbKind.Start);
                    break;

                case ThumbKind.End:
                    if (IsMoveToPoint)
                    {
                        UpperValue = Math.Min(Maximum, Math.Max(newValue, LowerValue));
                    }
                    else
                    {
                        var stepValue = IsSnapToTick ? GetNearestTickStep(UpperValue, button != MouseButton.Right)
                                                     : (button == MouseButton.Right ? -Step : Step);
                        var targetValue = UpperValue + stepValue;
                        UpperValue = Math.Min(Maximum, Math.Max(targetValue, LowerValue));
                    }
                    UpdateThumbPosition(ThumbKind.End);
                    break;
            }
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    private double GetNearestTickStep(double currentValue, bool forward)
    {
        var tickValues = GetTickValues();
        if (tickValues == null || tickValues.Count == 0)
            return forward ? Step : -Step;

        if (forward)
        {
            // 寻找下一个更大的刻度线
            var nextTick = tickValues.FirstOrDefault(t => t > currentValue + double.Epsilon);
            return nextTick > 0 ? nextTick - currentValue : Step;
        }
        else
        {
            // 寻找前一个更小的刻度线
            var prevTick = tickValues.LastOrDefault(t => t < currentValue - double.Epsilon);
            return prevTick >= Minimum ? prevTick - currentValue : -Step;
        }
    }

    #endregion Private Methods

    #region Event Handlers

    private void RangeSlider_Loaded(object sender, RoutedEventArgs e)
    {
        if (CanUpdate())
        {
            UpdateThumbPosition(ThumbKind.Start);
            UpdateThumbPosition(ThumbKind.End);
        }
    }

    private void StartRegion_Click(object sender, RoutedEventArgs e)
    {
        var position = Mouse.GetPosition(this);
        UpdateValueFromPoint(position, ThumbKind.Start, MouseButton.Middle);
    }

    private void EndRegion_Click(object sender, RoutedEventArgs e)
    {
        var position = Mouse.GetPosition(this);
        UpdateValueFromPoint(position, ThumbKind.End, MouseButton.Middle);
    }

    private void MiddleRegion_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var position = e.GetPosition(this);
        UpdateValueFromPoint(position, ThumbKind.Start, MouseButton.Left);
        e.Handled = true;
    }

    private void MiddleRegion_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var position = e.GetPosition(this);
        UpdateValueFromPoint(position, ThumbKind.End, MouseButton.Right);
        e.Handled = true;
    }

    #endregion Event Handlers
}