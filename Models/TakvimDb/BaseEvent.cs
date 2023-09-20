namespace databaseapi.Models.TakvimDb;

public class BaseEvent
{
    public DateTime Starts { get; set; }
    public DateTime Ends { get; set; }
    public string Desc { get; set; }
    public string Location { get; set; }
}
