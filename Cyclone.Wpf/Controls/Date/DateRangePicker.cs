using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = "PART_OpenButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_Calendar", Type = typeof(Calendar))]
[TemplatePart(Name = "PART_CalendarContainer", Type = typeof(Popup))]
[TemplatePart(Name = "PART_Root", Type = typeof(Grid))]
[TemplatePart(Name = "PART_FromTextBox", Type = typeof(DatePickerTextBox))]
[TemplatePart(Name = "PART_ToTextBox", Type = typeof(DatePickerTextBox))]
public class DateRangePicker : Control
{
    private const string PART_OpenButton = nameof(PART_OpenButton);
    private const string PART_Calendar = nameof(PART_Calendar);
    private const string PART_CalendarContainer = nameof(PART_CalendarContainer);

    private const string PART_Root = nameof(PART_Root);
    private const string PART_FromTextBox = nameof(PART_FromTextBox);
    private const string PART_ToTextBox = nameof(PART_ToTextBox);

    private Calendar _calendar;
    private Popup _container;
    private DatePickerTextBox _from;
    private DatePickerTextBox _to;

    static DateRangePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DateRangePicker), new FrameworkPropertyMetadata(typeof(DateRangePicker)));
    }

    #region From

    public DateTime? From
    {
        get => (DateTime?)GetValue(FromProperty);
        set => SetValue(FromProperty, value);
    }

    public static readonly DependencyProperty FromProperty =
        DependencyProperty.Register(nameof(From), typeof(DateTime?), typeof(DateRangePicker), new PropertyMetadata(default(DateTime?)));

    #endregion From

    #region To

    public DateTime? To
    {
        get => (DateTime?)GetValue(ToProperty);
        set => SetValue(ToProperty, value);
    }

    public static readonly DependencyProperty ToProperty =
        DependencyProperty.Register(nameof(To), typeof(DateTime?), typeof(DateRangePicker), new PropertyMetadata(default(DateTime?)));

    #endregion To

    #region SelectedDates

    public SelectedDatesCollection SelectedDates
    {
        get => (SelectedDatesCollection)GetValue(SelectedDatesProperty);
        private set => SetValue(SelectedDatesProperty, value);
    }

    public static readonly DependencyProperty SelectedDatesProperty =
        DependencyProperty.Register(nameof(SelectedDatesCollection), typeof(SelectedDatesCollection), typeof(DateRangePicker), new PropertyMetadata(default(SelectedDatesCollection)));

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
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(DateRangePicker), new PropertyMetadata(default(bool)));

    #endregion IsOpen

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
        _calendar = GetTemplateChild(PART_Calendar) as Calendar;
        _container = GetTemplateChild(PART_CalendarContainer) as Popup;
        _from = GetTemplateChild(PART_FromTextBox) as DatePickerTextBox;
        _to = GetTemplateChild(PART_ToTextBox) as DatePickerTextBox;

        if (_calendar != null)
        {
            _calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;
        }
    }

    private void Calendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
    }

    #endregion Override
}