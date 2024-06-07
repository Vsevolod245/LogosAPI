// Методы которые определяют настройки, которые должен задать пользователь
using System.Windows.Forms;
//****************************************************************
namespace Logos_TVD
{
    public partial class TLogos_TVD
    {
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Спросить пользователя о потребности перещемения химер
        /// </summary>
        public void AskAboutChimeraMovement()
        {
            // Проверка на наличие запрета на вызов Message box
            if (MessageBoxCallLocked) return;

            string title = "Перемещение химер";
            string message = "Потребуется ли перемещать химеры в данном расчете?";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.Yes)
            {
                ChimeraMovement = true;
            }
            else
            {
                ChimeraMovement = false;
            }
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Спросить пользователя о потребности варьировать высоту и число маха
        /// </summary>
        public void AskAboutHeightAndMachVariation()
        {
            // Проверка на наличие запрета на вызов Message box
            if (MessageBoxCallLocked) return;

            string title = "Высота и число Маха";
            string message = "Потребуется ли варьировать высоту и число Маха?";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.Yes)
            {
                HeightAndMachVariation = true;
            }
            else
            {
                HeightAndMachVariation = false;
            }
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
