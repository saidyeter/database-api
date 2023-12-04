namespace databaseapi.Models.TakvimDb;

public class GetEventDTO : BaseEvent
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; }
    public string DayColor { get; set; }
}
