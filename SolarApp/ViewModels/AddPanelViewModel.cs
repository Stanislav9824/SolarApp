using System.Windows.Input;
using SolarApp.Models;

namespace SolarApp.ViewModels
{
    /// <summary>
    /// ViewModel для діалогу додавання нової сонячної панелі.
    /// Містить поля вводу та валідацію даних.
    /// </summary>
    public class AddPanelViewModel : BaseViewModel
    {
        private string _cityName = string.Empty;
        private string _latitudeInput = string.Empty;
        private string _longitudeInput = string.Empty;
        private string _powerInput = "300";
        private int _selectedOrientationIndex;
        private string _errorMessage = string.Empty;

        public AddPanelViewModel()
        {
            OrientationOptions = new[]
            {
                "Південь",
                "Схід",
                "Захід",
                "Північ"
            };

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        // --- Властивості вводу ---

        /// <summary>Назва міста</summary>
        public string CityName
        {
            get => _cityName;
            set => SetProperty(ref _cityName, value);
        }

        /// <summary>Широта (текстовий ввід для валідації)</summary>
        public string LatitudeInput
        {
            get => _latitudeInput;
            set => SetProperty(ref _latitudeInput, value);
        }

        /// <summary>Довгота (текстовий ввід для валідації)</summary>
        public string LongitudeInput
        {
            get => _longitudeInput;
            set => SetProperty(ref _longitudeInput, value);
        }

        /// <summary>Потужність (текстовий ввід для валідації)</summary>
        public string PowerInput
        {
            get => _powerInput;
            set => SetProperty(ref _powerInput, value);
        }

        /// <summary>Індекс обраної орієнтації у ComboBox</summary>
        public int SelectedOrientationIndex
        {
            get => _selectedOrientationIndex;
            set => SetProperty(ref _selectedOrientationIndex, value);
        }

        /// <summary>Повідомлення про помилку валідації</summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>Варіанти орієнтації для ComboBox</summary>
        public string[] OrientationOptions { get; }

        // --- Команди ---

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // --- Результати (для передачі у MainViewModel) ---

        /// <summary>Чи був діалог підтверджений</summary>
        public bool IsConfirmed { get; private set; }

        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double PowerWatts { get; private set; }
        public PanelOrientation Orientation { get; private set; }

        /// <summary>Делегат закриття вікна (встановлюється у View)</summary>
        public System.Action<bool>? CloseAction { get; set; }

        /// <summary>
        /// Валідує дані та закриває діалог при успіху.
        /// </summary>
        private void Save(object? parameter)
        {
            // Валідація назви міста
            if (string.IsNullOrWhiteSpace(_cityName))
            {
                ErrorMessage = "Введіть назву міста.";
                return;
            }

            // Валідація широти (для України: 44–53)
            if (!double.TryParse(_latitudeInput, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double lat) ||
                lat < -90 || lat > 90)
            {
                ErrorMessage = "Широта повинна бути числом від -90 до 90.";
                return;
            }

            // Валідація довготи (для України: 22–40)
            if (!double.TryParse(_longitudeInput, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double lon) ||
                lon < -180 || lon > 180)
            {
                ErrorMessage = "Довгота повинна бути числом від -180 до 180.";
                return;
            }

            // Валідація потужності
            if (!double.TryParse(_powerInput, out double power) || power < 50 || power > 1000)
            {
                ErrorMessage = "Потужність повинна бути від 50 до 1000 Вт.";
                return;
            }

            Latitude = lat;
            Longitude = lon;
            PowerWatts = power;
            Orientation = (PanelOrientation)_selectedOrientationIndex;
            IsConfirmed = true;

            CloseAction?.Invoke(true);
        }

        /// <summary>Скасовує діалог</summary>
        private void Cancel(object? parameter)
        {
            IsConfirmed = false;
            CloseAction?.Invoke(false);
        }
    }
}
