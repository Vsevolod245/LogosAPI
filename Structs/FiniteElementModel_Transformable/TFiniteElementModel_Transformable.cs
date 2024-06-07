// Описание конечно-элементной модели
using System.Collections.Generic;
using System.IO;
using System.Globalization;
//
using AstraEngine.Geometry.Model3D;
using AstraEngine;
//
using Logos_TVD;
//******************************************************************
namespace Example
{
    /// <summary>
    /// Описание конечно-элементной модели
    /// </summary>
    public class TFiniteElementModel_Transformable
    {
        /// <summary>
        /// Графическое представление конечно-элементной модели
        /// </summary>
        public TModel3D Grid { get; set; }
        /// <summary>
        /// Logos_TVD API
        /// </summary>
        public TLogos_TVD API { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TFemElement_Transformable[] Elements { get; set; } = new TFemElement_Transformable[0];
        /// <summary>
        /// Массив вершин
        /// </summary>
        public TFemNode_Transformable[] Nodes = new TFemNode_Transformable[0];
        /// <summary>
        /// Массив граней(сторон)
        /// </summary>
        public TFemFace_Transformable[] Faces = new TFemFace_Transformable[0];
        /// <summary>
        /// Номера строки в которой заканчивается первая неизменяемая часть файла
        /// </summary>
        public int UnModifiedFirstPart = 0;
        /// <summary>
        /// Номер строки с которой начинается неизменяемая часть файла
        /// </summary>
        public int UnModifiedLastPart = 0;
        /// <summary>
        /// Число химер
        /// </summary>
        public int numberOfChimeras = 0;
        /// <summary>
        /// Таблица соответствия химер с объектами + родительская зависимость объектов
        /// </summary>
        //public TChimera_ObjectDependencyTable DependencyTable;
        /// <summary>
        /// Путь к пришедшему файлу Ngeom
        /// </summary>
        public string InitialNgeomFilePath = "";
    //----------------------------------------------------------------
        /// <summary>
        /// Удалить
        /// </summary>
        public void Dispose()
        {
            if (Grid != null)
            {
                Grid.Unload();
            }
            Elements = null;
            API = null;
            Nodes = new TFemNode_Transformable[0];
            Faces = new TFemFace_Transformable[0];
            UnModifiedFirstPart = 0;
            UnModifiedLastPart = 0;
            numberOfChimeras = 0;
            InitialNgeomFilePath = "";
        }
        //----------------------------------------------------------------
        public void ReadNgeom(string PathNGeom, string PathCell = "")
        {
            // Запись пути к изначальному файлу
            InitialNgeomFilePath = PathNGeom;
            // Список ячеек с их гранями, нужен, так как в пункте 1.3. запись такова, что написано какая грань каким ячейкам принадлежит, а не наоборот, поэтому изначально мы не знаем сколько граней в ячейке
            List<int>[] Cells = new List<int>[0];
            // Количество граней с телами
            int CountBody = 0;

            // Определение номера строки в котором заканчивается первая неизменяемая часть файла
            using (StreamReader SR = new StreamReader(PathNGeom))
            {
                while (!SR.EndOfStream)
                {
                    UnModifiedFirstPart++;
                    if (SR.ReadLine().StartsWith("1.1."))
                    {
                        break;
                    }
                }
            }


            bool count = true;
            bool check = true;
            using (StreamReader SR = new StreamReader(PathNGeom))
            {
                while (!SR.EndOfStream)
                {
                    if (count) UnModifiedLastPart++;
                    var line = SR.ReadLine();
                    if (line.StartsWith("1.2."))
                    {
                        count = false;
                    }

                    if (check)
                    {
                        // В этих строчках есть данные о количестве точек или граней
                        if (line.Contains("nombre DE SOMMETS") || line.Contains("nombre DE FACES") || line.Contains("nombre DE CELLULES") || line.Contains("nombre DE MARQUAGES"))
                        {
                            // Здесь идет поиск количества вершин, граней, ячеек, и образмеривание массивов, куда записываются эти данные
                            string str = line;
                            str = str.Trim();
                            int Num = str.IndexOf('<');
                            str = str.Remove(Num);
                            str = str.Trim();
                            int Count = int.Parse(str);
                            if (line.Contains("nombre DE SOMMETS"))
                            {
                                Nodes = new TFemNode_Transformable[Count];
                                continue;
                            }
                            if (line.Contains("nombre DE FACES"))
                            {
                                Faces = new TFemFace_Transformable[Count];
                                continue;
                            }
                            if (line.Contains("nombre DE CELLULES"))
                            {
                                Elements = new TFemElement_Transformable[Count];
                                Cells = new List<int>[Count];
                                continue;
                            }
                            if (line.Contains("nombre DE MARQUAGES"))
                            {
                                CountBody = Count;
                                check = false;
                            }
                            continue;
                        }
                    }

                    // Парсинг координат точек
                    if (line.StartsWith("1.1."))
                    {
                        for (int i = 0; i < Nodes.Length; i++)
                        {
                            if (count) UnModifiedLastPart++;
                            line = SR.ReadLine();
                            string[] Coord = line.Split(' ');
                            int Num = int.Parse(Coord[0]) - 1;
                            Coord[1] = Coord[1].Replace(".", ",");
                            Coord[2] = Coord[2].Replace(".", ",");
                            Coord[3] = Coord[3].Replace(".", ",");
                            Nodes[Num] = new TFemNode_Transformable
                            {
                                X = float.Parse(Coord[1]),
                                Y = float.Parse(Coord[2]),
                                Z = float.Parse(Coord[3]),
                            };
                        }
                    }

                    // Парсинг граней с их точками
                    if (line.StartsWith("1.2."))
                    {
                        for (int i = 0; i < Faces.Length; i++)
                        {
                            line = SR.ReadLine();
                            string[] Fac = line.Split(' ');
                            Faces[i] = new TFemFace_Transformable
                            {
                                Node_id = new int[int.Parse(Fac[1])]
                            };
                            for (int j = 2; j < Fac.Length; j++)
                            {
                                Faces[i].Node_id[j - 2] = int.Parse(Fac[j]) - 1;
                            }
                        }
                    }

                    // Парсинг ячеек с их гранями
                    if (line.StartsWith("1.3."))
                    {
                        for (int i = 0; i < Faces.Length; i++)
                        {
                            line = SR.ReadLine();
                            string[] Cell = line.Split(' ');

                            for (int j = 2; j < Cell.Length; j++)
                            {
                                if (Cells[int.Parse(Cell[j]) - 1] == null)
                                {
                                    Cells[int.Parse(Cell[j]) - 1] = new List<int>
                                    {
                                        int.Parse(Cell[0]) - 1
                                    };
                                }
                                else
                                {
                                    Cells[int.Parse(Cell[j]) - 1].Add(int.Parse(Cell[0]) - 1);
                                }
                            }
                        }
                    }
                }
            }
            List<int> NumbersChim = new List<int>();
            NumbersChim.Add(1);
            bool Proverka = false;
            bool Proverka2 = false;
            // Парсинг документа о принадлежности ячеек к химерам
            if (PathCell != "" && File.Exists(PathCell))
            {
                using (StreamReader SR = new StreamReader(PathCell))
                {
                    while (!SR.EndOfStream)
                    {
                        var line = SR.ReadLine();
                        Proverka2 = true;
                        break;
                    }
                }
                //Если файл cel не пустой, то из него извлекаются данные
                if (Proverka2)
                {
                    using (StreamReader SR = new StreamReader(PathCell))
                    {
                        while (!SR.EndOfStream)
                        {
                            var line = SR.ReadLine();
                            if (line.StartsWith("UNISYS_CELL"))
                            {
                                for (int i = 0; i < Elements.Length; i++)
                                {
                                    Proverka = false;
                                    line = SR.ReadLine();
                                    string[] StringInFile = line.Split(' ');
                                    Elements[i] = new TFemElement_Transformable
                                    {
                                        ChimeraId = int.Parse(StringInFile[1])
                                    };
                                    for (int j = 0; j < NumbersChim.Count; j++)
                                    {
                                        if (Elements[i].ChimeraId == NumbersChim[j])
                                        {
                                            Proverka = true;
                                            break;
                                        }
                                        else continue;
                                    }
                                    if (Proverka == false) NumbersChim.Add(Elements[i].ChimeraId);
                                }
                            }
                        }
                    }
                }
            }

            if (Proverka2 == false)
            {
                // Если пустой файл cel или вообще не пришел, то айди химеры всех элементов = 1
                for (int i = 0; i < Elements.Length; i++)
                {
                    Elements[i] = new TFemElement_Transformable
                    {
                        ChimeraId = 1
                    };
                }
            }

            numberOfChimeras = NumbersChim.Count;

            // Запись из листа с ячейками в FiniteElementModel.Elements_Arr 
            for (int i = 0; i < Cells.Length; i++)
            {
                if (Elements[i] == null) Elements[i] = new TFemElement_Transformable();

                Elements[i].Faces = new TFemFace_Transformable[Cells[i].Count];
                for (int j = 0; j < Cells[i].Count; j++)
                {
                    Elements[i].Faces[j] = Faces[Cells[i][j]];
                }
            }
        }
        //----------------------------------------------------------------
        /// <summary>
        ///  Изменение матрицы трансформации объекта и его детей
        /// </summary>
        /// <param name="MatrixTransform">Матрица трансформации (4х4)</param>
        /// <param name="IdChimera">Номер химеры</param>
        //public void GlobalTransform(Matrix MatrixTransform, int IdChimera)
        //{
        //    // Рекурсивный метод поиска детей, от того объекта, номер химеры, который прислали 
        //    for (int i = 0; i < DependencyTable.ChimeraID.Length; i++)
        //    {
        //        if (DependencyTable.ChimeraID[i] == IdChimera)
        //        {
        //            for (int j = 0; j < DependencyTable.ParentID.Length; j++)
        //            {
        //                if (DependencyTable.ParentID[j] == i)
        //                {
        //                    GlobalTransform(MatrixTransform, DependencyTable.ChimeraID[j]);
        //                }
        //            }
        //        }
        //    }
        //    // Локальная трансформация объека
        //    LocalTransform(MatrixTransform, IdChimera);
        //}
        //----------------------------------------------------------------
        /// <summary>
        /// Изменение матрицы трансформации объекта
        /// </summary>
        /// <param name="MatrixTransform">Матрица трансформации (4х4)</param>
        /// <param name="IdChimera">Номер химеры</param>
        public void LocalTransform(Matrix MatrixTransform, int IdChimera)
        {
            // Массив для определения, какие грани принадлежат указанной химере, и соответственно, нужно ли их матрицы менять
            bool[] TrueNode = new bool[Nodes.Length];
            for (int i = 0; i < TrueNode.Length; i++)
            {
                TrueNode[i] = false;
            }
            // Заполнения массива по принадлежности химер к граням
            for (int i = 0; i < Elements.Length; i++)
            {
                if (Elements[i].ChimeraId == IdChimera)
                {
                    for (int j = 0; j < Elements[i].Faces.Length; j++)
                    {
                        for (int k = 0; k < Elements[i].Faces[j].Node_id.Length; k++)
                        {
                            TrueNode[Elements[i].Faces[j].Node_id[k]] = true;
                        }
                    }
                }
                else continue;
            }
            // Изменение матриц граней
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (TrueNode[i] == true)
                {
                    Vector3 Coord = new Vector3(Nodes[i].X, Nodes[i].Y, Nodes[i].Z);
                    // Тут не изменяется вектор, может быть матрицу перемещения неправильно составила, хотя вроде по инету правильно
                    Vector3 NewCoord = Vector3.Transform(Coord, MatrixTransform);
                    Nodes[i].X = NewCoord.X;
                    Nodes[i].Y = NewCoord.Y;
                    Nodes[i].Z = NewCoord.Z;
                }
            }
        }
        //----------------------------------------------------------------
        /// <summary>
        /// Запись ngeom файла с измененными точками.
        /// </summary>
        /// <param name="PathNGeom">Путь к файлу для записи. Если файл по данному пути существует - он будет перезаписан.</param>
        public void WriteNgeom(string PathNGeom)
        {
            // Запись первой неизменяемой части файла
            var SR = new StreamReader(InitialNgeomFilePath);
            var SW = new StreamWriter(PathNGeom, false);
            int lineCounter = 0;
            while (!SR.EndOfStream)
            {
                lineCounter++;
                if (lineCounter <= UnModifiedFirstPart)
                {
                    SW.WriteLine(SR.ReadLine());
                }
                else { lineCounter--; break; }
            }
            SW.Close();

            string[] NodesTransform = new string[Nodes.Length];
            for (int i = 0; i < Nodes.Length; i++)
            {
                var InvC = CultureInfo.InvariantCulture;
                //
                string X = Nodes[i].X.ToString("g17", InvC);
                string Y = Nodes[i].Y.ToString("g17", InvC);
                string Z = Nodes[i].Z.ToString("g17", InvC);
                string Number = (i + 1).ToString(InvC);
                string line = Number + " " + X + " " + Y + " " + Z;
                NodesTransform[i] = line;
            }
            File.AppendAllLines(PathNGeom, NodesTransform);

            // Пропуск измененных строк
            SW = new StreamWriter(PathNGeom, true);
            while (!SR.EndOfStream)
            {
                lineCounter++;
                if(lineCounter < UnModifiedLastPart)
                {
                    SR.ReadLine();
                }
                else { break; }
            }
            // Запись последней неизменяемой части файла
            while (!SR.EndOfStream)
            {
                SW.WriteLine(SR.ReadLine());
            }
            SR.Close();
            SW.Close();
        }
        //----------------------------------------------------------------
        public TFiniteElementModel_Transformable(TLogos_TVD API)
        {
            this.API = API;
            //this.DependencyTable = API.DependencyTable;
        }
        //----------------------------------------------------------------
    }
}
