using IndigoChallenge.Dtos;

namespace IndigoChallenge.Services;

public sealed class CityTemperatureStatsService
{

    // COnfig and env are needed to get the path to the file
    private readonly IConfiguration config;
    private readonly IWebHostEnvironment env;
    // Gate is to lock the cahce
    private readonly object gate = new();


    private readonly CsvCityStatsCalculator calculator;

    private Dictionary<string, CityStatsDto> stats = new(StringComparer.OrdinalIgnoreCase);

    public DateTimeOffset? LastRebuildUtc { get; private set; }

    public CityTemperatureStatsService(IConfiguration config, IWebHostEnvironment env, CsvCityStatsCalculator calculator)
    {
        this.config = config;
        this.env = env;
        this.calculator = calculator;
    }

    public IReadOnlyDictionary<string, CityStatsDto> GetAll()
    {
        // Need lock incase if making a rebuild so that I dont return old/mid edit data.
        lock (gate)
        {
            return new Dictionary<string, CityStatsDto>(stats, stats.Comparer);
        }
    }

    public bool GetCache(string city, out CityStatsDto? stats)
    {
        lock (gate)
        {
            return this.stats.TryGetValue(city, out stats);
        }
    }

    public Task RebuildAsync()
    {
        var relativePath = config["Dataset:Path"];
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new InvalidOperationException("Dataset:Path is not configured.");
        }

        var fullPath = Path.Combine(env.ContentRootPath, relativePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Dataset not found.", fullPath);
        }

        var newStats = calculator.Calculate(fullPath);

        lock (gate)
        {
            stats = newStats;
            LastRebuildUtc = DateTimeOffset.UtcNow;
        }

        return Task.CompletedTask;
    }
}