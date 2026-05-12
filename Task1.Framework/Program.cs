using Task1.Framework.Contracts;
using Task1.Framework.Domain;
using Task1.Framework.Middleware;
using Task1.Framework.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IItemRepository, InMemoryItemRepository>();

builder.Services.AddTransient<RequestIdMiddleware>();
builder.Services.AddTransient<RequestTimingMiddleware>();
builder.Services.AddTransient<RequestLoggingMiddleware>();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

var app = builder.Build();

app.UseMiddleware<RequestIdMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

var api = app.MapGroup("/api/items");

api.MapGet("/", (IItemRepository repo, int? minPoints, int? maxPoints, string? sort, bool? desc) =>
{
    var items = repo.GetAll();

    if (minPoints is not null)
        items = items.Where(x => x.Points >= minPoints.Value);

    if (maxPoints is not null)
        items = items.Where(x => x.Points <= maxPoints.Value);

    var sortKey = (sort ?? "id").Trim().ToLowerInvariant();
    var isDesc = desc is true;

    items = sortKey switch
    {
        "title" => isDesc ? items.OrderByDescending(x => x.Title) : items.OrderBy(x => x.Title),
        "points" => isDesc ? items.OrderByDescending(x => x.Points) : items.OrderBy(x => x.Points),
        _ => isDesc ? items.OrderByDescending(x => x.Id) : items.OrderBy(x => x.Id),
    };

    return Results.Ok(items.Select(ItemDto.FromDomain));
});

api.MapGet("/{id:int}", (IItemRepository repo, int id) =>
{
    var item = repo.GetById(id);
    if (item is null)
        throw new NotFoundException(ErrorCodes.ItemNotFound, $"Item with id={id} was not found.");

    return Results.Ok(ItemDto.FromDomain(item));
});

api.MapPost("/", (IItemRepository repo, CreateItemRequest request, HttpContext httpContext) =>
{
    ItemValidation.ValidateCreate(request);

    var created = repo.Create(new ItemCreate(request.Title.Trim(), request.Points));
    var dto = ItemDto.FromDomain(created);

    return Results.Created($"/api/items/{dto.Id}", dto);
});

app.MapGet("/", () => Results.Redirect("/api/items"));
app.MapGet("/api", () => Results.Redirect("/api/items"));

app.Run();

public partial class Program { }
