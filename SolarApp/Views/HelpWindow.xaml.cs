using System.Windows;
using System.Windows.Input;

namespace SolarApp.Views
{
    /// <summary>
    /// Вікно довідки програми. Відкривається по клавіші F1.
    /// Містить інформацію про формули, коефіцієнти та гарячі клавіші.
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Закриває вікно довідки при натисканні Escape або F1.
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.F1)
            {
                Close();
                e.Handled = true;
            }
        }
    }
}
