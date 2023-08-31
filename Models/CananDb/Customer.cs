namespace databaseapi.Models.CananDb;

public class Customer : BaseCustomer
{
    public string CreationDate { get; set; }
    public int Id { get; set; }
    public static string Creation()
    {
        return @$"
CREATE TABLE IF NOT EXISTS {nameof(Customer)}s (
	[{nameof(Id)}] INTEGER PRIMARY KEY,
    [{nameof(FirstName)}] TEXT NOT NULL,
    [{nameof(LastName)}] TEXT NOT NULL,
    [{nameof(CreationDate)}] TEXT NOT NULL,
    [{nameof(PhoneNumber)}] TEXT NOT NULL,
    [{nameof(Note)}] TEXT NULL
);
";
    }
    public static string Drop()
    {
        return @$"DROP TABLE IF EXISTS [{nameof(Customer)}s]";
    }
    public GetCustomerDTO AsGetCustomerDTO()
    {
        return new GetCustomerDTO
        {
            Id = this.Id,
            Note = this.Note,
            CreationDate = this.CreationDate,
            FirstName = this.FirstName,
            LastName = this.LastName,
            PhoneNumber = this.PhoneNumber,
        };
    }
}
