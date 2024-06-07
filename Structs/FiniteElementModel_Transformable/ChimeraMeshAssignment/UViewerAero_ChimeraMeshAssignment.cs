// Окно для присваивания химер объектам и связывания объектов друг с другом
using System;
using System.Windows.Forms;
//******************************************************************
namespace Example
{
    public partial class UViewerAero_ChimeraMeshAssignment : Form
    {
        TControl_ChimeraMeshAssignment Control_ChimeraMeshAssignment;
        public UViewerAero_ChimeraMeshAssignment(TFiniteElementModel_Transformable FiniteEM_T)
        {
            InitializeComponent();
            // Создаем объект класса для управления окном и заполнения структуры FiniteEM_T
            //Control_ChimeraMeshAssignment = new TControl_ChimeraMeshAssignment(FiniteEM_T, this);
            //Control_ChimeraMeshAssignment.Draw();
        }
        public void OkButton_Click(object sender, EventArgs e)
        {
            //this.Close();
            //Control_ChimeraMeshAssignment.StartTransform();
        }

        private void UViewerAero_ChimeraMeshAssignment_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Control_ChimeraMeshAssignment.AfterClosing();
        }
    }
}
