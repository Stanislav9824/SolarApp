using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SolarApp.Models;
using SolarApp.Services;

namespace SolarApp.ViewModels
{
    /// <summary>
    /// Головний ViewModel додатку. Керує колекцією панелей, обраною панеллю,
    /// глобальною статистикою мережі та командами користувача.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        // Сервіси (розділення відповідальностей — принцип SOLID)
        private readonly IWeatherService _weatherService;
        private readonly IEnergyCalculator _energyCalculator;
        private readonly IPanelRepository _panelRepository;

        // Стан обраної панелі
        private SolarPanelViewModel? _selectedPanel;
        private bool _isPanelSelected;
        private string _tiltInput = "0";
        private string _powerInput = "300";
        private int _selectedOrientationIndex;

        // Стан додавання панелі
        private bool _isWaitingForPlacement;
        private SolarPanel? _pendingPanel;

        // Глобальна статистика
        private double _averageEfficiency;
        private double _totalEnergyKWh;
        private string _networkStatusText = "Очікуємо дані...";
        private string _statusText = "Оберіть сонячну панель";

        // Стан завантаження
        private bool _isLoading;

        public MainViewModel()
        {
            _weatherService = new WeatherService();
            _energyCalculator = new EnergyCalculator();
            _panelRepository = new PanelRepository();

            Panels = new ObservableCollection<SolarPanelViewModel>();
            OrientationOptions = new ObservableCollection<string>
            {
                "Південь (1.0)",
                "Схід (0.8)",
                "Захід (0.8)",
                "Північ (0.6)"
            };

            // Ініціалізація команд
            ApplySettingsCommand = new AsyncRelayCommand(ApplySettingsAsync, _ => _selectedPanel != null && !_isLoading);
            DeletePanelCommand = new RelayCommand(DeletePanel, _ => _selectedPanel != null);
            StartAddPanelCommand = new RelayCommand(StartAddPanel);
            FinishCommand = new RelayCommand(FinishOptimization);
            ExportResultsCommand = new RelayCommand(ExportResults, _ => Panels.Count > 0);
            PlaceOnMapCommand = new RelayCommand(PlaceOnMap);

            LoadPanelsFromFile();
        }

        // --- Колекції ---

        /// <summary>Колекція всіх сонячних панелей на карті</summary>
        public ObservableCollection<SolarPanelViewModel> Panels { get; }

        /// <summary>Варіанти орієнтації для ComboBox</summary>
        public ObservableCollection<string> OrientationOptions { get; }

        // --- Команди ---

        /// <summary>Застосувати налаштування обраної панелі та перерахувати</summary>
        public ICommand ApplySettingsCommand { get; }

        /// <summary>Видалити обрану панель</summary>
        public ICommand DeletePanelCommand { get; }

        /// <summary>Почати процес додавання нової панелі</summary>
        public ICommand StartAddPanelCommand { get; }

        /// <summary>Завершити оптимізацію</summary>
        public ICommand FinishCommand { get; }

        /// <summary>Експортувати результати у файл</summary>
        public ICommand ExportResultsCommand { get; }

        /// <summary>Розмістити панель на карті (виклик при кліку на Canvas)</summary>
        public ICommand PlaceOnMapCommand { get; }

        // --- Властивості стану ---

        public SolarPanelViewModel? SelectedPanel
        {
            get => _selectedPanel;
            set
            {
                if (SetProperty(ref _selectedPanel, value))
                {
                    IsPanelSelected = value != null;
                    if (value != null)
                    {
                        TiltInput = value.TiltAngle.ToString();
                        PowerInput = value.PowerWatts.ToString();
                        SelectedOrientationIndex = (int)value.Orientation;
                    }
                }
            }
        }

        public bool IsPanelSelected
        {
            get => _isPanelSelected;
            set => SetProperty(ref _isPanelSelected, value);
        }

        public string TiltInput
        {
            get => _tiltInput;
            set => SetProperty(ref _tiltInput, value);
        }

        public string PowerInput
        {
            get => _powerInput;
            set => SetProperty(ref _powerInput, value);
        }

        public int SelectedOrientationIndex
        {
            get => _selectedOrientationIndex;
            set => SetProperty(ref _selectedOrientationIndex, value);
        }

        public bool IsWaitingForPlacement
        {
            get => _isWaitingForPlacement;
            set => SetProperty(ref _isWaitingForPlacement, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        // --- Глобальна статистика ---

        public double AverageEfficiency
        {
            get => _averageEfficiency;
            set => SetProperty(ref _averageEfficiency, value);
        }

        public double TotalEnergyKWh
        {
            get => _totalEnergyKWh;
            set => SetProperty(ref _totalEnergyKWh, value);
        }

        public string NetworkStatusText
        {
            get => _networkStatusText;
            set => SetProperty(ref _networkStatusText, value);
        }

        // --- Делегати подій для View (мінімальний зв'язок з View) ---

        /// <summary>Делегат для відображення діалогу додавання панелі</summary>
        public Func<(string City, double Lat, double Lon, double Power, PanelOrientation Orientation)?>? ShowAddPanelDialog { get; set; }

        /// <summary>Делегат для відображення підтвердження</summary>
        public Func<string, string, bool>? ShowConfirmDialog { get; set; }

        /// <summary>Делегат для відображення повідомлення</summary>
        public Action<string, string>? ShowMessageDialog { get; set; }

        /// <summary>Делегат для вибору файлу для збереження</summary>
        public Func<string?>? ShowSaveFileDialog { get; set; }

        /// <summary>Делегат для закриття додатку</summary>
        public Action? CloseApplication { get; set; }

        // --- Методи ---

        /// <summary>
        /// Завантажує панелі з JSON файлу та створює ViewModel для кожної.
        /// </summary>
        private void LoadPanelsFromFile()
        {
            try
            {
                var models = _panelRepository.LoadPanels();
                foreach (var model in models)
                {
                    Panels.Add(new SolarPanelViewModel(model));
                }
            }
            catch (Exception ex)
            {
                ShowMessageDialog?.Invoke("Помилка завантаження", $"Не вдалося завантажити панелі: {ex.Message}");
            }
        }

        /// <summary>
        /// Зберігає поточний стан усіх панелей у JSON файл.
        /// </summary>
        private void SavePanelsToFile()
        {
            try
            {
                var models = Panels.Select(p => p.Model).ToList();
                _panelRepository.SavePanels(models);
            }
            catch (Exception ex)
            {
                ShowMessageDialog?.Invoke("Помилка збереження", ex.Message);
            }
        }

        /// <summary>
        /// Обирає панель та завантажує для неї погоду і розрахунки.
        /// Викликається при кліку на панель на карті.
        /// </summary>
        public async Task SelectPanelAsync(SolarPanelViewModel panel)
        {
            SelectedPanel = panel;
            StatusText = $"Завантаження погоди для {panel.City}...";
            IsLoading = true;

            try
            {
                var weather = await _weatherService.GetWeatherAsync(panel.Latitude, panel.Longitude);
                panel.ApplyWeatherData(weather);

                var result = _energyCalculator.Calculate(panel.Model, weather);
                panel.ApplyCalculationResult(result);

                StatusText = $"Дані для {panel.City} завантажено";
            }
            catch (Exception)
            {
                StatusText = "Помилка отримання погоди. Перевірте з'єднання.";
            }
            finally
            {
                IsLoading = false;
                UpdateGlobalStats();
            }
        }

        /// <summary>
        /// Застосовує нові налаштування (кут, потужність, орієнтація) та перераховує.
        /// </summary>
        private async Task ApplySettingsAsync(object? parameter)
        {
            if (_selectedPanel == null) return;

            // Валідація куту нахилу
            if (!double.TryParse(TiltInput, out double newTilt) || newTilt < 0 || newTilt > 60)
            {
                ShowMessageDialog?.Invoke("Помилка вводу", "Кут нахилу повинен бути числом від 0 до 60 градусів.");
                return;
            }

            // Валідація потужності
            if (!double.TryParse(PowerInput, out double newPower) || newPower < 50 || newPower > 1000)
            {
                ShowMessageDialog?.Invoke("Помилка вводу", "Потужність повинна бути числом від 50 до 1000 Вт.");
                return;
            }

            _selectedPanel.TiltAngle = newTilt;
            _selectedPanel.PowerWatts = newPower;
            _selectedPanel.Orientation = (PanelOrientation)SelectedOrientationIndex;

            IsLoading = true;
            StatusText = $"Перерахунок для {_selectedPanel.City}...";

            try
            {
                WeatherData weather;
                if (_selectedPanel.IsWeatherLoaded)
                {
                    weather = new WeatherData
                    {
                        Temperature = _selectedPanel.Temperature,
                        CloudCoverPercent = _selectedPanel.CloudCoverPercent,
                        SunHoursPerDay = _selectedPanel.SunHoursPerDay,
                        Condition = ClassifyWeatherFromCloud(_selectedPanel.CloudCoverPercent)
                    };
                }
                else
                {
                    weather = await _weatherService.GetWeatherAsync(
                        _selectedPanel.Latitude, _selectedPanel.Longitude);
                    _selectedPanel.ApplyWeatherData(weather);
                }

                var result = _energyCalculator.Calculate(_selectedPanel.Model, weather);
                _selectedPanel.ApplyCalculationResult(result);

                StatusText = $"Розрахунок для {_selectedPanel.City} оновлено";
            }
            catch (Exception)
            {
                StatusText = "Помилка перерахунку. Перевірте з'єднання.";
            }
            finally
            {
                IsLoading = false;
                SavePanelsToFile();
                UpdateGlobalStats();
            }
        }

        /// <summary>
        /// Видаляє обрану панель після підтвердження користувачем.
        /// </summary>
        private void DeletePanel(object? parameter)
        {
            if (_selectedPanel == null) return;

            bool confirmed = ShowConfirmDialog?.Invoke(
                "Підтвердження видалення",
                $"Ви дійсно хочете видалити панель у м. {_selectedPanel.City}?") ?? false;

            if (!confirmed) return;

            Panels.Remove(_selectedPanel);
            SelectedPanel = null;
            StatusText = "Панель видалено";

            SavePanelsToFile();
            UpdateGlobalStats();
        }

        /// <summary>
        /// Починає процес додавання нової панелі: відкриває діалог,
        /// потім чекає кліку на карту для розміщення.
        /// </summary>
        private void StartAddPanel(object? parameter)
        {
            var dialogResult = ShowAddPanelDialog?.Invoke();
            if (dialogResult == null) return;

            var (city, lat, lon, power, orientation) = dialogResult.Value;

            _pendingPanel = new SolarPanel
            {
                City = city,
                Latitude = lat,
                Longitude = lon,
                PowerWatts = power,
                TiltAngle = 0,
                Orientation = orientation
            };

            IsWaitingForPlacement = true;
            StatusText = "Клікніть на карту, щоб розмістити панель";
        }

        /// <summary>
        /// Розміщує нову панель на карті за координатами кліку.
        /// Параметр — об'єкт System.Windows.Point з координатами.
        /// </summary>
        private void PlaceOnMap(object? parameter)
        {
            if (!_isWaitingForPlacement || _pendingPanel == null) return;

            if (parameter is System.Windows.Point pos)
            {
                _pendingPanel.CanvasX = pos.X - 35;
                _pendingPanel.CanvasY = pos.Y - 25;

                var vm = new SolarPanelViewModel(_pendingPanel);
                Panels.Add(vm);

                IsWaitingForPlacement = false;
                _pendingPanel = null;
                StatusText = "Панель додано! Клікніть на неї для розрахунку.";

                SavePanelsToFile();
            }
        }

        /// <summary>
        /// Завершує процес оптимізації та показує фінальний результат.
        /// </summary>
        private void FinishOptimization(object? parameter)
        {
            if (Panels.Count == 0)
            {
                ShowMessageDialog?.Invoke("Увага", "Немає панелей для аналізу.");
                return;
            }

            var loaded = Panels.Where(p => p.IsWeatherLoaded).ToList();
            if (loaded.Count == 0)
            {
                ShowMessageDialog?.Invoke("Увага",
                    "Спочатку оберіть хоча б одну панель для завантаження даних.");
                return;
            }

            double totalEnergy = loaded.Sum(p => p.EnergyKWhPerDay);
            double avgEff = loaded.Average(p => p.OverallEfficiencyPercent);

            var sb = new StringBuilder();
            sb.AppendLine($"Загальна енергія мережі: {totalEnergy:F3} кВт·год/день");
            sb.AppendLine($"Середня ефективність: {avgEff:F1}%");
            sb.AppendLine($"Кількість панелей: {loaded.Count}");
            sb.AppendLine();

            foreach (var p in loaded)
            {
                double improvement = p.EnergyImprovement;
                string sign = improvement >= 0 ? "+" : "";
                sb.AppendLine($"  {p.City}: {p.EnergyKWhPerDay:F3} кВт·год/день ({sign}{improvement:F3})");
            }

            ShowMessageDialog?.Invoke("Результати оптимізації", sb.ToString());
        }

        /// <summary>
        /// Експортує результати розрахунків у текстовий файл.
        /// </summary>
        private void ExportResults(object? parameter)
        {
            string? filePath = ShowSaveFileDialog?.Invoke();
            if (filePath == null) return;

            var sb = new StringBuilder();
            sb.AppendLine("============================================");
            sb.AppendLine("  ЗВІТ — Solar Strategy");
            sb.AppendLine($"  Дата: {DateTime.Now:dd.MM.yyyy HH:mm}");
            sb.AppendLine("============================================");
            sb.AppendLine();

            foreach (var panel in Panels)
            {
                sb.AppendLine($"Місто: {panel.City}");
                sb.AppendLine($"  Координати: {panel.Latitude:F2}°N, {panel.Longitude:F2}°E");
                sb.AppendLine($"  Потужність: {panel.PowerWatts} Вт");
                sb.AppendLine($"  Кут нахилу: {panel.TiltAngle}° (оптимальний: {panel.OptimalTiltAngle}°)");
                sb.AppendLine($"  Орієнтація: {GetOrientationName(panel.Orientation)}");

                if (panel.IsWeatherLoaded)
                {
                    sb.AppendLine($"  Температура: {panel.Temperature}°C");
                    sb.AppendLine($"  Погода: {panel.WeatherConditionText} ({panel.CloudCoverPercent}%)");
                    sb.AppendLine($"  Сонячних годин: {panel.SunHoursPerDay}");
                    sb.AppendLine($"  --- Коефіцієнти ---");
                    sb.AppendLine($"  K погоди: {panel.WeatherCoefficient}");
                    sb.AppendLine($"  K орієнтації: {panel.OrientationCoefficient}");
                    sb.AppendLine($"  K нахилу: {panel.TiltCoefficient}");
                    sb.AppendLine($"  K температури: {panel.TemperatureCoefficient}");
                    sb.AppendLine($"  Загальна ефективність: {panel.OverallEfficiencyPercent}%");
                    sb.AppendLine($"  ЕНЕРГІЯ: {panel.EnergyKWhPerDay:F3} кВт·год/день");
                }
                else
                {
                    sb.AppendLine("  (Погодні дані не завантажено)");
                }

                sb.AppendLine();
            }

            var loaded = Panels.Where(p => p.IsWeatherLoaded).ToList();
            if (loaded.Count > 0)
            {
                sb.AppendLine("============================================");
                sb.AppendLine($"  ЗАГАЛЬНА ЕНЕРГІЯ: {loaded.Sum(p => p.EnergyKWhPerDay):F3} кВт·год/день");
                sb.AppendLine($"  СЕРЕДНЯ ЕФЕКТИВНІСТЬ: {loaded.Average(p => p.OverallEfficiencyPercent):F1}%");
                sb.AppendLine("============================================");
            }

            try
            {
                _panelRepository.ExportResults(filePath, sb.ToString());
                ShowMessageDialog?.Invoke("Експорт", $"Звіт збережено у файл:\n{filePath}");
            }
            catch (Exception ex)
            {
                ShowMessageDialog?.Invoke("Помилка", $"Не вдалося зберегти файл: {ex.Message}");
            }
        }

        /// <summary>
        /// Оновлює глобальну статистику мережі (середня ефективність, сумарна енергія).
        /// </summary>
        private void UpdateGlobalStats()
        {
            var loaded = Panels.Where(p => p.IsWeatherLoaded).ToList();
            if (loaded.Count == 0)
            {
                AverageEfficiency = 0;
                TotalEnergyKWh = 0;
                NetworkStatusText = "Очікуємо дані...";
                return;
            }

            AverageEfficiency = Math.Round(loaded.Average(p => p.OverallEfficiencyPercent), 1);
            TotalEnergyKWh = Math.Round(loaded.Sum(p => p.EnergyKWhPerDay), 3);

            double totalImprovement = loaded.Sum(p => p.EnergyImprovement);
            string sign = totalImprovement >= 0 ? "+" : "";
            NetworkStatusText = $"Зміна: {sign}{totalImprovement:F3} кВт·год/день";
        }

        /// <summary>Повертає українську назву орієнтації</summary>
        private static string GetOrientationName(PanelOrientation orientation)
        {
            return orientation switch
            {
                PanelOrientation.South => "Південь",
                PanelOrientation.East => "Схід",
                PanelOrientation.West => "Захід",
                PanelOrientation.North => "Північ",
                _ => "Невідомо"
            };
        }

        /// <summary>Класифікує погоду за хмарністю (для повторного розрахунку)</summary>
        private static WeatherCondition ClassifyWeatherFromCloud(int cloudPercent)
        {
            if (cloudPercent <= 25) return WeatherCondition.Sunny;
            if (cloudPercent <= 75) return WeatherCondition.PartlyCloudy;
            return WeatherCondition.Cloudy;
        }
    }
}
