using databaseapi.data_access;
using databaseapi.Models.CananDb;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<CananDb>();
var app = builder.Build();

app.UseHttpsRedirection();
app.Use((context, next) =>
{
    var headers = context.Request.Headers;
    var authHeader = headers.Authorization;
    var key = authHeader.FirstOrDefault();

    if (authHeader.Count == 0 || string.IsNullOrEmpty(key))
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    }

    var config = context.RequestServices.GetService<IConfiguration>();

    var prefix = context.Request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries)[0];

    var requiredkey = config.GetValue<string>("ApiKeys:" + prefix.ToLower());
    if (!requiredkey.Equals(key))
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    }

    return next(context);
});
#region Admin
var admin = app.MapGroup("/admin");
admin.MapGet("/getdb/{dbname}", (string dbname) =>
{
    if (string.IsNullOrEmpty(dbname))
    {
        return Results.BadRequest();
    }
    switch (dbname.ToLower())
    {
        case "canandb":
            var filename = nameof(CananDb).GetEncodedName();
            var dbcontent = File.ReadAllBytes(filename + ".bin");
            return Results.Ok(new
            {
                base64 = Convert.ToBase64String(dbcontent)
            });
        default:
            break;
    }
    return Results.BadRequest();
});


admin.MapGet("/initdb/{dbname}", async ([FromServices] IServiceProvider serviceProvider, string dbname) =>
{
    if (string.IsNullOrEmpty(dbname))
    {
        return Results.BadRequest();
    }
    switch (dbname.ToLower())
    {
        case "canandb":
            try
            {
                var canandb = serviceProvider.GetService<CananDb>();
                await canandb.Init();
                return Results.Ok();
            }
            catch (Exception e)
            {
                return Results.Problem(e.Message);
            }
        default:
            break;
    }
    return Results.BadRequest();
});

admin.MapGet("/truncatedb/{dbname}", async ([FromServices] IServiceProvider serviceProvider, string dbname) =>
{
    if (string.IsNullOrEmpty(dbname))
    {
        return Results.BadRequest();
    }
    switch (dbname.ToLower())
    {
        case "canandb":
            try
            {
                var canandb = serviceProvider.GetService<CananDb>();
                await canandb.Truncate();
                return Results.Ok();
            }
            catch (Exception e)
            {
                return Results.Problem(e.Message);
            }
        default:
            break;
    }
    return Results.BadRequest();
});
#endregion

#region Canan
var canan = app.MapGroup("/canan");
canan.MapGet("/seed", async ([FromServices] CananDb db) =>
{
    var transactions = new List<KeyValuePair<int, AddTransactionDTO>>();

    var customers = new AddCustomerDTO[]
    {
        new AddCustomerDTO
        {
            FirstName = "vahap",
            LastName = "bozkurt",
            Note = "",
            PhoneNumber = "533 111 22 33"
        },
        new AddCustomerDTO
        {
            FirstName = "seval",
            LastName = "torun",
            Note = "",
            PhoneNumber = "534 222"
        },
        new AddCustomerDTO
        {
            FirstName = "said",
            LastName = "yeter",
            Note = "sevalin esi",
            PhoneNumber = "535 333"
        },
        new AddCustomerDTO
        {
            FirstName = "lutfiye",
            LastName = "torun",
            Note = "halam",
            PhoneNumber = "536 444"
        },
        new AddCustomerDTO
        {
            FirstName = "ahmet",
            LastName = "celik",
            Note = "dahiliye dr",
            PhoneNumber = "537 555"
        },
        new AddCustomerDTO
        {
            FirstName = "mehmet",
            LastName = "demir",
            Note = "sekreter bashekimlik",
            PhoneNumber = "538 666"
        },
        new AddCustomerDTO
        {
            FirstName = "ayse",
            LastName = "topcu",
            Note = "2.kat",
            PhoneNumber = "539 777"
        },
        new AddCustomerDTO
        {
            FirstName = "fatma",
            LastName = "aslan",
            Note = "acil",
            PhoneNumber = "540 888"
        },
        new AddCustomerDTO
        {
            FirstName = "merve",
            LastName = "basoglu",
            Note = "berranin okuldan arkadasi pinarin annesi",
            PhoneNumber = "541 999"
        },
        new AddCustomerDTO
        {
            FirstName = "suzan",
            LastName = "toprak",
            Note = "vahap'in isyerinden",
            PhoneNumber = "542 101"
        }
    };

    foreach (var customer in customers)
    {
        var c = await db.AddCustomerAsync(customer);

        for (int i = 0; i < customer.FirstName.Length; i++)
        {
            transactions.Add(
                new KeyValuePair<int, AddTransactionDTO>(
                c,
                new AddTransactionDTO
                {
                    Date = DateTime.Now.AddDays(-1 * new Random().Next(1, 250)).ToISO(),
                    Amount = new Random().Next(50, 100),
                    Note = "",
                    Type = new Random().Next(1, 10) > 5 ? "Debit" : "Credit",
                }));
        }
        var totalDebit = transactions
                .Where(x => x.Key == c)
                .Where(x => x.Value.Type == "Debit")
                .Sum(x => x.Value.Amount);

        var totalCredit = transactions
                .Where(x => x.Key == c)
                .Where(x => x.Value.Type == "Credit")
                .Sum(x => x.Value.Amount);
        if (totalCredit > totalDebit)
        {
            var newAmount = new Random().Next(50);
            var diff = totalCredit - totalDebit;
            transactions.Add(
                new KeyValuePair<int, AddTransactionDTO>(
                c,
                new AddTransactionDTO
                {
                    Date = DateTime.Now.AddDays(-1 * new Random().Next(1, 250)).ToISO(),
                    Amount = newAmount + diff,
                    Note = "",
                    Type = "Debit",
                }));
        }
    }
    foreach (var transaction in transactions)
    {
        _ = await db.AddTransactionAsync(transaction.Key, transaction.Value);
    }

    return Results.Ok();
});

#region Customer
var cananCustomer = canan.MapGroup("/customer");

//Create Customer
cananCustomer.MapPost("/", async ([FromServices] CananDb db, AddCustomerDTO model) =>
{
    var res = await db.AddCustomerAsync(model);
    return res;
});

//Get single customer
cananCustomer.MapGet("/{id}", async ([FromServices] CananDb db, [FromRoute] int id) =>
{
    try
    {
        var res = await db.GetCustomerAsync(id);
        return Results.Ok(res);
    }
    catch (Exception)
    {
        return Results.NotFound();
    }
});

//Search customer
cananCustomer.MapGet("/search/{searchKey}", async ([FromServices] CananDb db, [FromRoute] string searchKey) =>
{
    var res = await db.SearchCustomersAsync(searchKey);
    return res;
});

//All customers
cananCustomer.MapGet("/", async ([FromServices] CananDb db) =>
{
    var res = await db.SearchCustomersAsync(string.Empty);
    return res;
});

//Update Customer
cananCustomer.MapPatch("/{id}", async ([FromServices] CananDb db, [FromRoute] int id, UpdateCustomerDTO updateCustomerDTO) =>
{
    var res = await db.UpdateCustomerAsync(id, updateCustomerDTO);
    return res;
});

//Remove customer
cananCustomer.MapDelete("/{id}", async ([FromServices] CananDb db, [FromRoute] int id) =>
{
    var res = await db.RemoveCustomerAsync(id);
    return res;
});
#endregion

#region Transaction
var cananTransaction = canan.MapGroup("/transaction");

cananTransaction.MapPost("/{customerId}/", async ([FromServices] CananDb db, [FromRoute] int customerId, AddTransactionDTO model) =>
{
    if (string.IsNullOrEmpty(model.Type))
    {
        return Results.BadRequest();
    }

    switch (model.Type.ToLower())
    {
        case "a":
        case "alacak":
        case "c":
        case "credit":
            model.Type = "c";
            break;

        case "b":
        case "borc":
        case "d":
        case "debit":
            model.Type = "d";
            break;

        default:
            return Results.BadRequest(new
            {
                msg = "invalid transaction type: " + model.Type
            });
    }

    var res = await db.AddTransactionAsync(customerId, model);
    return Results.Ok(res);
});

//Get Customer Transactions
cananTransaction.MapGet("/{customerId}", async ([FromServices] CananDb db, [FromRoute] int customerId) =>
{
    var res = await db.GetCustomerTransactionsAsync(customerId);
    return res;
});

cananTransaction.MapGet("/{customerId}/{transactionId}", async ([FromServices] CananDb db, [FromRoute] int customerId, [FromRoute] int transactionId) =>
{
    var res = await db.GetTransactionAsync(customerId, transactionId);
    return res;
});


cananTransaction.MapPatch("/{customerId}/{id}", async ([FromServices] CananDb db, [FromRoute] int id, [FromRoute] int customerId, UpdateTransactionDTO updateDTO) =>
{
    var res = await db.UpdateTransactionAsync(customerId, id, updateDTO);
    return res;
});

cananTransaction.MapDelete("/{customerId}/{id}", async ([FromServices] CananDb db, [FromRoute] int customerId, [FromRoute] int id) =>
{
    var res = await db.RemoveTransactionAsync(customerId, id);
    return res;
});
#endregion

#endregion

app.Run();
