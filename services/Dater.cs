namespace databaseapi.services;

public record GetMonthBoundariesResult(int startWeekDay, int monthLength);
public record MonthInfo(int month, int year, string name);
public record GetMonthsResult(MonthInfo selectedDate, MonthInfo previousDate, MonthInfo nextDate);
public static class Dater
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month">first month is 1 last is 12</param>
    /// <returns></returns>
    public static GetMonthBoundariesResult GetMonthBoundaries(int year, int month)
    {
        var date = new DateTime(year, month, 1);
        int weekDay = (int)date.DayOfWeek;
        weekDay = weekDay == 0 ? 7 : weekDay;
        date = date.AddMonths(1).AddDays(-1);
        var monthLength = date.Day;

        return new GetMonthBoundariesResult(weekDay, monthLength);
    }
    private static readonly string[] months = new[] { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık", };
    private static string GetMonth(int month)
    {
        if (month > months.Length || month <= 0)
        {
            return string.Empty;
        }
        return months[month - 1];
    }
    public static GetMonthsResult GetMonths(int year, int month)
    {
        var date = new DateTime(year, month, 1);
        var selectedDate = new MonthInfo(date.Month, date.Year, GetMonth(date.Month));
        var nextDateTime= date.AddMonths(1);
        var nextDate = new MonthInfo(nextDateTime.Month, nextDateTime.Year, GetMonth(nextDateTime.Month));
        var prevDateTime= date.AddMonths(-1);
        var prevDate = new MonthInfo(prevDateTime.Month, prevDateTime.Year, GetMonth(prevDateTime.Month));
        return new GetMonthsResult(selectedDate, prevDate, nextDate);
    }
}

