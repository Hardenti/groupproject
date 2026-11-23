using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FoodPantryManagement.Models;

namespace FoodPantryManagement.Services;

public sealed class OpenMeteoService(HttpClient httpClient, ILogger<OpenMeteoService> logger)
{
    public async Task<WeatherSummary> GetForecastAsync(
        PantrySite site,
        CancellationToken cancellationToken = default)
    {
        string uri = string.Format(
            CultureInfo.InvariantCulture,
            "https://api.open-meteo.com/v1/forecast?latitude={0}&longitude={1}&daily=weather_code,temperature_2m_max,precipitation_probability_max&forecast_days=7&timezone=auto",
            site.Latitude,
            site.Longitude);

        try
        {
            OpenMeteoResponse? response = await httpClient.GetFromJsonAsync<OpenMeteoResponse>(uri, cancellationToken);
            if (response?.Daily?.Time is null || response.Daily.TemperatureMax is null)
            {
                return new WeatherSummary
                {
                    Location = $"{site.Name}, {site.City}",
                    Error = "The forecast provider returned incomplete data."
                };
            }

            List<WeatherDay> days = [];
            for (int index = 0; index < response.Daily.Time.Length; index++)
            {
                days.Add(new WeatherDay(
                    DateOnly.Parse(response.Daily.Time[index], CultureInfo.InvariantCulture),
                    response.Daily.TemperatureMax[index],
                    GetAt(response.Daily.PrecipitationProbabilityMax, index),
                    GetAt(response.Daily.WeatherCode, index)));
            }

            return new WeatherSummary
            {
                Location = $"{site.Name}, {site.City}",
                Days = days
            };
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Could not retrieve Open-Meteo forecast for pantry site {PantrySiteId}.", site.PantrySiteId);
            return new WeatherSummary
            {
                Location = $"{site.Name}, {site.City}",
                Error = "Weather is temporarily unavailable. Pantry records are still available."
            };
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Open-Meteo returned an invalid forecast for pantry site {PantrySiteId}.", site.PantrySiteId);
            return new WeatherSummary
            {
                Location = $"{site.Name}, {site.City}",
                Error = "Weather data could not be read. Pantry records are still available."
            };
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Open-Meteo request timed out for pantry site {PantrySiteId}.", site.PantrySiteId);
            return new WeatherSummary
            {
                Location = $"{site.Name}, {site.City}",
                Error = "Weather request timed out. Pantry records are still available."
            };
        }
    }

    private static int? GetAt(int[]? values, int index) =>
        values is not null && index < values.Length ? values[index] : null;

    private sealed class OpenMeteoResponse
    {
        [JsonPropertyName("daily")]
        public DailyForecast? Daily { get; set; }
    }

    private sealed class DailyForecast
    {
        [JsonPropertyName("time")]
        public string[]? Time { get; set; }

        [JsonPropertyName("temperature_2m_max")]
        public double[]? TemperatureMax { get; set; }

        [JsonPropertyName("precipitation_probability_max")]
        public int[]? PrecipitationProbabilityMax { get; set; }

        [JsonPropertyName("weather_code")]
        public int[]? WeatherCode { get; set; }
    }
}
