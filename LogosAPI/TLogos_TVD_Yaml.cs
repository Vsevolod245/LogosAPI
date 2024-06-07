// Методы чтения, изменения и записи Yaml файлов
using System.Globalization;
using System.IO;
using System.Text;
//
using AstraEngine;
//***********************************************
namespace Logos_TVD
{
    public partial class TLogos_TVD
    {
        /// <summary>
        /// Таблица стандартной атмосферы
        /// </summary>
        TLogos_TVD_StandartAtmosphere StandartAtmosphere = new TLogos_TVD_StandartAtmosphere();
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Записать в ямл файл новое число Маха и высоту
        /// </summary>
        /// <param name="YamlPath">Путь к файлу yaml</param>
        /// <param name="Hvalue">Значение высоты</param>
        /// <param name="Mvalue">Число Маха</param>
        /// <param name="Velocity">Скорости U, V, W</param>
        public void ChangeMValueAndHValueInYaml(string YamlPath, float Hvalue, float Mvalue, Vector3 Velocity)
        {
            // Строка, в которую будет записываться измененный yaml
            string ChangedYaml = "";
            
            // Чтение из файла
            using (StreamReader reader = new StreamReader(YamlPath, encoding: Encoding.GetEncoding(1251)))
            {
                // Переменная для записи чисел с ".", а не с ","
                var invC = CultureInfo.InvariantCulture;
                // Флаги для отслеживания достигнутой части файла 

                // Индикатор того, что достигнут пункт PHYSICAL REGIONS
                bool PhysicalRegions = false;
                // Индикатор того, что достигнут пункт initialization в одном из регионов PHYSICAL REGIONS
                bool Initialization = false;
                // Индикатор того, что достигнут пункт PATCHES (внутри или вне пункта PHYSICAL REGIONS)
                bool Patches = false;
                // Индикатор того, что достигнут пункт PATCH_PRESSURE
                bool PatchPressure = false;
                // Индикатор того, что достигнут пункт PATCH_FREE_STREAM 
                bool PatchFreeStream = false;

                // Получаем значение давления и температуры на заданной высоте
                float P = StandartAtmosphere.GetPressure(Hvalue);
                float T = StandartAtmosphere.GetTemperatureOfKelvin(Hvalue);
                // Определяем модуль скорости и CosAx, CosAy, CosAz
                float VelocityModule = Velocity.Length();
                float CosAx = Velocity.X / VelocityModule;
                float CosAy = Velocity.Y / VelocityModule;
                float CosAz = Velocity.Z / VelocityModule;

                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    // Если находимся в пункте PHYSICAL REGIONS, то производим замену параметров атмосферы в каждом регионе и прикрепленных к нему pathces, если они есть
                    if (PhysicalRegions)
                    {
                        if (Initialization)
                        {
                            if (line.Contains("U:"))
                            {
                                ChangedYaml += ChangeOptionsInYaml(line, Velocity.X.ToString(invC));
                            }
                            else if (line.Contains("V:"))
                            {
                                ChangedYaml += ChangeOptionsInYaml(line, Velocity.Y.ToString(invC));
                            }
                            else if (line.Contains("W:"))
                            {
                                ChangedYaml += ChangeOptionsInYaml(line, Velocity.Z.ToString(invC));
                            }
                            else if (line.Contains("P:"))
                            {
                                ChangedYaml += ChangeOptionsInYaml(line, P.ToString(invC));
                            }
                            else if (line.Contains("T:"))
                            {
                                ChangedYaml += ChangeOptionsInYaml(line, T.ToString(invC));
                            }
                            else
                            {
                                Initialization = false;
                                ChangedYaml += line + "\n";
                            }
                        }
                        else if (Patches)
                        {
                            if (PatchPressure)
                            {
                                if (line.Contains("P:"))
                                {
                                    ChangedYaml += ChangeOptionsInYaml(line, P.ToString(invC));
                                }
                                else if (line.Contains("T:"))
                                {
                                    ChangedYaml += ChangeOptionsInYaml(line, T.ToString(invC));
                                }
                                else if (line.Contains("name:"))
                                {
                                    PatchPressure = false;
                                    ChangedYaml += line + "\n";
                                }
                                else if (line.Contains("group_name:"))
                                {
                                    PatchPressure = false;
                                    Patches = false;
                                    ChangedYaml += line + "\n";
                                }
                                else if (line.StartsWith("SPATIAL DISCRETIZATION:"))
                                {
                                    PatchPressure = false;
                                    Patches = false;
                                    PhysicalRegions = false;
                                    ChangedYaml += line + "\n";
                                }
                                else
                                {
                                    ChangedYaml += line + "\n";
                                }
                            }
                            else if (PatchFreeStream)
                            {
                                if (line.Contains("Pz:"))
                                {
                                    ChangedYaml += ChangeOptionsInYaml(line, P.ToString(invC));
                                }
                                else if(line.Contains("Tz:"))
                                {
                                    ChangedYaml += ChangeOptionsInYaml(line, T.ToString(invC));
                                }
                                else if (line.Contains("M:"))
                                {
                                    ChangedYaml += ChangeOptionsInYaml(line, Mvalue.ToString(invC));
                                }
                                else if(line.Contains("CosAx:"))
                                {
                                    ChangedYaml += ChangeOptionsInYaml(line, CosAx.ToString(invC));
                                }
                                else if(line.Contains("CosAy:"))
                                {
                                    ChangedYaml += ChangeOptionsInYaml(line, CosAy.ToString(invC));
                                }
                                else if(line.Contains("CosAz:"))
                                {
                                    ChangedYaml += ChangeOptionsInYaml(line, CosAz.ToString(invC));
                                }
                                else if (line.Contains("name:"))
                                {
                                    PatchFreeStream = false;
                                    ChangedYaml += line + "\n";
                                }
                                else if (line.Contains("group_name:"))
                                {
                                    PatchFreeStream = false;
                                    Patches = false;
                                    ChangedYaml += line + "\n";
                                }
                                else if (line.StartsWith("SPATIAL DISCRETIZATION:"))
                                {
                                    PatchFreeStream = false;
                                    Patches = false;
                                    PhysicalRegions = false;
                                    ChangedYaml += line + "\n";
                                }
                                else
                                {
                                    ChangedYaml += line + "\n";
                                }
                            }
                            else
                            {
                                if (line.Contains("PATCH_PRESSURE"))
                                {
                                    PatchPressure = true;
                                    ChangedYaml += line + "\n";
                                }
                                else if (line.Contains("PATCH_FREE_STREAM"))
                                {
                                    PatchFreeStream = true;
                                    ChangedYaml += line + "\n";
                                }
                                else if (line.Contains("group_name:"))
                                {
                                    Patches = false;
                                    ChangedYaml += line + "\n";
                                }
                                else if (line.StartsWith("SPATIAL DISCRETIZATION:"))
                                {
                                    Patches = false;
                                    PhysicalRegions = false;
                                    ChangedYaml += line + "\n";
                                }
                                else
                                {
                                    ChangedYaml += line + "\n";
                                }
                            }
                        }
                        else
                        {
                            // Отключаем стандартную атмосферу
                            if (line.Contains("StdModel:"))
                            {
                                ChangedYaml += "    StdModel: {";
                                string New_line = line.Replace("    StdModel: {", "");
                                string[] StdModel_Line = New_line.Split(',');
                                for (int i = 0; i < StdModel_Line.Length; i++)
                                {
                                    if (StdModel_Line[i].StartsWith("options"))
                                    {
                                        StdModel_Line[i] = StdModel_Line[i].Remove(StdModel_Line[i].LastIndexOf(' ') + 1);
                                        ChangedYaml += StdModel_Line[i] + "OFF";
                                        if (i == StdModel_Line.Length - 1)
                                        {
                                            ChangedYaml += "}\n";
                                            continue;
                                        }
                                        ChangedYaml += ",";
                                    }
                                    else
                                    {
                                        ChangedYaml += StdModel_Line[i];
                                        if (i == StdModel_Line.Length - 1)
                                        {
                                            ChangedYaml += "\n";
                                            continue;
                                        }
                                        ChangedYaml += ",";
                                    }
                                }
                            }
                            else if (line.Contains("initialization:"))
                            {
                                Initialization = true;
                                ChangedYaml += line + "\n";
                            }
                            else if (line.Contains("PATCHES:"))
                            {
                                Patches = true;
                                ChangedYaml += line + "\n";
                            }
                            else if (line.StartsWith("SPATIAL DISCRETIZATION:"))
                            {
                                PhysicalRegions = false;
                                ChangedYaml += line + "\n";
                            }
                            else
                            {
                                ChangedYaml += line + "\n";
                            }
                        }
                    }
                    // Если находимся в пункте PATCHES, то производим замену параметров каждого patch
                    else if (Patches)
                    {
                        if (PatchPressure)
                        {
                            if (line.Contains("P:"))
                            {
                                ChangedYaml += ChangeOptionsInYaml(line, P.ToString(invC));
                            }
                            else if (line.Contains("T:"))
                            {
                                ChangedYaml += ChangeOptionsInYaml(line, T.ToString(invC));
                            }
                            else if (line.Contains("name:"))
                            {
                                PatchPressure = false;
                                ChangedYaml += line + "\n";
                            }
                            else if (!line.StartsWith("  "))
                            {
                                PatchPressure = false;
                                Patches = false;
                                ChangedYaml += line + "\n";
                            }
                            else
                            {
                                ChangedYaml += line + "\n";
                            }
                        }
                        else if (PatchFreeStream)
                        {
                            if (line.Contains("Pz:"))
                            {
                                ChangedYaml += ChangeOptionsInYaml(line, P.ToString(invC));
                            }
                            else if (line.Contains("Tz:"))
                            {
                                ChangedYaml += ChangeOptionsInYaml(line, T.ToString(invC));
                            }
                            else if (line.Contains("M:"))
                            {
                                ChangedYaml += ChangeOptionsInYaml(line, Mvalue.ToString(invC));
                            }
                            else if (line.Contains("CosAx:"))
                            {
                                ChangedYaml += ChangeOptionsInYaml(line, CosAx.ToString(invC));
                            }
                            else if (line.Contains("CosAy:"))
                            {
                                ChangedYaml += ChangeOptionsInYaml(line, CosAy.ToString(invC));
                            }
                            else if (line.Contains("CosAz:"))
                            {
                                ChangedYaml += ChangeOptionsInYaml(line, CosAz.ToString(invC));
                            }
                            else if (line.Contains("name:"))
                            {
                                PatchFreeStream = false;
                                ChangedYaml += line + "\n";
                            }
                            else if (!line.StartsWith("  "))
                            {
                                PatchFreeStream = false;
                                Patches = false;
                                ChangedYaml += line + "\n";
                            }
                            else
                            {
                                ChangedYaml += line + "\n";
                            }
                        }
                        else
                        {
                            if (line.Contains("PATCH_PRESSURE"))
                            {
                                PatchPressure = true;
                                ChangedYaml += line + "\n";
                            }
                            else if (line.Contains("PATCH_FREE_STREAM"))
                            {
                                PatchFreeStream = true;
                                ChangedYaml += line + "\n";
                            }
                            else if (!line.StartsWith("  "))
                            {
                                Patches = false;
                                ChangedYaml += line + "\n";
                            }
                            else
                            {
                                ChangedYaml += line + "\n";
                            }
                        }
                    }
                    else
                    {
                        // Читаем файл в строку, пока не найдем места, в которых следует заменить параметры атмосферы
                        ChangedYaml += line + "\n";
                        if (line.StartsWith("PHYSICAL REGIONS:"))
                        {
                            PhysicalRegions = true;
                        }
                        else if (line.StartsWith("PATCHES:"))
                        {
                            Patches = true;
                        }
                    }
                }
            }
            // Запись измененного файла
            using (StreamWriter writer = new StreamWriter(YamlPath /*+ "out"*/, false, encoding: Encoding.GetEncoding(1251)))
            {
                writer.WriteLine(ChangedYaml);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Записать в ямл файл новое число Маха и высоту
        /// </summary>
        /// <param name="YamlPath">Путь к файлу yaml</param>
        /// <param name="Hvalue">Значение высоты</param>
        /// <param name="Mvalue">Число Маха</param>
        /// <param name="CosAx">Косинус угла для U</param>
        /// <param name="CosAy">Косинус угла для V</param>
        /// <param name="CosAz">Косинус угла для W</param>
        public void ChangeMValueAndHValueInYaml(string YamlPath, float Hvalue, float Mvalue, float CosAx, float CosAy, float CosAz)
        {
            // Получение скорости звука на заданной высоте
            float SpeedOfSound = StandartAtmosphere.GetSpeedOfSound(Hvalue);
            // Вычисление скорости набегающего потока при заданном Махе на данной высоте
            float FlowRate = Mvalue * SpeedOfSound;
            // Получение значений скоростей
            Vector3 Velocity = new Vector3(FlowRate * CosAx, FlowRate * CosAy, FlowRate * CosAz);
            ChangeMValueAndHValueInYaml(YamlPath, Hvalue, Mvalue, Velocity);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Изменить значение параметра строке в yaml
        /// </summary>
        /// <param name="line">исходная строка</param>
        /// <param name="option">значение для замены</param>
        /// <returns>строка с измененным параметром</returns>
        public string ChangeOptionsInYaml(string line, string option)
        {
            string NewLine = "";
            string[] Lines = line.Split(',');
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].StartsWith(" value"))
                {
                    Lines[i] = Lines[i].Remove(Lines[i].LastIndexOf(' ') + 1);
                    NewLine += Lines[i] + option;
                    if (i == Lines.Length - 1)
                    {
                        NewLine += "}";
                        continue;
                    }
                    NewLine += ",";
                }
                else
                {
                    NewLine += Lines[i];
                    if (i == Lines.Length - 1) continue;
                    NewLine += ",";
                }
            }
            return NewLine+"\n";
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Парсер ямла
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public (Vector3 Velocity, float M, float H, float Density, Vector3 Cos) ParserYaml(string Path)
        {
            
            string MValue = "";
            string UValue = "";
            string VValue = "";
            string WValue = "";
            string CosX = "";
            string CosY = "";
            string CosZ = "";
            string P="";
            Vector3 Velocity;
            float H;
            float M;
            // Флажок для окончания цикла
            int a = 0;

            // Поиск в ямл файле нужных данных
            using (StreamReader YAMLstr = new StreamReader(Path))
            {
                while (!YAMLstr.EndOfStream)
                {
                    var line = YAMLstr.ReadLine();
                    // Нахождение раздела, в котором хранятся соотвествующие данные
                    if (line.Contains("PHYSICAL REGIONS:"))
                    {
                        while (true)
                        {
                            line = YAMLstr.ReadLine();
                            if (line.Contains("initialization:"))
                            {
                                line = YAMLstr.ReadLine();
                                UValue = GetValueInLine(line);
                                line = YAMLstr.ReadLine();
                                VValue = GetValueInLine(line);
                                line = YAMLstr.ReadLine();
                                WValue = GetValueInLine(line);
                                break;
                            }
                            
                        }
                    }
                    if (line.Contains("PATCHES:"))
                    {
                        while (true)
                        {
                            // Идем по этому разделу
                            line = YAMLstr.ReadLine();
                            if (line.Contains("parameters:"))
                            {
                                while (true)
                                {
                                    line = YAMLstr.ReadLine();
                                    if (line.Contains("Pz:"))
                                    {
                                        //line = YAMLstr.ReadLine();
                                        P = GetValueInLine(line);
                                        line = YAMLstr.ReadLine();
                                        line = YAMLstr.ReadLine();
                                        MValue = GetValueInLine(line);
                                        line = YAMLstr.ReadLine();
                                        CosX = GetValueInLine(line);
                                        line = YAMLstr.ReadLine();
                                        CosY = GetValueInLine(line);
                                        line = YAMLstr.ReadLine();
                                        CosZ = GetValueInLine(line);
                                        break;
                                    }
                                }
                                break;
                            }
                            
                        }
                        a = 1;
                    }
                    // Если читает следующий раздел, то окончание поиска
                    if (a == 1) break;
                }
            }
            H = StandartAtmosphere.GetAltitudeFromPressure(float.Parse(P));
            M = float.Parse(MValue);
            Velocity = new Vector3(float.Parse(UValue), float.Parse(VValue), float.Parse(WValue));
            float Density = StandartAtmosphere.GetDensity(H);
            // Получение значений скоростей
            Vector3 Cos = new Vector3(float.Parse(CosX), float.Parse(CosY), float.Parse(CosZ));

            return (Velocity, M, H, Density, Cos);
        }
        //-----
        /// <summary>
        /// Получить высоту по плотности
        /// </summary>
        /// <param name="Density">плотность набегающего потока</param>
        /// <returns></returns>
        public float GetH (float Density)
        {
            float H = StandartAtmosphere.GetAltitudeFromDensity(Density);
            return H;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить из строки нужную часть с числовым значением
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public string GetValueInLine (string line)
        {
            int Num = line.IndexOf("value:");
            line = line.Remove(0, Num + 6);
            Num = line.IndexOf("}");
            line = line.Remove(Num);
            line = line.Trim();
            line = line.Replace(".", ",");
            return line;
        }
        #region
        ///// <summary>
        /////  Запись из ямл файла числа Маха и высоты по регионам
        ///// </summary>
        ///// <returns></returns>
        //public List<TYAMLCharacteristics> FindMValueAndHValue()
        //{
        //    // Список ямл характеристик
        //    List<TYAMLCharacteristics> HMValue = new List<TYAMLCharacteristics>();

        //    string Path = PathToCluster + UID.ToString();
        //    string YamlName = "";

        //    // Поиск ямл файла на удаленном компьютере
        //    string[] FileNames = SshConnection.RunCommand("cd " + Path + "; ls").Result.ToString().Trim('\n').Split('\n');
        //    foreach (string FileName in FileNames)
        //    {
        //        string Extension = System.IO.Path.GetExtension(FileName);
        //        if (Extension.Equals(".yaml"))
        //        {
        //            YamlName = FileName;
        //            break;
        //        }
        //    }

        //    // Скачивание ямл файла на локальный компьютер
        //    string LocalPath = Directory.GetCurrentDirectory() + "/" + YamlName;
        //    var Scp = new ScpClient(SshConnection.ConnectionInfo);
        //    Scp.Connect();
        //    if (Scp.IsConnected)
        //    {
        //        FileInfo LocalFile = new FileInfo(LocalPath);
        //        Scp.Download(Path + "/" + YamlName, LocalFile);
        //    }
        //    Scp.Disconnect();


        //    string HValue;
        //    string MValue;
        //    // Флажок для окончания цикла
        //    int a = 0;

        //    // Поиск в ямл файле нужных данных
        //    using (StreamReader YAMLstr = new StreamReader(LocalPath))
        //    {
        //        while (!YAMLstr.EndOfStream)
        //        {
        //            var line = YAMLstr.ReadLine();
        //            // Нахождение раздела, в котором хранятся соотвествующие данные
        //            if (line.Contains("PHYSICAL REGIONS:"))
        //            {
        //                while (true)
        //                {
        //                    // Идем по этому разделу
        //                    line = YAMLstr.ReadLine();

        //                    // group_name их несколько, поэтому проходим по разделу, пока он не закончится
        //                    if (line.Contains("group_name:"))
        //                    {
        //                        line = line.Trim();
        //                        line = line.Remove(0, 12);
        //                        TYAMLCharacteristics YAMLCharacteristics = new TYAMLCharacteristics();
        //                        HMValue.Add(YAMLCharacteristics);
        //                        HMValue[HMValue.Count - 1].GroupName = line;
        //                    }
        //                    if (line.Contains("StdModel:"))
        //                    {
        //                        HValue = line;
        //                        MValue = line;

        //                        int Num = HValue.IndexOf("Hvalue");
        //                        HValue = HValue.Remove(0, Num);
        //                        Num = HValue.IndexOf(",");
        //                        HValue = HValue.Remove(Num);
        //                        Num = HValue.IndexOf(":");
        //                        HValue = HValue.Remove(0, Num + 1);
        //                        HValue.Trim();
        //                        HValue = HValue.Replace(".", ",");
        //                        HMValue[HMValue.Count - 1].H = float.Parse(HValue);

        //                        Num = MValue.IndexOf("Mvalue");
        //                        MValue = MValue.Remove(0, Num);
        //                        Num = MValue.IndexOf(",");
        //                        MValue = MValue.Remove(Num);
        //                        Num = MValue.IndexOf(":");
        //                        MValue = MValue.Remove(0, Num + 1);
        //                        MValue = MValue.Trim();
        //                        MValue = MValue.Replace(".", ",");
        //                        HMValue[HMValue.Count - 1].M = float.Parse(MValue);
        //                    }

        //                    if (line.Contains("SPATIAL DISCRETIZATION"))
        //                    {
        //                        a = 1;
        //                        break;
        //                    }

        //                }
        //            }
        //            // Если читает следующий раздел, то окончание поиска
        //            if (a == 1) break;
        //        }
        //    }

        //    // Удаление ямл файла с локального компьюетра
        //    File.Delete(LocalPath);

        //    return HMValue;
        //}
        #endregion
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}

#region 
//----------------------------------------------------------------------------------------------------------
/// <summary>
/// Структура, в которой хранятся названия групп и их значения числа Маха и высоты
/// </summary>
//public class TYAMLCharacteristics
//{
//    /// <summary>
//    /// Название группы (региона)
//    /// </summary>
//    public string GroupName = "";
//    /// <summary>
//    /// Значение высоты
//    /// </summary>
//    public float H = -1;
//    /// <summary>
//    /// Значение числа Маха
//    /// </summary>
//    public float M = -1;
//}
#endregion