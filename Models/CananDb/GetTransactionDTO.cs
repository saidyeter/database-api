namespace databaseapi.Models.CananDb;

public class GetTransactionDTO: BaseTransaction
{
    public int Id { get; set; }
    public new string Type { get; set; }
    public new string Date { get; set; }
}
