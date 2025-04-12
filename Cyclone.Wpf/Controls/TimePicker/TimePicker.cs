using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Globalization;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = "PART_PopupButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[TemplatePart(Name = "PART_HourSelector", Type = typeof(TimeSelector))]
[TemplatePart(Name = "PART_MinuteSelector", Type = typeof(TimeSelector))]
[TemplatePart(Name = "PART_SecondSelector", Type = typeof(TimeSelector))]
[TemplatePart(Name = "PART_ConfirmButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_CancelButton", Type = typeof(Button))]
public class TimePicker : Control
{
    #region Private Fields

    private Button _popupButton;
    private Popup _popup;
    private TimeSelector _hourSelector;
    private TimeSelector _minuteSelector;
    private TimeSelector _secondSelector;
    private Button _confirmButton;
    private Button _cancelButton;
    private TimeSpan? _tempSelectedTime;

    #endregion Private Fields

    #region Construction

    static TimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
    }

    public TimePicker()
    {
        Loaded += TimePicker_Loaded;
    }

    private void TimePicker_Loaded(object sender, RoutedEventArgs e)
    {
        // 初始化默认时间
        if (SelectedTime == null && DefaultTime != null)
        {
            SelectedTime = DefaultTime;
        }
        else if (SelectedTime == null)
        {
            SelectedTime = DateTime.Now.TimeOfDay;
        }
    }

    #endregion Construction

    #region Properties

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
        timePicker.UpdateDisplayText();
    }

    #endregion SelectedTime

    #region DefaultTime

    public TimeSpan? DefaultTime
    {
        get { return (TimeSpan?)GetValue(DefaultTimeProperty); }
        set { SetValue(DefaultTimeProperty, value); }
    }

    public static readonly DependencyProperty DefaultTimeProperty =
        DependencyProperty.Register("DefaultTime", typeof(TimeSpan?), typeof(TimePicker), new PropertyMetadata(null));

    #endregion DefaultTime

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
        timePicker.UpdateDisplayText();
    }

    #endregion TimeFormat

    #region ShowSeconds

    public bool ShowSeconds
    {
        get { return (bool)GetValue(ShowSecondsProperty); }
        set { SetValue(ShowSecondsProperty, value); }
    }

    public static readonly DependencyProperty ShowSecondsProperty =
        DependencyProperty.Register("ShowSeconds", typeof(bool), typeof(TimePicker),
            new PropertyMetadata(true, OnShowSecondsChanged));

    private static void OnShowSecondsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var timePicker = (TimePicker)d;
        timePicker.UpdateTimeFormat();
        timePicker.UpdateDisplayText();
    }

    #endregion ShowSeconds

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
        if ((bool)e.NewValue) // 打开
        {
            timePicker._tempSelectedTime = timePicker.SelectedTime;
            timePicker.UpdateTimeSelectors();
        }
    }

    #endregion IsOpen

    #region WatermarkText

    public string WatermarkText
    {
        get { return (string)GetValue(WatermarkTextProperty); }
        set { SetValue(WatermarkTextProperty, value); }
    }

    public static readonly DependencyProperty WatermarkTextProperty =
        DependencyProperty.Register("WatermarkText", typeof(string), typeof(TimePicker),
            new PropertyMetadata("请选择时间"));

    #endregion WatermarkText

    #endregion Properties

    #region Overrides

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 解绑之前的事件
        if (_popupButton != null)
        {
            _popupButton.Click -= PopupButton_Click;
        }

        if (_confirmButton != null)
        {
            _confirmButton.Click -= ConfirmButton_Click;
        }

        if (_cancelButton != null)
        {
            _cancelButton.Click -= CancelButton_Click;
        }

        // 获取模板部件
        _popupButton = GetTemplateChild("PART_PopupButton") as Button;
        _popup = GetTemplateChild("PART_Popup") as Popup;
        _hourSelector = GetTemplateChild("PART_HourSelector") as TimeSelector;
        _minuteSelector = GetTemplateChild("PART_MinuteSelector") as TimeSelector;
        _secondSelector = GetTemplateChild("PART_SecondSelector") as TimeSelector;
        _confirmButton = GetTemplateChild("PART_ConfirmButton") as Button;
        _cancelButton = GetTemplateChild("PART_CancelButton") as Button;

        // 绑定事件
        if (_popupButton != null)
        {
            _popupButton.Click += PopupButton_Click;
        }

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
        }

        if (_minuteSelector != null)
        {
            _minuteSelector.ValueChanged += TimeSelector_ValueChanged;
            _minuteSelector.SelectorType = TimeSelectorType.Minute;
        }

        if (_secondSelector != null)
        {
            _secondSelector.ValueChanged += TimeSelector_ValueChanged;
            _secondSelector.SelectorType = TimeSelectorType.Second;
        }

        // 初始化显示文本
        UpdateDisplayText();
    }

    #endregion Overrides

    #region Event Handlers

    private void PopupButton_Click(object sender, RoutedEventArgs e)
    {
        IsOpen = !IsOpen;
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedTime = _tempSelectedTime;
        IsOpen = false;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        IsOpen = false;
    }

    private void TimeSelector_ValueChanged(object sender, TimeValueChangedEventArgs e)
    {
        UpdateTempSelectedTime();
    }

    #endregion Event Handlers

    #region Methods

    private void UpdateDisplayText()
    {
        if (SelectedTime.HasValue)
        {
            // 创建一个表示今天的DateTime，并将其时间部分设置为SelectedTime
            DateTime dateTime = DateTime.Today.Add(SelectedTime.Value);
            // 使用DateTime的ToString方法格式化时间
            DisplayText = dateTime.ToString(TimeFormat);
        }
        else
        {
            DisplayText = string.Empty;
        }
    }

    private void UpdateTimeFormat()
    {
        TimeFormat = ShowSeconds ? "HH:mm:ss" : "HH:mm";
    }

    private void UpdateTimeSelectors()
    {
        if (_hourSelector == null || _minuteSelector == null || _secondSelector == null)
            return;

        if (_tempSelectedTime.HasValue)
        {
            _hourSelector.SelectedValue = _tempSelectedTime.Value.Hours;
            _minuteSelector.SelectedValue = _tempSelectedTime.Value.Minutes;
            _secondSelector.SelectedValue = _tempSelectedTime.Value.Seconds;
        }
        else
        {
            // 默认当前时间
            var now = DateTime.Now;
            _hourSelector.SelectedValue = now.Hour;
            _minuteSelector.SelectedValue = now.Minute;
            _secondSelector.SelectedValue = now.Second;
        }
    }

    private void UpdateTempSelectedTime()
    {
        if (_hourSelector == null || _minuteSelector == null || _secondSelector == null)
            return;

        int hour = _hourSelector.SelectedValue;
        int minute = _minuteSelector.SelectedValue;
        int second = _secondSelector.SelectedValue;

        _tempSelectedTime = new TimeSpan(hour, minute, second);
    }

    #endregion Methods
}