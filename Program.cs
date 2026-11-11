using CLTTestApi.Application;
using CLTTestApi.Endpoints;
using CLTTestApi.Infrastructure;
using CLTTestApi.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI + ProblemDetails + manejador global de excepciones.
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// EF Core + SQLite.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=clt_test.db"));

// CQRS handlers y validadores FluentValidation.
builder.Services.AddApplication();

var app = builder.Build();

// Pipeline HTTP.
app.UseExceptionHandler();
app.UseMiddleware<ApiKeyMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Endpoints (Minimal APIs).
app.MapUserEndpoints();
app.MapAddressEndpoints();
app.MapCurrencyEndpoints();

// Crea/aplica la base SQLite y carga monedas base la primera vez.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (db.Database.GetMigrations().Any())
    {
        await db.Database.MigrateAsync();
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
    }

    await DbSeeder.SeedAsync(db);
}

app.Run();
