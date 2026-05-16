using System;
using SolarApp.Models;

namespace SolarApp.Services
{
    /// <summary>
    /// Калькулятор енергопродуктивності сонячних панелей.
    /// 
    /// Формула розрахунку:
    /// E (кВт·год/день) = P(кВт) × H_сон × K_погода × K_орієнтація × K_нахил × K_температура
    /// 
    /// де:
    ///   P       — потужність панелі (кВт)
    ///   H_сон   — кількість сонячних годин на день
    ///   K_погода — коефіцієнт погодних умов (1.0 / 0.7 / 0.4)
    ///   K_орієнт — коефіцієнт орієнтації (1.0 / 0.8 / 0.6)
    ///   K_нахил  — коефіцієнт відхилення від оптимального кута
    ///   K_темп   — температурний коефіцієнт (втрата 0.4% на кожен °C вище 25°C)
    /// </summary>
    public class EnergyCalculator : IEnergyCalculator
    {
        /// <summary>
        /// Розраховує прогнозовану денну енергію для сонячної панелі.
        /// </summary>
        public CalculationResult Calculate(SolarPanel panel, WeatherData weather)
        {
            double powerKw = panel.PowerWatts / 1000.0;
            double optimalTilt = GetOptimalTiltAngle(panel.Latitude);

            double weatherCoeff = GetWeatherCoefficient(weather.Condition);
            double orientationCoeff = GetOrientationCoefficient(panel.Orientation);
            double tiltCoeff = GetTiltCoefficient(panel.TiltAngle, optimalTilt);
            double tempCoeff = GetTemperatureCoefficient(weather.Temperature);

            double overallEfficiency = weatherCoeff * orientationCoeff * tiltCoeff * tempCoeff;
            double energyKwh = powerKw * weather.SunHoursPerDay * overallEfficiency;

            return new CalculationResult
            {
                EnergyKWhPerDay = Math.Round(energyKwh, 3),
                WeatherCoefficient = weatherCoeff,
                OrientationCoefficient = orientationCoeff,
                TiltCoefficient = Math.Round(tiltCoeff, 3),
                TemperatureCoefficient = Math.Round(tempCoeff, 3),
                OptimalTiltAngle = Math.Round(optimalTilt, 1),
                OverallEfficiencyPercent = Math.Round(overallEfficiency * 100, 1)
            };
        }

        /// <summary>
        /// Обчислює оптимальний кут нахилу панелі для заданої широти.
        /// Використовує емпіричну формулу: optimalTilt ≈ latitude × 0.76 + 3.1
        /// Для України (широта ~46–52°) це дає приблизно 38–43°.
        /// </summary>
        public double GetOptimalTiltAngle(double latitude)
        {
            return Math.Abs(latitude) * 0.76 + 3.1;
        }

        /// <summary>
        /// Повертає коефіцієнт погодних умов:
        /// Сонячно = 1.0, Змінна хмарність = 0.7, Хмарно = 0.4
        /// </summary>
        private static double GetWeatherCoefficient(WeatherCondition condition)
        {
            return condition switch
            {
                WeatherCondition.Sunny => 1.0,
                WeatherCondition.PartlyCloudy => 0.7,
                WeatherCondition.Cloudy => 0.4,
                _ => 0.7
            };
        }

        /// <summary>
        /// Повертає коефіцієнт орієнтації панелі:
        /// Південь = 1.0, Схід/Захід = 0.8, Північ = 0.6
        /// </summary>
        private static double GetOrientationCoefficient(PanelOrientation orientation)
        {
            return orientation switch
            {
                PanelOrientation.South => 1.0,
                PanelOrientation.East => 0.8,
                PanelOrientation.West => 0.8,
                PanelOrientation.North => 0.6,
                _ => 1.0
            };
        }

        /// <summary>
        /// Обчислює коефіцієнт кута нахилу на основі відхилення від оптимального.
        /// Використовує косинус відхилення для плавного зменшення ефективності.
        /// При ідеальному куті K = 1.0, при відхиленні 90° — K ≈ 0.
        /// </summary>
        private static double GetTiltCoefficient(double actualTilt, double optimalTilt)
        {
            double deviationDegrees = Math.Abs(actualTilt - optimalTilt);
            double deviationRadians = deviationDegrees * Math.PI / 180.0;
            return Math.Max(0.0, Math.Cos(deviationRadians));
        }

        /// <summary>
        /// Обчислює температурний коефіцієнт.
        /// Кремнієві панелі втрачають ~0.4% ефективності на кожен °C вище 25°C.
        /// При температурі нижче 25°C коефіцієнт = 1.0 (без втрат).
        /// </summary>
        private static double GetTemperatureCoefficient(double temperature)
        {
            if (temperature <= 25.0)
                return 1.0;

            double loss = (temperature - 25.0) * 0.004;
            return Math.Max(0.5, 1.0 - loss);
        }
    }
}
