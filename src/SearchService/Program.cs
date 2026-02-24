using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// Polly Resilience Policies
// =========================================================================

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
    .WaitAndRetryForeverAsync(
        _ => TimeSpan.FromSeconds(3),
        onRetry: (outcome, timeSpan) =>
        {
            Console.WriteLine(
                $"  🔁 [Polly] Retrying after {timeSpan.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}"
            );
        }
    );

var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
    TimeSpan.FromSeconds(10),
    Polly.Timeout.TimeoutStrategy.Optimistic,
    onTimeoutAsync: (_, timeSpan, _) =>
    {
        Console.WriteLine($"  ⏱ [Polly] Request timed out after {timeSpan.TotalSeconds}s");
        return Task.CompletedTask;
    }
);

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak: (_, duration) =>
            Console.WriteLine($"  🔴 [Polly] Circuit OPEN for {duration.TotalSeconds}s"),
        onReset: () => Console.WriteLine("  🟢 [Polly] Circuit CLOSED — service recovered"),
        onHalfOpen: () => Console.WriteLine("  🟡 [Polly] Circuit HALF-OPEN — testing...")
    );

// Add services to the container.

builder.Services.AddControllers();
builder
    .Services.AddHttpClient<AuctionSvcHttpClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["AuctionService:BaseUrl"]
                ?? throw new ArgumentNullException(
                    "AuctionService:BaseUrl configuration is missing"
                )
        );
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(10);
        // You can set other default headers or settings for the HttpClient here if needed
    })
    .AddPolicyHandler(retryPolicy);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

await app.DbInitialize();

app.Run();
