using Scalar.AspNetCore;
using IndigoChallenge.Endpoints;
using IndigoChallenge.Services;

var builder = WebApplication.CreateBuilder(args);

const string apiKeyHeaderName = "X-Api-Key";
var configuredApiKey = builder.Configuration["API_KEY"];

if (string.IsNullOrWhiteSpace(configuredApiKey))
{
    throw new InvalidOperationException("API_KEY is not configured.");
}

builder.Services.AddOpenApi();
builder.Services.AddSingleton<CsvCityStatsCalculator>();
builder.Services.AddSingleton<CityTemperatureStatsService>();

var app = builder.Build();

var cityStatsCache = app.Services.GetRequiredService<CityTemperatureStatsService>();
// Initial build
await cityStatsCache.RebuildAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title = "Weather Stations API";
    });
}

app.UseHttpsRedirection();

app.Use(
    async (context, next) =>
    {
        var requiresAuth = context.Request.Path.StartsWithSegments(
            "/api",
            StringComparison.OrdinalIgnoreCase
        );

        if (!requiresAuth)
        {
            await next();
            return;
        }

        if (
            !context.Request.Headers.TryGetValue(apiKeyHeaderName, out var providedApiKey)
            || !string.Equals(providedApiKey, configuredApiKey, StringComparison.Ordinal)
        )
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(
                new { message = "Missing or invalid API key." }
            );
            return;
        }

        await next();
    }
);

app.MapTemperatureEndpoints();

app.Run();