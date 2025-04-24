using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Globalization;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = "PART_OpenButton", Type = typeof(ToggleButton))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[TemplatePart(Name = "PART_HourSelector", Type = typeof(TimeSelector))]
[TemplatePart(Name = "PART_MinuteSelector", Type = typeof(TimeSelector))]
[TemplatePart(Name = "PART_SecondSelector", Type = typeof(TimeSelector))]
[TemplatePart(Name = "PART_ConfirmButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_CancelButton", Type = typeof(Button))]
public class TimePicker : Control
{
    #region 私有字段

    private ToggleButton _openButton;
    private Popup _popup;
    private TimeSelector _hourSelector;
    private TimeSelector _minuteSelector;
    private TimeSelector _secondSelector;
    private Button _confirmButton;
    private Button _cancelButton;

    #endregion 私有字段

    #region 构造函数

    static TimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
    }

    public TimePicker()
    {
        Loaded += TimePicker_Loaded;

        // 设置默认可见项数量为5
        VisibleItemCount = 5;
    }

    private void TimePicker_Loaded(object sender, RoutedEventArgs e)
    {
        // 初始化时间
        if (SelectedTime == null)
        {
            SelectedTime = DateTime.Now.TimeOfDay;
        }
    }

    #endregion 构造函数

    #region 依赖属性

    #region MaxContainerHeight

    public double MaxContainerHeight
    {
        get => (double)GetValue(MaxContainerHeightProperty);
        set => SetValue(MaxContainerHeightProperty, value);
    }

    public static readonly DependencyProperty MaxContainerHeightProperty =
        DependencyProperty.Register(nameof(MaxContainerHeight), typeof(double), typeof(TimePicker), new PropertyMetadata(150d));

    #endregion MaxContainerHeight

    #region SelectedTime

    public TimeSpan? SelectedTime
    {
        get { return (TimeSpan?)GetValue(SelectedTimeProperty); }
        set { SetValue(SelectedTimeProperty, value); }
    }

    public static readonly DependencyProperty SelectedTimeProperty =
        DependencyProperty.Register("SelectedTime", typeof(TimeSpan?), typeof(TimePicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedTimeChanged));

    private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var timePicker = (TimePicker)d;
    }

    #endregion SelectedTime

    #region DisplayText

    public string DisplayText
    {
        get { return (string)GetValue(DisplayTextProperty); }
        set { SetValue(DisplayTextProperty, value); }
    }

    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register("DisplayText", typeof(string), typeof(TimePicker), new PropertyMetadata(string.Empty));

    #endregion DisplayText

    #region TimeFormat

    public string TimeFormat
    {
        get { return (string)GetValue(TimeFormatProperty); }
        set { SetValue(TimeFormatProperty, value); }
    }

    public static readonly DependencyProperty TimeFormatProperty =
        DependencyProperty.Register("TimeFormat", typeof(string), typeof(TimePicker),
            new PropertyMetadata("HH:mm:ss", OnTimeFormatChanged));

    private static void OnTimeFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var timePicker = (TimePicker)d;
    }

    #endregion TimeFormat

    #region IsOpen

    public bool IsOpen
    {
        get { return (bool)GetValue(IsOpenProperty); }
        set { SetValue(IsOpenProperty, value); }
    }

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register("IsOpen", typeof(bool), typeof(TimePicker),
            new PropertyMetadata(false, OnIsOpenChanged));

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var timePicker = (TimePicker)d;
        bool newValue = (bool)e.NewValue;
    }

    #endregion IsOpen

    #region Watermark

    public string Watermark
    {
        get { return (string)GetValue(WatermarkProperty); }
        set { SetValue(WatermarkProperty, value); }
    }

    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.Register("Watermark", typeof(string), typeof(TimePicker),
            new PropertyMetadata("请选择时间"));

    #endregion Watermark

    #region VisibleItemCount

    /// <summary>
    /// 时间选择器中可见项目的数量，必须为奇数
    /// </summary>
    public int VisibleItemCount
    {
        get { return (int)GetValue(VisibleItemCountProperty); }
        set { SetValue(VisibleItemCountProperty, value); }
    }

    public static readonly DependencyProperty VisibleItemCountProperty =
        DependencyProperty.Register("VisibleItemCount", typeof(int), typeof(TimePicker),
            new PropertyMetadata(5, OnVisibleItemCountChanged));

    private static void OnVisibleItemCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var timePicker = (TimePicker)d;
        int value = (int)e.NewValue;

        // 确保值为正奇数
        if (value <= 0 || value % 2 == 0)
        {
            // 如果不是奇数，则取最近的奇数
            int correctedValue = value <= 0 ? 5 : (value % 2 == 0 ? value + 1 : value);
            timePicker.SetCurrentValue(VisibleItemCountProperty, correctedValue);
        }
    }

    #endregion VisibleItemCount

    #endregion 依赖属性

    #region 重写方法

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_confirmButton != null)
        {
            _confirmButton.Click -= ConfirmButton_Click;
        }

        if (_cancelButton != null)
        {
            _cancelButton.Click -= CancelButton_Click;
        }

        // 解除选择器事件
        if (_hourSelector != null)
        {
            _hourSelector.ValueChanged -= TimeSelector_ValueChanged;
        }

        if (_minuteSelector != null)
        {
            _minuteSelector.ValueChanged -= TimeSelector_ValueChanged;
        }

        if (_secondSelector != null)
        {
            _secondSelector.ValueChanged -= TimeSelector_ValueChanged;
        }

        // 获取模板部件
        _openButton = GetTemplateChild("PART_OpenButton") as ToggleButton;
        _popup = GetTemplateChild("PART_Popup") as Popup;
        _hourSelector = GetTemplateChild("PART_HourSelector") as TimeSelector;
        _minuteSelector = GetTemplateChild("PART_MinuteSelector") as TimeSelector;
        _secondSelector = GetTemplateChild("PART_SecondSelector") as TimeSelector;
        _confirmButton = GetTemplateChild("PART_ConfirmButton") as Button;
        _cancelButton = GetTemplateChild("PART_CancelButton") as Button;

        if (_confirmButton != null)
        {
            _confirmButton.Click += ConfirmButton_Click;
        }

        if (_cancelButton != null)
        {
            _cancelButton.Click += CancelButton_Click;
        }

        // 初始化选择器
        if (_hourSelector != null)
        {
            _hourSelector.ValueChanged += TimeSelector_ValueChanged;
            _hourSelector.SelectorType = TimeSelectorType.Hour;
            _hourSelector.VisibleItemCount = VisibleItemCount;
        }

        if (_minuteSelector != null)
        {
            _minuteSelector.ValueChanged += TimeSelector_ValueChanged;
            _minuteSelector.SelectorType = TimeSelectorType.Minute;
            _minuteSelector.VisibleItemCount = VisibleItemCount;
        }

        if (_secondSelector != null)
        {
            _secondSelector.ValueChanged += TimeSelector_ValueChanged;
            _secondSelector.SelectorType = TimeSelectorType.Second;
            _secondSelector.VisibleItemCount = VisibleItemCount;
        }
    }

    #endregion 重写方法

    #region 事件处理

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // 在关闭弹出窗口之前，直接从选择器获取值并立即应用
            if (_hourSelector != null && _minuteSelector != null && _secondSelector != null)
            {
                // 获取当前选择器的值
                int hour = _hourSelector.SelectedTimeValue;
                int minute = _minuteSelector.SelectedTimeValue;
                int second = _secondSelector.SelectedTimeValue;

                // 使用当前选择的值构造新的TimeSpan
                TimeSpan newTime = new TimeSpan(hour, minute, second);

                // 设置最终选择的时间
                SelectedTime = newTime;

                // 确保显示文本被更新 - 使用更高的优先级确保立即更新
                Dispatcher.BeginInvoke(new Action(() =>
                {
                }), DispatcherPriority.Render);
            }
        }
        catch (Exception ex)
        {
            // 记录异常但继续执行
            System.Diagnostics.Debug.WriteLine($"确认时间时出错: {ex.Message}");
        }

        // 关闭弹出窗口
        IsOpen = false;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // 取消操作，不保存临时选择的时间
        IsOpen = false;
    }

    private void TimeSelector_ValueChanged(object sender, TimeValueChangedEventArgs e)
    {
    }

    #endregion 事件处理
}