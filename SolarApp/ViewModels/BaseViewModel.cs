using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SolarApp.ViewModels
{
    /// <summary>
    /// Базовий клас для всіх ViewModel.
    /// Реалізує INotifyPropertyChanged для підтримки прив'язки даних (Data Binding).
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Подія, що сповіщає View про зміну властивості.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Викликає подію PropertyChanged для вказаної властивості.
        /// Атрибут CallerMemberName автоматично підставляє ім'я властивості.
        /// </summary>
        /// <param name="propertyName">Назва зміненої властивості</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Встановлює значення поля та сповіщає View про зміну, якщо значення дійсно змінилось.
        /// Повертає true, якщо значення було змінено.
        /// </summary>
        /// <typeparam name="T">Тип властивості</typeparam>
        /// <param name="field">Посилання на поле</param>
        /// <param name="value">Нове значення</param>
        /// <param name="propertyName">Назва властивості (підставляється автоматично)</param>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
