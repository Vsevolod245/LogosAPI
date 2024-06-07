// Класс для визуализации химер и моделей при выборе их в окне присваивания
using System;
using System.Collections.Generic;
using System.Drawing;
//
using AstraEngine.Render;
using AstraEngine.Scene;
using AstraEngine.Geometry.Model3D;
using AstraEngine.Components;
using AstraEngine;
//******************************************************************
namespace Example
{
    public class TChimeraMeshAssignment_Visualizer
    {
        /// <summary>
        /// Рендер
        /// </summary>
        protected TRender Render = TStaticContent.Content.Game.Render;
        /// <summary>
        /// Массив химер
        /// </summary>
        List<TModel3D>[] Chimeras;
        /// <summary>
        /// Лист объектов
        /// </summary>
        TModel3D[] Objects;
        /// <summary>
        /// Цвет объекта
        /// </summary>
        public Color Colour_Object = Color.FromArgb(255, 0, 113, 188);
        /// <summary>
        /// Цвет объекта (выбранного)
        /// </summary>
        public Color Colour_Object_Highlight = Color.FromArgb(255, 255, 127, 0);
        /// <summary>
        /// Цвет химеры
        /// </summary>
        public Color Colour_Chimera = Color.FromArgb(255, 0, 113, 188);
        /// <summary>
        /// Цвет химеры (выбранной)
        /// </summary>
        public Color Colour_Chimera_Highlight = Color.FromArgb(255, 255, 127, 0);
        //---------------------------------------------------------------
        /// <summary>
        /// Создать объект визуализатора для отрисовки объектов и химер
        /// </summary>
        /// <param name="numberOfChimeras">Количество объектов</param>
        /// <param name="numberOfChimeras">Количество химер</param>
        public TChimeraMeshAssignment_Visualizer(int numberOfObj, int numberOfChimeras)
        {
            Objects = new TModel3D[numberOfObj];
            Chimeras = new List<TModel3D>[numberOfChimeras];
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Визуализировать объект 
        /// </summary>
        /// <param name="ObjId"> Индекс объекта</param>
        /// <param name="Path"> Путь до файла модели</param>
        public void ObjectRender(int ObjId, string Path)
        {
            // Обесцветить все объекты
            ObjectsDiscolor();
            // Если объект уже была нарисована ранее, то только окрашиваем ее
            if (Objects[ObjId] == null)
                ObjectCreate(ObjId, Path);
            else
                ObjectColor(ObjId);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Визуализировать химеру (в виде Bounding Box)
        /// </summary>
        /// <param name="ChimeraId"> Индекс химеры</param>
        /// <param name="DomainEdges">Грани BB</param>
        /// <param name="BB">Бокс химеры</param>
        /// <param name="BBSizeK">Коэффициент толщины линии химеры</param>
        public void ChimeraRender(int ChimeraId, List<Vector3[]> DomainEdges, BoundingBox BB, float BBSizeK = 0.0025f)
        {
            // Обесцветить все химеры
            ChimerasDiscolor();
            // Если химера уже была нарисована ранее, то только окрашиваем ее
            if (Chimeras[ChimeraId - 1] == null)
                ChimeraCreate(ChimeraId, DomainEdges, BB, BBSizeK);
            else
                ChimeraColor(ChimeraId);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Создать объект
        /// </summary>
        /// <param name="ObjId"> Индекс объекта</param>
        /// <param name="Path"> Путь до файла модели</param>
        private void ObjectCreate(int ObjId, string Path)
        {
            TModel3D Object = new TModel3D(Render);
            // Грузим файл
            Object.ControlLoad.LoadModel(Path);
            // Упрощаем модель
            //List<TTriangleContainer> EasyModel = new List<TTriangleContainer>
            //{
            //    Object.ControlOperations.Reduce(100)
            //};
            //Object.ControlLoad.LoadModel(EasyModel);
            //Object.Scale = new Vector3(1000, 1000, 1000);
            Object.ControlLight.SetAmbientLight(Color.White, 0.5f);
            Object.ControlLight.AddDirectionLight(Color.White, 0.5f, new Vector3(1000, 1000, 1000));
            Object.SetColour(Colour_Object_Highlight);
            Objects[ObjId] = Object;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Создать химеру
        /// </summary>
        /// <param name="ChimeraId"> Индекс химеры</param>
        /// <param name="DomainEdges">Грани BB</param>
        /// <param name="BB">Бокс химеры</param>
        /// <param name="BBSizeK">Коэффициент толщины линии химеры</param>
        private void ChimeraCreate(int ChimeraId, List<Vector3[]> DomainEdges, BoundingBox BB, float BBSizeK = 0.0025f)
        {
            // Лист линий, из которых состоит химера
            List<TModel3D> ChimerasLines = new List<TModel3D>();
            float BBSize = Math.Max(BB.Max.X - BB.Min.X, Math.Max(BB.Max.Y - BB.Min.Y, BB.Max.Z - BB.Min.Z)) * BBSizeK;
            for (int i = 0; i < DomainEdges.Count; i++)
            {
                TModel3D Line = new TModel3D(Render);
                ChimerasLines.Add(Line);
                Line.ControlPrimitive.CreateLine(DomainEdges[i][0], DomainEdges[i][1], BBSize);
                Line.SetColour(Colour_Chimera_Highlight);
            }
            Chimeras[ChimeraId - 1] = ChimerasLines;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Окрасить объект
        /// </summary>
        /// <param name="ObjId"> Индекс объекта</param>
        private void ObjectColor(int ObjId)
        {
            Objects[ObjId].SetColour(Colour_Object_Highlight);
            Objects[ObjId].SetAlpha(1f);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Окрасить химеру
        /// </summary>
        /// <param name="ChimeraId"> Индекс химеры</param>
        private void ChimeraColor(int ChimeraId)
        {
            foreach (var Line in Chimeras[ChimeraId - 1])
            {
                Line.SetColour(Colour_Chimera_Highlight);
                Line.SetAlpha(1f);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Обесцветить все объекты
        /// </summary>
        public void ObjectsDiscolor()
        {
            foreach (var Object in Objects)
            {
                if (Object != null)
                {
                    Object.SetColour(Colour_Object);
                    Object.SetAlpha(0.5f);
                }
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Обесцветить все химеры
        /// </summary>
        public void ChimerasDiscolor()
        {
            foreach (var ChimeraLines in Chimeras)
            {
                if (ChimeraLines != null)
                {
                    foreach (var Line in ChimeraLines)
                    {
                        Line.SetColour(Colour_Chimera);
                        Line.SetAlpha(0.5f);
                    }
                }
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Очистить объекты и химеры
        /// </summary>
        public void DisposeAll()
        {
            try
            {
                ObjectsDispose();
                ChimerasDispose();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0012: Error TChimeraMeshAssignmen_Visualizer:DisposeAll(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Очистить объекты
        /// </summary>
        private void ObjectsDispose()
        {
            try
            {
                foreach (var Object in Objects)
                {
                    if (Object != null)
                        Object.Unload();
                }
                Objects = null;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0012: Error TChimeraMeshAssignmen_Visualizer:ObjectsDispose(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Очистить химеры
        /// </summary>
        private void ChimerasDispose()
        {
            try
            {
                foreach (var ChimeraLines in Chimeras)
                {
                    if (ChimeraLines != null)
                    {
                        foreach (var Line in ChimeraLines)
                        {
                            if (Line != null)
                                Line.Unload();
                        }
                        ChimeraLines.Clear();
                    }
                }
                Chimeras = null;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0012: Error TChimeraMeshAssignmen_Visualizer:ChimerasDispose(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
    }
}
