using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SolarApp.Models;
using SolarApp.ViewModels;

namespace SolarApp.Views
{
    /// <summary>
    /// Code-behind для головного вікна. Містить мінімальну логіку,
    /// яка стосується виключно візуального відображення (MVVM-підхід).
    /// Уся бізнес-логіка знаходиться в MainViewModel.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // Налаштування делегатів для діалогів (View-специфічна логіка)
            _viewModel.ShowAddPanelDialog = ShowAddPanelDialogHandler;
            _viewModel.ShowConfirmDialog = ShowConfirmDialogHandler;
            _viewModel.ShowMessageDialog = ShowMessageDialogHandler;
            _viewModel.ShowSaveFileDialog = ShowSaveFileDialogHandler;
            _viewModel.CloseApplication = () => Application.Current.Shutdown();

            // Відображення панелей на канвасі після завантаження вікна
            Loaded += (_, _) => RenderPanelsOnCanvas();

            // Оновлення канвасу при зміні колекції панелей
            _viewModel.Panels.CollectionChanged += (_, _) => RenderPanelsOnCanvas();
        }

        /// <summary>
        /// Відображає всі панелі як кнопки з іконками на канвасі карти.
        /// Очищує канвас та створює кнопку для кожної панелі з колекції ViewModel.
        /// </summary>
        private void RenderPanelsOnCanvas()
        {
            MapCanvas.Children.Clear();

            foreach (var panelVm in _viewModel.Panels)
            {
                var btn = new Button
                {
                    Width = 70,
                    Height = 50,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Tag = panelVm,
                    ToolTip = $"{panelVm.City} ({panelVm.PowerWatts}Вт)",
                    Content = new Image
                    {
                        Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/solar_panel.png")),
                        Width = 70
                    }
                };

                Canvas.SetLeft(btn, panelVm.CanvasX);
                Canvas.SetTop(btn, panelVm.CanvasY);

                btn.Click += PanelButton_Click;
                MapCanvas.Children.Add(btn);
            }
        }

        /// <summary>
        /// Обробник кліку на кнопку панелі на карті.
        /// Передає вибір у ViewModel для завантаження погоди та розрахунку.
        /// </summary>
        private async void PanelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is SolarPanelViewModel panelVm)
            {
                // Підсвітка обраної панелі
                foreach (var child in MapCanvas.Children.OfType<Button>())
                {
                    child.BorderThickness = new Thickness(0);
                    child.BorderBrush = null;
                }
                btn.BorderThickness = new Thickness(2);
                btn.BorderBrush = Brushes.Gold;

                await _viewModel.SelectPanelAsync(panelVm);
            }
        }

        /// <summary>
        /// Обробник кліку на канвас карти — для розміщення нової панелі.
        /// </summary>
        private void MapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_viewModel.IsWaitingForPlacement) return;

            Point pos = e.GetPosition(MapCanvas);
            _viewModel.PlaceOnMapCommand.Execute(pos);
        }

        /// <summary>
        /// Обробник гарячих клавіш:
        /// F1 — довідка, Escape — зняти виділення, Enter — застосувати.
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                    var helpWindow = new HelpWindow { Owner = this };
                    helpWindow.ShowDialog();
                    e.Handled = true;
                    break;

                case Key.Escape:
                    _viewModel.SelectedPanel = null;
                    _viewModel.IsWaitingForPlacement = false;
                    _viewModel.StatusText = "Оберіть сонячну панель";
                    foreach (var child in MapCanvas.Children.OfType<Button>())
                    {
                        child.BorderThickness = new Thickness(0);
                        child.BorderBrush = null;
                    }
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (_viewModel.IsPanelSelected && _viewModel.ApplySettingsCommand.CanExecute(null))
                        _viewModel.ApplySettingsCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }

        // --- Делегати діалогів (View-специфічна логіка) ---

        /// <summary>Відкриває діалог додавання панелі та повертає результат</summary>
        private (string City, double Lat, double Lon, double Power, PanelOrientation Orientation)? ShowAddPanelDialogHandler()
        {
            var dialog = new AddPanelWindow { Owner = this };
            if (dialog.ShowDialog() == true && dialog.ViewModel.IsConfirmed)
            {
                var vm = dialog.ViewModel;
                return (vm.CityName, vm.Latitude, vm.Longitude, vm.PowerWatts, vm.Orientation);
            }
            return null;
        }

        /// <summary>Відображає діалог підтвердження (Так/Ні)</summary>
        private bool ShowConfirmDialogHandler(string title, string message)
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }

        /// <summary>Відображає інформаційне повідомлення</summary>
        private void ShowMessageDialogHandler(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>Відображає діалог вибору файлу для збереження</summary>
        private string? ShowSaveFileDialogHandler()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Текстовий файл (*.txt)|*.txt|CSV файл (*.csv)|*.csv|Усі файли (*.*)|*.*",
                DefaultExt = ".txt",
                FileName = "SolarReport"
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
