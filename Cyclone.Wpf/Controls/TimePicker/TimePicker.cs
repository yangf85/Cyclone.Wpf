using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Globalization;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 时间选择器控件，集成了小时、分钟和秒的选择
/// </summary>
[TemplatePart(Name = "PART_OpenButton", Type = typeof(ToggleButton))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[TemplatePart(Name = "PART_HourSelector", Type = typeof(TimeSelector))]
[TemplatePart(Name = "PART_MinuteSelector", Type = typeof(TimeSelector))]
[TemplatePart(Name = "PART_SecondSelector", Type = typeof(TimeSelector))]
[TemplatePart(Name = "PART_ConfirmButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_CancelButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_DisplayText", Type = typeof(TextBox))]
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
    private TextBox _displayText;
    private TimeSpan? _tempSelectedTime; // 临时存储用户选择但未确认的时间
    private bool _isSyncingSelectors = false; // 是否正在同步选择器
    private bool _isPopupClosing = false; // 标记弹窗是否正在关闭

    #endregion 私有字段

    #region 构造函数

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
        if (SelectedTime == null)
        {
            SelectedTime = DateTime.Now.TimeOfDay;
        }
        else
        {
            UpdateDisplayText();
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
        timePicker.UpdateDisplayText();

        if (timePicker.IsOpen && !timePicker._isPopupClosing)
        {
            timePicker.SyncSelectorsWithTime();
        }
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
        timePicker.SyncSelectorsWithTime();
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

    public int VisibleItemCount
    {
        get { return (int)GetValue(VisibleItemCountProperty); }
        set { SetValue(VisibleItemCountProperty, value); }
    }

    public static readonly DependencyProperty VisibleItemCountProperty =
        DependencyProperty.Register("VisibleItemCount", typeof(int), typeof(TimePicker),
            new PropertyMetadata(5));

    #endregion VisibleItemCount

    #endregion 依赖属性

    #region 私有方法

    private void UpdateDisplayText()
    {
        if (SelectedTime.HasValue)
        {
            try
            {
                TimeSpan time = SelectedTime.Value;
                DateTime dateTime = DateTime.Today.Add(time);
                DisplayText = dateTime.ToString(TimeFormat);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"时间格式化错误: {ex.Message}");
                DisplayText = SelectedTime.Value.ToString();
            }
        }
        else
        {
            DisplayText = string.Empty;
        }
    }

    private void SyncSelectorsWithTime()
    {
        if (_hourSelector == null || _minuteSelector == null || _secondSelector == null) { return; }
        if (SelectedTime == null) { return; }
        _hourSelector.SelectedIndex = SelectedTime.Value.Hours;
        _minuteSelector.SelectedIndex = SelectedTime.Value.Minutes;
        _secondSelector.SelectedIndex = SelectedTime.Value.Seconds;
    }

    /// <summary>
    /// 从选择器获取当前选中的时间
    /// </summary>
    private TimeSpan GetTimeFromSelectors()
    {
        // 如果选择器未初始化，则返回零时间
        if (_hourSelector == null || _minuteSelector == null || _secondSelector == null)
            return TimeSpan.Zero;

        try
        {
            // 从选择器获取值并构造TimeSpan
            int hour = _hourSelector.SelectedTimeValue;
            int minute = _minuteSelector.SelectedTimeValue;
            int second = _secondSelector.SelectedTimeValue;

            return new TimeSpan(hour, minute, second);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"从选择器获取时间错误: {ex.Message}");
            return TimeSpan.Zero;
        }
    }

    #endregion 私有方法

    #region 重写方法

    /// <summary>
    /// 应用模板时获取必要的模板部件
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        UnsubscribeEvents();

        _openButton = GetTemplateChild("PART_OpenButton") as ToggleButton;
        _popup = GetTemplateChild("PART_Popup") as Popup;
        _hourSelector = GetTemplateChild("PART_HourSelector") as TimeSelector;
        _minuteSelector = GetTemplateChild("PART_MinuteSelector") as TimeSelector;
        _secondSelector = GetTemplateChild("PART_SecondSelector") as TimeSelector;
        _confirmButton = GetTemplateChild("PART_ConfirmButton") as Button;
        _cancelButton = GetTemplateChild("PART_CancelButton") as Button;
        _displayText = GetTemplateChild("PART_DisplayText") as TextBox;

        SubscribeEvents();

        UpdateDisplayText();
    }

    /// <summary>
    /// 解除事件绑定
    /// </summary>
    private void UnsubscribeEvents()
    {
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

        if (_popup != null)
        {
            _popup.Closed -= Popup_Closed;
        }
    }

    /// <summary>
    /// 绑定事件
    /// </summary>
    private void SubscribeEvents()
    {
        if (_confirmButton != null)
        {
            _confirmButton.Click += ConfirmButton_Click;
        }

        if (_cancelButton != null)
        {
            _cancelButton.Click += CancelButton_Click;
        }

        // 绑定选择器事件
        if (_hourSelector != null)
        {
            _hourSelector.ValueChanged += TimeSelector_ValueChanged;
        }

        if (_minuteSelector != null)
        {
            _minuteSelector.ValueChanged += TimeSelector_ValueChanged;
        }

        if (_secondSelector != null)
        {
            _secondSelector.ValueChanged += TimeSelector_ValueChanged;
        }

        if (_popup != null)
        {
            _popup.Closed += Popup_Closed;
        }
    }

    #endregion 重写方法

    #region 事件处理

    /// <summary>
    /// 确认按钮点击事件处理
    /// </summary>
    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // 从选择器获取时间值
            TimeSpan newTime = GetTimeFromSelectors();

            // 设置最终选择的时间
            SelectedTime = newTime;

            // 清除临时选中的时间
            _tempSelectedTime = null;
        }
        catch (Exception ex)
        {
            // 记录异常但继续执行
            System.Diagnostics.Debug.WriteLine($"确认时间时出错: {ex.Message}");
        }

        // 关闭弹出窗口
        IsOpen = false;
    }

    /// <summary>
    /// 取消按钮点击事件处理
    /// </summary>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // 取消操作，不保存临时选择的时间
        _tempSelectedTime = null;
        IsOpen = false;
    }

    /// <summary>
    /// Popup关闭事件处理
    /// </summary>
    private void Popup_Closed(object sender, EventArgs e)
    {
        // 确保IsOpen状态与Popup状态同步
        if (IsOpen && _popup != null && !_popup.IsOpen)
        {
            IsOpen = false;
        }
    }

    /// <summary>
    /// 时间选择器值变化事件处理
    /// </summary>
    private void TimeSelector_ValueChanged(object sender, TimeValueChangedEventArgs e)
    {
        // 避免在同步选择器过程中处理值变化事件
        if (_isSyncingSelectors)
            return;

        // 当选择器的值发生变化时，更新临时选中的时间
        _tempSelectedTime = GetTimeFromSelectors();
    }

    #endregion 事件处理
}