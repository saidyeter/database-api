using Dapper;
using Dapper.Contrib.Extensions;
using databaseapi.Models.CananDb;
using Microsoft.Data.Sqlite;
using System.Data;

namespace databaseapi.data_access;

public class CananDb
{
    private readonly IDbConnection db;
    public CananDb()
    {

        var filename = nameof(CananDb).GetEncodedName();
        var connectionString = $"Data Source={filename}.bin;";
        db = new SqliteConnection(connectionString);
        var task = Init();
        task.Wait();
    }
    public async Task Init()
    {
        await db.ExecuteAsync(Transaction.Creation());
        await db.ExecuteAsync(Customer.Creation());
    }
    public async Task Truncate()
    {
        await db.ExecuteAsync(Transaction.Drop());
        await db.ExecuteAsync(Customer.Drop());
    }
    #region Transaction

    private async Task<Transaction> GetTransaction(int customerId, int transactionId)
    {
        var result = await db.GetAsync<Transaction>(transactionId) ?? throw new Exception("Transaction not found");
        if (result.CustomerId != customerId)
        {
            throw new Exception("Customer does not matches");
        }
        return result;
    }
    public async Task<GetTransactionDTO> GetTransactionAsync(int customerId, int transactionId)
    {
        var result = await GetTransaction(customerId, transactionId);

        return result.AsGetTransactionDTO();
    }

    public async Task<GetTransactionDTO[]> GetCustomerTransactionsAsync(int customerId)
    {
        var transactions = await db.QueryAsync<Transaction>($@"
SELECT * 
FROM [{nameof(Transaction)}s]
WHERE [{nameof(Transaction.CustomerId)}]= @CustomerId
ORDER BY [{nameof(Transaction.Date)}] ASC;
", new { CustomerId = customerId });
        var result = transactions.Select(t => t.AsGetTransactionDTO());
        return result.ToArray();
    }

    public async Task<int> AddTransactionAsync(int customerId, AddTransactionDTO val)
    {
        _ = await db.GetAsync<Customer>(customerId)
            ?? throw new Exception("Customer not found");
        var transaction = new Transaction
        {
            Amount = val.Amount,
            CustomerId = customerId,
            Note = val.Note,
            Type = val.Type,//.ToString(),
            Date = val.Date,//.ToISO(),
        };
        var result = await db.InsertAsync(transaction);
        return result;
    }

    public async Task<bool> UpdateTransactionAsync(int customerId, int transactionId, UpdateTransactionDTO val)
    {
        var rec = await GetTransaction(customerId, transactionId);
        rec.Amount = val.Amount;
        rec.Type = val.Type.ToString();
        rec.Note = val.Note;
        var result = await db.UpdateAsync(rec);
        return result;
    }

    public async Task<bool> RemoveTransactionAsync(int customerId, int transactionId)
    {
        try
        {
            var rec = await GetTransaction(customerId, transactionId);
            var result = await db.DeleteAsync(rec);
            return result;
        }
        catch
        {
            return true;
        }
    }

    #endregion

    #region Customer
    public async Task<GetCustomerDTO[]> SearchCustomersAsync(string searchKey)
    {
        var cleanSearchKey = searchKey.RemoveNonAlphaNumeric();
        var customers = await db.QueryAsync<Customer>($@"
SELECT * 
FROM [{nameof(Customer)}s] 
WHERE 
    [{nameof(Customer.FirstName)}] LIKE '%{cleanSearchKey}%' OR 
    [{nameof(Customer.LastName)}] LIKE '%{cleanSearchKey}%' OR 
    [{nameof(Customer.PhoneNumber)}] LIKE '%{cleanSearchKey}%'
ORDER BY [{nameof(Customer.FirstName)}] ASC;
");

        var result = customers.Select(t => t.AsGetCustomerDTO());
        return result.ToArray();
    }
    public async Task<GetCustomerDTO> GetCustomerAsync(int customerId)
    {
        var result = await db.GetAsync<Customer>(customerId) ??
            throw new Exception("Customer not found");

        return result.AsGetCustomerDTO();
    }

    public async Task<int> AddCustomerAsync(AddCustomerDTO val)
    {
        var result = await db.InsertAsync(new Customer
        {

            CreationDate = DateTime.Now.ToISO(),
            PhoneNumber = val.PhoneNumber,
            LastName = val.LastName,
            FirstName = val.FirstName,
            Note = val.Note,

        });
        return result;
    }

    public async Task<bool> UpdateCustomerAsync(int customerId, UpdateCustomerDTO val)
    {
        var rec = await db.GetAsync<Customer>(customerId) ??
            throw new Exception("Customer not found");
        rec.FirstName = val.FirstName;
        rec.LastName = val.LastName;
        rec.PhoneNumber = val.PhoneNumber;
        rec.Note = val.Note;
        var result = await db.UpdateAsync(rec);
        return result;
    }

    public async Task<bool> RemoveCustomerAsync(int customerId)
    {
        var rec = await db.GetAsync<Customer>(customerId);
        if (rec is null)
        {
            return true;
        }
        var result = await db.DeleteAsync(rec);
        return result;
    }

    #endregion
}