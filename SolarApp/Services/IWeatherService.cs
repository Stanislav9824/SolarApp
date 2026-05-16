using System.Threading.Tasks;
using SolarApp.Models;

namespace SolarApp.Services
{
    /// <summary>
    /// Інтерфейс сервісу для отримання погодних даних.
    /// Дозволяє замінити реалізацію (наприклад, для тестування).
    /// </summary>
    public interface IWeatherService
    {
        /// <summary>
        /// Отримує поточні погодні дані для заданих координат.
        /// </summary>
        /// <param name="latitude">Географічна широта</param>
        /// <param name="longitude">Географічна довгота</param>
        /// <returns>Об'єкт з погодними даними</returns>
        Task<WeatherData> GetWeatherAsync(double latitude, double longitude);
    }
}
