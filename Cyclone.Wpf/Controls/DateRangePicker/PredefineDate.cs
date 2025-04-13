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

public class PredefineDateGenerator
{
    #region Private

    public static List<IPredefineDate> Generate()
    {
        var today = DateTime.Today;

        // 辅助方法：获取指定日期的月份第一天
        DateTime GetFirstDayOfMonth(DateTime date) => new DateTime(date.Year, date.Month, 1);

        // 辅助方法：获取指定日期的月份最后一天
        DateTime GetLastDayOfMonth(DateTime date) => GetFirstDayOfMonth(date).AddMonths(1).AddDays(-1);

        // 辅助方法：获取当前周的周一（中国习惯周一到周日）
        DateTime GetStartOfWeek(DateTime date)
        {
            DateTime startOfWeek = date;
            while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                startOfWeek = startOfWeek.AddDays(-1);
            }
            return startOfWeek;
        }

        // 获取当前周的周日
        DateTime GetEndOfWeek(DateTime date) => GetStartOfWeek(date).AddDays(6);

        // 获取上周的周一和周日
        DateTime GetStartOfPreviousWeek(DateTime date) => GetStartOfWeek(date).AddDays(-7);
        DateTime GetEndOfPreviousWeek(DateTime date) => GetEndOfWeek(date).AddDays(-7);

        // 获取下周的周一和周日
        DateTime GetStartOfNextWeek(DateTime date) => GetStartOfWeek(date).AddDays(7);
        DateTime GetEndOfNextWeek(DateTime date) => GetEndOfWeek(date).AddDays(7);

        return
        [
            // 当天范围
            new PredefineDate("今天", today, today),

            // 近期范围
            new PredefineDate("昨天", today.AddDays(-1), today.AddDays(-1)),
            new PredefineDate("近3天", today.AddDays(-2), today),
            new PredefineDate("近7天", today.AddDays(-6), today),
            new PredefineDate("近15天", today.AddDays(-14), today),

            // 周范围（周一到周日）
            new PredefineDate("本周", GetStartOfWeek(today), GetEndOfWeek(today)),
            new PredefineDate("上周", GetStartOfPreviousWeek(today), GetEndOfPreviousWeek(today)),
            new PredefineDate("下周", GetStartOfNextWeek(today), GetEndOfNextWeek(today)),

            // 月范围
            new PredefineDate("本月", GetFirstDayOfMonth(today), today),
            new PredefineDate("上月", GetFirstDayOfMonth(today.AddMonths(-1)), GetLastDayOfMonth(today.AddMonths(-1))),
            new PredefineDate("下月", GetFirstDayOfMonth(today.AddMonths(1)), GetLastDayOfMonth(today.AddMonths(1))),

            // 近期月范围
            new PredefineDate("近3个月", GetFirstDayOfMonth(today.AddMonths(-3)), today),
            new PredefineDate("前3个月", GetFirstDayOfMonth(today.AddMonths(-3)), GetLastDayOfMonth(today.AddMonths(-3))),
            new PredefineDate("前6个月", GetFirstDayOfMonth(today.AddMonths(-6)), GetLastDayOfMonth(today.AddMonths(-6))),

            // 年范围
            new PredefineDate("本年", new DateTime(today.Year, 1, 1), today),
            new PredefineDate("去年", new DateTime(today.Year - 1, 1, 1), new DateTime(today.Year - 1, 12, 31)),
            new PredefineDate("前年", new DateTime(today.Year - 2, 1, 1), new DateTime(today.Year - 2, 12, 31)),
        ];
    }

    #endregion Private
}