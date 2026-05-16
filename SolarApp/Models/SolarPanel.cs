using System.Text.Json.Serialization;

namespace SolarApp.Models
{
    /// <summary>
    /// Модель сонячної панелі з усіма фізичними та географічними параметрами.
    /// Використовується для збереження/завантаження даних та передачі в калькулятор.
    /// </summary>
    public class SolarPanel
    {
        /// <summary>Назва міста розташування панелі</summary>
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        /// <summary>Географічна широта (градуси)</summary>
        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        /// <summary>Географічна довгота (градуси)</summary>
        [JsonPropertyName("lon")]
        public double Longitude { get; set; }

        /// <summary>Номінальна потужність панелі (Вт)</summary>
        [JsonPropertyName("powerWatts")]
        public double PowerWatts { get; set; } = 300;

        /// <summary>Кут нахилу панелі (0–60 градусів)</summary>
        [JsonPropertyName("tiltAngle")]
        public double TiltAngle { get; set; }

        /// <summary>Орієнтація панелі відносно сторін світу</summary>
        [JsonPropertyName("orientation")]
        public PanelOrientation Orientation { get; set; } = PanelOrientation.South;

        /// <summary>Позиція X на канвасі карти (пікселі)</summary>
        [JsonPropertyName("canvasX")]
        public double CanvasX { get; set; }

        /// <summary>Позиція Y на канвасі карти (пікселі)</summary>
        [JsonPropertyName("canvasY")]
        public double CanvasY { get; set; }
    }
}
