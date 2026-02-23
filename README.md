# IndigoChallenge

Simple ASP.NET Core Web API that reads weather measurements from CSV and exposes cached min/max/average temperatures per city.

## Assignment goal

Build a RESTful API that:

- Calculates city-level `min`, `max`, and `average` across the full dataset.
- Caches calculated results so endpoints return instantly.
- Supports:
  - all cities stats,
  - single city stats,
  - filtering by average temperature (`gt` / `lt`).
- Allows recalculation when the CSV dataset changes.

## Tech stack

- .NET 10 Web API
- OpenAPI + Scalar UI
- In-memory cache for computed city stats

## Dataset

- Expected file path: `IndigoChallenge/App_Data/measurements.csv`
- CSV format: `date;city;temperature`
- Delimiter: semicolon (`;`)
- Temperature parsing uses invariant culture (dot decimal separator)

## Run locally

1. Set API key for local run in [IndigoChallenge/Properties/launchSettings.json](IndigoChallenge/Properties/launchSettings.json):

  - `API_KEY`: `change-me` (or your own value)

2. Start the API:

   ```bash
   dotnet run --project IndigoChallenge/IndigoChallenge.csproj
   ```

3. Open docs:

   - Scalar UI: `http://localhost:5111/scalar/`
   - OpenAPI JSON: `http://localhost:5111/openapi/v1.json`

## Authentication

- Simple API key auth is implemented.
- Header name is fixed: `X-Api-Key`
- Key value is loaded from environment/config key `API_KEY`.
- All `/api/*` endpoints require this header.
- Missing/invalid key returns `401 Unauthorized`.

## API endpoints

Base route: `/api/temperatures`

- `GET /api/temperatures/`
  - Returns all cities with `min`, `max`, `average`.

- `GET /api/temperatures/{city}`
  - Returns one city stats.
  - Returns `404` if city is not found.

- `GET /api/temperatures/filter?gt={value}&lt={value}`
  - Returns city + average list filtered by optional bounds.
  - `gt` = average greater than value
  - `lt` = average lower than value

- `GET /api/temperatures/last-recalculated`
  - Returns last rebuild timestamp (`UTC`).

- `POST /api/temperatures/recalculate`
  - Rebuilds cached values from current CSV file.

## Example requests

```bash
# all cities
curl -H "X-Api-Key: change-me" \
  http://localhost:5111/api/temperatures/

# one city
curl -H "X-Api-Key: change-me" \
  http://localhost:5111/api/temperatures/Tokyo

# average > 20
curl -H "X-Api-Key: change-me" \
  "http://localhost:5111/api/temperatures/filter?gt=20"

# average between 10 and 20
curl -H "X-Api-Key: change-me" \
  "http://localhost:5111/api/temperatures/filter?gt=10&lt=20"

# last recalculation timestamp
curl -H "X-Api-Key: change-me" \
  http://localhost:5111/api/temperatures/last-recalculated

# manual recalculation
curl -X POST -H "X-Api-Key: change-me" \
  http://localhost:5111/api/temperatures/recalculate
```

## Caching and recalculation

- Stats are calculated once at startup and stored in-memory.
- GET endpoints serve cached values (no per-request recalculation).
- Recalculation is manual via `POST /api/temperatures/recalculate` when CSV is replaced.


## Use of LLMs

I have used LLMs for brainstorming, generating README as well as helping me with certain code snipepts (file reading).