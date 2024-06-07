// Структура настроек для моментов логоса
using AstraEngine;
//*******************************************************************
namespace Logos_TVD
{
    internal class TSettings_Moment
    {
        /// <summary>
        /// Имя тела, для которого будут получены моменты
        /// </summary>
        public string NameBody = "";
        /// <summary>
        /// Флаг, нужно ли получать моменты для этого тела
        /// </summary>
        public bool NeedMoment = false;
        /// <summary>
        /// Точка, относительно которой будут получены моменты
        /// </summary>
        public Vector3 PointMoment = new Vector3();
        /// <summary>
        /// Относительная площадь
        /// </summary>
        public float RefS = 1;
        /// <summary>
        /// Относительная длина
        /// </summary>
        public float RefL = 1;
    }
}
