using Scalar.AspNetCore;
using IndigoChallenge.Endpoints;
using IndigoChallenge.Services;

var builder = WebApplication.CreateBuilder(args);

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

app.MapTemperatureEndpoints();

app.Run();