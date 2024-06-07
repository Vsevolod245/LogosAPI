// Класс, содержащий события, происходящие с созданными к коде объектами
using AstraEngine.Components;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Xml.Linq;
//******************************************************************
namespace Example
{
    internal partial class TControl_ChimeraMeshAssignment
    {
        //---------------------------------------------------------------
        /// <summary>
        /// Отрисовать выбранный объект
        /// </summary>
        /// <param name="sender">Объект (в данном случае ComboBox)</param>
        /// <param name="e"></param>
        //private void ObjectHasBeenSelected(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if ((sender as ComboBox).SelectedIndex > 0)
        //        {
        //            // Рисуем объект
        //            // Получаем индекс интересующего нас объекта
        //            int ObjId = (sender as ComboBox).SelectedIndex;
        //            string ObjName = (sender as ComboBox).SelectedItem.ToString();
        //            Visualizer.ObjectRender(ObjId, FiniteEM_T.DependencyTable.PathsToSTL[ObjId]);
        //            // Прикрепляем к выбранному объекту объект из той же строки
        //            // НУЖНО СДЕЛАТЬ ПРОВЕРКУ НА ПРИКРЕПЛЕНИЕ ОБЪЕКТА К ПРИКРПЕЛЕННОМУ К НЕМУ ОБЪЕКТУ И ВЫКИДЫВАТЬ ПРЕДУПРЕЖДЕНИЕ ИЛИ ЧТО-ТО ЕЩЕ
        //            // Получаем индекс строки TLP, в которой находится ComboBox 
        //            int RowID = GetRowID((sender as ComboBox).Name);
        //            // Ищем индекс элемента с выбранным названием
        //            int ParentID = -1;
        //            for (int i = 0; i < FiniteEM_T.DependencyTable.PathsToSTL.Count(); i++)
        //            {
        //                if (System.IO.Path.GetFileNameWithoutExtension(FiniteEM_T.DependencyTable.PathsToSTL[i]) == ObjName)
        //                    ParentID = i;
        //            }
        //            // Присваиваем индекс выбранного объекта элементу массива
        //            FiniteEM_T.DependencyTable.ParentID[RowID] = ParentID;
        //            // Ищем ветвь с таким объектом в дереве
        //            TreeNode ChildNode = IfObjectAlreadyExistInTree(RowID, WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.Nodes);
        //            // Удаляем старый объект из дерева
        //            DelObjectFromTree(RowID, WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.Nodes);
        //            // Прикрепляем объект к объекту в дереве объектов (и все прикрепленный к нему объекты тоже)
        //            AddObjectInTree(RowID, ObjName, WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.Nodes, ChildNode);
        //            // Раскрываем дерево
        //            WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.ExpandAll();
        //        }
        //        else
        //        {
        //            string ObjName = (sender as ComboBox).SelectedItem.ToString();
        //            Visualizer.ObjectsDiscolor();
        //            // Получаем индекс строки TLP, в которой находится ComboBox
        //            int RowID = GetRowID((sender as ComboBox).Name);
        //            // Обнуляем индекс родителя
        //            FiniteEM_T.DependencyTable.ParentID[RowID] = -1;
        //            // Ищем ветвь с таким объектом в дереве
        //            TreeNode ChildNode = IfObjectAlreadyExistInTree(RowID, WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.Nodes);
        //            // Удаляем старый объект из дерева
        //            DelObjectFromTree(RowID, WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.Nodes);
        //            // Прикрепляем объект в качестве корня в дереве объектов (и все прикрепленный к нему объекты тоже)
        //            if (ChildNode != null)
        //            {
        //                WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.Nodes.Insert(RowID, ChildNode);
        //            }
        //            else
        //            {
        //                WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.Nodes.Insert(RowID, new TreeNode(System.IO.Path.GetFileNameWithoutExtension(FiniteEM_T.DependencyTable.PathsToSTL[RowID])));
        //            }
        //            // Раскрываем дерево
        //            WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.ExpandAll();
        //        }
        //    }
        //    catch (Exception E)
        //    {
        //        TJournalLog.WriteLog("C0006: Error TControl_ChimeraMeshAssignment:ObjectHasBeenSelected(): " + E.Message);
        //    }
        //}
        ////---------------------------------------------------------------
        ///// <summary>
        ///// Отрисовать выбранную химеру
        ///// </summary>
        ///// <param name="sender">Объект (в данном случае ComboBox)</param>
        ///// <param name="e"></param>
        //private void ChimeraHasBeenSelected(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if ((sender as ComboBox).SelectedIndex > 0)
        //        {
        //            // Получаем индекс интересующей нас химеры
        //            int ChimeraId = Convert.ToInt32((sender as ComboBox).SelectedItem);
        //            DrawChimera(ChimeraId);
        //            // Проверяем остальные ChimeraComboBox (выбрана ли в них данная химера). Если да - убираем химеру из старого места
        //            CheckChimeraAlreadySelected(ChimeraId);
        //            // Получаем индекс строки TLP, в которой находится ComboBox 
        //            int RowID = GetRowID((sender as ComboBox).Name);
        //            // Присваиваем индекс выбранной химеры элементу массива
        //            FiniteEM_T.DependencyTable.ChimeraID[RowID] = ChimeraId;
        //            // Ищем ветвь с таким объектом в дереве
        //            TreeNode ChildNode = IfObjectAlreadyExistInTree(RowID, WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.Nodes);
        //            // Удаляем предыдущую химеру из этой ветви, если она там была
        //            if (ChildNode != null && ChildNode.Nodes.Count > 0 && ChildNode.Nodes[0].Text.Contains("Chimera_"))
        //                ChildNode.Nodes.Remove(ChildNode.Nodes[0]);
        //            // Удаляем текущую химеру из дерева
        //            DelChimeraFromTree(ChimeraId.ToString(), WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.Nodes);
        //            // Прикрепляем химеру к объекту в дереве объектов (всегда ставим химеру на нулевую позицию)
        //            if (ChildNode == null)
        //                WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.Nodes[RowID].Nodes.Insert(0, new TreeNode("Chimera_" + ChimeraId.ToString()));
        //            else
        //                ChildNode.Nodes.Insert(0, new TreeNode("Chimera_" + ChimeraId.ToString()));
        //            // Раскрываем дерево
        //            WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.ExpandAll();
        //        }
        //        else
        //        {
        //            // Обесцвечиваем химеры
        //            Visualizer.ChimerasDiscolor();
        //            // Получаем индекс строки TLP, в которой находится ComboBox 
        //            int RowID = GetRowID((sender as ComboBox).Name);
        //            // Ищем ветвь с таким объектом в дереве
        //            TreeNode ChildNode = IfObjectAlreadyExistInTree(RowID, WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.Nodes);
        //            // Удаляем предыдущую химеру из этой ветви, если она там была
        //            if (ChildNode != null && ChildNode.Nodes.Count > 0 && ChildNode.Nodes[0].Text.Contains("Chimera_"))
        //                ChildNode.Nodes.Remove(ChildNode.Nodes[0]);
        //            // Открепляем химеру от объекта
        //            FiniteEM_T.DependencyTable.ChimeraID[RowID] = 0;
        //        }
        //    }
        //    catch (Exception E)
        //    {
        //        TJournalLog.WriteLog("C0007: Error TControl_ChimeraMeshAssignment:ChimeraHasBeenSelected(): " + E.Message);
        //    }
        //}
        ///// <summary>
        ///// Отрисовать выбранный объект
        ///// </summary>
        ///// <param name="sender">Объект (в данном случае label)</param>
        ///// <param name="e"></param>
        //private void ShowObject(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        // Получаем индекс строки TLP, в которой находится Label 
        //        int RowID = GetRowID((sender as Label).Name);
        //        Visualizer.ObjectRender(RowID, FiniteEM_T.DependencyTable.PathsToSTL[RowID]);
        //    }
        //    catch (Exception E)
        //    {
        //        TJournalLog.WriteLog("C0006: Error TControl_ChimeraMeshAssignment:ObjectHasBeenSelected(): " + E.Message);
        //    }
        //}
        //---------------------------------------------------------------
    }
}
