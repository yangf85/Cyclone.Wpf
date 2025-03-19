using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

public interface IPredefineDate
{
    string Name { get; }
    DateTime? Start { get; }
    DateTime? End { get; }

    IList<DateTime> BlockoutDates { get; }
}

public class PredefineDate : IPredefineDate
{
    public string Name { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }

    public IList<DateTime> BlockoutDates { get; set; }

    public PredefineDate(string name, DateTime? start, DateTime? end)
    {
        Name = name;
        Start = start;
        End = end;
        BlockoutDates = [];
    }
}

[TemplatePart(Name = PART_OpenButton, Type = typeof(Button))]
[TemplatePart(Name = PART_Calendar, Type = typeof(Calendar))]
[TemplatePart(Name = PART_Container, Type = typeof(Popup))]
[TemplatePart(Name = PART_Root, Type = typeof(Grid))]
[TemplatePart(Name = PART_StartTextBox, Type = typeof(DatePickerTextBox))]
[TemplatePart(Name = PART_EndTextBox, Type = typeof(DatePickerTextBox))]
[TemplatePart(Name = PART_ComboBox, Type = typeof(ComboBox))]
public class DateRangePicker : Control
{
    private const string PART_OpenButton = nameof(PART_OpenButton);
    private const string PART_Calendar = nameof(PART_Calendar);
    private const string PART_Container = nameof(PART_Container);

    private const string PART_Root = nameof(PART_Root);
    private const string PART_StartTextBox = nameof(PART_StartTextBox);
    private const string PART_EndTextBox = nameof(PART_EndTextBox);

    private const string PART_ComboBox = nameof(PART_ComboBox);

    private Button _button;
    private Calendar _calendar;
    private Popup _container;
    private DatePickerTextBox _start;
    private DatePickerTextBox _end;
    private ComboBox _comboBox;

    static DateRangePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DateRangePicker), new FrameworkPropertyMetadata(typeof(DateRangePicker)));
    }

    #region Start

    public DateTime? Start
    {
        get => (DateTime?)GetValue(StartProperty);
        set => SetValue(StartProperty, value);
    }

    public static readonly DependencyProperty StartProperty =
        DependencyProperty.Register(nameof(Start), typeof(DateTime?), typeof(DateRangePicker),
            new FrameworkPropertyMetadata(default(DateTime?), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnStartChanged));

    private static void OnStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (DateRangePicker)d;
        picker.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (e.NewValue != null && picker._calendar != null)
            {
                var date = (DateTime)e.NewValue;
                picker._calendar.SelectedDates.Add(date);
            }
        }), DispatcherPriority.Loaded);
    }

    #endregion Start

    #region End

    public DateTime? End
    {
        get => (DateTime?)GetValue(EndProperty);
        set => SetValue(EndProperty, value);
    }

    public static readonly DependencyProperty EndProperty =
        DependencyProperty.Register(nameof(End), typeof(DateTime?), typeof(DateRangePicker),
            new FrameworkPropertyMetadata(default(DateTime?), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnEndChanged));

    private static void OnEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (DateRangePicker)d;
        picker.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (e.NewValue != null && picker._calendar != null)
            {
                var date = (DateTime)e.NewValue;
                picker._calendar.SelectedDates.Add(date);
            }
        }), DispatcherPriority.Loaded);
    }

    #endregion End

    #region IsShowPredfine

    public bool IsShowPredfine
    {
        get => (bool)GetValue(IsShowPredfineProperty);
        set => SetValue(IsShowPredfineProperty, value);
    }

    public static readonly DependencyProperty IsShowPredfineProperty =
        DependencyProperty.Register(nameof(IsShowPredfine), typeof(bool), typeof(DateRangePicker), new PropertyMetadata(false));

    #endregion IsShowPredfine

    #region PredefineDates

    public IList<IPredefineDate> PredefineDates
    {
        get => (IList<IPredefineDate>)GetValue(PredefineDatesProperty);
        set => SetValue(PredefineDatesProperty, value);
    }

    public static readonly DependencyProperty PredefineDatesProperty =
        DependencyProperty.Register(nameof(PredefineDates), typeof(IList<IPredefineDate>), typeof(DateRangePicker), new PropertyMetadata(default(IList<IPredefineDate>)));

    #endregion PredefineDates

    #region SelectedDates

    public IList<DateTime> SelectedDates
    {
        get => (IList<DateTime>)GetValue(SelectedDatesProperty);
        set => SetValue(SelectedDatesProperty, value);
    }

    public static readonly DependencyProperty SelectedDatesProperty =
        DependencyProperty.Register(nameof(SelectedDates), typeof(IList<DateTime>), typeof(DateRangePicker), new PropertyMetadata(default(IList<DateTime>)));

    #endregion SelectedDates

    #region SelectedDateFormat

    public string SelectedDateFormat
    {
        get => (string)GetValue(SelectedDateFormatProperty);
        set => SetValue(SelectedDateFormatProperty, value);
    }

    public static readonly DependencyProperty SelectedDateFormatProperty =
        DependencyProperty.Register(nameof(SelectedDateFormat), typeof(string), typeof(DateRangePicker), new PropertyMetadata(default(string)));

    #endregion SelectedDateFormat

    #region FirstDayOfWeek

    public DayOfWeek FirstDayOfWeek
    {
        get => (DayOfWeek)GetValue(FirstDayOfWeekProperty);
        set => SetValue(FirstDayOfWeekProperty, value);
    }

    public static readonly DependencyProperty FirstDayOfWeekProperty =
        DependencyProperty.Register(nameof(FirstDayOfWeek), typeof(DayOfWeek), typeof(DateRangePicker), new PropertyMetadata(default(DayOfWeek)));

    #endregion FirstDayOfWeek

    #region IsOpen

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(DateRangePicker), new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion IsOpen

    #region Seperater

    public object Seperater
    {
        get => (object)GetValue(SeperaterProperty);
        set => SetValue(SeperaterProperty, value);
    }

    public static readonly DependencyProperty SeperaterProperty =
        DependencyProperty.Register(nameof(Seperater), typeof(object), typeof(DateRangePicker), new PropertyMetadata("-"));

    #endregion Seperater

    #region BlackoutDates

    public IList BlackoutDates
    {
        get => (IList)GetValue(BlackoutDatesProperty);
        set => SetValue(BlackoutDatesProperty, value);
    }

    public static readonly DependencyProperty BlackoutDatesProperty =
        DependencyProperty.Register(nameof(BlackoutDates), typeof(IList), typeof(DateRangePicker), new PropertyMetadata(default(IList)));

    #endregion BlackoutDates

    #region IsTodayHighlighted

    public bool IsTodayHighlighted
    {
        get => (bool)GetValue(IsTodayHighlightedProperty);
        set => SetValue(IsTodayHighlightedProperty, value);
    }

    public static readonly DependencyProperty IsTodayHighlightedProperty =
        DependencyProperty.Register(nameof(IsTodayHighlighted), typeof(bool), typeof(DateRangePicker), new PropertyMetadata(default(bool)));

    #endregion IsTodayHighlighted

    #region Override

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _button = GetTemplateChild(PART_OpenButton) as Button;
        _calendar = GetTemplateChild(PART_Calendar) as Calendar;
        _container = GetTemplateChild(PART_Container) as Popup;
        _start = GetTemplateChild(PART_StartTextBox) as DatePickerTextBox;
        _end = GetTemplateChild(PART_EndTextBox) as DatePickerTextBox;
        _comboBox = GetTemplateChild(PART_ComboBox) as ComboBox;

        if (_button != null)
        {
            _button.Click -= Button_Click;
            _button.Click += Button_Click;
        }
        if (_calendar != null)
        {
            _calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;
        }
        if (_container != null)
        {
        }
        if (_comboBox != null)
        {
            PredefineDates = GeneratePredefineDates();
            _comboBox.ItemsSource = PredefineDates;
            _comboBox.SelectedIndex = 0;
            _comboBox.DisplayMemberPath = nameof(IPredefineDate.Name);

            _comboBox.SelectionChanged -= ComboBox_SelectionChanged;
            _comboBox.SelectionChanged += ComboBox_SelectionChanged;
        }
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_comboBox == null || _calendar == null) return;

        if (_comboBox.SelectedItem is IPredefineDate predefineDate)
        {
            // 清除之前的黑名单日期
            _calendar.BlackoutDates.Clear();

            // 添加预定义的黑名单日期到Calendar
            foreach (var blockDate in predefineDate.BlockoutDates)
            {
                _calendar.BlackoutDates.Add(new CalendarDateRange(blockDate));
            }

            // 清除现有选中的日期并添加新的日期范围
            _calendar.SelectedDates.Clear();
            if (predefineDate.Start.HasValue && predefineDate.End.HasValue)
            {
                DateTime start = predefineDate.Start.Value;
                DateTime end = predefineDate.End.Value;
                _calendar.SelectedDates.AddRange(start, end);
            }

            // 关闭日期选择弹出框
            IsOpen = false;
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        IsOpen = !IsOpen;
    }

    private void Calendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_calendar == null) { return; }

        var dates = _calendar.SelectedDates;
        var order = dates.Order();

        Start = order.FirstOrDefault();
        End = order.LastOrDefault();
        if (_start != null)
        {
            _start.Text = Start?.ToString(SelectedDateFormat) ?? string.Empty;
        }
        if (_end != null)
        {
            _end.Text = End?.ToString(SelectedDateFormat) ?? string.Empty;
        }
    }

    #endregion Override

    #region Private

    // 预定义范围集合
    private List<IPredefineDate> GeneratePredefineDates()
    {
        DateTime today = DateTime.Today;
        DateTime yesterday = today.AddDays(-1);

        return new List<IPredefineDate>
        {
            // 当天范围
            new PredefineDate("今日", today, today),

            // 近期范围
            new PredefineDate("昨日", yesterday, yesterday),
            new PredefineDate("近3天", today.AddDays(-2), today), // 含今天共3天
            new PredefineDate("近7天", today.AddDays(-6), today),
            new PredefineDate("近15天", today.AddDays(-14), today),

            // 月范围
            new PredefineDate("本月", FirstDayOfMonth(today), today),
            new PredefineDate("上月",
                FirstDayOfMonth(today.AddMonths(-1)),
                LastDayOfMonth(today.AddMonths(-1))),
            new PredefineDate("近3个月", FirstDayOfMonth(today.AddMonths(-2)), today),

            // 年范围
            new PredefineDate("本年", new DateTime(today.Year, 1, 1), today),
            new PredefineDate("去年",
                new DateTime(today.Year - 1, 1, 1),
                new DateTime(today.Year - 1, 12, 31)),

            // 周范围（按中国习惯周一到周日）
            new PredefineDate("本周",
                today.AddDays(-(today.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)today.DayOfWeek - 1)),
                today),

            // 特殊范围
            new PredefineDate("过去三年", today.AddYears(-3).Date, today)
        };

        // 计算月份首日
    }

    private static DateTime FirstDayOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    // 计算月份最后一日
    private static DateTime LastDayOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1)
            .AddMonths(1)
            .AddDays(-1);
    }

    #endregion Private
}