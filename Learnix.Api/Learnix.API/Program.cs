using Learnix.API.Data;
using Learnix.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddScoped<CurrentUserService>();

var connectionString = Environment.GetEnvironmentVariable("LEARNIX_DB_CONNECTION")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=55432;Database=learnix_db;Username=postgres;Password=jara130308";

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(
    connectionString,
    npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 6,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    })
    .ConfigureWarnings(warnings =>
    {
        warnings.Ignore(RelationalEventId.PendingModelChangesWarning);
    }));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        await EnsureDatabaseExistsAsync(connectionString);
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await LearnixSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine("Learnix API could not initialize PostgreSQL.");
        Console.Error.WriteLine(ex.Message);
        Environment.ExitCode = 1;
        return;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.MapControllers();

app.Run();

static async Task EnsureDatabaseExistsAsync(string connectionString)
{
    var builder = new NpgsqlConnectionStringBuilder(connectionString);
    var databaseName = builder.Database;
    if (string.IsNullOrWhiteSpace(databaseName))
    {
        return;
    }

    builder.Database = "postgres";
    await using var connection = new NpgsqlConnection(builder.ConnectionString);
    await connection.OpenAsync();

    await using var checkCommand = new NpgsqlCommand(
        "SELECT 1 FROM pg_database WHERE datname = @databaseName",
        connection);
    checkCommand.Parameters.AddWithValue("databaseName", databaseName);
    var exists = await checkCommand.ExecuteScalarAsync();
    if (exists != null)
    {
        return;
    }

    var escapedDatabaseName = databaseName.Replace("\"", "\"\"");
    await using var createCommand = new NpgsqlCommand($"CREATE DATABASE \"{escapedDatabaseName}\"", connection);
    await createCommand.ExecuteNonQueryAsync();
}
