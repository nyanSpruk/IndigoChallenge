using System.Globalization;
using IndigoChallenge.Dtos;

namespace IndigoChallenge.Services;

public sealed class CsvCityStatsCalculator
{
    public Dictionary<string, CityStatsDto> Calculate(string fullPath)
    {
        var cityDict = new Dictionary<string, DataAccumulator>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in File.ReadLines(fullPath))
        {
            if (!TryParseLine(line, out var city, out var temp))
            {
                continue;
            }

            if (!cityDict.TryGetValue(city, out var accumulator))
            {
                accumulator = new DataAccumulator();
                cityDict[city] = accumulator;
            }

            accumulator.Add(temp);
        }

        var result = new Dictionary<string, CityStatsDto>(StringComparer.OrdinalIgnoreCase);
        foreach (var (city, accumulator) in cityDict)
        {
            result[city] = accumulator.ToStatsDto(city);
        }

        return result;
    }

    private static bool TryParseLine(string line, out string city, out double temp)
    {
        city = string.Empty;
        temp = default;

        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        if (line.TrimStart().StartsWith("date;", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var parts = line.Split(';', 3);
        if (parts.Length < 3)
        {
            return false;
        }

        city = parts[1].Trim();
        if (string.IsNullOrWhiteSpace(city))
        {
            return false;
        }

        return double.TryParse(
            parts[2].Trim(),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out temp
        );
    }

    private sealed class DataAccumulator
    {
        private double min = double.PositiveInfinity;
        private double max = double.NegativeInfinity;
        private double sum;
        private long count;

        public void Add(double value)
        {
            min = Math.Min(min, value);
            max = Math.Max(max, value);

            sum += value;
            count++;
        }

        public CityStatsDto ToStatsDto(string city)
        {
            var avg = count == 0 ? 0 : sum / count;
            return new CityStatsDto(city, min, max, avg);
        }
    }
}