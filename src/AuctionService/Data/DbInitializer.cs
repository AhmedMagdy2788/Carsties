using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public static class DbInitializer
{
    public static async Task Initialize(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuctionDbContext>>();

        try
        {
            // Apply pending migrations based on environment
            if (app.Environment.IsDevelopment())
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully in Development");
            }
            else
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    if (logger.IsEnabled(LogLevel.Warning))
                    {
                        logger.LogWarning(
                            "Pending migrations detected in {Environment}: {Migrations}. Please apply migrations manually.",
                            app.Environment.EnvironmentName,
                            string.Join(", ", pendingMigrations)
                        );
                    }
                }
                else
                {
                    logger.LogInformation(
                        "Database is up to date in {Environment}",
                        app.Environment.EnvironmentName
                    );
                }
            }

            // Seed data
            await SeedAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database initialization");
            throw;
        }
    }

    private static async Task SeedAsync(AuctionDbContext context, ILogger logger)
    {
        // Check if data already exists
        if (await context.Auctions.AnyAsync())
        {
            logger.LogInformation("Database already seeded, skipping seed operation");
            return;
        }

        logger.LogInformation("Seeding database with initial auction data");

        var auctions = InitialData.Auctions;
        context.Auctions.AddRange(auctions);
        await context.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Database seeded successfully with {Count} auctions",
                auctions.Count
            );
        }
    }
}
