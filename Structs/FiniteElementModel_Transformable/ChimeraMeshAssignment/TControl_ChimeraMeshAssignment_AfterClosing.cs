// Класс, содержащий методы, вызывающиеся после закрытия окна присвоения химер объектам
using System;
using System.Linq;
using System.Windows.Forms;
//
using AstraEngine.Components.MathHelper;
//
using MPT707.Aerodynamics.Logos_TVD;
//******************************************************************
namespace Example
{
    internal partial class TControl_ChimeraMeshAssignment
    {
        /// <summary>
        /// Объект класса для записи связи химер с объектами в файл и чтения из файла
        /// </summary>
        TChimeraMeshAssignment_History History = new TChimeraMeshAssignment_History();
//---------------------------------------------------------------
        /// <summary>
        /// Очищение всех химер и объектов с экрана
        /// </summary>
//        public void AfterClosing()
//        {
//            // Очищаем все химеры и объекты
//            Visualizer.DisposeAll();
//        }
////---------------------------------------------------------------
//        /// <summary>
//        /// Начать трансформацию химер
//        /// </summary>
//        public void StartTransform()
//        {
//            AstraEngine.Matrix M = new AstraEngine.Matrix();
//            M = AstraEngine.Matrix.CreateRotationZ(TMath.Angle_ToRadians(90));
//            FiniteEM_T.GlobalTransform(M, 2);
//            FiniteEM_T.WriteNgeom("D:\\Study\\Laba\\Files\\RaeChim\\RaeChim_out.1.ngeom");
//        }
////---------------------------------------------------------------
//        /// <summary>
//        /// Сохранить информацию о связи химер с объектами и объектов друг с другом
//        /// </summary>
//        public void SaveConnection(TLogos_TVD Logos_TVD)
//        {
//            History.SaveInFile(Logos_TVD.UID, Logos_TVD.DependencyTable);
//            History.ReadFromFile(Logos_TVD.UID, Logos_TVD.DependencyTable);
//        }
//---------------------------------------------------------------
    }
}
