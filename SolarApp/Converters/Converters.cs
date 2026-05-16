using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SolarApp.Converters
{
    /// <summary>
    /// Конвертує bool у Visibility: true → Visible, false → Collapsed.
    /// Використовується для показу/приховування елементів через Binding.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Visible;
        }
    }

    /// <summary>
    /// Інвертований конвертер: true → Collapsed, false → Visible.
    /// Корисний для показу "підказки" коли нічого не обрано.
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v != Visibility.Visible;
        }
    }

    /// <summary>
    /// Конвертує числове значення покращення у колір:
    /// позитивне → зелений, негативне → червоний, нуль → сірий.
    /// </summary>
    public class ImprovementToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                if (d > 0.001) return new SolidColorBrush(Colors.LimeGreen);
                if (d < -0.001) return new SolidColorBrush(Colors.Tomato);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертує значення ефективності (0–100) у ширину прямокутника для гістограми.
    /// Параметр — максимальна ширина контейнера.
    /// </summary>
    public class EnergyToBarWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double energy && parameter is string maxWidthStr &&
                double.TryParse(maxWidthStr, out double maxWidth))
            {
                double maxEnergy = 5.0;
                double width = (energy / maxEnergy) * maxWidth;
                return Math.Max(5, Math.Min(width, maxWidth));
            }
            return 5.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертує значення ефективності у колір стовпця гістограми:
    /// високе значення → зелений, середнє → жовтий, низьке → червоний.
    /// </summary>
    public class EfficiencyToBarColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double efficiency)
            {
                if (efficiency >= 70) return new SolidColorBrush(Color.FromRgb(76, 175, 80));
                if (efficiency >= 40) return new SolidColorBrush(Color.FromRgb(255, 193, 7));
                return new SolidColorBrush(Color.FromRgb(244, 67, 54));
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертує рядок у Visibility: непорожній рядок → Visible, порожній → Collapsed.
    /// Використовується для показу повідомлень про помилки.
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
