using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Helpers;

public class DateHelper
{
    #region BlockoutDates

    public static readonly DependencyProperty BlockoutDatesProperty =
                DependencyProperty.RegisterAttached("BlockoutDates", typeof(IList), typeof(DateHelper), new PropertyMetadata(default(IList), OnBlockoutDateChanged));

    private static void OnBlockoutDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DatePicker datePicker)
        {
            UpdateBlockoutDates(datePicker.BlackoutDates, e.NewValue as IList);
        }
        else if (d is Calendar calendar)
        {
            UpdateBlockoutDates(calendar.BlackoutDates, e.NewValue as IList);
        }
        else
        {
            return;
        }
    }

    static void UpdateBlockoutDates(CalendarBlackoutDatesCollection dates, IList newDates)
    {
        if (dates == null || newDates == null) { return; }
        dates.Clear();
        foreach (DateTime day in newDates)
        {
            dates.Add(new CalendarDateRange(day));
        }
    }

    public static IList GetBlockoutDates(DependencyObject obj) => (IList)obj.GetValue(BlockoutDatesProperty);

    public static void SetBlockoutDates(DependencyObject obj, IList value) => obj.SetValue(BlockoutDatesProperty, value);

    #endregion BlockoutDates
}