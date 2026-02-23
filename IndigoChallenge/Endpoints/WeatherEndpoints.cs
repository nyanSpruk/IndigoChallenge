using IndigoChallenge.Dtos;
using IndigoChallenge.Services;

namespace IndigoChallenge.Endpoints;

public static class TemperatureEndpoints
{
    public static IEndpointRouteBuilder MapTemperatureEndpoints(
        this IEndpointRouteBuilder app
    )
    {
        MapTemperatureGroup(app.MapGroup("/api/temperatures").WithTags("Temperatures"));

        return app;
    }

    private static void MapTemperatureGroup(RouteGroupBuilder group)
    {
        static IResult GetCityStats(
            string city,
            CityTemperatureStatsService cache
        )
        {
            if (cache.GetCache(city, out var stats) && stats is not null)
            {
                return TypedResults.Ok(stats);
            }

            return TypedResults.NotFound();
        }

        group.MapGet(
                "/",
                (CityTemperatureStatsService cache) =>
                {
                    var all = cache.GetAll().Values.OrderBy(x => x.City).ToList();
                    return TypedResults.Ok(all);
                }
            )
            .WithName("GetAllCityTemperatureStats")
            .Produces<List<CityStatsDto>>(StatusCodes.Status200OK);

        group.MapGet(
                "/{city}",
                GetCityStats
            )
            .WithName("GetCityTemperatureStats")
            .Produces<CityStatsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet(
                "/filter",
            (double? gt, double? lt, CityTemperatureStatsService cache) =>
                {
                    var filtered = cache
                        .GetAll()
                        .Values
                        .Where(x => !gt.HasValue || x.Average > gt.Value)
                        .Where(x => !lt.HasValue || x.Average < lt.Value)
                        .Select(x => new CityAvgDto(x.City, x.Average))
                        .OrderBy(x => x.City)
                        .ToList();

                    return TypedResults.Ok(filtered);
                }
            )
            .WithName("FilterCitiesByAverageTemperature")
            .Produces<List<CityAvgDto>>(StatusCodes.Status200OK);

        group.MapPost(
                "/recalculate",
                async (CityTemperatureStatsService cache) =>
                {
                    await cache.RebuildAsync();
                    return Results.Accepted();
                }
            )
            .WithName("RecalculateTemperatureStats")
            .Produces(StatusCodes.Status202Accepted);
    }
}

