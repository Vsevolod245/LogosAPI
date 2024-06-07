// Пасринг файлов из логоса для графиков
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Reflection;
//
using AstraEngine.Components;
using AstraEngine;
using AstraEngine.Components.MathHelper;
using Project.Frontend;
using Logos_TVD;
//*****************************************************************
namespace Example
{
    internal partial class TViewerAero_Parser
    {
        /// <summary>
        /// Список путей к файлам
        /// </summary>
        public List<string> PathFiles=new List<string>();
        //-------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Список строчек из файла форс
        /// </summary>
        private List<string> Force = new List<string>();
        //-------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Список строчек из файла форс коэффициенты
        /// </summary>
        private List<string> Coeff = new List<string>();
        //-------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Список кортежей для моментов
        /// </summary>
        private List<(string Name,List<string> Values)> Moment = new List<(string, List<string>)>();
        /// <summary>
        /// Углы атаки
        /// </summary>
        private List<float> AngleOfAttack = new List<float>();
        //-------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Углы скольжения
        /// </summary>
        private List<float> SlidingAngle = new List<float>();
        //-------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Список настроек моментов для тел
        /// </summary>
        public List<TSettings_Moment> Settings_Moment = new List<TSettings_Moment>();
        //-------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Плотность для данного расчета
        /// </summary>
        public double Density;
        //-------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Скорость для данного расчета
        /// </summary>
        public double Velocity;
        //-------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Парсинг файлов моментов и сил из логоса
        /// </summary>
        /// <param name="LocalPathInDownloadF"></param>
        /// <param name="LocalPathInDownloadM"></param>
        /// <param name="PathWriter"></param>
        public void ParserForceAndMoment(string LocalPathInDownloadF, string LocalPathInDownloadM, string PathWriter, int Index)
        {
            try
            {
                // Получить список путей файлов с силами
                string[] ForcAndCoeff = Directory.GetFiles(LocalPathInDownloadF);
                // индекс файла с силами
                int index = Array.FindIndex(ForcAndCoeff, x => System.IO.Path.GetExtension(x) == ".force");
                // индекс файла с моментами
                int index1 = Array.FindIndex(ForcAndCoeff, x => System.IO.Path.GetExtension(x) == ".force_coeff");
                // отпарсить и записать эти файлы
                ParserForceAndCoef(ForcAndCoeff[index], ForcAndCoeff[index1], PathWriter, Index);
                // список путей с файлами моментов 
                var M = Directory.GetFiles(LocalPathInDownloadM);
                // Распределение файлов моментов по телам
                List<List<string>> Moments = new List<List<string>>();
                Moments.Add(new List<string>());
                Moments[0].Add(M[0]);
                bool flag = false;
                for (int i = 1; i < M.Length; i++)
                {
                    var p = Path.GetFileNameWithoutExtension(M[i]);
                    p = p.Remove(0, 3);
                    for (int j = 0; j < Moments.Count; j++)
                    {
                        if (Moments[j].FindIndex(item => item.Contains(p)) != -1)
                        {
                            Moments[j].Add(M[i]);
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        Moments.Add(new List<string>());
                        Moments[Moments.Count - 1].Add(M[i]);
                    }
                    flag = false;
                }
                Moment.Clear();
                // Запись строчек из моментов для одного тела в одну строчку
                for (int i = 0; i < Moments.Count; i++)
                {
                    OneStrMoment(Moments[i]);
                }
                // Парсинг моментов и сохранение результатов
                ParserMoment(PathWriter, Index);
                // Удалить логосовские файлы из папок, чтобы следубщие расчеты туда сохранялись без ошибок
                var p1 = Directory.GetFiles(LocalPathInDownloadF);
                for (int i=0; i<p1.Length; i++)
                {
                    File.Delete(p1[i]);
                }
                p1= Directory.GetFiles(LocalPathInDownloadM);
                for (int i = 0; i < p1.Length; i++)
                {
                    File.Delete(p1[i]);
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0232: Error TViewerAero_Parser_ForceAndMoment:ParserForceAndMoment(): " + E.Message);
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Объединение одной строчки файлов моментов в одну для одного тела
        /// </summary>
        /// <param name="FilesForMoment">Список путей к файлам</param>
        public void OneStrMoment (List<string> FilesForMoment)
        {
            try
            {
                // Получить имя тела
                var p = Path.GetFileNameWithoutExtension(FilesForMoment[0]);
                p = p.Remove(0, 3);
                Moment.Add((p, new List<string>()));
                // Запись значений из каждого файла
                List<List<string>> StructMoments = new List<List<string>>();
                for (int i = 0; i < FilesForMoment.Count; i++)
                {
                    StructMoments.Add(new List<string>());
                    string line;
                    using (StreamReader SR = new StreamReader(FilesForMoment[i]))
                    {
                        while (!SR.EndOfStream)
                        {
                            line = SR.ReadLine();
                            var Array = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            StructMoments[i].Add(Array[2]);
                        }
                    }
                }
                // Запись значений в правильном порядке
                for (int i = 0; i < StructMoments.Count; i++)
                {
                    for (int j = 0; j < StructMoments[i].Count; j++)
                    {
                        if (i == 0)
                        {
                            Moment[Moment.Count - 1].Values.Add(StructMoments[i][j] + ' ');
                            continue;
                        }
                        Moment[Moment.Count - 1].Values[j] += StructMoments[i][j] + ' ';
                    }
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0232: Error TViewerAero_Parser_ForceAndMoment:OneStrMoment(): " + E.Message);
            }
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Парсинг файлов с коэффициентами
        /// </summary>
        /// <param name="PathReaderForce">Путь файла, который читаем</param>
        /// <param name="PathWriter">Путь до папки, куда записываем</param>
        public void ParserForceAndCoef (string PathReaderForce, string PathReaderCoeff, string PathWriter, int Index)
        {
            try
            { 
                string line;
                PathFiles.Clear();
                // Список моделей
                var Models = TFrontend_Project.Get_AerodynamicModels3D();
                // Читаем первую строчку в файле, в ней имена всех тел
                using (StreamReader SR = new StreamReader(PathReaderForce))
                {
                    line = SR.ReadLine();
                    line = line.Trim();
                    var t = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 2; i < t.Length; i += 7)
                    {
                        if (!(Directory.Exists(PathWriter + t[i] + "//")))
                        {
                            Directory.CreateDirectory(PathWriter + t[i] + "//");
                        }
                        // Добавить углы атаки и скольжения тел для данной итерации
                        for (int k = 0; k < Models.Count; k++)
                        {
                            if (Models[k].Name.Contains(t[i]))
                            {
                                var PosRotScale = TMath.World_DecomposeMatrix(Models[k].GetWorldMatrix());
                                AngleOfAttack.Add(-PosRotScale.Item2.Z);
                                SlidingAngle.Add(-PosRotScale.Item2.Y);
                                break;
                            }  
                        }
                        PathFiles.Add(PathWriter + t[i] + "//"+ Index + "_" + t[i] + "_ForceAndMoment" + ".csv");
                    }
                }
                // Читаем файл .Force и записываем значения оттуда в лист
                using (StreamReader SR = new StreamReader(PathReaderForce))
                {
                    
                    while (!SR.EndOfStream)
                    {
                        line = SR.ReadLine();
                        Force.Add(line);
                    }
                }
                // Читаем файл .ForceCoeff и записываем значения оттуда в лист
                using (StreamReader SR = new StreamReader(PathReaderCoeff))
                {
                    while (!SR.EndOfStream)
                    {
                        Coeff.Add(line);
                        line = SR.ReadLine();
                    }
                }
                for (int i=0; i<Force.Count; i++)
                {
                    Force[i] = Force[i].Trim();
                    Coeff[i] = Coeff[i].Trim();
                    var F = Force[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var C = Coeff[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int a = 2;
                    for (int j = 0; j < PathFiles.Count; j++)
                    {
                        // Записываем строчку в нужный файл
                        WriteBody(PathFiles[j], a, F, C, j);
                        a += 7;
                    }
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0204: Error TViewerAero_Parser:ParserForceAndCoef(): " + E.Message);
            }
        }
        //-------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Запись строчки в нужный файл
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="Index"></param>
        /// <param name="Line"></param>
        public void WriteBody(string Path, int Index, string[] LineForce, string[] LineCoeff, int IndexAngle)
        {
            try
            { 
                // Если файла не существует, то создаем его и записываем шапку таблички
                if (!File.Exists(Path))
                {
                    using (StreamWriter WR = new StreamWriter(Path))
                    {
                        WR.WriteLine("AngleOfAttack;SlidingAngle");
                        WR.WriteLine(AngleOfAttack[IndexAngle] + ";" + SlidingAngle[IndexAngle]);
                        WR.WriteLine("Iteration;Cx;Cy;Cz;DragForce;LiftForce;SideForce");
                    }
                }
                // Записываем строчку в файл
                using (StreamWriter WR=new StreamWriter(Path,true))
                {
                    CultureInfo culture = new CultureInfo("ru-RU");
                    CultureInfo.DefaultThreadCurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentUICulture= culture;
                    //double o = double.Parse(Line[Index + 4], CultureInfo.InvariantCulture);
                    string one = (double.Parse(LineCoeff[Index + 4], CultureInfo.InvariantCulture)+ double.Parse(LineCoeff[Index + 1], CultureInfo.InvariantCulture)).ToString(culture);
                    string two = (double.Parse(LineCoeff[Index + 5], CultureInfo.InvariantCulture) + double.Parse(LineCoeff[Index + 2], CultureInfo.InvariantCulture)).ToString(culture);
                    string three = (double.Parse(LineCoeff[Index + 6], CultureInfo.InvariantCulture) + double.Parse(LineCoeff[Index + 3], CultureInfo.InvariantCulture)).ToString(culture);
                    string four = (double.Parse(LineForce[Index + 4], CultureInfo.InvariantCulture) + double.Parse(LineForce[Index + 1], CultureInfo.InvariantCulture)).ToString(culture);
                    string five = (double.Parse(LineForce[Index + 5], CultureInfo.InvariantCulture) + double.Parse(LineForce[Index + 2], CultureInfo.InvariantCulture)).ToString(culture);
                    string six = (double.Parse(LineForce[Index + 6], CultureInfo.InvariantCulture) + double.Parse(LineForce[Index + 3], CultureInfo.InvariantCulture)).ToString(culture);
                    WR.WriteLine(LineForce[0] + ";" + one + ";" + two + ";" + three+ ";" + four + ";" + five + 
                        ";" + six);
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0204: Error TViewerAero_Parser:WriteBody(): " + E.Message);
            }
        }
        //-------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Парсинг моментов и сохранение результатов
        /// </summary>
        /// <param name="PathWriter">Путь до папки для сохранения</param>
        public void ParserMoment(string PathWriter, int Index)
        {
            try
            {

                // Проходим по всем телам 
                for (int i = 0; i < Moment.Count; i++)
                {
                    // Проверяем есть ли папка для каждого тела
                    if (!(Directory.Exists(PathWriter + Moment[i].Name + "//")))
                    {
                        Directory.CreateDirectory(PathWriter + Moment[i].Name + "//");
                    }
                    // Путь для сохранения
                    string PathM = PathWriter + Moment[i].Name + "//" + Index + "_" + Moment[i].Name + "_ForceAndMoment" + ".csv";
                    CultureInfo culture = new CultureInfo("ru-RU");
                    CultureInfo.DefaultThreadCurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentUICulture = culture;
                    // Ищем настройки моментов для данного тела
                    var index = Settings_Moment.FindIndex(item => item.NameBody == Moment[i].Name);
                    // Если файл существует, значит, туда записали силы, добавляем к этим силам моменты
                    if (File.Exists(PathM))
                    {
                        List<string> File = new List<string>();
                        string line;
                        using (StreamReader SR = new StreamReader(PathM))
                        {
                            int a = 0;
                            while (!SR.EndOfStream)
                            {
                                line = SR.ReadLine();
                                if (line.Contains("AngleOfAttack"))
                                {
                                    a++;
                                    if (a > 1) break;
                                }
                                File.Add(line);
                            }
                        }
                        using (StreamWriter WR = new StreamWriter(PathM, false))
                        {
                            WR.WriteLine(File[0] + ";PointX;PointY;PointZ");
                            WR.WriteLine(File[1] + $";{Settings_Moment[index].PointMoment.X.ToString()};{Settings_Moment[index].PointMoment.Y.ToString()};{Settings_Moment[index].PointMoment.Z.ToString()}");
                            WR.WriteLine(File[2] + ";Mx;My;Mz;RollMoment;YawMoment;PitchMoment");
                            for (int j = 0; j < Moment[i].Values.Count; j++)
                            {
                                var str = Moment[i].Values[j].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                double Mx = double.Parse(str[0], CultureInfo.InvariantCulture);
                                double My = double.Parse(str[1], CultureInfo.InvariantCulture);
                                double Mz = double.Parse(str[2], CultureInfo.InvariantCulture);
                                double RollMoment = Mx * Density * Velocity * Velocity * 0.5 * Settings_Moment[index].RefL * Settings_Moment[index].RefS;
                                double YawMoment = My * Density * Velocity * Velocity * 0.5 * Settings_Moment[index].RefL * Settings_Moment[index].RefS;
                                double PitchMoment = Mz * Density * Velocity * Velocity * 0.5 * Settings_Moment[index].RefL * Settings_Moment[index].RefS;
                                WR.WriteLine($"{File[j+3]};{Mx.ToString(culture)};{My.ToString(culture)};{Mx.ToString(culture)};{RollMoment.ToString(culture)};" +
                                    $"{YawMoment.ToString(culture)};{PitchMoment.ToString(culture)}");
                            }
                        }  
                    }
                    else
                    {
                        // Если файл не существует, то создаем новый, где будут только моменты
                        float AngleOfAttac = 0;
                        float SlidingAngle = 0;
                        var Models = TFrontend_Project.Get_AerodynamicModels3D();
                        for (int k = 0; k < Models.Count; k++)
                        {
                            if (Models[k].Name.Contains(Moment[i].Name))
                            {
                                var PosRotScale = TMath.World_DecomposeMatrix(Models[k].GetWorldMatrix());
                                AngleOfAttac = -PosRotScale.Item2.Z;
                                SlidingAngle = -PosRotScale.Item2.Y;
                                break;
                            }
                        }
                        using (StreamWriter WR = new StreamWriter(PathM))
                        {
                            WR.WriteLine("AngleOfAttack;SlidingAngle;PointX;PointY;PointZ");
                            WR.WriteLine($"{AngleOfAttac};{SlidingAngle};{Settings_Moment[index].PointMoment.X.ToString()};{Settings_Moment[index].PointMoment.Y.ToString()};{Settings_Moment[index].PointMoment.Z.ToString()}");
                            WR.WriteLine("Iteration;Mx;My;Mz;RollMoment;YawMoment;PitchMoment");
                            for (int j = 0; j < Moment[i].Values.Count; j++)
                            {
                                var str = Moment[i].Values[j].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                double Mx = double.Parse(str[0], CultureInfo.InvariantCulture);
                                double My = double.Parse(str[1], CultureInfo.InvariantCulture);
                                double Mz = double.Parse(str[2], CultureInfo.InvariantCulture);
                                double RollMoment = Mx * Density * Velocity * Velocity * 0.5 * Settings_Moment[index].RefL * Settings_Moment[index].RefS;
                                double YawMoment = My * Density * Velocity * Velocity * 0.5 * Settings_Moment[index].RefL * Settings_Moment[index].RefS;
                                double PitchMoment = Mz * Density * Velocity * Velocity * 0.5 * Settings_Moment[index].RefL * Settings_Moment[index].RefS;
                                WR.WriteLine($"{j + 1};{Mx.ToString(culture)};{My.ToString(culture)};{Mx.ToString(culture)};{RollMoment.ToString(culture)};" +
                                    $"{YawMoment.ToString(culture)};{PitchMoment.ToString(culture)}");
                            }
                        }
                    }
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0204: Error TViewerAero_Parser:ParserMoment(): " + E.Message);
            }
        }
        //--------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Чтение файла для извлечения нужной величины
        /// </summary>
        /// <param name="Path">Путь до файла</param>
        /// <param name="Index">Индекс силы, которую нужно считать</param>
        /// <returns></returns>
        public (double[] xs, double[] ys, string Name) ReaderForce (string Path, int Index)
        {

            List<double> xs = new List<double>();
            List<double> ys = new List<double>();
            string name = "";
            string line;
            using (StreamReader SR = new StreamReader(Path))
            {
                line = SR.ReadLine();
                line = SR.ReadLine();
                line = SR.ReadLine();
                var t = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                name = t[Index];
                line = SR.ReadLine();
                int a = 0;
                while (line != null)
                {
                    try
                    {
                        line = line.Trim();
                        t = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (t.Length < 1)
                        {
                            break;
                        }

                        if (double.Parse(t[0]) == 1)
                        {
                            a++;
                            if (a > 1)
                            {
                                break;
                            }
                        }

                        xs.Add(double.Parse(t[0]));
                        ys.Add(double.Parse(t[Index]));
                        line = SR.ReadLine();
                    }
                    catch (Exception E)
                    {
                        TJournalLog.WriteLog("C0204: Error TViewerAero_Parser:ReaderForce (Цикл)(): " + E.Message);
                        break;
                    }

                }
            }
            return (xs.ToArray(), ys.ToArray(), name);
        }
        //-------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Отпарсить ямл файл, чтобы получить моменты, если они там есть
        /// </summary>
        /// <param name="PathYaml"></param>
        public void ParserInYamlMoment(string PathYaml)
        {
            try
            { 
                string line = "";
                Vector3 Point = new Vector3();
                using (StreamReader SR = new StreamReader(PathYaml))
                {
                    line = SR.ReadLine();
                    while (line != null)
                    {
                        line = SR.ReadLine();
                        if (line.Contains("PATCHES:"))
                        {
                            while (true)
                            {
                                line = SR.ReadLine();
                                if (line.Contains("name:") && !line.Contains("{"))
                                {
                                    var a = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    var b = Settings_Moment.FindAll(item => item.NameBody == a[a.Length - 1]);
                                    if (b.Count < 1)
                                    {
                                        TSettings_Moment SM = new TSettings_Moment();
                                        SM.NameBody = a[a.Length - 1];
                                        Settings_Moment.Add(SM);
                                    }
                                }
                                if (line.Contains("ACOUSTIC_OBSERVERS:")||line.Contains("SPATIAL DISCRETIZATION:")) break;
                            }
                        }
                        if (line.Contains("MOMENTS_USER:"))
                        {
                            while (true)
                            {
                                line = SR.ReadLine();
                            
                                if (line.Contains("point:"))
                                {
                                    line = line.Trim();
                                    double x;
                                    List<double> values = new List<double>();
                                    CultureInfo culture = new CultureInfo("ru-RU");
                                    CultureInfo.DefaultThreadCurrentCulture = culture;
                                    CultureInfo.DefaultThreadCurrentUICulture = culture;
                                    var Array= line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    for (int i=0; i<Array.Length; i++)
                                    {
                                        var ArrayMini = Array[i].Split(new char[] { ',', '}' }, StringSplitOptions.RemoveEmptyEntries);
                                        for (int j=0; j<ArrayMini.Length; j++)
                                        {
                                            bool isNumber = double.TryParse(ArrayMini[j], NumberStyles.Float, CultureInfo.InvariantCulture, out x);
                                            if (isNumber) values.Add(x);
                                        }
                                    }
                                    if (values.Count>2)
                                    {
                                        Point.X = (float)values[0];
                                        Point.Y = (float)values[1];
                                        Point.Z = (float)values[2];
                                    }
                                
                                }
                                if (line.Contains("patches:"))
                                {
                                    line= SR.ReadLine();
                                    var a = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    var b = Settings_Moment.FindAll(item => item.NameBody == a[a.Length - 1]);
                                    if (b.Count < 1)
                                    {
                                        TSettings_Moment SM = new TSettings_Moment();
                                        SM.NameBody = a[a.Length - 1];
                                        SM.PointMoment = Point;
                                        SM.NeedMoment = true;
                                        Settings_Moment.Add(SM);
                                    }
                                    else
                                    {
                                        var index = Settings_Moment.FindIndex(item => item.NameBody == a[a.Length - 1]);
                                        Settings_Moment[index].PointMoment = Point;
                                        Settings_Moment[index].NeedMoment = true;
                                    }
                                }
                                if (line.Contains("FORCE_USER:")) break;
                            }
                        }
                        if (line.Contains("ACOUSTIC_OBSERVERS:")) break;
                    }
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0204: Error TViewerAero_Parser:ParserInYamlMoment(): " + E.Message);
            }
        }
        //-------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Изменить настройки моментов
        /// </summary>
        /// <param name="SM"></param>
        public void ChangingSettings(TSettings_Moment SM)
        {
            try
            {
                var index = Settings_Moment.FindIndex(item => item.NameBody == SM.NameBody);
                Settings_Moment[index] = SM;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0204: Error TViewerAero_Parser:ChangingSettings(): " + E.Message);
            }
        }
        //-------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Переписать ямл файл с моментами
        /// </summary>
        /// <param name="Velocity">Скорость</param>
        /// <param name="Density">Плотность</param>
        /// <param name="PathYaml">Путь до ямла файла на локальном компьютере</param>
        public void WriteMomentsInYaml (float Velocity, float Density, string PathYaml)
        {
            try
            {
                // Создаем строку с заготовленным шаблоном для моментов
                string Text = "";
                var invC = CultureInfo.InvariantCulture;
                for (int i = 0; i < Settings_Moment.Count; i++)
                {
                    if (Settings_Moment[i].NeedMoment)
                    {
                        string Shablon =
                            $"    -\r\n" +
                            $"      name: Mx_{Settings_Moment[i].NameBody}\r\n" +
                            $"      parameters:\r\n" +
                            $"        axis: {{type: direction, x: 1, y: 0, z: 0, x2: 0, y2: 0, z2: 2}}\r\n" +
                            $"        point: {{x: {Settings_Moment[i].PointMoment.X.ToString(invC)}, y: {Settings_Moment[i].PointMoment.Y.ToString(invC)}, z: {Settings_Moment[i].PointMoment.Z.ToString(invC)}}}\r\n" +
                            $"        reference: {{RefS: {Settings_Moment[i].RefS.ToString(invC)}, RefL: {Settings_Moment[i].RefL.ToString(invC)}, InfV: {Velocity.ToString(invC)}, InfR: {Density.ToString(invC)}}}\r\n" +
                            $"        output: {{step: 1, time: 1}}\r\n" +
                            $"        patches:\r\n" +
                            $"          - {Settings_Moment[i].NameBody}\r\n" +
                            $"    -\r\n" +
                            $"      name: My_{Settings_Moment[i].NameBody}\r\n" +
                            $"      parameters:\r\n" +
                            $"        axis: {{type: direction, x: 0, y: 1, z: 0, x2: 0, y2: 0, z2: 2}}\r\n" +
                            $"        point: {{x: {Settings_Moment[i].PointMoment.X.ToString(invC)}, y: {Settings_Moment[i].PointMoment.Y.ToString(invC)}, z: {Settings_Moment[i].PointMoment.Z.ToString(invC)}}}\r\n" +
                            $"        reference: {{RefS: {Settings_Moment[i].RefS.ToString(invC)}, RefL: {Settings_Moment[i].RefL.ToString(invC)}, InfV: {Velocity.ToString(invC)}, InfR: {Density.ToString(invC)}}}\r\n" +
                            $"        output: {{step: 1, time: 1}}\r\n" +
                            $"        patches:\r\n" +
                            $"          - {Settings_Moment[i].NameBody}\r\n" +
                            $"    -\r\n" +
                            $"      name: Mz_{Settings_Moment[i].NameBody}\r\n" +
                            $"      parameters:\r\n" +
                            $"        axis: {{type: direction, x: 0, y: 0, z: 1, x2: 0, y2: 0, z2: 2}}\r\n" +
                            $"        point: {{x: {Settings_Moment[i].PointMoment.X.ToString(invC)}, y: {Settings_Moment[i].PointMoment.Y.ToString(invC)}, z: {Settings_Moment[i].PointMoment.Z.ToString(invC)}}}\r\n" +
                            $"        reference: {{RefS: {Settings_Moment[i].RefS.ToString(invC)}, RefL: {Settings_Moment[i].RefL.ToString(invC)}, InfV: {Velocity.ToString(invC)}, InfR: {Density.ToString(invC)}}}\r\n" +
                            $"        output: {{step: 1, time: 1}}\r\n" +
                            $"        patches:\r\n" +
                            $"          - {Settings_Moment[i].NameBody}\r\n";
                        Text += Shablon;
                    }
                }
                string TextYaml = "";
                string line = "";
                // Читаем ямл файл в строку и в нужном месте добавляем заготовленный шаблон
                using (StreamReader SR = new StreamReader(PathYaml, encoding: Encoding.GetEncoding(1251)))
                {
                    line = SR.ReadLine();
                    while (line != null)
                    {
                        TextYaml += line+ "\r\n";
                        if (line.Contains("SCENES") || line.Contains("LOGOS_AVIA")) break;
                        line = SR.ReadLine();
                        if (line.Contains("MOMENTS_USER:"))
                        {
                            TextYaml += $"  MOMENTS_USER:\r\n" + Text;
                            //TextYaml += "  FORCE_USER: ~\r\n";
                            while (!line.Contains("FORCE_USER")) line = SR.ReadLine();
                        }
                    }
                }
                // Запись измененного файла
                using (StreamWriter writer = new StreamWriter(PathYaml, false, encoding: Encoding.GetEncoding(1251)))
                {
                    writer.WriteLine(TextYaml);
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0049: Error TViewerAero_Parser:WriteMomentsInYaml(): " + E.Message);
            }
        }
        //-------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Записать значения в нужном порядке для Эйлера 
        /// </summary>
        /// <param name="Index">Индекс итерации</param>
        /// <param name="Name">Имя тела</param>
        /// <returns>Флаг - проверка, Values - лист значений для одного тела</returns>
        public (int flag, List<double> Values) ValuesForceAndMoment (int Index, string Name)
        {
            // Путь до результатов
            string PathToFiles = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "FilesForGraphs" + "\\";
            // Поиск нужной папки
            var d=Directory.GetDirectories(PathToFiles);
            int index= Array.FindIndex(d, x => x.Contains($"{Name}"));
            // Если такой папки не нашлось, то выход
            if (index < 0)
            {
                TJournalLog.WriteLog("C0049: Error TViewerAero_Parser:ValuesForceAndMoment(): Результатов для данного тела нет");
                return(-1, new List<double>());
            }
            // Поиск нужного файла для конкретной итерации
            var f = Directory.GetFiles(d[index]);
            index = Array.FindIndex(f, x => x.Contains($"{Index}_{Name}"));
            // Если такого файла не нашлось, то выход
            if (index < 0)
            {
                TJournalLog.WriteLog("C0049: Error TViewerAero_Parser:ValuesForceAndMoment(): Результатов для данного тела нет");
                return (-1, new List<double>());
            }
            // Парсим и записываем значения
            List<double> Values = new List<double>();
            string line;
            int str=0;
            string NeedLine="";
            using (StreamReader SR = new StreamReader(f[index]))
            {
                line = SR.ReadLine();
                line = SR.ReadLine();
                line = SR.ReadLine();
                
                while (!SR.EndOfStream)
                {
                    line = SR.ReadLine();
                    if (line.Contains("Iteration")) break;
                    var a = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    str = int.Parse(a[0]);
                }
            }
            using (StreamReader SR = new StreamReader(f[index]))
            {
                line = SR.ReadLine();
                line = SR.ReadLine();
                line = SR.ReadLine();

                while (!SR.EndOfStream)
                {
                    line = SR.ReadLine();
                    var a = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (str == int.Parse(a[0]))
                    {
                        NeedLine = line;
                        break;
                    }
                    
                }
            }
            // Нам нужны только силы и моменты, не коэффициенты
            var s= NeedLine.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if(s.Length > 0)
            {
                Values.Add(double.Parse(s[4]));
                Values.Add(double.Parse(s[5]));
                Values.Add(double.Parse(s[6]));
                Values.Add(double.Parse(s[10]));
                Values.Add(double.Parse(s[11]));
                Values.Add(double.Parse(s[12]));
            }
            else
            {
                s = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                Values.Add(double.Parse(s[4], CultureInfo.InvariantCulture));
                Values.Add(double.Parse(s[5], CultureInfo.InvariantCulture));
                Values.Add(double.Parse(s[6], CultureInfo.InvariantCulture));
                Values.Add(double.Parse(s[10], CultureInfo.InvariantCulture));
                Values.Add(double.Parse(s[11], CultureInfo.InvariantCulture));
                Values.Add(double.Parse(s[12], CultureInfo.InvariantCulture));

            }
            return (0, Values);
        }
    }
}
