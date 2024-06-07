// Настройки расчета, флаги, пути к полученным результатам, события
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
//
using AstraEngine;
using AstraEngine.Scene;
//***********************************************************
namespace Logos_TVD
{
    /// <summary>
    /// Делегат для событий класса TLogos_TVD
    /// </summary>
    public delegate void AActionLogos(TLogos_TVD Logos_TVD);

    public partial class TLogos_TVD
    {
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

            // События и флаги \\

        /// <summary>
        /// Лист событий, запускающихся после окончания расчета
        /// </summary>
        protected List<AActionLogos> EventsOnEndCompute = new List<AActionLogos>();
        /// <summary>
        /// Событие, запускающееся после окончания расчета
        /// </summary>
        public event AActionLogos OnEnd_Compute
        {
            add
            {
                EventsOnEndCompute.Add(value);
            }
            remove
            {
                EventsOnEndCompute.Remove(value);
            }
        }
        /// <summary>
        /// Указывет, запущен ли расчет
        /// </summary>
        public bool IsCompute { get; set; } = false;
        /// <summary>
        /// Программа LogosTo загружена на кластер
        /// </summary>
        public bool LogosToIsUploaded = false;
        /// <summary>
        /// Требуется ди двигать химеры
        /// </summary>
        public bool ChimeraMovement = false;
        /// <summary>
        /// Требуется ли менять значения высоты и числа Маха
        /// </summary>
        public bool HeightAndMachVariation = false;
        /// <summary>
        /// Запрещено ли вызывать MessageBox-ы с вопросами о потребности перещения химер и варьирования высоты и числа Маха
        /// </summary>
        public bool MessageBoxCallLocked = false;
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

            // Идентефикаторы расчета \\

        /// <summary>
        /// Уникальный идентификатор расчета
        /// </summary>
        public Guid UID { get; set; } = Guid.NewGuid();
        /// <summary>
        /// Уникальный идентификатор расчета в slurm
        /// </summary>
        public string JobID = "";
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        
            // Пути, которые надо задать до начала каких-либо действий \\

        /// <summary>
        /// Начальный путь на кластере к рабочей папке проекта
        /// </summary>
        public string PathToCluster
        {
            get
            { 
                var path =  TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_Path_WorkFolder"];
                if (path[path.Length - 1] != '/')
                {
                    path += "/";
                }
                return path;
            }
        }
        /// <summary>
        /// Путь к локальной рабочей папке. Должен заканчиваться на слеш.
        /// </summary>
        public string LocalWorkFolder { get { return Settings.LocalWorkFolder; } }
        /// <summary>
        /// Путь к файлу mesh, который должен быть указан при открытии проекта
        /// </summary>
        public string PathToInitialMeshFile { get { return Settings.PathToMesh; } }
        /// <summary>
        /// Путь к файлу Yaml, который должен быть указан при открытии проекта
        /// </summary>
        public string PathToInitialYamlFile { get { return Settings.PathToYaml; } }
        /// <summary>
        /// Путь к файлу task.name, который должен быть указан при открытии проекта
        /// </summary>
        public string PathToInitialTaskNameFile { get { return Settings.PathToTaskName; } }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Пути получаемые из изначально задаваемых \\

        /// <summary>
        /// Путь к рабочей папке на кластере. Заканчивается на "/"
        /// </summary>
        public string RemoteWorkFolder
        {
            get
            {
                return PathToCluster + UID.ToString() + "/";
            }
        }
        /// <summary>
        /// Название задачи. Такое название используется в названии файлов расчета, а так же записано в файле task.name
        /// </summary>
        public string TaskName
        {
            get
            {
                return System.IO.File.ReadAllText(PathToInitialTaskNameFile).TrimEnd('\n');
            }
        }
        /// <summary>
        /// Путь к папке запущенного приложения с "\\" на конце
        /// </summary>
        public string ApplicationRootFolder { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        
            // Пути - результаты расчетов/конвертаций \\

        /// <summary>
        /// Пути к STL моделям на локальном компьютере, полученным по файлу Mesh
        /// </summary>
        public List<string> LocalPathsToSTL = new List<string>();
        /// <summary>
        /// Пути к STL моделям на кластере, полученным по файлу Mesh
        /// </summary>
        public List<string> RemotePathsToSTL = new List<string>();
        /// <summary>
        /// Путь к Geo файлу на локальном компьютере
        /// </summary>
        public string LocalPathToGeo = "";
        /// <summary>
        /// Пути до полей на локальном компьютере
        /// </summary>
        public List<List<string>> LocalPathsToFields = new List<List<string>>();
        public string NgeomFolder { get { return Settings.LocalWorkFolder + "Ngeom\\"; } }
        public string InitialNgeomFile { get { return NgeomFolder + TaskName+".1.ngeom"; } }
        public string InitialNgeomCelFile { get { return NgeomFolder + TaskName + ".1.cel"; } }
        public string NewNgeomFile = "";
        public string NewNgeomCelFile = "";
        public string STLFolder { get { return Settings.LocalWorkFolder + "STL\\"; } }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Счетчики \\

        /// <summary>
        /// Счетчик update, для пропуска лишних обновлений
        /// </summary>
        private int UpdateCounter = 0;
        /// <summary>
        /// Счетчик расчетов. Когда начианется новый расчет этот счетчик инкрементируется
        /// </summary>
        public int SolveCounter = 0;
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

            // Перемещение \\

        /// <summary>
        /// Таблица соответствия химер с объектами + родительская зависимость объектов
        /// </summary>
        //public TChimera_ObjectDependencyTable DependencyTable;
        /// <summary>
        /// Лист массивов матриц трансформации объектов. Каждый лист относится к отдельному расчету. 
        /// </summary>
        public List<Matrix[]> TransformationMatrices;
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

            // Глобальные настройки \\

        /// <summary>
        /// IP кластера
        /// </summary>
        public string IP
        {
            get { return TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_IP"]; }
        }
        /// <summary>
        /// Port кластера
        /// </summary>
        public int Port
        {
            get { return int.Parse(TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_Port"]); }
        }
        /// <summary>
        /// Имя организации для лицензии
        /// </summary>
        public string CUSTOMER_NAME
        {
            get { return TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_CUSTOMER_NAME"]; }
        }
        /// <summary>
        /// Адрес лицензии для ЛОГОС
        /// </summary>
        public string LICADDR
        {
            get { return TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_LICADDR"]; }
        }
        /// <summary>
        /// Дополнительные бибилотеки ЛОГОС
        /// </summary>
        public string Path_LogosVars
        {
            get { return TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_Path_LogosVars"]; }
        }
        /// <summary>
        /// Путь к решателю
        /// </summary>
        public string Path_Logos_TVD
        {
            get { return TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_Path_Logos_TVD"]; }
        }
        /// <summary>
        /// Путь до OMPI LIB
        /// </summary>
        public string Path_OMPI_LIB
        {
            get { return TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_Path_OMPI_LIB"]; }
        }
        /// <summary>
        /// Путь до OMPI BIN
        /// </summary>
        public string Path_OMPI_BIN
        {
            get { return TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_Path_OMPI_BIN"]; }
        }
        /// <summary>
        /// Путь к корневой папке TVD
        /// </summary>
        public string Path_TVDROOT
        {
            get { return TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_Path_TVDROOT"]; }
        }
        /// <summary>
        /// Путь к папке с бинарными файлами Logos
        /// </summary>
        public string Path_COMMONBIN
        {
            get { return TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_Path_COMMONBIN"]; }
        }
        ///// <summary>
        ///// Путь до exe самой программы
        ///// </summary>
        //public string Path_EXE = "";
        /// <summary>
        /// Названия расчетных узлов
        /// </summary>
        public List<string> PartitionNames
        {
            get { return TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_PartitionNames"].
                    Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Локальные настройки \\

        /// <summary>
        /// Логин пользователя
        /// </summary>
        public string Login
        {
            get { return TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_Login"]; }
        }
        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public string Password
        {
            get { return TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_Password"]; }
        }
        ///// <summary>
        ///// Кол-во нодов на кластере, на которых будет выполняться задача
        ///// </summary>
        //public int NodesCount = 2;
        ///// <summary>
        ///// Кол-во ядер в ноде
        ///// </summary>
        //public int NTasks_Per_Node = 36;
        ///// <summary>
        ///// Время, через которое расчет закончится принудительно, в часах
        ///// </summary>
        //public TimeSpan TimeMaxSolve = TimeSpan.FromHours(240);
        ///// <summary>
        ///// Имя расчетных узлов
        ///// </summary>
        //public string NameNodes = "nodes";
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
