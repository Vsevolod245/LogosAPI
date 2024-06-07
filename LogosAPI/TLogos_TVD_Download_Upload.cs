// Методы для загрузки файлов на и с кластера
using System;
using System.IO;
//
using AstraEngine.Components;
using Renci.SshNet;
//*************************************************************
namespace Logos_TVD
{
    public partial class TLogos_TVD
    {
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Отправка файлов с локального компьютера на кластер
        /// </summary>
        /// <param name="FilesPaths">Пути к файлам для отправки. Пути к файлам НЕ должны заканчиваться на слеш</param>
        /// <param name="DestinationDirectory">Путь к папке на кластере. Всегда должен идти со слешом на конце</param>
        /// <returns>true - успех, false - провал</returns>
        public bool UploadFiles(string[] FilesPaths, string DestinationDirectory)
        {
            try
            {
                var Scp = new ScpClient(SshConnection.ConnectionInfo);
                Scp.Connect();
                if (Scp.IsConnected)
                {
                    TJournalLog.WriteLog("Scp Connection Established");
                    // Загрузка файлов из массива файлов на кластер
                    foreach (var file in FilesPaths)
                    {
                        StreamReader sr = new StreamReader(file);
                        Scp.Upload(sr.BaseStream, DestinationDirectory + Path.GetFileName(file));
                        TJournalLog.WriteLog(file + " uploaded to " + DestinationDirectory);
                        sr.Close();
                    }
                    Scp.Disconnect();
                    TJournalLog.WriteLog("Scp Disconnected");
                }
                else
                {
                    TJournalLog.WriteLog("Scp Connection Error");
                    return false;
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0011: Error TLogosTVD:UploadFiles(): " + E.Message);
            }
            return true;
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Отправить файлы в папку расчета
        /// </summary>
        /// <param name="FilesPaths">Пути к файлам для отправки. Пути к файлам НЕ должны заканчиваться на слеш</param>
        /// <returns>true - успех, false - провал</returns>
        public bool UploadFilesToWorkfolder(string[] FilesPaths)
        {
            // Путь до рабочей директории
            var DestinatioDirectory = PathToCluster + UID.ToString() + "/";
            return UploadFiles(FilesPaths, DestinatioDirectory);
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Скачка файлов с кластера на локальный компьютер
        /// </summary>
        /// <param name="DestinationDirectory">Путь к папке на локальном компьютере. Всегда должен идти со слешом на конце</param>
        /// <param name="FilesPaths">Пути к файлам для скачки. Пути к файлам НЕ должны заканчиваться на слеш</param>
        /// <returns>true - успех, false - провал</returns>
        public bool DownloadFiles(string DestinationDirectory, string[] FilesPaths)
        {
            try
            {
                if (!(System.IO.Directory.Exists(DestinationDirectory)))
                {
                    Directory.CreateDirectory(DestinationDirectory);
                }
                var Scp = new ScpClient(SshConnection.ConnectionInfo);
                Scp.Connect();
                if (Scp.IsConnected)
                {
                    TJournalLog.WriteLog("Scp Connection Established");
                    // Загрузка файлов из массива файлов на локальный компьютер
                    foreach (var file in FilesPaths)
                    {
                        StreamWriter sw = new StreamWriter(DestinationDirectory + Path.GetFileName(file), false);
                        Scp.Download(file, sw.BaseStream);
                        TJournalLog.WriteLog(file + " downloaded to " + DestinationDirectory);
                        sw.Close();
                    }
                    Scp.Disconnect();
                    TJournalLog.WriteLog("Scp Disconnected");
                }
                else
                {
                    TJournalLog.WriteLog("Scp Connection Error");
                    return false;
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0011: Error TLogosTVD:DownloadFiles(): " + E.Message);
            }
            return true;
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Скачать файлы из рабочей папки в указаную папку на локальном компьютере
        /// </summary>
        /// <param name="DestinationDirectory">Путь к папке на локальном компьютере. Всегда должен идти со слешом на конце</param>
        /// <param name="FilesNames">Название файлов для скачки. Названия файлов НЕ должны начинаться и заканчиваться на слеш</param>
        /// <returns>true - успешно. false - неуспешно</returns>
        public bool DownloadFilesFromWorkFolder(string DestinationDirectory, string[] FilesNames)
        {
            string[] FilesPaths = new string[FilesNames.Length];
            for (int i = 0; i < FilesNames.Length; i++)
            {
                FilesPaths[i] = PathToCluster + UID.ToString() + "/" + FilesNames[i];
            }
            return DownloadFiles(DestinationDirectory, FilesPaths);
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Загрузить LogosTo в рабочую папку на кластере
        /// </summary>
        /// <returns>true - успешно. false - неуспешно</returns>
        public bool UploadLogosToToWorkFolder()
        {
            // Проверка не был ли уже загружен LogosTo
            if (LogosToIsUploaded)
            {
                TJournalLog.WriteLog("LogosTo is already uploaded");
                return true;
            }
            // Грузим LogosTo в папку с расчетом
            var Scp = new ScpClient(SshConnection.ConnectionInfo);
            Scp.Connect();
            if (Scp.IsConnected)
            {
                // Локальный путь до файла LogosTo
                var LocalPathToLogosTo = ApplicationRootFolder + "LogosTo";
                // Удаленный путь до файла LogosTo
                var RemotePathToLogosTo = PathToCluster + UID.ToString() + "/LogosTo";
                // Открываем поток до файла LogosTo
                StreamReader sr = new StreamReader(LocalPathToLogosTo);
                // Загружаем файл
                Scp.Upload(sr.BaseStream, RemotePathToLogosTo);
                // Закрываем поток чтения
                sr.Close();
                // Закрываем SCP
                Scp.Disconnect();
                LogosToIsUploaded = true;
                TJournalLog.WriteLog("LogosTo is Uploaded");
                // Прописываем права файла LogosTo
                SshConnection.RunCommand("cd " + RemoteWorkFolder + "; chmod 777 LogosTo");
                return true;
            }
            TJournalLog.WriteLog("Unable to upload LogosTo");
            return false;
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Загрузить STL файлы с кластера на локальный компьютер в "локальную робочую папку". (LocalWorkFolder)
        /// </summary>
        /// <returns>true - успешно. false - неуспешно</returns>
        public bool DownloadSTLFiles()
        {
            var result = DownloadFiles(STLFolder, RemotePathsToSTL.ToArray());
            for (int i = 0; i < RemotePathsToSTL.Count; i++)
            {
                LocalPathsToSTL.Add(STLFolder + System.IO.Path.GetFileName(RemotePathsToSTL[i]));
            }
            return result;
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Загрузить Ngeom файлы с кластера на локальный компьютер в "локальную робочую папку". (LocalWorkFolder)
        /// </summary>
        /// <returns>true - успешно. false - неуспешно</returns>
        public bool DownloadNgeomFiles()
        {
            var result = DownloadFilesFromWorkFolder(NgeomFolder, new string[]
            {
                TaskName+".1.ngeom",
                TaskName+".1.cel"
            });
            
            return result;
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
