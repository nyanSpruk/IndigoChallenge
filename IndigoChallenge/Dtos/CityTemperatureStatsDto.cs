namespace IndigoChallenge.Dtos;

public sealed record CityStatsDto(
    string City,
    double Min,
    double Max,
    double Average
);