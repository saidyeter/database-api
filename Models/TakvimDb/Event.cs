using databaseapi.Models.CananDb;

namespace databaseapi.Models.TakvimDb;

public class Event : BaseEvent
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; }

    public static string Creation()
    {
        return @$"
CREATE TABLE IF NOT EXISTS [{nameof(Event)}s] (
	[{nameof(Id)}] INTEGER PRIMARY KEY,
    [{nameof(CreationDate)}] TEXT NOT NULL,
    [{nameof(Starts)}] TEXT NOT NULL,
    [{nameof(Ends)}] TEXT NOT NULL,
    [{nameof(Desc)}] TEXT NOT NULL,
    [{nameof(Location)}] TEXT NULL
);
";
    }
    public static string Drop()
    {
        return @$"DROP TABLE IF EXISTS [{nameof(Event)}s]";
    }
}
