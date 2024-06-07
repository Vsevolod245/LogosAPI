// Класс для управления интерфесом для присваивания химер объектам и связывания объектов друг с другом
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
//
using AstraEngine;
using AstraEngine.Components;
//******************************************************************
namespace Example
{
    internal partial class TControl_ChimeraMeshAssignment
    {
        /// <summary>
        /// Структура объектов и химер
        /// </summary>
        TFiniteElementModel_Transformable FiniteEM_T;
        /// <summary>
        /// Окно для присваивания химер объектам и связывания объектов друг с другом
        /// </summary>
        UViewerAero_ChimeraMeshAssignment WindowChimeraMeshAssignment;
        /// <summary>
        /// Вспомогательный класс, хранящий методы, потребные для TViewerAero и TViewerAero_Plane
        /// </summary>
        protected TViewerAero_Helper VA_Helper = new TViewerAero_Helper();
        /// <summary>
        /// Класс для отрисовки моделей и химер
        /// </summary>
        protected TChimeraMeshAssignment_Visualizer Visualizer;
        //---------------------------------------------------------------
        //public TControl_ChimeraMeshAssignment(TFiniteElementModel_Transformable FiniteEM_T, UViewerAero_ChimeraMeshAssignment WindowChimeraMeshAssignment)
        //{
        //    this.FiniteEM_T = FiniteEM_T;
        //    this.WindowChimeraMeshAssignment = WindowChimeraMeshAssignment;
        //    Visualizer = new TChimeraMeshAssignment_Visualizer(FiniteEM_T.DependencyTable.PathsToSTL.Length, FiniteEM_T.numberOfChimeras);
        //}
        ////---------------------------------------------------------------
        ///// <summary>
        ///// Отрисовать окна со списком объектов и химер
        ///// </summary>
        //public void Draw()
        //{
        //    try
        //    {
        //        // Выставляем настройки первой строки, без которых она разрастается как слон и один черт знает, почему
        //        WindowChimeraMeshAssignment.TLP_ListOfObj.RowStyles[1].Height = 0;
        //        WindowChimeraMeshAssignment.TLP_ListOfObj.RowStyles[1].SizeType = SizeType.AutoSize;
        //        // Создаем перечень индексов всех химер (начиная с первого)
        //        string[] ChimerasId = new string[FiniteEM_T.numberOfChimeras];
        //        for (int i = 0; i < FiniteEM_T.numberOfChimeras; i++)
        //        {
        //            ChimerasId[i] = (i + 1).ToString();
        //        }
        //        // Добавляем все элементы в таблицу объектов
        //        for (int i = 0; i < FiniteEM_T.DependencyTable.PathsToSTL.Count(); i++)
        //        {
        //            // Обнуляем индексы родительских узлов
        //            FiniteEM_T.DependencyTable.ParentID[i] = -1;
        //            string None = "-";
        //            // Создаем выпадающий список с перечнем всех объектов
        //            ComboBox ObjComboBox = new ComboBox();
        //            // Добавляем названия объектов в список
        //            ObjComboBox.Items.Add(None);
        //            // Создаем список имен объектов
        //            string[] ObjNames = new string[FiniteEM_T.DependencyTable.PathsToSTL.Count() - 1];
        //            int k = 0;
        //            for (int j = 0; j < FiniteEM_T.DependencyTable.PathsToSTL.Count() - 1; j++)
        //            {
        //                if (j == i) k++;
        //                ObjNames[j] = System.IO.Path.GetFileNameWithoutExtension(FiniteEM_T.DependencyTable.PathsToSTL[k]);
        //                k++;
        //            }
        //            ObjComboBox.Items.AddRange(ObjNames);
        //            ObjComboBox.Name = "ObjComboBox_" + i.ToString();
        //            ObjComboBox.Dock = DockStyle.Top;
        //            ObjComboBox.SelectedIndexChanged += ObjectHasBeenSelected;
        //            ObjComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        //            ObjComboBox.DropDownWidth = 300;
        //            // Создаем выпадающий список с перечнем всех химер
        //            ComboBox ChimeraComboBox = new ComboBox();
        //            // Добавляем номера химер в список
        //            ChimeraComboBox.Items.Add(None);
        //            ChimeraComboBox.Items.AddRange(ChimerasId);
        //            ChimeraComboBox.Name = "ChimeraComboBox_" + i.ToString();
        //            ChimeraComboBox.Dock = DockStyle.Top;
        //            ChimeraComboBox.SelectedIndexChanged += ChimeraHasBeenSelected;
        //            ChimeraComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        //            // Добавляем в первый столбец новой строки лейбл с названием объекта
        //            Label ObjName = new Label();
        //            ObjName.Name = "ObjName_" + i.ToString();
        //            ObjName.Dock = DockStyle.Top;
        //            ObjName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //            ObjName.Text = System.IO.Path.GetFileNameWithoutExtension(FiniteEM_T.DependencyTable.PathsToSTL[i]);
        //            ObjName.AutoEllipsis = true;
        //            ObjName.MouseDoubleClick += ShowObject;
        //            // Добавляем строку в таблицу объектов
        //            WindowChimeraMeshAssignment.TLP_ListOfObj.RowCount++;
        //            WindowChimeraMeshAssignment.TLP_ListOfObj.Controls.Add(ObjName, 0, WindowChimeraMeshAssignment.TLP_ListOfObj.RowCount - 1);
        //            // Добавляем во второй столбец новой строки выпадающий список с перечнем всех объектов
        //            WindowChimeraMeshAssignment.TLP_ListOfObj.Controls.Add(ObjComboBox, 1, WindowChimeraMeshAssignment.TLP_ListOfObj.RowCount - 1);
        //            // Добавляем в третий столбец новой строки выпадающий список с перечнем химер
        //            WindowChimeraMeshAssignment.TLP_ListOfObj.Controls.Add(ChimeraComboBox, 2, WindowChimeraMeshAssignment.TLP_ListOfObj.RowCount - 1);
        //            // Добавляем объекты в дерево
        //            WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.Nodes.Add(new TreeNode(System.IO.Path.GetFileNameWithoutExtension(FiniteEM_T.DependencyTable.PathsToSTL[i])));
        //        }
        //    }
        //    catch (Exception E)
        //    {
        //        TJournalLog.WriteLog("C0001: Error TControl_ChimeraMeshAssignment:Draw(): " + E.Message);
        //    }
        //}
        ////---------------------------------------------------------------
        ///// <summary>
        ///// Рисуем химеру
        ///// </summary>
        ///// <param name="ChimeraId">Индекс химеры</param>
        //private void DrawChimera(int ChimeraId)
        //{
        //    try
        //    {
        //        // Ищем максимумы и минимумы среди точек, принадлежащих химере
        //        Vector3 MinP = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        //        Vector3 MaxP = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        //        foreach (TFemElement_Transformable Chim in FiniteEM_T.Elements)
        //        {
        //            if (Chim.ChimeraId != ChimeraId) continue;
        //            foreach (TFemFace_Transformable Face in Chim.Faces)
        //            {
        //                for (int i = 0; i < Face.Node_id.Count(); i++)
        //                {
        //                    if (MinP.X > FiniteEM_T.Nodes[Face.Node_id[i]].X)
        //                    {
        //                        MinP.X = FiniteEM_T.Nodes[Face.Node_id[i]].X;
        //                    }
        //                    if (MinP.Y > FiniteEM_T.Nodes[Face.Node_id[i]].Y)
        //                    {
        //                        MinP.Y = FiniteEM_T.Nodes[Face.Node_id[i]].Y;
        //                    }
        //                    if (MinP.Z > FiniteEM_T.Nodes[Face.Node_id[i]].Z)
        //                    {
        //                        MinP.Z = FiniteEM_T.Nodes[Face.Node_id[i]].Z;
        //                    }

        //                    if (MaxP.X < FiniteEM_T.Nodes[Face.Node_id[i]].X)
        //                    {
        //                        MaxP.X = FiniteEM_T.Nodes[Face.Node_id[i]].X;
        //                    }
        //                    if (MaxP.Y < FiniteEM_T.Nodes[Face.Node_id[i]].Y)
        //                    {
        //                        MaxP.Y = FiniteEM_T.Nodes[Face.Node_id[i]].Y;
        //                    }
        //                    if (MaxP.Z < FiniteEM_T.Nodes[Face.Node_id[i]].Z)
        //                    {
        //                        MaxP.Z = FiniteEM_T.Nodes[Face.Node_id[i]].Z;
        //                    }
        //                }
        //            }
        //        }
        //        // Рисуем BoundingBox этой химеры
        //        BoundingBox BB = new BoundingBox(MinP/* * 1000f*/, MaxP/* * 1000f*/);
        //        List<Vector3[]> DomainEdges = VA_Helper.DefineDomainEdges(BB);
        //        // Рисуем химеру в виде бокса
        //        Visualizer.ChimeraRender(ChimeraId, DomainEdges, BB);
        //    }
        //    catch (Exception E)
        //    {
        //        TJournalLog.WriteLog("C0002: Error TControl_ChimeraMeshAssignment:DrawChimera(): " + E.Message);
        //    }
        //}
        ////---------------------------------------------------------------
        ///// <summary>
        ///// Проверяем, была ли уже ранее данная химера присвоена другому объекту
        ///// </summary>
        ///// <param name="ChimeraId">Индекс химеры</param>
        //private void CheckChimeraAlreadySelected(int ChimeraId)
        //{
        //    try
        //    {
        //        for (int i = 0; i < FiniteEM_T.DependencyTable.ChimeraID.Length; i++)
        //        {
        //            if (FiniteEM_T.DependencyTable.ChimeraID[i] == ChimeraId)
        //            {
        //                // Удаляем химеру со старого места в массиве
        //                FiniteEM_T.DependencyTable.ChimeraID[i] = 0;
        //                // Очищаем старый ComboBox
        //                Control[] ChimeraComboBoxes = WindowChimeraMeshAssignment.TLP_ListOfObj.Controls.Find("ChimeraComboBox_" + i.ToString(), false);
        //                (ChimeraComboBoxes[0] as ComboBox).SelectedIndex = 0;
        //                // Удаляем химеру из дерева
        //                DelChimeraFromTree(i.ToString(), WindowChimeraMeshAssignment.Tree_ObjectsAndChimeras.Nodes);
        //            }
        //        }
        //    }
        //    catch (Exception E)
        //    {
        //        TJournalLog.WriteLog("C0003: Error TControl_ChimeraMeshAssignment:CheckChimeraAlreadySelected(): " + E.Message);
        //    }
        //}
        ////---------------------------------------------------------------
        ///// <summary>
        ///// Получаем индекс строки TLP, в которой находится объект
        ///// </summary>
        ///// <param name="sender">Объект, содержащий в своем названии номер строки TLP, в которой находится</param>
        //private int GetRowID(string Name)
        //{
        //    try
        //    {
        //        string line = Name.Remove(Name.LastIndexOf(@"_") + 1);
        //        return Convert.ToInt32(Name.Substring(line.Length, Name.Length - line.Length));
        //    }
        //    catch (Exception E)
        //    {
        //        TJournalLog.WriteLog("C0004: Error TControl_ChimeraMeshAssignment:GetRowID(): " + E.Message);
        //        return -1;
        //    }
        //}
        ////---------------------------------------------------------------
        ///// <summary>
        ///// Удалить объект из дерева
        ///// </summary>
        ///// <param name="RowID">Индекс строки, содержащий объект в дереве и выбранную химеру</param>
        ///// <param name="ParentNode">Родительский узел дерева</param>
        //private TreeNode IfObjectAlreadyExistInTree(int RowID, TreeNodeCollection ParentNode)
        //{
        //    try
        //    {
        //        TreeNode returnNode = null;
        //        foreach (TreeNode Node in ParentNode)
        //        {
        //            if (Node == null) continue;
        //            if (Node.Nodes.Count > 0)
        //            {
        //                returnNode = IfObjectAlreadyExistInTree(RowID, Node.Nodes);
        //                if (returnNode != null)
        //                {
        //                    return returnNode;
        //                }
        //            }
        //            if (Node.Text.Contains(System.IO.Path.GetFileNameWithoutExtension(FiniteEM_T.DependencyTable.PathsToSTL[RowID])))
        //            {
        //                return Node;
        //            }
        //        }
        //        return returnNode;

        //    }
        //    catch (Exception E)
        //    {
        //        TJournalLog.WriteLog("C0005: Error TControl_ChimeraMeshAssignment:IfObjectAlreadyExistInTree(): " + E.Message);
        //        return null;
        //    }
        //}
        ////---------------------------------------------------------------
        ///// <summary>
        ///// Прикрепить объект к другому объекту в дереве
        ///// </summary>
        ///// <param name="RowID">Индекс строки, содержащий объект в дереве и выбранную химеру</param>
        ///// <param name="ObjName">Имя объекта в дереве</param>
        ///// <param name="ParentNode">Родительский узел дерева</param>
        //private void AddObjectInTree(int RowID, string ObjName, TreeNodeCollection ParentNode, TreeNode NewNode)
        //{
        //    try
        //    {
        //        foreach (TreeNode Node in ParentNode)
        //        {
        //            if (Node == null) continue;
        //            if (Node.Nodes.Count > 0)
        //            {
        //                AddObjectInTree(RowID, ObjName, Node.Nodes, NewNode);
        //            }
        //            if (Node.Text.Contains(ObjName))
        //            {
        //                if (NewNode != null)
        //                {
        //                    Node.Nodes.Add(NewNode);
        //                }
        //                else
        //                {
        //                    Node.Nodes.Add(new TreeNode(System.IO.Path.GetFileNameWithoutExtension(FiniteEM_T.DependencyTable.PathsToSTL[RowID])));
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception E)
        //    {
        //        TJournalLog.WriteLog("C0006: Error TControl_ChimeraMeshAssignment:AddObjectInTree(): " + E.Message);
        //    }
        //}
        ////---------------------------------------------------------------
        ///// <summary>
        ///// Удалить объект из дерева
        ///// </summary>
        ///// <param name="RowID">Индекс строки, содержащий объект в дереве и выбранную химеру</param>
        ///// <param name="ParentNode">Родительский узел дерева</param>
        //private void DelObjectFromTree(int RowID, TreeNodeCollection ParentNode)
        //{
        //    try
        //    {
        //        foreach (TreeNode Node in ParentNode)
        //        {
        //            if (Node == null) continue;
        //            if (Node.Nodes.Count > 0)
        //            {
        //                DelObjectFromTree(RowID, Node.Nodes);
        //            }
        //            if (Node.Text.Contains(System.IO.Path.GetFileNameWithoutExtension(FiniteEM_T.DependencyTable.PathsToSTL[RowID])))
        //            {
        //                ParentNode.Remove(Node);
        //            }
        //        }
        //    }
        //    catch (Exception E)
        //    {
        //        TJournalLog.WriteLog("C0007: Error TControl_ChimeraMeshAssignment:DelObjectFromTree(): " + E.Message);
        //    }
        //}
        ////---------------------------------------------------------------
        ///// <summary>
        ///// Удалить химеру из дерева
        ///// </summary>
        ///// <param name="ChimeraID">Индекс химеры</param>
        ///// <param name="ParentNode">Родительский узел дерева</param>
        //private void DelChimeraFromTree(string ChimeraID, TreeNodeCollection ParentNode)
        //{
        //    try
        //    {
        //        foreach (TreeNode Node in ParentNode)
        //        {
        //            if (Node == null) continue;
        //            if (Node.Nodes.Count > 0)
        //            {
        //                if (Node.Nodes[0].Text.Contains("Chimera_" + ChimeraID))
        //                {
        //                    Node.Nodes.Remove(Node.Nodes[0]);
        //                }
        //                else
        //                {
        //                    DelChimeraFromTree(ChimeraID, Node.Nodes);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception E)
        //    {
        //        TJournalLog.WriteLog("C0008: Error TControl_ChimeraMeshAssignment:DelChimeraFromTree(): " + E.Message);
        //    }
        //}
        //---------------------------------------------------------------
    }
}
