namespace databaseapi.Models.CananDb;

public class Transaction : BaseTransaction
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public static string Creation()
    {
        return @$"
CREATE TABLE IF NOT EXISTS [{nameof(Transaction)}s] (
	[{nameof(Id)}] INTEGER PRIMARY KEY,
    [{nameof(CustomerId)}] INTEGER  NULL,
    [{nameof(Date)}] TEXT NOT NULL,
    [{nameof(Amount)}] REAL NOT NULL,
    [{nameof(Type)}] TEXT NOT NULL,
    [{nameof(Note)}] TEXT NULL
);
";
    }
    public static string Drop()
    {
        return @$"DROP TABLE IF EXISTS [{nameof(Transaction)}s]";
    }

    public GetTransactionDTO AsGetTransactionDTO()
    {
        return new GetTransactionDTO
        {
            Date = this.Date,
            Amount = this.Amount,
            Id = this.Id,
            Note = this.Note,
            Type = this.Type,
        };
    }
}
