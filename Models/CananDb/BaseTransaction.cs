namespace databaseapi.Models.CananDb;

public class BaseTransaction
{
    public string Date { get; set; }
    public string Type { get; set; }
    public decimal Amount { get; set; }
    public string Note { get; set; }
}
