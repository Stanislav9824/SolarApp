using System.Windows;
using System.Windows.Input;
using SolarApp.ViewModels;

namespace SolarApp.Views
{
    /// <summary>
    /// Діалогове вікно для додавання нової сонячної панелі.
    /// Використовує AddPanelViewModel для валідації та збереження даних.
    /// </summary>
    public partial class AddPanelWindow : Window
    {
        public AddPanelViewModel ViewModel { get; }

        public AddPanelWindow()
        {
            InitializeComponent();

            ViewModel = new AddPanelViewModel();
            DataContext = ViewModel;

            // Зв'язуємо закриття вікна з ViewModel
            ViewModel.CloseAction = (result) =>
            {
                DialogResult = result;
                Close();
            };
        }

        /// <summary>
        /// Обробник гарячих клавіш: Enter — зберегти, Escape — скасувати.
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ViewModel.SaveCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Escape:
                    ViewModel.CancelCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }
    }
}
