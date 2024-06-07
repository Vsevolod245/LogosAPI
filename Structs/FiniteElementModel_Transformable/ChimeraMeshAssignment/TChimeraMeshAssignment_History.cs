// Класс, содержащий методы для сохранения связи химер с объектами в файл и чтения из файла
using System;
using System.IO;
using System.Linq;
//
using AstraEngine.Components;
//******************************************************************
namespace Example
{
    /// <summary>
    /// Класс, содержащий методы для сохранения связи химер с объектами в файл и чтения из файла
    /// </summary>
    internal class TChimeraMeshAssignment_History
    {
        /// <summary>
        /// Путь, по которому будет записан журнал истории
        /// </summary>
        private readonly string Path = Directory.GetCurrentDirectory() + "\\ChimeraMeshAssignment";
        //---------------------------------------------------------------
        /// <summary>
        /// Сохранить таблицу в файл
        /// </summary>
        public void SaveInFile(Guid UID, TChimera_ObjectDependencyTable DependencyTable)
        {
            try
            {
                string UID_string = UID.ToString();
                // Формированеи строки со всей информацией
                string info = UID_string + "\n";
                for (int i = 0; i < DependencyTable.PathsToSTL.Length; i++)
                {
                    info += DependencyTable.PathsToSTL[i];
                    if (i == DependencyTable.PathsToSTL.Length - 1) break;
                    info += ";";
                }
                info += "\n";
                for (int i = 0; i < DependencyTable.ChimeraID.Length; i++)
                {
                    info += DependencyTable.ChimeraID[i].ToString();
                    if (i == DependencyTable.ChimeraID.Length - 1) break;
                    info += ";";
                }
                info += "\n";
                for(int i=0; i< DependencyTable.ParentID.Length; i++)
                {
                    info += DependencyTable.ParentID[i].ToString();
                    if (i == DependencyTable.ParentID.Length - 1) break;
                    info += ";";
                }
                info += "\n";
                // запись строки в файл
                using (StreamWriter writer = new StreamWriter(Path, true))
                {
                    writer.WriteLine(info);
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0001: Error TChimeraMeshAssignment_History:SaveInFile(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Чтение таблицы из файла
        /// </summary>
        public void ReadFromFile(Guid UID, TChimera_ObjectDependencyTable DT)
        {
            try
            {
                string UID_string = UID.ToString();
                bool uid = false;
                // Счетчик срок, после находления нужного UID
                int count = 0;
                using (StreamReader reader = new StreamReader(Path))
                {
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (uid)
                        {
                            switch (count)
                            {
                                case 0:
                                    DT.PathsToSTL = line.Split(';');
                                    break;
                                case 1:
                                    string[] ChimeraIDstr = line.Split(';');
                                    for (int i = 0; i < ChimeraIDstr.Count(); i++)
                                    {
                                        DT.ChimeraID[i] = Convert.ToInt32(ChimeraIDstr[i]);
                                    }
                                    break;
                                case 2:
                                    string[] ParentIDstr = line.Split(';');
                                    for (int i = 0; i < ParentIDstr.Count(); i++)
                                    {
                                        DT.ParentID[i] = Convert.ToInt32(ParentIDstr[i]);
                                    }
                                    break;
                                default:
                                    break;
                            }
                            if (count >= 2) break;
                            count++;
                        }
                        // Ищем строчку с нужным UID
                        else if (line.StartsWith(UID_string))
                        {
                            uid = true;
                        }
                    }
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0002: Error TChimeraMeshAssignment_History:ReadFromFile(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удаление таблицы из файла
        /// </summary>
        public void ClearFromFile(Guid UID)
        {
            try
            {
                // Строка для чтения файла без заданного UID
                string file_without_uid = "";
                string UID_string = UID.ToString();
                bool uid = false;
                // Счетчик срок, после находления нужного UID
                int count = 0;
                // Чтение файла
                using (StreamReader reader = new StreamReader(Path))
                {
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        //Пропускаем три строки после UID
                        if (uid)
                        {
                            count++;
                            if (count == 3) uid=false;
                        }
                        // Ищем строчку с нужным UID
                        else if (line.StartsWith(UID_string))
                        {
                            uid = true;
                        }
                        else
                        {
                            file_without_uid += line+"\n";
                        }
                    }
                }
                // Перезапись файла без uid
                using (StreamWriter writer = new StreamWriter(Path, false))
                {
                    writer.WriteLine(file_without_uid);
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0003: Error TChimeraMeshAssignment_History:ClearFromFile(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить файл
        /// </summary>
        public void ClearFile()
        {
            try
            {
                if (File.Exists(Path)) File.Delete(Path);
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0004: Error TChimeraMeshAssignment_History:ClearFile(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
    }
}
