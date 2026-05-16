namespace SolarApp.Models
{
    /// <summary>
    /// Дані про погодні умови, отримані з API або введені вручну.
    /// Використовуються для розрахунку продуктивності сонячної панелі.
    /// </summary>
    public class WeatherData
    {
        /// <summary>Поточна температура повітря (°C)</summary>
        public double Temperature { get; set; }

        /// <summary>Хмарність у відсотках (0–100%)</summary>
        public int CloudCoverPercent { get; set; }

        /// <summary>Класифікована погодна умова</summary>
        public WeatherCondition Condition { get; set; }

        /// <summary>Кількість сонячних годин на день</summary>
        public double SunHoursPerDay { get; set; }
    }

    /// <summary>
    /// Результат розрахунку енергопродуктивності сонячної панелі.
    /// Містить кінцеве значення енергії та всі проміжні коефіцієнти.
    /// </summary>
    public class CalculationResult
    {
        /// <summary>Прогнозована енергія за день (кВт·год/день)</summary>
        public double EnergyKWhPerDay { get; set; }

        /// <summary>Коефіцієнт погодних умов (0.4–1.0)</summary>
        public double WeatherCoefficient { get; set; }

        /// <summary>Коефіцієнт орієнтації панелі (0.6–1.0)</summary>
        public double OrientationCoefficient { get; set; }

        /// <summary>Коефіцієнт кута нахилу (0.0–1.0)</summary>
        public double TiltCoefficient { get; set; }

        /// <summary>Коефіцієнт впливу температури (зниження при T > 25°C)</summary>
        public double TemperatureCoefficient { get; set; }

        /// <summary>Оптимальний кут нахилу для даної широти (градуси)</summary>
        public double OptimalTiltAngle { get; set; }

        /// <summary>Загальна ефективність системи у відсотках</summary>
        public double OverallEfficiencyPercent { get; set; }
    }
}
