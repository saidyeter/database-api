using Dapper;
using Dapper.Contrib.Extensions;
using databaseapi.Models.TakvimDb;
using Microsoft.Data.Sqlite;
using System.Data;

namespace databaseapi.data_access;

public class TakvimDb
{
    private readonly IDbConnection db;
    public TakvimDb()
    {

        var filename = nameof(TakvimDb).GetEncodedName();
        var connectionString = $"Data Source={filename}.bin;";
        db = new SqliteConnection(connectionString);
        var task = Init();
        task.Wait();
    }
    public async Task Init()
    {
        await db.ExecuteAsync(Event.Creation());
    }
    public async Task Truncate()
    {
        await db.ExecuteAsync(Event.Drop());
    }

    #region Event
    public async Task<GetEventDTO[]> GetMonthlyEventsAsync(DateTime starts, DateTime ends)
    {
        var transactions = await db.QueryAsync<Event>($@"
SELECT * 
FROM [{nameof(Event)}s]
WHERE 
    ([{nameof(Event.Starts)}] > @starts AND  [{nameof(Event.Starts)}] < @ends) OR
    ([{nameof(Event.Ends)}] > @starts AND  [{nameof(Event.Ends)}] < @ends)
ORDER BY [{nameof(Event.Starts)}] ASC;
", new { starts, ends });
        return transactions.Select(val => new GetEventDTO
        {
            Id = val.Id,
            CreationDate = val.CreationDate,
            Desc = val.Desc,
            Ends = val.Ends,
            Starts = val.Starts,
            Location = val.Location,
            DayColor = string.Empty
        }).ToArray();
    }

    public async Task<GetEventDTO> GetEventAsync(int id)
    {
        var val = await db.GetAsync<Event>(id) ??
            throw new Exception("Event not found");

        return new GetEventDTO
        {
            Id = val.Id,
            CreationDate = val.CreationDate,
            Desc = val.Desc,
            Ends = val.Ends,
            Starts = val.Starts,
            Location = val.Location,
        };
    }

    public async Task<int> AddEventAsync(AddEventDTO val)
    {
        var result = await db.InsertAsync(new Event
        {

            CreationDate = DateTime.Now,
            Desc = val.Desc,
            Ends = val.Ends,
            Starts = val.Starts,
            Location = val.Location,

        });
        return result;
    }

    public async Task<bool> UpdateEventAsync(int id, UpdateEventDTO val)
    {
        var rec = await db.GetAsync<Event>(id) ??
            throw new Exception("Event not found");
        rec.Desc = val.Desc;
        rec.Ends = val.Ends;
        rec.Starts = val.Starts;
        rec.Location = val.Location;
        var result = await db.UpdateAsync(rec);
        return result;
    }

    public async Task<bool> RemoveEventAsync(int id)
    {
        var rec = await db.GetAsync<Event>(id);
        if (rec is null)
        {
            return true;
        }
        var result = await db.DeleteAsync(rec);
        return result;
    }

    #endregion
}