using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SolarApp.Models;

namespace SolarApp.Services
{
    /// <summary>
    /// Репозиторій для збереження та завантаження панелей у форматі JSON.
    /// Файл зберігається у папці Resources поруч із виконуваним файлом.
    /// </summary>
    public class PanelRepository : IPanelRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public PanelRepository()
        {
            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            _filePath = Path.Combine(directory, "panels.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        /// <summary>
        /// Завантажує панелі з JSON файлу. Якщо файл не існує,
        /// створює набір панелей за замовчуванням для основних міст України.
        /// </summary>
        public List<SolarPanel> LoadPanels()
        {
            if (!File.Exists(_filePath))
            {
                var defaults = CreateDefaultPanels();
                SavePanels(defaults);
                return defaults;
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<SolarPanel>>(json, _jsonOptions)
                       ?? new List<SolarPanel>();
            }
            catch (Exception)
            {
                return CreateDefaultPanels();
            }
        }

        /// <summary>
        /// Зберігає список панелей у JSON файл з форматуванням.
        /// </summary>
        public void SavePanels(List<SolarPanel> panels)
        {
            try
            {
                string json = JsonSerializer.Serialize(panels, _jsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                throw new IOException($"Помилка збереження панелей: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Експортує текстовий вміст (звіт) у вказаний файл.
        /// </summary>
        public void ExportResults(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// Створює набір панелей за замовчуванням для п'яти міст України.
        /// </summary>
        private static List<SolarPanel> CreateDefaultPanels()
        {
            return new List<SolarPanel>
            {
                new SolarPanel { City = "Львів",      Latitude = 49.84, Longitude = 24.03, PowerWatts = 350, TiltAngle = 40, Orientation = PanelOrientation.South, CanvasX = 89,  CanvasY = 224 },
                new SolarPanel { City = "Київ",       Latitude = 50.45, Longitude = 30.52, PowerWatts = 400, TiltAngle = 35, Orientation = PanelOrientation.South, CanvasX = 365, CanvasY = 203 },
                new SolarPanel { City = "Запоріжжя",  Latitude = 47.83, Longitude = 35.13, PowerWatts = 300, TiltAngle = 30, Orientation = PanelOrientation.South, CanvasX = 593, CanvasY = 351 },
                new SolarPanel { City = "Харків",     Latitude = 49.99, Longitude = 36.23, PowerWatts = 350, TiltAngle = 35, Orientation = PanelOrientation.East,  CanvasX = 634, CanvasY = 213 },
                new SolarPanel { City = "Одеса",      Latitude = 46.48, Longitude = 30.72, PowerWatts = 400, TiltAngle = 33, Orientation = PanelOrientation.South, CanvasX = 382, CanvasY = 443 }
            };
        }
    }
}
