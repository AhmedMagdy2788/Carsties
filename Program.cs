using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddDbContext<AuctionDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("AuctionDatabase"));
        });

        builder.Services.AddAutoMapper(typeof(Program).Assembly);

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi(); // Serves the OpenAPI document
            app.MapScalarApiReference(); // Modern UI (alternative to Swagger UI)
        }

        // app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        await app.Initialize();

        await app.RunAsync();
    }
}