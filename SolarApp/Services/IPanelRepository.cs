using System.Collections.Generic;
using SolarApp.Models;

namespace SolarApp.Services
{
    /// <summary>
    /// Інтерфейс репозиторію для збереження та завантаження сонячних панелей.
    /// </summary>
    public interface IPanelRepository
    {
        /// <summary>Завантажує список панелей з файлу</summary>
        List<SolarPanel> LoadPanels();

        /// <summary>Зберігає список панелей у файл</summary>
        void SavePanels(List<SolarPanel> panels);

        /// <summary>Експортує результати розрахунків у текстовий файл</summary>
        void ExportResults(string filePath, string content);
    }
}
