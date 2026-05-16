using System;
using SolarApp.Models;

namespace SolarApp.ViewModels
{
    /// <summary>
    /// ViewModel для окремої сонячної панелі.
    /// Обгортає модель SolarPanel та зберігає результати розрахунків.
    /// Підтримує прив'язку даних через INotifyPropertyChanged.
    /// </summary>
    public class SolarPanelViewModel : BaseViewModel
    {
        private readonly SolarPanel _model;

        // Погодні дані
        private double _temperature;
        private int _cloudCoverPercent;
        private string _weatherConditionText = "Невідомо";
        private double _sunHoursPerDay;
        private bool _isWeatherLoaded;

        // Результати розрахунку
        private double _energyKWhPerDay;
        private double _weatherCoefficient;
        private double _orientationCoefficient;
        private double _tiltCoefficient;
        private double _temperatureCoefficient;
        private double _optimalTiltAngle;
        private double _overallEfficiencyPercent;
        private double _initialEnergyKWh = -1;

        public SolarPanelViewModel(SolarPanel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>Повертає модель даних для збереження</summary>
        public SolarPanel Model => _model;

        // --- Властивості панелі (прив'язані до моделі) ---

        public string City
        {
            get => _model.City;
            set { _model.City = value; OnPropertyChanged(); }
        }

        public double Latitude
        {
            get => _model.Latitude;
            set { _model.Latitude = value; OnPropertyChanged(); }
        }

        public double Longitude
        {
            get => _model.Longitude;
            set { _model.Longitude = value; OnPropertyChanged(); }
        }

        public double PowerWatts
        {
            get => _model.PowerWatts;
            set
            {
                if (value < 50) value = 50;
                if (value > 1000) value = 1000;
                _model.PowerWatts = value;
                OnPropertyChanged();
            }
        }

        public double TiltAngle
        {
            get => _model.TiltAngle;
            set
            {
                if (value < 0) value = 0;
                if (value > 60) value = 60;
                _model.TiltAngle = value;
                OnPropertyChanged();
            }
        }

        public PanelOrientation Orientation
        {
            get => _model.Orientation;
            set { _model.Orientation = value; OnPropertyChanged(); }
        }

        public double CanvasX
        {
            get => _model.CanvasX;
            set { _model.CanvasX = value; OnPropertyChanged(); }
        }

        public double CanvasY
        {
            get => _model.CanvasY;
            set { _model.CanvasY = value; OnPropertyChanged(); }
        }

        // --- Погодні дані ---

        public double Temperature
        {
            get => _temperature;
            set => SetProperty(ref _temperature, value);
        }

        public int CloudCoverPercent
        {
            get => _cloudCoverPercent;
            set => SetProperty(ref _cloudCoverPercent, value);
        }

        public string WeatherConditionText
        {
            get => _weatherConditionText;
            set => SetProperty(ref _weatherConditionText, value);
        }

        public double SunHoursPerDay
        {
            get => _sunHoursPerDay;
            set => SetProperty(ref _sunHoursPerDay, value);
        }

        public bool IsWeatherLoaded
        {
            get => _isWeatherLoaded;
            set => SetProperty(ref _isWeatherLoaded, value);
        }

        // --- Результати розрахунку ---

        public double EnergyKWhPerDay
        {
            get => _energyKWhPerDay;
            set => SetProperty(ref _energyKWhPerDay, value);
        }

        public double WeatherCoefficient
        {
            get => _weatherCoefficient;
            set => SetProperty(ref _weatherCoefficient, value);
        }

        public double OrientationCoefficient
        {
            get => _orientationCoefficient;
            set => SetProperty(ref _orientationCoefficient, value);
        }

        public double TiltCoefficient
        {
            get => _tiltCoefficient;
            set => SetProperty(ref _tiltCoefficient, value);
        }

        public double TemperatureCoefficient
        {
            get => _temperatureCoefficient;
            set => SetProperty(ref _temperatureCoefficient, value);
        }

        public double OptimalTiltAngle
        {
            get => _optimalTiltAngle;
            set => SetProperty(ref _optimalTiltAngle, value);
        }

        public double OverallEfficiencyPercent
        {
            get => _overallEfficiencyPercent;
            set => SetProperty(ref _overallEfficiencyPercent, value);
        }

        public double InitialEnergyKWh
        {
            get => _initialEnergyKWh;
            set => SetProperty(ref _initialEnergyKWh, value);
        }

        /// <summary>Різниця між поточною та початковою енергією</summary>
        public double EnergyImprovement =>
            _initialEnergyKWh >= 0 ? _energyKWhPerDay - _initialEnergyKWh : 0;

        /// <summary>
        /// Оновлює результати розрахунку з об'єкта CalculationResult.
        /// Якщо це перший розрахунок — зберігає початкове значення енергії.
        /// </summary>
        public void ApplyCalculationResult(CalculationResult result)
        {
            EnergyKWhPerDay = result.EnergyKWhPerDay;
            WeatherCoefficient = result.WeatherCoefficient;
            OrientationCoefficient = result.OrientationCoefficient;
            TiltCoefficient = result.TiltCoefficient;
            TemperatureCoefficient = result.TemperatureCoefficient;
            OptimalTiltAngle = result.OptimalTiltAngle;
            OverallEfficiencyPercent = result.OverallEfficiencyPercent;

            if (_initialEnergyKWh < 0)
                _initialEnergyKWh = result.EnergyKWhPerDay;

            OnPropertyChanged(nameof(EnergyImprovement));
        }

        /// <summary>
        /// Оновлює погодні дані з об'єкта WeatherData.
        /// </summary>
        public void ApplyWeatherData(WeatherData weather)
        {
            Temperature = weather.Temperature;
            CloudCoverPercent = weather.CloudCoverPercent;
            SunHoursPerDay = weather.SunHoursPerDay;
            WeatherConditionText = weather.Condition switch
            {
                WeatherCondition.Sunny => "Сонячно",
                WeatherCondition.PartlyCloudy => "Змінна хмарність",
                WeatherCondition.Cloudy => "Хмарно",
                _ => "Невідомо"
            };
            IsWeatherLoaded = true;
        }
    }
}
