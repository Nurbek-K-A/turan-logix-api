using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TuranLogix.Application.Common.Interfaces;

namespace TuranLogix.Infrastructure.External.Mapbox;

/// <summary>
/// Геокодирование городов и расчёт расстояний через Mapbox Geocoding API
/// </summary>
public class MapboxService : IMapboxService
{
    private readonly HttpClient _httpClient;
    private readonly string _accessToken;
    private readonly ILogger<MapboxService> _logger;

    /// <param name="httpClient">HTTP-клиент (регистрируется через IHttpClientFactory)</param>
    /// <param name="configuration">Конфигурация (Mapbox:AccessToken)</param>
    /// <param name="logger">Логгер</param>
    public MapboxService(HttpClient httpClient, IConfiguration configuration, ILogger<MapboxService> logger)
    {
        _httpClient = httpClient;
        _accessToken = configuration["Mapbox:AccessToken"] ?? string.Empty;
        _logger = logger;
    }

    /// <inheritdoc/>
    /// <remarks>Возвращает null, если AccessToken не задан или город не найден</remarks>
    public async Task<(double Lat, double Lng)?> GeocodeAsync(string city, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            _logger.LogWarning("Mapbox AccessToken не настроен, геокодирование недоступно");
            return null;
        }

        try
        {
            var encoded = Uri.EscapeDataString(city);
            var url = $"https://api.mapbox.com/geocoding/v5/mapbox.places/{encoded}.json?access_token={_accessToken}&limit=1";
            var response = await _httpClient.GetStringAsync(url, cancellationToken);

            using var doc = JsonDocument.Parse(response);
            var features = doc.RootElement.GetProperty("features");
            if (features.GetArrayLength() == 0) return null;

            var coords = features[0].GetProperty("center");
            var lng = coords[0].GetDouble();
            var lat = coords[1].GetDouble();
            return (lat, lng);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка геокодирования для города {City}", city);
            return null;
        }
    }

    /// <inheritdoc/>
    public double CalculateDistance(double lat1, double lng1, double lat2, double lng2)
    {
        const double earthRadiusKm = 6371.0;
        var dLat = ToRadians(lat2 - lat1);
        var dLng = ToRadians(lng2 - lng1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
              * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    /// <summary>
    /// Перевести градусы в радианы
    /// </summary>
    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
