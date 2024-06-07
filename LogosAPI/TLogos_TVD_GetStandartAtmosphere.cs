// Класс, хранящий методы работы с таблицей стандартной атмосферы
//
using AstraEngine.Components.MathHelper;
//**********************************************************
namespace Logos_TVD
{
    public partial class TLogos_TVD_StandartAtmosphere
    {
        private int Count = 143;
        //---------------------------------------------------------------
        /// <summary>
        /// Получить высоту при заданной плотности воздуха
        /// </summary>
        /// <param name="p">Интересующая плотность воздуха</param>
        /// <returns>высота при заданной плотности</returns>
        public float GetAltitudeFromDensity(float p)
        {
            float[] massivAltitude = Altitude();
            float[] massivDensity = Density();
            return TLinearyInterpolation.Interpolate(massivDensity, massivAltitude, Count, p);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получить давление на заданной высоте
        /// </summary>
        /// <param name="H">Интересующая высота</param>
        /// <returns>давление на заданной высоте</returns>
        public float GetPressure(float H)
        {
            float[] massivAltitude = Altitude();
            float[] massivPressure = Pressure();
            return TLinearyInterpolation.Interpolate(massivAltitude, massivPressure, Count, H);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получить плотность воздуха на заданной высоте
        /// </summary>
        /// <param name="H">Интересующая высота</param>
        /// <returns>плотность воздуха на заданной высоте</returns>
        public float GetDensity(float H)
        {
            float[] massivAltitude = Altitude();
            float[] massivDensity = Density();
            return TLinearyInterpolation.Interpolate(massivAltitude, massivDensity, Count, H);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получить скорость звука на заданной высоте
        /// </summary>
        /// <param name="H">Интересующая высота</param>
        /// <returns>скорость звука на заданной высоте</returns>
        public float GetSpeedOfSound(float H)
        {
            float[] massivAltitude = Altitude();
            float[] massivSpeedOfSound = SpeedOfSound();
            return TLinearyInterpolation.Interpolate(massivAltitude, massivSpeedOfSound, Count, H);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получить ускорение свободного падения на заданной высоте
        /// </summary>
        /// <param name="H">Интересующая высота</param>
        /// <returns>ускорение свободного падения на заданной высоте</returns>
        public float GetAccelerationOfGravity(float H)
        {
            float[] massivAltitude = Altitude();
            float[] massivAccelerationOfGravity = AccelerationOfGravity();
            return TLinearyInterpolation.Interpolate(massivAltitude, massivAccelerationOfGravity, Count, H);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получить температуру в градусах Цельсия на заданной высоте
        /// </summary>
        /// <param name="H">Интересующая высота</param>
        /// <returns>температура в градусах Цельсия на заданной высоте</returns>
        public float GetTemperatureOfCelsius(float H)
        {
            float[] massivAltitude = Altitude();
            float[] massivTemperatureOfCelsius = TemperatureOfCelsius();
            return TLinearyInterpolation.Interpolate(massivAltitude, massivTemperatureOfCelsius, Count, H);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получить температуру в градусах Кельвина на заданной высоте
        /// </summary>
        /// <param name="H">Интересующая высота</param>
        /// <returns>температура в градусах Кельвина на заданной высоте</returns>
        public float GetTemperatureOfKelvin(float H)
        {
            float[] massivAltitude = Altitude();
            float[] massivTemperatureOfKelvin = TemperatureOfKelvin();
            return TLinearyInterpolation.Interpolate(massivAltitude, massivTemperatureOfKelvin, Count, H);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получить высоту при заданном давлении
        /// </summary>
        /// <param name="P">давление на заданной высоте</param>
        /// <returns>высота</returns>
        public float GetAltitudeFromPressure(float P)
        {
            float[] massivAltitude = Altitude();
            float[] massivPressure = Pressure();
            return TLinearyInterpolation.Interpolate(massivPressure, massivAltitude, Count, P);
        }
        //---------------------------------------------------------------
    }
}
