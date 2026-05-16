using SolarApp.Models;

namespace SolarApp.Services
{
    /// <summary>
    /// Інтерфейс калькулятора енергопродуктивності сонячних панелей.
    /// </summary>
    public interface IEnergyCalculator
    {
        /// <summary>
        /// Розраховує прогнозовану енергію за день для сонячної панелі.
        /// </summary>
        /// <param name="panel">Параметри сонячної панелі</param>
        /// <param name="weather">Погодні дані</param>
        /// <returns>Результат розрахунку з енергією та коефіцієнтами</returns>
        CalculationResult Calculate(SolarPanel panel, WeatherData weather);

        /// <summary>
        /// Обчислює оптимальний кут нахилу для заданої географічної широти.
        /// </summary>
        /// <param name="latitude">Географічна широта (градуси)</param>
        /// <returns>Оптимальний кут нахилу (градуси)</returns>
        double GetOptimalTiltAngle(double latitude);
    }
}
