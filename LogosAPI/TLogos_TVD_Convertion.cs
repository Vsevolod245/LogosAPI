// Методы дял конвертации файлов на кластере
using System;
//
using AstraEngine.Components;
//**************************************************
namespace Logos_TVD
{
    public partial class TLogos_TVD
    {
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Сконвертировать Mesh в Ngeom
        /// </summary>
        /// <returns>true - успешно. false - неуспешно</returns>
        public bool ConvertMeshToNgeom()
        {
            // Загрузка LogosTo в рабочую папку кластера
            if (!UploadLogosToToWorkFolder()) return false;
            // Конвертация
            TJournalLog.WriteLog("Ngeom creation has begun");
            SshConnection.RunCommand("cd " + RemoteWorkFolder + "; ./LogosTo ngeom");
            return true;
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Сконвертировать Ngeom в Mesh
        /// </summary>
        /// <returns>true - успешно. false - неуспешно</returns>
        public bool ConvertNgeomToMesh()
        {
            // Запись скрипта в директории программы для отправки
            System.IO.File.WriteAllText(ApplicationRootFolder + "Logos_TVD", new TNgeomToMeshBatch().ToStringBatch_NgeomToMesh());
            // Отправка
            UploadFilesToWorkfolder(new string[]
            {
                ApplicationRootFolder + "Logos_TVD"
            });
            // Прописываем права файла Logos_TVD
            SshConnection.RunCommand("cd " + RemoteWorkFolder + "; chmod 777 Logos_TVD");
            TJournalLog.WriteLog("Converting Ngeom to mesh");
            SshConnection.RunCommand("cd " + RemoteWorkFolder + "; ./Logos_TVD -n2m");

            TJournalLog.WriteLog("Ngeom converted to mesh");
            return true;
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Сконвертировать Mesh и результаты расчета в EnsightGold
        /// </summary>
        /// <returns>true - успешно. false - неуспешно</returns>
        public bool ConvertMeshAndResultsToEnsightGold()
        {
            // Загрузка LogosTo в рабочую папку кластера
            if (!UploadLogosToToWorkFolder()) return false;
            // Конвертация
            TJournalLog.WriteLog("EnsightGold creation has begun");
            SshConnection.RunCommand("cd " + RemoteWorkFolder + "; ./LogosTo egt");
            return true;
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Сконвертировать Mesh в STL. Удаленные пути к STL файлам будут записаны в список
        /// </summary>
        /// <returns>true - успешно. false - неуспешно</returns>
        public bool ConvertMeshToSTL()
        {
            // Загрузка LogosTo в рабочую папку кластера
            if (!UploadLogosToToWorkFolder()) return false;
            // Конвертация в STL записанное в кодировке ASCII
            TJournalLog.WriteLog("STL creation has begun");
            var ResultOfCommand = SshConnection.RunCommand("cd " + RemoteWorkFolder + "; ./LogosTo STL -STL_txt").Result;
            var Lines = ResultOfCommand.Split('\n');
            // Получение названий файлов и запись их путей на кластере в поле
            int counter = 1;
            for (int i = 0; i < Lines.Length; i++)
            {
                // Поиск названий файлов STL
                if (Lines[i].Contains("  " + counter.ToString() + " "))
                {
                    // Отбрасываем пробелы в конце строки
                    var TrimmedLine = Lines[i].TrimEnd();
                    // Строка в которую будет записано название файла STL
                    var Name = "";
                    // Записываем название файла STL задом на перед
                    for (int j = TrimmedLine.Length - 1; j >= 0; j--)
                    {
                        if (TrimmedLine[j] == ' ') break;
                        Name += TrimmedLine[j];
                    }
                    // Инвертируем последовательность знаков в строке, чтобы она была правильной
                    var CA = Name.ToCharArray();
                    Array.Reverse(CA);
                    Name = new string(CA);
                    // Записываем путь к STL файлу на кластере
                    RemotePathsToSTL.Add(RemoteWorkFolder + TaskName + "." + Name + ".txt.stl");
                    counter++;
                }
            }
            return true;
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
