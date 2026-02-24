using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

public static class DbInitializer
{
    public static async Task DbInitialize(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILogger<Item>>();

        try
        {
            await DB.InitAsync(
                "searchdb",
                MongoClientSettings.FromConnectionString(
                    app.Configuration.GetConnectionString("MongoDbConnection")
                )
            );

            await DB.Index<Item>()
                .Key(i => i.Make, KeyType.Text)
                .Key(i => i.Model, KeyType.Text)
                .Key(i => i.Color, KeyType.Text)
                .CreateAsync();

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "MongoDB initialized successfully in {Environment}",
                    app.Environment.EnvironmentName
                );
            }

            // Seed data only in Development
            if (app.Environment.IsDevelopment())
            {
                await SeedAsync(app, logger);
            }
            else
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Skipping database seeding in {Environment}",
                        app.Environment.EnvironmentName
                    );
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database initialization");
            throw;
        }
    }

    private static async Task SeedAsync(WebApplication app, ILogger logger)
    {
        var count = await DB.CountAsync<Item>();
        if (count > 0)
        {
            logger.LogInformation("Database already seeded, skipping seed operation");
            return;
        }

        logger.LogInformation("Seeding database with initial auction data");

        using var scope = app.Services.CreateScope();
        var auctionClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
        var items = await auctionClient.GetItemsFroSearchDb();

        // var itemData = await File.ReadAllTextAsync("Data/Auctions.json");
        // var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        // var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);

        if (items != null && items.Count > 0)
        {
            await DB.SaveAsync(items);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Database seeded successfully with {Count} items",
                    items.Count
                );
            }
        }
    }
}
