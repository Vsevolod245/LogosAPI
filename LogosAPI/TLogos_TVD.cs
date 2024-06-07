// Программный интерфейс для взаимодействия с внешним модулем ЛОГОС аэродинамика
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//
using AstraEngine.Components;
//
using Renci.SshNet;
//**********************************************************
namespace Logos_TVD
{
    /// <summary>
    /// Программный интерфейс для взаимодействия с внешним модулем ЛОГОС аэродинамика
    /// </summary>
    public partial class TLogos_TVD
    {
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Ssh клиент для доступа к удаленному кластеру
        /// </summary>
        public SshClient SshConnection { get; set; }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Объект класса, который предназначен для выполнения обновления объекта текущего класса в отдельном потоке
        /// </summary>
        public TAPI_Updater API_Updater { get; set; }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Ссылка на Task, который выполняет обновление объекта текущего класса в отдельном потоке
        /// </summary>
        public Task API_Updater_Task;
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Объект настроек
        /// </summary>
        public TSettings Settings;
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public int IndexItteration;
        /// <summary>
        /// Инициализировать доступ к суперкомпьтеру/удаленому вычислительному кластеру
        /// </summary>
        /// <param name="IP">IP Адрес для подключения</param>
        /// <param name="Port">Порт для подключения</param> 
        /// <param name="Login">Логин</param>
        /// <param name="Password">Пароль</param>
        /// <param name="Error">Ошибка в процессе подключения</param>
        /// <returns>Успешно/нет</returns>
        public bool Intialize(out string Error)
        {
            string IP = this.IP;
            int Port = this.Port;
            string Login = this.Login;
            string Password = this.Password;

            TJournalLog.EnableFiltrationRepeat = false;
            Error = "";
            //
            try
            {
                // Установить соединение
                SshConnection = new SshClient(IP, Port, Login, Password);

                SshConnection.Connect();

                return SshConnection.IsConnected;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0001: Error TLogos_TVD:Intialize(): " + E.Message);
                Error = E.Message + ": " + E.InnerException;
                return false;
            }
        }
        //------------------------------------------------------------
        /// <summary>
        /// Проверить соединение между локальной машиной и суперкомпьтером/удаленым вычислительным кластером 
        /// </summary>
        /// <returns>Есть/нет</returns>
        public bool IsConnected()
        {
            try
            {
                if (SshConnection == null) return false;
                return SshConnection.IsConnected;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0002: Error TLogos_TVD:IsConnected(): " + E.Message);
                return false;
            }
        }
        //------------------------------------------------------------
        /// <summary>
        /// Открыть в решателе проект (загрузить на суперкомпьтер и подготовить исходные данные)
        /// </summary>
        public void OpenProject()
        {
            try
            {
                // Создаем рабочую папку
                CreateRemoteDirectory(PathToCluster, UID.ToString());
                // Записываем в историю расчетов
                var History = new TSolvingHistory();
                History.SaveIds(UID.ToString(), JobID);
                //
                TJournalLog.WriteLog("Uploading project files...");
                UploadFilesToWorkfolder(new string[3]
                {
                    PathToInitialMeshFile,
                    PathToInitialYamlFile,
                    PathToInitialTaskNameFile
                });
                TJournalLog.WriteLog("Project files upload is over");

                //// Если счетчик расчетов равен 0 (еще ни один расчет не был запущен), то заполняем настройки в зависимости от выборов пользователей
                //if(SolveCounter == 0)
                //{
                //    // Задание вопроса о необходимости перемещения химер в данном расчете
                //    AskAboutChimeraMovement();
                //    // Задание вопроса о необходимости варьировать высоту и число Маха
                //    AskAboutHeightAndMachVariation();
                //}

                this.API_Updater_Task = Task.Run(() => API_Updater = new TAPI_Updater(this));
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0003: Error TLogosTVD:OpenProject(): " + E.Message);
            }
        }
        //------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        public Task StartUpdateLoop()
        {
            this.API_Updater = new TAPI_Updater(this);
            this.API_Updater_Task = Task.Run(() => {
                Thread.CurrentThread.Name = "LogosAPI_Updater";
                this.API_Updater.Start();
                });
            return this.API_Updater_Task;
        }
        //------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        public void StopUpdateLoop()
        {
            API_Updater.Stop();
        }
        //------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        public void DisposeUpdateLoop()
        {
            API_Updater.Dispose();
        }
        //------------------------------------------------------------
        /// <summary>
        /// Открыть в решателе проект (загрузить на суперкомпьтер и подготовить исходные данные)
        /// </summary>
        /// <param name="FileName_Mesh">Файл дискретной модели</param>
        /// <param name="FileName_YAML">Файл конфигурации расчета</param>
        /// <param name="FileName_Task">Файл описание задачи (имя файла YAML)</param>
        public List<string> OpenProjectChim(string FileName_Mesh, string FileName_YAML, string FileName_Task, string LocalPath)
        {
            try
            {
                List<string> PathsToNgeom = new List<string>();
                var TaskName = Path.GetFileNameWithoutExtension(FileName_Mesh);
                // Создаем рабочую папку
                TJournalLog.WriteLog("Folder " + UID + " created with exit code: " + SshConnection.RunCommand("cd " + PathToCluster + "; mkdir " + UID.ToString()).ExitStatus);

                var History = new TSolvingHistory();
                History.SaveIds(UID.ToString(), JobID);

                string FullPathToClacter = PathToCluster + UID.ToString() + "/";
                // Инициализация Scp
                var Scp = new ScpClient(SshConnection.ConnectionInfo);

                // Подключение Scp
                Scp.Connect();
                // Проверка соедининия Scp
                if (Scp.IsConnected)
                {
                    TJournalLog.WriteLog("Scp connection established");
                    TJournalLog.WriteLog("Uploading project files...");
                    StreamReader sr = new StreamReader(FileName_Mesh);
                    Scp.Upload(sr.BaseStream, FullPathToClacter + System.IO.Path.GetFileName(FileName_Mesh));
                    sr.Close();

                    sr = new StreamReader(FileName_YAML);
                    Scp.Upload(sr.BaseStream, FullPathToClacter + System.IO.Path.GetFileName(FileName_YAML));
                    sr.Close();

                    sr = new StreamReader(FileName_Task);
                    Scp.Upload(sr.BaseStream, FullPathToClacter + System.IO.Path.GetFileName(FileName_Task));
                    sr.Close();

                    //==================================================================
                    sr = new StreamReader(Directory.GetCurrentDirectory() + "/LogosTo");
                    //==================================================================
                    string Path = PathToCluster + UID.ToString();
                    Scp.Upload(sr.BaseStream, Path + "/LogosTo");
                    sr.Close();
                    TJournalLog.WriteLog("The LogosTo in the house");
                    //Расширяем права у файла LogosTo
                    SshConnection.RunCommand("cd " + Path + "; chmod 777 LogosTo");
                    TJournalLog.WriteLog("Upload is over");

                    // Запускаем процесс создания Ngeom
                    TJournalLog.WriteLog("Ngeom creation has begun");
                    SshConnection.RunCommand("cd " + Path + "; ./LogosTo ngeom");
                    // Скачиваем файлы нужных расширений, когда те появятся в папках
                    TJournalLog.WriteLog("Started looking for NGEOM files");
                    string[] NgeomExtensions =
                    {
                        ".1.cel", ".1.ngeom"
                    };
                    // Скачиваем Файлы Ngeom
                    foreach (string extention in NgeomExtensions)
                    {
                        while (true)
                        {
                            // Получаем названия всех файлов в папке с результатами создания EnSightGoldData
                            string AllFiles = SshConnection.RunCommand("cd " + Path + "; ls").Result.ToString();
                            if (!AllFiles.Contains(TaskName + extention))
                            {
                                TJournalLog.WriteLog("There is no needed files");
                                Thread.Sleep(3000);
                                continue;
                            }
                            // Загружаем файл на локальный компьютер
                            TJournalLog.WriteLog($"Downloading {TaskName}{extention}");
                            FileInfo LocalFile = new FileInfo(LocalPath + TaskName + extention);
                            Scp.Download(Path + "/" + TaskName + extention, LocalFile);
                            PathsToNgeom.Add(LocalFile.FullName);
                            break;
                        }
                    }

                    Scp.Disconnect();
                    TJournalLog.WriteLog("Scp Disconnected");
                    return PathsToNgeom;
                }
                else
                {
                    TJournalLog.WriteLog("Scp connection Error");
                    throw new Exception("Unable to start solving. Scp is not connected");
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0003: Error TLogosTVD:OpenProjectChim(): " + E.Message);
                return new List<string>();
            }
        }
        //------------------------------------------------------------
        /// <summary>
        /// Открыть в решателе проект (загрузить на суперкомпьтер и подготовить исходные данные)
        /// </summary>
        /// <param name="FileName_Mesh">Файл дискретной модели</param>
        /// <param name="FileName_YAML">Файл конфигурации расчета</param>
        /// <param name="FileName_Task">Файл описание задачи (имя файла YAML)</param>
        public void OpenProjectChimNext(string FileName_ngeom, string FileName_cel, string FileName_YAML, string FileName_Task)
        {
            try
            {
                // Создаем рабочую папку
                TJournalLog.WriteLog("Folder " + UID + " created with exit code: " + SshConnection.RunCommand("cd " + PathToCluster + "; mkdir " + UID.ToString()).ExitStatus);

                var History = new TSolvingHistory();
                History.SaveIds(UID.ToString(), JobID);

                string FullPathToClacter = PathToCluster + UID.ToString() + "/";
                // Инициализация Scp
                var Scp = new ScpClient(SshConnection.ConnectionInfo);

                // Подключение Scp
                Scp.Connect();
                // Проверка соедининия Scp
                if (Scp.IsConnected)
                {
                    TJournalLog.WriteLog("Scp connection established");
                    TJournalLog.WriteLog("Uploading project files...");
                    StreamReader sr = new StreamReader(FileName_ngeom);
                    Scp.Upload(sr.BaseStream, FullPathToClacter + Path.GetFileName(FileName_ngeom));
                    sr.Close();

                    sr = new StreamReader(FileName_cel);
                    Scp.Upload(sr.BaseStream, FullPathToClacter + Path.GetFileName(FileName_cel));
                    sr.Close();

                    sr = new StreamReader(FileName_YAML);
                    Scp.Upload(sr.BaseStream, FullPathToClacter + Path.GetFileName(FileName_YAML));
                    sr.Close();

                    sr = new StreamReader(FileName_Task);
                    Scp.Upload(sr.BaseStream, FullPathToClacter + Path.GetFileName(FileName_Task));
                    sr.Close();

                    TNgeomToMeshBatch NgeomToMeshBatch = new TNgeomToMeshBatch();
                    // Запись скрипта в директории программы для отправки
                    File.WriteAllText(Directory.GetCurrentDirectory() + "\\Logos_TVD", NgeomToMeshBatch.ToStringBatch_NgeomToMesh());
                    // Открытие потока скрипта для отправки
                    sr = new StreamReader(Directory.GetCurrentDirectory() + "\\Logos_TVD");
                    TJournalLog.WriteLog("Uploading batch...");
                    // Отправка
                    Scp.Upload(sr.BaseStream, PathToCluster + UID.ToString() + "/" + "Logos_TVD");
                    sr.Close();
                    TJournalLog.WriteLog("Upload is over");

                    TJournalLog.WriteLog("Converting Ngeom to mesh");
                    var TheRealCommand = SshConnection.RunCommand("cd " + PathToCluster + UID.ToString() + "/" + "; ./Logos_TVD -n2m");

                    // Требуется добавить проверку окончания конвертации

                    Scp.Disconnect();
                    TJournalLog.WriteLog("Scp Disconnected");
                }
                else
                {
                    TJournalLog.WriteLog("Scp connection Error");
                    throw new Exception("Unable to start solving. Scp is not connected");
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0003: Error TLogosTVD:OpenProjectChimNext(): " + E.Message);
            }
        }
        //------------------------------------------------------------
        /// <summary>
        /// Получить доступ при разрыве соединения к проекту
        /// </summary>
        /// <param name="UID">Уникальный идентификатор проекта</param>
        public void ConnectProject(string UID)
        {
            try
            {
                this.UID = new Guid(UID);
                // Определяем идентификатор расчета в slurm
                string JobID = GetJobID(UID);
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0004: Error TLogosTVD:ConnectProject(): " + E.Message);
            }
        }
        //------------------------------------------------------------
        /// <summary>
        /// Получить идентификатор расчета в slurm
        /// </summary>
        /// <param name="UID">Уникальный идентификатор проекта</param>
        public string GetJobID(string UID)
        {
            try
            {
                var History = new TSolvingHistory();
                return History.GetJobID_ByUID(UID);
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0005: Error TLogosTVD:GetJobID(): " + E.Message);
                return null;
            }
        }
        //------------------------------------------------------------
        /// <summary>
        /// Выполнить расчет FileName_YAML
        /// </summary>
        /// <param name="LogosTVDBatch">Правилла запуска задачи</param>
        public void Solve()
        {
            try
            {
                // Удалить старый ямл файл с кластера и загрузить новый с моментами
                if (IndexItteration==0) DeleteOldYAML_AndDownoloadNew();
                
                //
                TLogosTVDBatch LogosTVDBatch = new TLogosTVDBatch(Settings);
                TJournalLog.WriteLog("Solve Called");
                // Инициализация Scp
                var Scp = new ScpClient(SshConnection.ConnectionInfo);
                // Подключение Scp
                Scp.Connect();
                // Проверка соедининия Scp
                if (Scp.IsConnected)
                    TJournalLog.WriteLog("Scp Connection Established");
                else
                {
                    TJournalLog.WriteLog("Scp Connection Error");
                    throw new Exception("Unable to start solving. Scp is not connected");
                }
                // Запись скрипта в директории программы для отправки
                File.WriteAllText(ApplicationRootFolder + "tvd_nodes.batch", LogosTVDBatch.ToStringBatch_Slurm());
                // Открытие потока скрипта для отправки
                StreamReader sr = new StreamReader(ApplicationRootFolder + "tvd_nodes.batch");
                TJournalLog.WriteLog("Uploading batch...");
                // Отправка
                Scp.Upload(sr.BaseStream, RemoteWorkFolder + "tvd_nodes.batch");
                sr.Close();
                TJournalLog.WriteLog("Upload is over");
                Scp.Disconnect();
                TJournalLog.WriteLog("Scp Disconnected");
                // Отправка комманды о запуске батч файла
                using (var TheRealCommand = SshConnection.RunCommand("cd " + PathToCluster + UID.ToString() + "/" + "; sbatch tvd_nodes.batch"))
                {
                    StringBuilder jobid = new StringBuilder();
                    for (int i = 0; i < TheRealCommand.Result.Length; i++)
                        if (char.IsNumber(TheRealCommand.Result[i])) jobid.Append(TheRealCommand.Result[i]);
                    JobID = jobid.ToString();
                }
                IsCompute = true;
                var History = new TSolvingHistory();
                History.SaveIds(UID.ToString(), JobID);
                TJournalLog.WriteLog("Solving id " + JobID + "...");
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0006: Error TLogosTVD:Solve(): " + E.Message);
            }
        }
        //------------------------------------------------------------
        /// <summary>
        /// Получить статус выполнения операции 0-100 (100 выполнено) для расчета и выгрузки полей
        /// </summary>
        /// <returns>0-100 (100 выполнено)</returns>
        public float GetStatus()
        {
            try
            {
                string Path = PathToCluster + UID.ToString();
                // Общее заданное количество итераций
                int CountIterations = 0;
                // Итерация, которая выполняется в данный момент
                int Step = -1;
                float Persent = 0;
                string YamlName = "";
                // Поиск файлов нужного расширения
                string[] FileNames = SshConnection.RunCommand("cd " + Path + "; ls").Result.ToString().Trim('\n').Split('\n');
                foreach (string FileName in FileNames)
                {
                    // Проверяем, есть ли sbox файл в папке. Если есть - останавливаем метод
                    if (FileName == "sbox") return 0;
                    string Extension = System.IO.Path.GetExtension(FileName);
                    // Поиск .yaml файла
                    if (Extension.Equals(".yaml"))
                        YamlName = FileName;
                }
                // Нахождение количества итераций в файле yaml
                string[] YamlStr = SshConnection.RunCommand($"cat {Path}/{YamlName}").Result.ToString().Trim('\n').Split('\n');
                foreach (string YamlLine in YamlStr)
                {
                    if (YamlLine.Contains("stop step:"))
                    {
                        string NewLine = YamlLine;
                        NewLine = NewLine.Trim();
                        int Num = NewLine.IndexOf(':') + 2;
                        NewLine = NewLine.Remove(0, Num);
                        CountIterations = int.Parse(NewLine);
                        break;
                    }
                }
                // Если число не найдено, то вывод ошибки
                if (CountIterations == 0)
                {
                    TJournalLog.WriteLog("Unnable to find the stop step line\n");
                    return 0;
                }
                // Если в папке уже есть lgs с последним шагом итерации - конец метода
                foreach (string FileName in FileNames)
                {
                    if (FileName.Contains($"0{CountIterations}.lgs"))
                    {
                        Persent = 100f;
                        TJournalLog.WriteLog(Persent.ToString() + "% done");
                        IsCompute = false;
                        return Persent;
                    }
                }
                // Создание в папке sbox файла
                SshConnection.RunCommand("touch " + Path + "/sbox");
                // Создается дата и время, в пределах которых можно ожидать создание lgs файла
                DateTime date = DateTime.Now.AddMinutes(1);
                // Поиск выполняемой итерации
                List<string> FilesLgs = new List<string>();
                // Название lgs файла с наибольшим номером итеррации
                string LastLgsName = "";
                while (Step == -1 && DateTime.Now < date)
                {
                    string[] FileNames1 = SshConnection.RunCommand("cd " + Path + "; ls").Result.ToString().Trim('\n').Split('\n');
                    foreach (string FileName in FileNames1)
                    {
                        // Находим все lgs и добавляем в список
                        string Extension = System.IO.Path.GetExtension(FileName);
                        if (Extension.Equals(".lgs"))
                            FilesLgs.Add(FileName);
                    }
                    // Сравниваем номер шага в каждом lgs и выбираем наибольший
                    foreach (string FileLgs in FilesLgs)
                    {
                        string NewString = System.IO.Path.GetFileNameWithoutExtension(FileLgs);
                        NewString = NewString.Remove(0, NewString.LastIndexOf('_') + 1).TrimStart('0');
                        if (Convert.ToInt32(NewString) > Step)
                        {
                            Step = Convert.ToInt32(NewString);
                            LastLgsName = FileLgs;
                        }
                    }
                }
                // Если выполняемая итерация не найдена, то вывод ошибки
                if (Step == -1)
                {
                    TJournalLog.WriteLog("Error with the search for the iteration being performed\n");
                    return 0;
                }
                else
                {
                    // Удаляем lgs файл из папки на суперкомпьютере
                    if (LastLgsName != "" && Step != CountIterations)
                    {
                        string LgsSize = "";
                        while (true)
                        {
                            Thread.Sleep(1000);
                            // Получаем размер файла на кластере в данный момент
                            string LgsSizeClaster = SshConnection.RunCommand("cd " + Path + "; ls -sh --block-size=K " + LastLgsName).Result.ToString();
                            // Сравниваем размер файла с записанным ранее. Если они одинаковые - удаляем файл
                            if (LgsSizeClaster == LgsSize) break;
                            LgsSize = LgsSizeClaster;
                        }
                        SshConnection.RunCommand("rm " + Path + "/" + LastLgsName);
                    }  
                }
                // Процент выполнения задачи
                Persent = (float)(Step) / (float)(CountIterations) * 100f;
                TJournalLog.WriteLog(Persent.ToString() + "% done");
                // Если шаг расчета - больше либо равен максимальному - расчет окончен
                if (Step > CountIterations - 1)
                {
                    IsCompute = false;
                }
                return Persent;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0007: Error TLogosTVD:GetStatus(): " + E.Message);
                return 0;
            }
        }
        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить статус выполнения операции 0-100 (100 выполнено) для расчета и выгрузки полей
        /// </summary>
        /// <returns>0-100 (100 выполнено)</returns>
        public float GetStatus(bool b)
        {
            try
            {
                // Путь до папки с расчетом
                string Path = PathToCluster + UID.ToString();
                // Общее заданное количество итераций
                int CountIterations = -1;
                // Итерация, которая выполняется в данный момент
                int Step = -1;
                float Persent = 0;
                bool OutputAmgExists = false;
                string YamlName = "";
                // Получаем название всех файлов в папке
                string[] FileNames = GetFileNamesFromDirectory(Path);
                // Поиск нужных файлов
                foreach (string FileName in FileNames)
                {
                    string Extension = System.IO.Path.GetExtension(FileName);
                    // Поиск .yaml файла
                    if (Extension.Equals(".yaml"))
                        YamlName = FileName;
                    // Проверка существования в папке файла output.amg
                    else if (FileName == "output.amg")
                        OutputAmgExists = true;
                }
                // Если файла output.amg нет в папке - вывод ошибки
                if (OutputAmgExists == false)
                {
                    TJournalLog.WriteLog($"Unnable to find output.amg in {Path}\n");
                    return Persent;
                }
                // Нахождение количества итераций в файле yaml
                string[] YamlStr = SshConnection.RunCommand($"cat {Path}/{YamlName}").Result.ToString().Trim('\n').Split('\n');
                foreach (string YamlLine in YamlStr)
                {
                    if (!YamlLine.Contains("stop step:")) continue;
                    string NewLine = YamlLine;
                    NewLine = NewLine.Trim();
                    int Num = NewLine.IndexOf(':') + 2;
                    NewLine = NewLine.Remove(0, Num);
                    CountIterations = int.Parse(NewLine);
                    break;
                }
                // Если число не найдено, то вывод ошибки
                if (CountIterations == -1)
                {
                    TJournalLog.WriteLog("Unnable to find the stop step line\n");
                    return Persent;
                }
                // Скачивание файла на локальный компьютер
                if (!DownloadFilesFromWorkFolder(LocalWorkFolder, new string[] { "output.amg" }))
                {
                    TJournalLog.WriteLog("output.amg wasn't downloaded\n");
                    return Persent;
                }
                // Нахождение максимальной итеррации в файле output.amg
                string[] OutputAmgStr = File.ReadAllLines(LocalWorkFolder + "output.amg");
                //string[] OutputAmgStr = SshConnection.RunCommand($"cat {Path}/output.amg").Result.ToString().Trim('\n').Split('\n');
                // Читаем массив с конца, чтобы всегда самым первым считывать максимальный Step 
                for (int i = OutputAmgStr.Length - 1; i >= 0; i--)
                {
                    if (!OutputAmgStr[i].StartsWith("Step:")) continue;
                    // Получаем номер итерации
                    int Num = OutputAmgStr[i].IndexOf(':') + 2;
                    OutputAmgStr[i] = OutputAmgStr[i].Remove(0, Num);
                    Step = Convert.ToInt32(OutputAmgStr[i]);
                    break;
                }
                // Проверяем номер итеррации
                if (Step == -1)
                {
                    TJournalLog.WriteLog("Unnable to find the Step line in output.amg\n");
                    return Persent;
                }
                // Считаем процент выполнения расчета
                Persent = (float)(Step) / (float)(CountIterations) * 100f;
                TJournalLog.WriteLog(Persent.ToString() + "% done");
                // Если шаг расчета больше либо равен максимальному - расчет окончен
                if (Step >= CountIterations)
                {
                    // Удаляем файл output.amg
                    if(File.Exists(LocalWorkFolder + "output.amg"))
                        File.Delete(LocalWorkFolder + "output.amg");
                    IsCompute = false;
                }
                return Persent;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0007: Error TLogosTVD:GetStatus(): " + E.Message);
                return 0;
            }
        }
        //--------------------------------------------------------------------------------------------------------
        public void Update()
        {
            if (IsCompute)
            {
                UpdateCounter++;
                if (UpdateCounter > 100)
                {
                    // Триггер события окончания расчета
                    if (GetStatus(true) >= 100f)
                    {
                        IsCompute = false;
                        API_Updater.IsUpdate = false;
                        foreach (var Event in EventsOnEndCompute)
                        {
                            Event(this);
                        }
                    }
                    UpdateCounter = 0;
                }
            }
        }
        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Сохранить полученные поля и сетку в указанное место
        /// </summary>
        /// <param name="LocalPath">Путь к папке для хранения файлов</param>
        public List<string> ExportResults(string LocalPath = "")
        {
            try
            {
                var PathsToResultFiles = new List<string>();
                // Перечень всех расширений для скачивания
                string[] Extensions =
                {
                ".P", ".U", ".V", ".W", ".UMAG"
                };
                if (LocalPath == "")
                {
                    LocalPath = Settings.LocalWorkFolder;
                    if (!Directory.Exists(LocalPath))
                    {
                        Directory.CreateDirectory(LocalPath);
                    }
                    //LocalPath += "/";
                }
                // Путь к расчету на кластере
                //==================================================================
                string Path = PathToCluster + UID.ToString();
                //==================================================================
                // Получаем названия всех файлов в папке с расчетом
                int LastStep = -1;
                string TaskNameWithNumber = "";
                // Ищем lgs файл до тех пор, пока не найдем
                while (LastStep == -1)
                {
                    string[] FileNames = SshConnection.RunCommand("cd " + Path + "; ls").Result.ToString().Trim('\n').Split('\n');
                    
                    foreach (string FileName in FileNames)
                    {
                        if (!System.IO.Path.GetExtension(FileName).Equals(".lgs")) continue;
                        // Если файл - lgs, получаем номер итерации из его названия
                        string NewString = System.IO.Path.GetFileNameWithoutExtension(FileName);
                        NewString = NewString.Remove(0, NewString.LastIndexOf('_') + 1).TrimStart('0');
                        if (Convert.ToInt32(NewString) > LastStep)
                        {
                            LastStep = Convert.ToInt32(NewString);
                            TaskNameWithNumber = System.IO.Path.GetFileNameWithoutExtension(FileName);
                        }
                    }
                }
                TJournalLog.WriteLog($"The last lgs is {TaskNameWithNumber}.lgs");
                // Проверяем, нашелся ли хоть один lgs (если нет - выводим сообщение об ошибке)
                if (LastStep == -1)
                {
                    TJournalLog.WriteLog("There is no .lgs file in the folder");
                    return null;
                }
                // Грузим LogosTo в папку с расчетом
                UploadLogosToToWorkFolder();
                // Запускаем процесс создания EnSightGoldData
                ConvertMeshAndResultsToEnsightGold();
                var Scp = new ScpClient(SshConnection.ConnectionInfo);
                Scp.Connect();
                if (!Scp.IsConnected)
                {
                    TJournalLog.WriteLog("Scp Connection Error");
                    return null;
                }
                TJournalLog.WriteLog("Scp Connection Established");
                // Скачиваем файлы нужных расширений, когда те появятся в папках
                string TaskName = TaskNameWithNumber.Replace(TaskNameWithNumber.Remove(0, TaskNameWithNumber.LastIndexOf('_')), "");
                TJournalLog.WriteLog("Started looking for the .geo");
                // Скачиваем .geo
                while (true)
                {
                    // Получаем названия всех файлов в папке с результатами расчета
                    string AllFiles = SshConnection.RunCommand("cd " + Path + "; ls").Result.ToString();
                    if (!AllFiles.Contains(TaskName + ".geo"))
                    {
                        TJournalLog.WriteLog("There is no .geo");
                        Thread.Sleep(3000);
                        continue;
                    }
                    // Загружаем файл на локальный компьютер
                    TJournalLog.WriteLog($"Downloading {TaskName}.geo");
                    FileInfo LocalFile = new FileInfo(LocalWorkFolder + TaskName + ".geo");
                    Scp.Download(Path + "/" + TaskName + ".geo", LocalFile);
                    PathsToResultFiles.Add(LocalFile.FullName);
                    break;
                }
                TJournalLog.WriteLog("Started looking for the other files");
                // Скачиваем остальные расширения
                foreach (string extention in Extensions)
                {
                    while (true)
                    {
                        // Получаем названия всех файлов в папке с результатами создания EnSightGoldData
                        string AllFiles = SshConnection.RunCommand("cd " + Path + "/EnSightGoldData; ls").Result.ToString();
                        if (!AllFiles.Contains(TaskNameWithNumber + extention))
                        {
                            TJournalLog.WriteLog("There is no needed files");
                            Thread.Sleep(3000);
                            continue;
                        }
                        // Загружаем файл на локальный компьютер
                        TJournalLog.WriteLog($"Downloading {TaskNameWithNumber}{extention}");
                        FileInfo LocalFile = new FileInfo(LocalWorkFolder + TaskNameWithNumber + extention);
                        Scp.Download(Path + "/EnSightGoldData/" + TaskNameWithNumber + extention, LocalFile);
                        PathsToResultFiles.Add(LocalFile.FullName);
                        break;
                    }
                }
                Scp.Disconnect();
                return PathsToResultFiles;
            }
            catch(Exception E)
            {
                TJournalLog.WriteLog("C0008: Error TLogosTVD:ExportResults(): " + E.Message);
                return null;
            }
        }
        //------------------------------------------------------------
        /// <summary>
        /// Сохранить полученные поля и сетку в указанное место 
        /// </summary>
        /// <param name="LocalPath">Путь к папке для хранения файлов</param>
        /// <returns></returns>
        public List<string> ExportResultsChim(string LocalPath)
        {
            try
            {
                var PathsToResultFiles = new List<string>();
                // Перечень всех расширений Ngeom для скачивания
                string[] NgeomExtensions =
                {
                ".1.cel", ".1.ngeom"
                };
                // Перечень всех расширений EnsightGold для скачивания
                string[] EnsightExtensions =
                {
                ".P", ".U", ".V", ".W", ".UMAG"
                };
                // Если не был указан путь на локальном компьютере, то он определяется здесь
                if (LocalPath == "")
                {
                    LocalPath = Directory.GetCurrentDirectory() + "/" + UID.ToString();
                    if (!Directory.Exists(LocalPath))
                    {
                        Directory.CreateDirectory(LocalPath);
                    }
                    LocalPath += "/";
                }
                // Путь к расчету на кластере
                //==================================================================
                string Path = PathToCluster + UID.ToString();
                //==================================================================
                // Получаем названия всех файлов в папке с расчетом
                string[] FileNames = SshConnection.RunCommand("cd " + Path + "; ls").Result.ToString().Trim('\n').Split('\n');
                
                string TaskNameWithNumber = "";
                // Последний шаг расчета
                int LastStep = -1;
                // Получение последнего шага расчета
                foreach (string FileName in FileNames)
                {
                    if (!System.IO.Path.GetExtension(FileName).Equals(".lgs")) continue;
                    // Если файл - lgs, получаем номер итерации из его названия
                    string NewString = System.IO.Path.GetFileNameWithoutExtension(FileName);
                    NewString = NewString.Remove(0, NewString.LastIndexOf('_') + 1).TrimStart('0');
                    if (Convert.ToInt32(NewString) > LastStep)
                    {
                        LastStep = Convert.ToInt32(NewString);
                        TaskNameWithNumber = System.IO.Path.GetFileNameWithoutExtension(FileName);
                    }
                }
                TJournalLog.WriteLog($"The last lgs is {TaskNameWithNumber}.lgs");
                // Проверяем, нашелся ли хоть один lgs (если нет - выводим сообщение об ошибке)
                if (LastStep == -1)
                {
                    TJournalLog.WriteLog("There is no .lgs file in the folder");
                    return null;
                }
                // Грузим LogosTo в папку с расчетом
                var Scp = new ScpClient(SshConnection.ConnectionInfo);
                Scp.Connect();
                if (!Scp.IsConnected)
                {
                    TJournalLog.WriteLog("Scp Connection Error");
                    return null;
                }
                TJournalLog.WriteLog("Scp Connection Established");
                //==================================================================
                StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "/LogosTo");
                //==================================================================
                Scp.Upload(sr.BaseStream, Path + "/LogosTo");
                sr.Close();
                TJournalLog.WriteLog("The LogosTo in the house");
                //Расширяем права у файла LogosTo
                SshConnection.RunCommand("cd " + Path + "; chmod 777 LogosTo");
                // Запускаем процесс создания Ngeom
                TJournalLog.WriteLog("Ngeom creation has begun");
                SshConnection.RunCommand("cd " + Path + "; ./LogosTo ngeom");
                // Скачиваем файлы нужных расширений, когда те появятся в папках
                string TaskName = TaskNameWithNumber.Replace(TaskNameWithNumber.Remove(0, TaskNameWithNumber.LastIndexOf('_')), "");
                TJournalLog.WriteLog("Started looking for NGEOM files");
                // Скачиваем Файлы Ngeom
                foreach (string extention in NgeomExtensions)
                {
                    while (true)
                    {
                        // Получаем названия всех файлов в папке с результатами создания EnSightGoldData
                        string AllFiles = SshConnection.RunCommand("cd " + Path + "; ls").Result.ToString();
                        if (!AllFiles.Contains(TaskName + extention))
                        {
                            TJournalLog.WriteLog("There is no ngeom");
                            Thread.Sleep(3000);
                            continue;
                        }
                        // Загружаем файл на локальный компьютер
                        TJournalLog.WriteLog($"Downloading {TaskName}{extention}");
                        FileInfo LocalFile = new FileInfo(LocalPath + TaskName + extention);
                        Scp.Download(Path + "/" + TaskName + extention, LocalFile);
                        PathsToResultFiles.Add(LocalFile.FullName);
                        break;
                    }
                }
                // Запускаем процесс создания EnSightGoldData
                SshConnection.RunCommand("cd " + Path + "; ./LogosTo egt");
                TJournalLog.WriteLog("EnSightGoldData creation has begun");
                // Скачиваем файлы нужных расширений, когда те появятся в папках
                TJournalLog.WriteLog("Started looking for the .geo");
                // Скачиваем .geo
                while (true)
                {
                    // Получаем названия всех файлов в папке с результатами расчета
                    string AllFiles = SshConnection.RunCommand("cd " + Path + "; ls").Result.ToString();
                    if (!AllFiles.Contains(TaskName + ".geo"))
                    {
                        TJournalLog.WriteLog("There is no .geo");
                        Thread.Sleep(3000);
                        continue;
                    }
                    // Загружаем файл на локальный компьютер
                    TJournalLog.WriteLog($"Downloading {TaskName}.geo");
                    FileInfo LocalFile = new FileInfo(LocalPath + TaskName + ".geo");
                    Scp.Download(Path + "/" + TaskName + ".geo", LocalFile);
                    PathsToResultFiles.Add(LocalFile.FullName);
                    break;
                }
                TJournalLog.WriteLog("Started looking for the other files");
                // Скачиваем остальные расширения
                foreach (string extention in EnsightExtensions)
                {
                    while (true)
                    {
                        // Получаем названия всех файлов в папке с результатами создания EnSightGoldData
                        string AllFiles = SshConnection.RunCommand("cd " + Path + "/EnSightGoldData; ls").Result.ToString();
                        if (!AllFiles.Contains(TaskNameWithNumber + extention))
                        {
                            TJournalLog.WriteLog("There is no needed files");
                            Thread.Sleep(3000);
                            continue;
                        }
                        // Загружаем файл на локальный компьютер
                        TJournalLog.WriteLog($"Downloading {TaskNameWithNumber}{extention}");
                        FileInfo LocalFile = new FileInfo(LocalPath + TaskNameWithNumber + extention);
                        Scp.Download(Path + "/EnSightGoldData/" + TaskNameWithNumber + extention, LocalFile);
                        PathsToResultFiles.Add(LocalFile.FullName);
                        break;
                    }
                }
                Scp.Disconnect();
                return PathsToResultFiles;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C1008: Error TLogosTVD:ExportResults(): " + E.Message);
                return null;
            }
        }
        //------------------------------------------------------------
        /// <summary>
        /// Остановить все текущие расчеты/операции 
        /// </summary>
        public void Stop()
        {
            try
            {
                SshConnection.RunCommand("scancel " + JobID);
                IsCompute = false;
                TJournalLog.WriteLog("Job " + JobID + " has been canceled");
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0009: Error TLogosTVD:Stop(): " + E.Message);
            }
        }
        //------------------------------------------------------------
        /// <summary>
        /// Поставить на паузу расчеты/операции (по факту - остановить расчет c сохранением данных о последней итерации)
        /// </summary>
        public void Pause()
        {
            try
            {
                bool WasComputing = false;
                if (IsCompute) { WasComputing = true; }
                IsCompute = false;
                SshConnection.RunCommand("touch " + RemoteWorkFolder + "qbox");
                if (WasComputing)
                {
                    TJournalLog.WriteLog("Solving of " + JobID + " has been stoped");
                }
                TJournalLog.WriteLog("Project with path " + RemoteWorkFolder + " has been stoped");
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0010: Error TLogosTVD:Pause(): " + E.Message);
            }
        }
        //------------------------------------------------------------
        /// <summary>
        /// Продолжить расчеты/операции 
        /// </summary>
        public void Continue()
        {
            try
            {
                // Номер итерации
                int NumberLGS = -1;
                // Название ямла
                string YamlName = "";
                string LocalYamlName = "";
                // Лист со всеми файлами lgs
                List<string> FilesLgs = new List<string>();
                // Добавляем все lgs в лист
                string[] FileNames = SshConnection.RunCommand("cd " + RemoteWorkFolder + "; ls").Result.ToString().Trim('\n').Split('\n');
                foreach (string FileName in FileNames)
                {
                    if (System.IO.Path.GetExtension(FileName).Equals(".lgs"))
                        FilesLgs.Add(System.IO.Path.GetFileNameWithoutExtension(FileName));
                    else if (System.IO.Path.GetExtension(FileName).Equals(".yaml"))
                    {
                        YamlName = FileName;
                        LocalYamlName = FileName;
                    }
                }
                // Ищем среди всех lgs тот, который содержит наибольший шаг расчета
                foreach (string FileLgs in FilesLgs)
                {
                    string NewString = FileLgs.Remove(0, FileLgs.LastIndexOf('_') + 1).TrimStart('0');
                    if (int.Parse(NewString) > NumberLGS)
                        NumberLGS = int.Parse(NewString);
                }
                // Загрузка ямла из суперкомпьютера на локальный
                string LocalPath = Directory.GetCurrentDirectory() + "/";
                // Проверяем наличие ямл файла с таким же названием в папке, куда хотим его загрузить
                if (File.Exists(LocalPath + YamlName) == true)
                {
                    int i = 0;
                    // Приписываем номер к названию файла
                    do
                    {
                        LocalYamlName = System.IO.Path.GetFileNameWithoutExtension(YamlName) + "(" + i.ToString() + ").yaml";
                        i++;
                    }
                    while (File.Exists(LocalPath + LocalYamlName) == true);
                }
                var Scp = new ScpClient(SshConnection.ConnectionInfo);
                Scp.Connect();
                if (Scp.IsConnected)
                {
                    TJournalLog.WriteLog("Scp Connection Established");
                    FileInfo LocalFile = new FileInfo(LocalPath + LocalYamlName);
                    Scp.Download(RemoteWorkFolder + YamlName, LocalFile);
                    // Изменение ямл файла
                    string Line;
                    string Yaml = "";
                    var dstEncoding = Encoding.GetEncoding(1251);
                    using (StreamReader ReadYaml = new StreamReader(LocalPath + LocalYamlName, encoding: dstEncoding))
                    {
                        while ((Line = ReadYaml.ReadLine()) != null)
                        {
                            if (Line.Contains("start step") == true)
                                Line = "  start step: " + NumberLGS;
                            Yaml = Yaml + "\n" + Line;
                        }
                    }
                    //Перезапись ямл файла на ЛК
                    using (StreamWriter WriteYaml = new StreamWriter(LocalPath + LocalYamlName, append: false, encoding: dstEncoding))
                    {
                        WriteYaml.Write(Yaml);
                        WriteYaml.Close();
                    }
                    TJournalLog.WriteLog("YAML has been changed");
                    // Удаление старого ямл файла c суперкомпьютера
                    SshConnection.RunCommand("rm " + RemoteWorkFolder + YamlName);
                    // Загрузка нового ямл файла на суперкомпьютер
                    StreamReader sr = new StreamReader(LocalPath + LocalYamlName);
                    Scp.Upload(sr.BaseStream, RemoteWorkFolder + YamlName);
                    sr.Close();
                    Scp.Disconnect();
                    // Запуск расчета
                    Solve();
                    TJournalLog.WriteLog("Project with path " + RemoteWorkFolder + " has been continued");
                    //Удаление нового ямла с локального компьютера
                    File.Delete(LocalPath + LocalYamlName);
                }
                else
                {
                    TJournalLog.WriteLog("Scp Connection Error");
                    throw new Exception("Unable to start solving. Scp is not connected");
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0011: Error TLogosTVD:Continue(): " + E.Message);
            }
        }
        //------------------------------------------------------------
        /// <summary>
        /// Закрыть проект в решателе и удалить с суперкомпьтера
        /// </summary>
        public void RemoveProject()
        {
            try
            {
                SshConnection.RunCommand("rm -r " + RemoteWorkFolder);
                IsCompute = false;
                TJournalLog.WriteLog("Project with path " + RemoteWorkFolder + " has been deleted");
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0012: Error TLogosTVD:RemoveProject(): " + E.Message);
            }
        }
        //------------------------------------------------------------
        /// <summary>
        /// Остановить все текущие расчеты/операции и удалить
        /// связь с решателем, удалить проект с суперкомпьтера
        /// </summary>
        public void Dispose()
        {
            try
            {
                Stop();
                RemoveProject();
                //
                if (IsConnected())
                {
                    SshConnection.Disconnect();
                }
                //if (YamlConfiguration != null)
                //{
                //    YamlConfiguration = null;
                //}
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0013: Error TLogosTVD:Dispose(): " + E.Message);
            }
            
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public TLogos_TVD()
        {
            //var initialFolder =  Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //int trimCounter = initialFolder.Length;
            //for(int i  = initialFolder.Length - 1; i >= 0; i--)
            //{
            //    if (initialFolder[i] == '\\') { trimCounter = i; break; }
            //}
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Скачивание файлов с силами, коэффциентами, моментами на локальный компьютер
        /// </summary>
        /// <param name="LocalPathForce"></param>
        public void DownoloadForceAndMoment()
        {
            try
            {
                // Путь на локальном компьютере до папки с результатами для сил и коэффициентов сил
                string LocalPathInDownloadF = LocalWorkFolder + "ForceAndCoeff" + "\\";
                // Путь на локальном компьютере до папки с результатами для коэффициентов моментов
                string LocalPathInDownloadM = LocalWorkFolder + "Moments" + "\\";
               
                // Создать эти папки
                if (!Directory.Exists(LocalPathInDownloadF))
                {
                    Directory.CreateDirectory(LocalPathInDownloadF);
                }
                if (!Directory.Exists(LocalPathInDownloadM))
                {
                    Directory.CreateDirectory(LocalPathInDownloadM);
                }
                // Путь к расчету на кластере
                //==================================================================
                string Path = PathToCluster + UID.ToString() + "/";
                var Scp = new ScpClient(SshConnection.ConnectionInfo);
                Scp.Connect();
                if (!Scp.IsConnected)
                {
                    TJournalLog.WriteLog("Scp Connection Error");
                    return;
                }
                // Пишем в модель данных название ямл файла
                string Name = System.IO.Path.GetFileNameWithoutExtension(Settings.PathToYaml);
                string extention = ".force";
                // Скачиваем файл форс
                TJournalLog.WriteLog($"Downloading {Name}{extention}");
                FileInfo LocalFile = new FileInfo(LocalPathInDownloadF + Name + extention);
                Scp.Download(Path + Name + extention, LocalFile);
                // Скачиваем файл форс коэффициенты
                extention = ".force_coeff";
                TJournalLog.WriteLog($"Downloading {Name}{extention}");
                LocalFile = new FileInfo(LocalPathInDownloadF + Name + extention);
                Scp.Download(Path + Name + extention, LocalFile);
                // Массив с путями на кластере со всеми файлами
                string[] FileNames = SshConnection.RunCommand("cd " + RemoteWorkFolder + "; ls").Result.ToString().Trim('\n').Split('\n');
                List<string> MomentsFiles = new List<string>();
                // Ищем файлы моментов
                foreach (string FileName in FileNames)
                {
                    if (System.IO.Path.GetExtension(FileName).Equals(".moment_coeff"))
                        MomentsFiles.Add(System.IO.Path.GetFileNameWithoutExtension(FileName));
                }
                extention = ".moment_coeff";
                // Скачиваем их
                for (int i = 0; i < MomentsFiles.Count; i++)
                {
                    TJournalLog.WriteLog($"Downloading {MomentsFiles[i]}{extention}");
                    LocalFile = new FileInfo(LocalPathInDownloadM + MomentsFiles[i] + extention);
                    Scp.Download(Path + MomentsFiles[i] + extention, LocalFile);
                }
            }
            catch (Exception E) 
            {
                TJournalLog.WriteLog("C0012: Error TLogosTVD:DownoloadForceAndMoment(): " + E.Message);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Удалить старый ямл файл на кластере и скачать новый
        /// </summary>
        public void DeleteOldYAML_AndDownoloadNew ()
        {
            try
            {
                string Path = PathToCluster + UID.ToString();
                // Удаление файла
                SshConnection.RunCommand("rm " + Path + "/" + Settings.YamlFile);
                UploadFilesToWorkfolder(new string[1]
                {
                        Settings.PathToYaml
                });
                TJournalLog.WriteLog("YAML file downoload");
            }
            catch (Exception E) 
            {
                TJournalLog.WriteLog("C0012: Error TLogosTVD:DeleteOldYAML_AndDownoloadNew(): " + E.Message);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
