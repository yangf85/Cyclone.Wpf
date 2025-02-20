using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Cyclone.Wpf.Converters;

public class BrushConverter
{
    public static FuncValueConverter<bool, Brush> BooleanToBrushConverter { get; } =
        new(b => b ? Brushes.Green : Brushes.Red);

    public static FuncValueConverter<int, Brush> IntToBrushConverter { get; } =
        new(i => i switch
        {
            -1 => Brushes.Yellow,
            < 1 => Brushes.Red,
            _ => Brushes.Green
        });

    public static FuncValueConverter<CalendarDayButton, Brush> WeekendDate { get; } =
        new(calendarDayButton =>
        {
            var dateTime = (DateTime)calendarDayButton.DataContext;
            if (!calendarDayButton.IsMouseOver &&
                !calendarDayButton.IsSelected &&
                !calendarDayButton.IsBlackedOut &&
                (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday))
            {
                return new SolidColorBrush(Color.FromArgb(255, 255, 47, 47)); // 周末颜色
            }
            else
            {
                return new SolidColorBrush(Color.FromArgb(255, 51, 51, 51)); // 默认颜色
            }
        });
}