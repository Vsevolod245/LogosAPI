// Класс предназначен для непрерывного обновления API в отдельном потоке
using System.Threading;
//******************************************
namespace Logos_TVD
{
    public class TAPI_Updater
    {
        /// <summary>
        /// Api в котором вызываем метод обновления
        /// </summary>
        public TLogos_TVD API;
        /// <summary>
        /// Задержка между циклами обновления
        /// </summary>
        public int Delay = 100;
        /// <summary>
        /// Крутить ли цикл обновления
        /// </summary>
        public bool IsUpdate = false;
        /// <summary>
        /// Метод вызывающий обновление Api
        /// </summary>
        public void UpdateLoop()
        {
            try
            {
                while (IsUpdate)
                {
                    Thread.Sleep(Delay);
                    this.API.Update();
                }
            }
            catch { }
        }
        /// <summary>
        /// Старт цикла обновления в отдельном потоке
        /// </summary>
        public void Start()
        {
            if (IsUpdate) return;
            IsUpdate = true;
            UpdateLoop();
        }
        /// <summary>
        /// Остановка цикла обновления
        /// </summary>
        public void Stop()
        {
            if (!IsUpdate) return;
            IsUpdate = false;
        }
        /// <summary>
        /// Конструктор принимающий в себя ссылку на объект класса API, который будем обновлять
        /// </summary>
        /// <param name="API"></param>
        public TAPI_Updater(TLogos_TVD API)
        { 
            this.API = API;
        }
        /// <summary>
        /// Метод удаления связей с АПИ и прекращение обновления
        /// </summary>
        public void Dispose()
        {
            IsUpdate = false;
            this.API = null;
        }
    }
}
