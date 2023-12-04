using databaseapi.data_access;
using databaseapi.Models.TakvimDb;
using databaseapi.services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TakvimDb>();
builder.Services.AddSingleton<EventManager>();
var app = builder.Build();
var protectedRoutes = new[] { "admin", "takvim" };
app.UseHttpsRedirection();
app.Use((context, next) =>
{

    var pathParams = context.Request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries);

    if (pathParams.Length > 0 && !protectedRoutes.Contains(pathParams[0]))
    {
        var prefix = pathParams[0];
        var headers = context.Request.Headers;
        var authHeader = headers.Authorization;
        var key = authHeader.FirstOrDefault();

        if (authHeader.Count == 0 || string.IsNullOrEmpty(key))
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }

        var config = context.RequestServices.GetService<IConfiguration>();

        var requiredkey = config.GetValue<string>("ApiKeys:" + prefix.ToLower());
        if (protectedRoutes.Contains(prefix) && !requiredkey.Equals(key))
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
    }

    return next(context);
});

app.MapGet("/", () => "hello from db api");

#region Admin
{
    var admin = app.MapGroup("/admin");
    admin.MapGet("/getdb/{dbname}", (string dbname) =>
    {
        if (string.IsNullOrEmpty(dbname))
        {
            return Results.BadRequest();
        }
        switch (dbname.ToLower())
        {
            case "takvim":
                var filename = nameof(TakvimDb).GetEncodedName();
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
            case "takvim":
                try
                {
                    var canandb = serviceProvider.GetService<TakvimDb>();
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
            case "takvim":
                try
                {
                    var canandb = serviceProvider.GetService<TakvimDb>();
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
}
#endregion

#region Takvim
{
    var takvim = app.MapGroup("/takvim");


    #region Event
    var takvimEvent = takvim.MapGroup("/event");

    //Create Event
    takvimEvent.MapPost("/", async ([FromServices] TakvimDb db, AddEventDTO model) =>
    {
        var res = await db.AddEventAsync(model);
        return res;
    });

    //Get single Event
    takvimEvent.MapGet("/{id}", async ([FromServices] TakvimDb db, [FromRoute] int id) =>
    {
        try
        {
            var res = await db.GetEventAsync(id);
            return Results.Ok(res);
        }
        catch (Exception)
        {
            return Results.NotFound();
        }
    });

    //Monthly Event
    takvimEvent.MapGet("/monthly", async ([FromServices] TakvimDb db, [FromQuery] DateTime starts, [FromQuery] DateTime ends) =>
    {
        var res = await db.GetMonthlyEventsAsync(starts, ends);
        return res;
    });

    //Update Event
    takvimEvent.MapPatch("/{id}", async ([FromServices] TakvimDb db, [FromRoute] int id, UpdateEventDTO updateEventDTO) =>
    {
        var res = await db.UpdateEventAsync(id, updateEventDTO);
        return res;
    });

    //Remove Event
    takvimEvent.MapDelete("/{id}", async ([FromServices] TakvimDb db, [FromRoute] int id) =>
    {
        var res = await db.RemoveEventAsync(id);
        return res;
    });



    takvim.MapGet("/", ([FromServices] EventManager eventManager, [FromQuery] int year, [FromQuery] int month) =>
    {
        if (year < 2020 || year > 2030 || month < 1 || month > 12)
        {
            return Results.NotFound();
        }
        try
        {
            var res = eventManager.GetMonthlyEvents(year, month);
            return Results.Ok(res);
        }
        catch (Exception)
        {
            return Results.NotFound();
        }
    });


    #endregion

}
#endregion

app.Run();
