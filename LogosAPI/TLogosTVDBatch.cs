// Описание стартового скрипта для ЛОГОС
using System;
using System.Text;
//
using AstraEngine.Components;
using AstraEngine.Scene;
//***********************************************************
namespace Logos_TVD
{
    /// <summary>
    /// Описание стартового скрипта для ЛОГОС
    /// </summary>
    public class TLogosTVDBatch
    {
        public TLogosTVDBatch(TSettings Settings)
        {
            Nodes = Settings.NumberOfNodes;
            NTasks_Per_Node = Settings.NumberOfTasksPerNode;
            Setup_NameNodes += Settings.PartitionName;
        }
        /// <summary>
        /// Кол-во нодов на кластере на которых будет выполняться задача
        /// </summary>
        public int Nodes = 2;
        /// <summary>
        /// Кол-во ядер в ноде
        /// </summary>
        public int NTasks_Per_Node = 36;
        /// <summary>
        /// Установить принцип записи журнала
        /// </summary>
        public string Setup_OutLogger = "#SBATCH -o job_%j_slurm.log";
        /// <summary>
        /// Установить принцип записи ошибок
        /// </summary>
        public string Setup_OutErrors = "#SBATCH -e job_%j_slurm.err";
        /// <summary>
        /// Время через которое расчет закончится принудительно
        /// </summary>
        public TimeSpan TimeMaxSolve = TimeSpan.FromHours(240);
        /// <summary>
        /// Установить имя расчетных узлов
        /// </summary>
        public string Setup_NameNodes = "#SBATCH --partition=";
        /// <summary>
        /// Дополнительные бибилотеки ЛОГОС
        /// </summary>
        public string Setup_Logos_Vars
        {
            get { return ". " + TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_Path_LogosVars"]; }
        }
        /// <summary>
        /// Имя организации для лицензии
        /// </summary>
        public string Setup_CUSTOMER_NAME
        {
            get { return "export CUSTOMER_NAME=" + TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_CUSTOMER_NAME"]; }
        }
        /// <summary>
        /// Путь к бибилотекам MPI
        /// </summary>
        public string Setup_MPI
        {
            get
            {
                return "export PATH=$PATH:" + TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_Path_OMPI_BIN"] +
                "\nexport LD_LIBRARY_PATH=$LD_LIBRARY_PATH:" + TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_Path_OMPI_LIB"];
            }
        }
        /// <summary>
        /// Адрес лицензии для ЛОГОС
        /// </summary>
        public string Setup_LICADDR
        {
            get
            {
                return "export LICADDR=" + TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_LICADDR"];
            }
        }
        /// <summary>
        /// Команда старта расчета с указанием пути к решателю
        /// </summary>
        public string Setup_Run
        {
            get
            {
                return "mpirun  --mca btl_openib_warn_no_device_params_found 0 -x LICADDR -x CUSTOMER_NAME " + TStaticContent.Content.GameOptions.GetGameOptions()["Cluster_Path_Logos_TVD"];
            }
        }
//---------------------------------------------------------------------
        /// <summary>
        /// Сформировать файл в текстовом представлении
        /// </summary>
        /// <returns>Текстовое представление</returns>
        public string ToStringBatch_Slurm()
        {
            StringBuilder StringBatch = new StringBuilder();
            //
            StringBatch.Append("#!/bin/bash\n");
            //
            StringBatch.Append("#SBATCH --nodes=" + Nodes.ToString()+"\n");
            StringBatch.Append("#SBATCH --ntasks-per-node=" + NTasks_Per_Node.ToString() + "\n");
            StringBatch.Append(Setup_OutLogger + "\n");
            StringBatch.Append(Setup_OutErrors + "\n");
            StringBatch.Append("#SBATCH --time=" + TimeMaxSolve.TotalDays.ToString() + "-1:0:0" + "\n");
            StringBatch.Append(Setup_NameNodes + "\n");
            //
            StringBatch.Append("\n");
            //
            StringBatch.Append(Setup_LICADDR + "\n");
            StringBatch.Append(Setup_CUSTOMER_NAME + "\n");
            //
            //StringBatch.Append("\n");
            //
            //StringBatch.Append(Setup_Logos_Vars + "\n");
            //
            StringBatch.Append("\n");
            //
            StringBatch.Append(Setup_MPI + "\n");
            //
            StringBatch.Append("\n");
            //
            StringBatch.Append(Setup_Run + "\n");
            //
            return StringBatch.ToString();
        }
//---------------------------------------------------------------------
        /// <summary>
        /// Выполнить проверку настроек
        /// </summary>
        /// <returns>Настройки корректны/нет</returns>
        public bool ControlBatch()
        {
            //
            if(Nodes <= 0)
            {
                TJournalLog.WriteLog("C0107: Error TLogosTVDBatch:ControlBatch(): Nodes <= 0");
                return false;
            }
            //
            if (Nodes <= 0)
            {
                TJournalLog.WriteLog("C0107: Error TLogosTVDBatch:ControlBatch(): Nodes <= 0");
                return false;
            }


            return true;
        }
//---------------------------------------------------------------------
    }
}

/*
 * #!/bin/bash
#SBATCH --nodes=10
#SBATCH --ntasks-per-node=36
#SBATCH -o job_%j_slurm.log
#SBATCH -e job_%j_slurm.err
#SBATCH --time=10-1:0:0
#SBATCH --partition=nodes

export LICADDR=
export CUSTOMER_NAME=

. /home/logosvars.sh

#export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/home/LogosTVD/
export PATH=$PATH:/apps/openmpi-4.0.5_v3/bin
export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/apps/openmpi-4.0.5_v3/lib

mpirun  --mca btl_openib_warn_no_device_params_found 0 -x LICADDR -x CUSTOMER_NAME /home/LOGOS-5.3.21.725/LOGOS-CFD/Bin/Logos_TVD

*/