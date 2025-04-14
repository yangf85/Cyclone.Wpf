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
[TemplatePart(Name = "PART_CenterMask", Type = typeof(FrameworkElement))]
public class TimePicker : Control
{
    #region Private Fields

    private ToggleButton _openButton;
    private Popup _popup;
    private TimeSelector _hourSelector;
    private TimeSelector _minuteSelector;
    private TimeSelector _secondSelector;
    private Button _confirmButton;
    private Button _cancelButton;
    private FrameworkElement _centerMask;
    private TimeSpan? _tempSelectedTime;
    private bool _isInternalUpdate;
    private bool _isClosingPopup;

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
        // 初始化时间
        if (SelectedTime == null)
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

        if (newValue) // 打开
        {
            timePicker._isClosingPopup = false;

            // 保存当前选中的时间作为临时时间
            timePicker._tempSelectedTime = timePicker.SelectedTime;

            // 确保在UI更新后再更新选择器
            timePicker.Dispatcher.BeginInvoke(new Action(() =>
            {
                timePicker.UpdateTimeSelectors();
            }), DispatcherPriority.Loaded);
        }
        else // 关闭
        {
            timePicker._isClosingPopup = true;

            // 在下一个UI周期后重置标志
            timePicker.Dispatcher.BeginInvoke(new Action(() =>
            {
                timePicker._isClosingPopup = false;
            }), DispatcherPriority.Loaded);
        }
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

    #region SecondsVisible

    public bool SecondsVisible
    {
        get { return (bool)GetValue(SecondsVisibleProperty); }
        set { SetValue(SecondsVisibleProperty, value); }
    }

    public static readonly DependencyProperty SecondsVisibleProperty =
        DependencyProperty.Register("SecondsVisible", typeof(bool), typeof(TimePicker),
            new PropertyMetadata(true));

    #endregion SecondsVisible

    #endregion Properties

    #region Overrides

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
        _centerMask = GetTemplateChild("PART_CenterMask") as FrameworkElement;

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

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // 在关闭弹出窗口之前，直接从选择器获取值并立即应用
            if (_hourSelector != null && _minuteSelector != null && _secondSelector != null)
            {
                // 获取当前选择器的值
                int hour = _hourSelector.SelectedValue;
                int minute = _minuteSelector.SelectedValue;
                int second = _secondSelector.SelectedValue;

                // 使用当前选择的值构造新的TimeSpan
                TimeSpan newTime = new TimeSpan(hour, minute, second);

                // 设置最终选择的时间
                SelectedTime = newTime;

                // 确保显示文本被更新 - 使用更高的优先级确保立即更新
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateDisplayText();
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
        if (_isInternalUpdate || _isClosingPopup)
        {
            return;
        }

        // 立即更新临时时间
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

    private void UpdateTimeSelectors()
    {
        if (_hourSelector == null || _minuteSelector == null || _secondSelector == null)
        {
            return;
        }

        _isInternalUpdate = true;
        try
        {
            if (_tempSelectedTime.HasValue)
            {
                // 更新选择器的值
                _hourSelector.ForceScrollToValueAndSelect(_tempSelectedTime.Value.Hours);
                _minuteSelector.ForceScrollToValueAndSelect(_tempSelectedTime.Value.Minutes);
                _secondSelector.ForceScrollToValueAndSelect(_tempSelectedTime.Value.Seconds);
            }
            else
            {
                // 默认当前时间
                var now = DateTime.Now;
                _hourSelector.ForceScrollToValueAndSelect(now.Hour);
                _minuteSelector.ForceScrollToValueAndSelect(now.Minute);
                _secondSelector.ForceScrollToValueAndSelect(now.Second);

                // 更新临时选择的时间
                _tempSelectedTime = new TimeSpan(now.Hour, now.Minute, now.Second);
            }
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    private void UpdateTempSelectedTime()
    {
        if (_hourSelector == null || _minuteSelector == null || _secondSelector == null)
        {
            return;
        }

        if (_isInternalUpdate || _isClosingPopup)
        {
            return;
        }

        int hour = _hourSelector.SelectedValue;
        int minute = _minuteSelector.SelectedValue;
        int second = _secondSelector.SelectedValue;

        // 更新临时选择的时间
        _tempSelectedTime = new TimeSpan(hour, minute, second);
    }

    #endregion Methods
}