// Парсинг файлов EnsightGold с готовыми расчетами
using AstraEngine.Components;
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
//***************************************************************
namespace Example
{
    internal partial class TViewerAero_Parser
    {
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Отпарсить файлы Ensight Gold, записав значения точек в соответствующие списки. !!!В первую очередь необходимо отпарсить файл .geo!!!
        /// </summary>
        /// <param name="Path">Путь к файлу</param>
        /// <param name="Points"></param>
        /// <param name="PointsOnSurface"></param>
        internal void ParseEnsight(string Path, TFiniteElementModel_Visual FiniteEM)
        {
            try
            {
                // Вызываем парсер соответствующий расширению файла
                switch (System.IO.Path.GetExtension(Path))
                {
                    case ".geo":
                        ParseGeo(Path, FiniteEM);
                        break;
                    default:
                        if (FiniteEM.Elements.Length == 0)
                            throw new Exception("Select geo file for parsing before the rest");
                        ParsePhysicalQuantities(Path, FiniteEM);
                        break;
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("Error E0000 " + E.Message);
            }
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Парсинг файла .geo 
        /// </summary>
        /// <param name="Path">Путь к файлу, который будет отпарсен</param>
        /// <param name="Points"></param>
        /// <param name="PointsOnSurface"></param>
        private void ParseGeo(string Path, TFiniteElementModel_Visual FiniteEM)
        {
            using (StreamReader SR = new StreamReader(Path))
            {
                // Счетчик разделов файла
                int part = 0;
                string patchName = "";
                // Массив для координат, где их индекс соответствует индексам из файла (Разный для каждого раздла файла)
                var coordinates = new AstraEngine.Vector3[0];
                while (!SR.EndOfStream)
                {
                    var line = SR.ReadLine();
                    // Парсинг начала нового раздела включая координаты
                    if (line == "part")
                    {
                        part++;
                        // пропуск не имеющих значение строк
                        line = SR.ReadLine();
                        // Считывание строки с названием парта
                        line = SR.ReadLine();
                        var SplitWithPartName = line.Split();
                        patchName = SplitWithPartName[SplitWithPartName.Count() - 1];
                        // пропуск не имеющих значение строк
                        line = SR.ReadLine();
                        // Считывание строки с кол-вом координат
                        line = SR.ReadLine();
                        // Количество координат в данном разделе
                        var numberOfCoordinates = int.Parse(line);
                        // Массив с координатами
                        coordinates = new AstraEngine.Vector3[numberOfCoordinates];
                        //
                        var InvariantCulture = CultureInfo.InvariantCulture;
                        // Считывание всех компонент Х у координат данного раздела
                        for (int i = 0; i < numberOfCoordinates; i++)
                        {
                            line = SR.ReadLine();
                            coordinates[i] = new AstraEngine.Vector3(float.Parse(line, InvariantCulture), 0f, 0f);
                        }
                        // Считывание всех компонент У у координат данного раздела
                        for (int i = 0; i < numberOfCoordinates; i++)
                        {
                            line = SR.ReadLine();
                            coordinates[i].Y = float.Parse(line, InvariantCulture);
                        }
                        // Считывание всех компонент Z у координат данного раздела
                        for (int i = 0; i < numberOfCoordinates; i++)
                        {
                            line = SR.ReadLine();
                            coordinates[i].Z = float.Parse(line, InvariantCulture);
                        }
                        continue;
                    }
                    // Парсинг nfaced части
                    if (line == "nfaced")
                    {
                        FiniteEM.Nodes = new TFemNode_Visual[coordinates.Length];
                        for (int i = 0; i < coordinates.Length; i++)
                        {
                            FiniteEM.Nodes[i] = new TFemNode_Visual()
                            {
                                X = coordinates[i].X,
                                Y = coordinates[i].Y,
                                Z = coordinates[i].Z
                            };
                        }
                        // Обнуляем более непотребный массив
                        coordinates = new AstraEngine.Vector3[0];

                        line = SR.ReadLine();
                        // Количество полиедрических эллемментов
                        var numberOfElements = int.Parse(line);
                        // Создаем массив по количеству элементов для получения среднего центра ячейки
                        var Elements_IDs = new int[numberOfElements][][];

                        // Определяем ячейки массива, как массивы по количеству сторон элемента
                        for (int i = 0; i < numberOfElements; i++)
                        {
                            line = SR.ReadLine();
                            Elements_IDs[i] = new int[int.Parse(line)][];
                        }
                        // Определяем ячейки массива, как массивы по количеству вершин стороны
                        for (int i = 0; i < numberOfElements; i++)
                        {
                            var numberOfFaces = Elements_IDs[i].Length;

                            for (int j = 0; j < numberOfFaces; j++)
                            {
                                line = SR.ReadLine();
                                Elements_IDs[i][j] = new int[int.Parse(line)];
                            }
                        }

                        // Заполнение индексами
                        for (int i = 0; i < numberOfElements; i++)
                        {
                            var numberOfFaces = Elements_IDs[i].Length;
                            for (int j = 0; j < numberOfFaces; j++)
                            {
                                line = SR.ReadLine();
                                var DirtyString = line.Split().ToList();
                                DirtyString.RemoveAll(x => x == "");
                                string[] Numbers = DirtyString.ToArray();

                                var numberOfNodes = Elements_IDs[i][j].Length;
                                for (int k = 0; k < numberOfNodes; k++)
                                {
                                    Elements_IDs[i][j][k] = int.Parse(Numbers[k]) - 1;
                                }
                            }
                        }

                        // Создаем массив по колличеству эллементов в конечноэлементной модели
                        FiniteEM.Elements = new TFemElement_Visual[numberOfElements];
                        // Заполнение конечно элементной модели
                        for (int i = 0; i < Elements_IDs.Length; i++)
                        {
                            FiniteEM.Elements[i] = new TFemElement_Visual();
                            // Создаем список в который будем записывать индексы i-ой ячейки
                            var NodeIDs = new List<int>();
                            // Записываем все индексы нод элемента
                            for(int j = 0; j < Elements_IDs[i].Length; j++)
                            {
                                for (int k = 0; k < Elements_IDs[i][j].Length; k++)
                                {
                                    NodeIDs.Add(Elements_IDs[i][j][k]);
                                }
                            }
                            // Избавляемся от повторяющихся индексов и записываем в конечно элементную модель
                            FiniteEM.Elements[i].Nodes = NodeIDs.Distinct().ToArray();
                        }

                        // Получение примерного центра ячейки(эллемента)
                        for (int i = 0; i < numberOfElements; i++)
                        {
                            // Средняя координата элемента(ячейки). Изначально присваеваем занчения первой вершины
                            var ElementAvarage = new AstraEngine.Vector3()
                            {
                                X = FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[0]].X,
                                Y = FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[0]].Y,
                                Z = FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[0]].Z
                            };
                            // Вычисление средней координаты элемента(ячейки)
                            for (int j = 1; j < FiniteEM.Elements[i].Nodes.Length; j++)
                            {
                                ElementAvarage *= j;
                                ElementAvarage.X += FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[j]].X;
                                ElementAvarage.Y += FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[j]].Y;
                                ElementAvarage.Z += FiniteEM.Nodes[FiniteEM.Elements[i].Nodes[j]].Z;
                                ElementAvarage = ElementAvarage / ((float)j + 1f);
                            }
                            // Запись средней координаты в конечно элементную структуру
                            FiniteEM.Elements[i].Position = ElementAvarage;
                        }
                    }
                    // Парсинг nsided части
                    if (line == "nsided")
                    {
                        int oldSize = FiniteEM.Nodes.Length;
                        line = SR.ReadLine();
                        var numberOfElements = int.Parse(line);
                        Array.Resize(ref FiniteEM.Nodes, oldSize + coordinates.Length);
                        TFemFace_Visual[] Faces = new TFemFace_Visual[numberOfElements];
                        int[][] Elements = new int[numberOfElements][];
                        // Парсинг кол-ва вершин в элементе
                        for (int i = 0; i < numberOfElements; i++)
                        {
                            line = SR.ReadLine();
                            Elements[i] = new int[int.Parse(line)];
                        }
                        // Парсинг id координат элементов
                        for (int i = 0; i < numberOfElements; i++)
                        {
                            line = SR.ReadLine();
                            var DirtyString = line.Split().ToList();
                            DirtyString.RemoveAll(x => x == "");
                            string[] Numbers = DirtyString.ToArray();
                            for (int j = 0; j < Numbers.Length; j++)
                                Elements[i][j] = int.Parse(Numbers[j]) - 1;
                        }


                        //==================Временная затычка=====================
                        // Заполнение дополнительных нод
                        for (int i = 0; i < coordinates.Length; i++)
                        {
                            FiniteEM.Nodes[i + oldSize] = new TFemNode_Visual()
                            {
                                X = coordinates[i].X,
                                Y = coordinates[i].Y,
                                Z = coordinates[i].Z
                            };
                        }
                        //========================================================


                        // Вычисление среднего и заполнение индексами нод
                        for (int i = 0; i < numberOfElements; i++)
                        {
                            
                            var AveragePosition = coordinates[Elements[i][0]];
                            for (int j = 1; j < Elements[i].Length; j++)
                            {
                                AveragePosition = (AveragePosition * j + coordinates[Elements[i][j]]) / ((float)j + 1f);
                            }
                            Faces[i] = new TFemFace_Visual()
                            {
                                Position = AveragePosition,
                                Nodes = Elements[i]
                                
                            };

                            //==================Временная затычка=====================
                            // Пересчет ид нод
                            for (int j=0; j < Faces[i].Nodes.Length; j++)
                            {
                                Faces[i].Nodes[j] += oldSize;
                            }
                            //========================================================
                        }


                        FiniteEM.Surfaces.Add((patchName, Faces));
                        #region Старый триангуляционный подход
                        //    // Треангуляция
                        //    var TriangleIds = new int[NumberOfTriangles][];
                        //int TriangleID = 0;
                        //foreach (var element in Elements_IDs)
                        //{
                        //    var Triangulated = Triangulate(element);
                        //    foreach (var Triangle in Triangulated)
                        //    {
                        //        TriangleIds[TriangleID] = Triangle;
                        //        var position = (coordinates[Triangle[0]] + coordinates[Triangle[1]] + coordinates[Triangle[2]]) / 3f;
                        //        PointsOnSurface.Add(new TFEMFluidFrame()
                        //        {
                        //            ID_Node = part,
                        //            Position = new Vector3d(position.X, position.Y, position.Z)
                        //        });
                        //        TriangleID++;
                        //    }
                        //}
                        //Elements_IDs = null;
                        //// Cборка в TFEMFluidFrame
                        //for (int i = 0; i < TriangleIds.Length; i++)
                        //{
                        //    var position = (coordinates[TriangleIds[i][0]] + coordinates[TriangleIds[i][1]] + coordinates[TriangleIds[i][2]]) / 3f;
                        //    PointsOnSurface.Add(new TFEMFluidFrame()
                        //    {
                        //        ID_Node = part,
                        //        Position = new Vector3d(position.X, position.Y, position.Z)
                        //    });
                        //}
                        #endregion
                    }
                }
            }
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Парсинг физических значений (Ensight Gold)
        /// </summary>
        /// <param name="Path">Путь к файлу</param>
        /// <param name="Points"></param>
        public void ParsePhysicalQuantities(string Path, TFiniteElementModel_Visual FiniteEM)
        {
            string extention = System.IO.Path.GetExtension(Path);
            using (StreamReader SR = new StreamReader(Path))
            {
                var InvariantCulture = CultureInfo.InvariantCulture;
                int part = 0;
                int NsidedCounter = -1;
                while (!SR.EndOfStream)
                {
                    var line = SR.ReadLine();
                    if (line == "part")
                    {
                        part++;
                    }
                    if (line == "nfaced")
                    {
                        for (int i = 0; i < FiniteEM.Elements.Length; i++)
                        {
                            WriteValues(float.Parse(SR.ReadLine(), InvariantCulture), extention, FiniteEM, i);
                        }
                    }
                    if (line == "nsided")
                    {
                        NsidedCounter++;
                        for (int i = 0; i < FiniteEM.Surfaces[NsidedCounter].Item2.Length; i++)
                        {
                            WriteValues(float.Parse(SR.ReadLine(), InvariantCulture), extention, FiniteEM, i, NsidedCounter);
                        }
                    }
                }
            }
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Записать значения на поверхности в конечно элементную модель
        /// </summary>
        /// <param name="Value">Значение</param>
        /// <param name="extension">Расширение читаемого файла</param>
        /// <param name="PointsOnSurface">Список списков точек в который будет записано значение</param>
        /// <param name="Id">Проядковый номер величины в парте файла</param>
        /// <param name="NsidedID">Порядковый номер nsided эллемента</param>
        /// <exception cref="Exception"></exception>
        private void WriteValues(float Value, string extension, TFiniteElementModel_Visual FiniteEM, int Id, int NsidedID)
        {
            switch (extension)
            {
                case ".P":
                    FiniteEM.Surfaces[NsidedID].Item2[Id].Pressure = Value;
                    break;
                case ".U":
                    FiniteEM.Surfaces[NsidedID].Item2[Id].Velocity.X = Value;
                    break;
                case ".V":
                    FiniteEM.Surfaces[NsidedID].Item2[Id].Velocity.Y = Value;
                    break;
                case ".W":
                    FiniteEM.Surfaces[NsidedID].Item2[Id].Velocity.Z = Value;
                    break;
                case ".UMAG":
                    //FiniteEM.Surfaces[NsidedID].Item2[Id].VelocityModule = Value;
                    break;
                default:
                    throw new Exception("Extension \""+ extension + "\" is not supported");
                    break;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Записать значения в ячейках в конечно элементную модель
        /// </summary>
        /// <param name="Value">Значение</param>
        /// <param name="extension">Расширение читаемого файла</param>
        /// <param name="Points">Список точек в который будет записано значение</param>
        /// <param name="Id">Проядковый номер величины в парте файла</param>
        /// <exception cref="Exception"></exception>
        private void WriteValues(float Value, string extension, TFiniteElementModel_Visual FiniteEM, int Id)
        {
            switch (extension)
            {
                case ".P":
                    FiniteEM.Elements[Id].Pressure = Value;
                    break;
                case ".U":
                    FiniteEM.Elements[Id].Velocity.X = Value;
                    break;
                case ".V":
                    FiniteEM.Elements[Id].Velocity.Y = Value;
                    break;
                case ".W":
                    FiniteEM.Elements[Id].Velocity.Z = Value;
                    break;
                case ".UMAG":
                    FiniteEM.Elements[Id].VelocityModule = Value;
                    break;
                default:
                    throw new Exception("Extension \"" + extension + "\" is not supported");
                    break;
            }
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
