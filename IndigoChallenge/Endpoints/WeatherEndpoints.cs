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

    }
}

