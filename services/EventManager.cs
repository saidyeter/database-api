using databaseapi.data_access;

namespace databaseapi.services;
public record DayInfo(string dayColor, DateTime day, List<string> info);

public record GetMonthlyEventsResult(Models.TakvimDb.GetEventDTO[] events, List<List<DayInfo>> weeks, GetMonthsResult months);

public class EventManager
{
    private readonly TakvimDb db;

    public EventManager(TakvimDb db)
    {
        this.db = db;
    }

    public async Task<GetMonthlyEventsResult> GetMonthlyEvents(int year, int month)
    {
        var b = Dater.GetMonthBoundaries(year, month);
        var monthEnds = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);

        var starts = new DateTime(year, month, 1);
        var ends = new DateTime(year, month, 1).AddMonths(1);


        var oneWeek = 7;
        var sixWeeks = 6 * oneWeek;
        var daysForOtherMonths = sixWeeks - monthEnds.Day;
        var daysForPreviousMonth = b.startWeekDay - 1;
        var daysForNextMonth = daysForOtherMonths - daysForPreviousMonth;

        if (b.startWeekDay == 1 && b.monthLength == 28)
        {
            starts = starts.AddDays(-oneWeek);
            ends = ends.AddDays(oneWeek);
        }
        else if (b.startWeekDay > 6 && b.monthLength == 30)
        {
            starts = starts.AddDays(-daysForPreviousMonth);
            ends = ends.AddDays(+daysForNextMonth);
        }
        else if (b.startWeekDay > 5 && b.monthLength == 31)
        {
            starts = starts.AddDays(-daysForPreviousMonth);
            ends = ends.AddDays(+daysForNextMonth);
        }
        else
        {
            starts = starts.AddDays(-daysForPreviousMonth);
            ends = ends.AddDays(+daysForNextMonth);
        }

        // var coloredEvents= new List<SingleColoredEvent>();
        var events = await db.GetMonthlyEventsAsync(starts, ends);
        var weeks = new List<List<DayInfo>>();
        var week = new List<DayInfo>();
        for (
            var day = starts;
            day < ends;
            day = day.AddDays(1)
        )
        {
            var currentDayStarting = new DateTime(day.Year, day.Month, day.Day, 0, 0, 0);
            var currentDayEnding = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59);
            var dayinfo = new DayInfo(day.Month != month ? "grey" : "white", currentDayStarting, new List<string>());
            foreach (var item in events.Where((e) => e.Starts <= currentDayEnding && e.Ends >= currentDayStarting))
            {

                var color = pickColor(item.Id);
                dayinfo.info.Add(color);
                item.DayColor = color;
            }
            week.Add(dayinfo);
            if (day.DayOfWeek == DayOfWeek.Sunday)
            {
                weeks.Add(week);
                week = new();
            }
        }

        if (week.Count > 0)
        {
            weeks.Add(week);
        }
        return new GetMonthlyEventsResult(events, weeks, Dater.GetMonths(year, month));
    }



    private static string[] colors = new[]{
    "#A6D0DD",
    "#FF6969",
    "#FFD3B0",
    "#E8A0BF",
    "#BA90C6",
    "#C7E9B0",
    "#CCD5AE",
    "#FFB4B4",
    "#FFACAC",
    "#FFAACF",
    "#FFCEFE",
    "#F8CBA6",
    "#CDE990",
    "#8DCBE6",
    "#FD8A8A",
    "#BCEAD5",
    "#9ED5C5",
    "#BCCEF8",
    "#ABD9FF",
    "#FFABE1",
    "#B1D7B4",
    "#F7ECDE",
    "#B2C8DF",
    "#C4D7E0",
    "#C7D36F",
    "#E0DECA",
    "#CDC2AE",
    "#92B4EC",
};

    private string pickColor(int id)
    {
        var colorIndex = id % colors.Length;
        return colors[colorIndex];
    }
}
