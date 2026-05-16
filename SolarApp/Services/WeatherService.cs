using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SolarApp.Models;

namespace SolarApp.Services
{
    /// <summary>
    /// Сервіс для отримання погодних даних через Open-Meteo API.
    /// Повертає температуру, хмарність та кількість сонячних годин.
    /// </summary>
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;

        /// <summary>Базова URL-адреса API Open-Meteo</summary>
        private const string BaseUrl = "https://api.open-meteo.com/v1/forecast";

        public WeatherService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        /// <summary>
        /// Отримує погодні дані з Open-Meteo API для заданих координат.
        /// Запитує поточну погоду, хмарність та тривалість сонячного світла.
        /// </summary>
        public async Task<WeatherData> GetWeatherAsync(double latitude, double longitude)
        {
            string latStr = latitude.ToString(CultureInfo.InvariantCulture);
            string lonStr = longitude.ToString(CultureInfo.InvariantCulture);

            string url = $"{BaseUrl}?latitude={latStr}&longitude={lonStr}" +
                         "&current_weather=true&hourly=cloudcover" +
                         "&daily=sunshine_duration&timezone=auto&forecast_days=1";

            string response = await _httpClient.GetStringAsync(url);

            using JsonDocument doc = JsonDocument.Parse(response);
            JsonElement root = doc.RootElement;

            // Отримання температури з поточної погоди
            double temperature = root
                .GetProperty("current_weather")
                .GetProperty("temperature")
                .GetDouble();

            // Отримання хмарності (перше значення з масиву погодинних даних)
            int cloudCover = root
                .GetProperty("hourly")
                .GetProperty("cloudcover")[0]
                .GetInt32();

            // Отримання тривалості сонячного світла (секунди → години)
            double sunHours = 6.0; // значення за замовчуванням
            if (root.TryGetProperty("daily", out JsonElement daily) &&
                daily.TryGetProperty("sunshine_duration", out JsonElement sunDuration) &&
                sunDuration.GetArrayLength() > 0)
            {
                double sunSeconds = sunDuration[0].GetDouble();
                sunHours = sunSeconds / 3600.0;
            }

            // Класифікація погодних умов за рівнем хмарності
            WeatherCondition condition = ClassifyWeather(cloudCover);

            return new WeatherData
            {
                Temperature = temperature,
                CloudCoverPercent = cloudCover,
                Condition = condition,
                SunHoursPerDay = Math.Round(sunHours, 1)
            };
        }

        /// <summary>
        /// Класифікує погодні умови за відсотком хмарності.
        /// 0–25% → Сонячно, 26–75% → Змінна хмарність, 76–100% → Хмарно.
        /// </summary>
        private static WeatherCondition ClassifyWeather(int cloudCoverPercent)
        {
            if (cloudCoverPercent <= 25)
                return WeatherCondition.Sunny;
            if (cloudCoverPercent <= 75)
                return WeatherCondition.PartlyCloudy;
            return WeatherCondition.Cloudy;
        }
    }
}
