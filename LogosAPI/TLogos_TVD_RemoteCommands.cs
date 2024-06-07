// Методы, запускающие выполнение различных комманд на кластере
using System;
using System.Collections.Generic;
//
using AstraEngine.Components;
//********************************************************************
namespace Logos_TVD
{
    public partial class TLogos_TVD
    {
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Создать папку на кластере
        /// </summary>
        /// <param name="Path">Путь к директории в которой будет создана папка. Со слешом на конце</param>
        /// <param name="DirectoryName">Название директории которю надо создать. Без слеша в начале и конце</param>
        /// <returns></returns>
        public bool CreateRemoteDirectory(string Path, string DirectoryName)
        {
            var exiteCode = SshConnection.RunCommand("cd " + Path + "; mkdir " + DirectoryName).ExitStatus;

            TJournalLog.WriteLog("Folder \"" + DirectoryName + "\" created in folder \"" + Path + "\" with exit code: " + exiteCode);
            return true;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить массив всех файлов в папке на кластере (без папок)
        /// </summary>
        /// <param name="PathToDirectory">Путь до папки на кластере. Со слешом на конце</param>
        /// <returns>массив названий всех файлов в папке</returns>
        public string[] GetFileNamesFromDirectory(string PathToDirectory)
        {
            try 
            {
                List<string> FileNames = new List<string>();
                // Получаем названия всех файлов в папке с расчетом
                string[] Names = SshConnection.RunCommand("cd " + PathToDirectory + "; ls").Result.ToString().Trim('\n').Split('\n');
                // Проверяем, какие из полученных названий принадлежат файлам, а какие - папкам
                foreach (string Name in Names)
                {
                    var OpenFolderCommand = SshConnection.RunCommand("cd " + PathToDirectory + Name);
                    //Если файл - папка, то повторяем предыдущий пункт
                    if (OpenFolderCommand.ExitStatus == 0) continue;
                    // Добавляем название файла в лист
                    FileNames.Add(Name);
                }
                return FileNames.ToArray();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0002: Error TLogosTVD:GetFileNamesFromDirectory(): " + E.Message);
                return null;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
