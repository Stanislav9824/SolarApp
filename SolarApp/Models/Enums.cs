namespace SolarApp.Models
{
    /// <summary>
    /// Орієнтація сонячної панелі відносно сторін світу.
    /// Кожна орієнтація має свій коефіцієнт ефективності.
    /// </summary>
    public enum PanelOrientation
    {
        /// <summary>Південь — найефективніша орієнтація (коефіцієнт 1.0)</summary>
        South,

        /// <summary>Схід — помірна ефективність (коефіцієнт 0.8)</summary>
        East,

        /// <summary>Захід — помірна ефективність (коефіцієнт 0.8)</summary>
        West,

        /// <summary>Північ — найменш ефективна орієнтація (коефіцієнт 0.6)</summary>
        North
    }

    /// <summary>
    /// Погодні умови, що впливають на продуктивність сонячних панелей.
    /// </summary>
    public enum WeatherCondition
    {
        /// <summary>Сонячно — коефіцієнт 1.0</summary>
        Sunny,

        /// <summary>Змінна хмарність — коефіцієнт 0.7</summary>
        PartlyCloudy,

        /// <summary>Хмарно — коефіцієнт 0.4</summary>
        Cloudy
    }
}
