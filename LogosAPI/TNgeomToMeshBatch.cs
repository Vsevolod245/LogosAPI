// Описание скрипта для конвертации Ngeom в mesh
using System.Text;
//***********************************************************
namespace Logos_TVD
{
    internal class TNgeomToMeshBatch
    {
        /// <summary>
        /// Путь к файлу logosvars.sh (так же подойдет путь /home/NIO-101/LAB_1/Distr/logosvars/2_0_3/logosvars.sh)
        /// </summary>
        public string Setup_LogosVars = "/home/software/VNIIEF/LOGOS-5.3.22.805/logosvars.sh";
        /// <summary>
        /// Путь к корневой папке TVD
        /// </summary>
        public string Setup_TVDROOT = "/home/software/VNIIEF/LOGOS-5.3.22.805/LOGOS-CFD/Bin/common/tvd";
        /// <summary>
        /// Путь к папке с бинарными файлами Logos
        /// </summary>
        public string Setup_COMMONBIN = "/home/software/VNIIEF/LOGOS-5.3.22.805/LOGOS-COMMON/Bin";
        /// <summary>
        /// Путь до OMPI
        /// </summary>
        public string Setup_OMPI_LIB = "/home/software/OpenMPI/openmpi-4.1.4/lib";
        /// <summary>
        /// Выбор пути до бибилиотеки
        /// </summary>
        public string Setup_LD_LIBRARY_PATH = "if [ x\"$LD_LIBRARY_PATH\" = x ] ; then\n" +
            "    export LD_LIBRARY_PATH=$TVDROOT:$COMMONBIN:$OMPI_LIB\nelse\n" +
            "    export LD_LIBRARY_PATH=$TVDROOT:$COMMONBIN:$LD_LIBRARY_PATH:$OMPI_LIB\n" +
            "fi";
        /// <summary>
        /// Сборка путей в одну переменную
        /// </summary>
        public string Setup_PATH = "export PATH=$TVDROOT:$PATH:$OMPI_LIB";
        /// <summary>
        /// Исполлнение файла с передачей параметров
        /// </summary>
        public string Setup_Logos_TVD = "$TVDROOT/Logos_TVD $@";
        //---------------------------------------------------------------------
        /// <summary>
        /// Сформировать файл в текстовом представлении
        /// </summary>
        /// <returns>Код скрипта для конвертации Ngeom в Mesh</returns>
        public string ToStringBatch_NgeomToMesh()
        {
            StringBuilder StringBatch = new StringBuilder();
            //
            StringBatch.Append("#!/usr/bin/env bash\n");
            StringBatch.Append("\n");
            // # Экспорт общих переменных
            StringBatch.Append("if [ -f " + Setup_LogosVars + " ] ; then \n");
            StringBatch.Append("    . " + Setup_LogosVars + "\n");
            StringBatch.Append("fi");
            StringBatch.Append("\n\n");
            // # Установка переменных для компонента
            StringBatch.Append("TVDROOT=" + Setup_TVDROOT + "\n");
            StringBatch.Append("COMMONBIN=" + Setup_COMMONBIN + "\n");
            StringBatch.Append("OMPI_LIB=" + Setup_OMPI_LIB + "\n");
            StringBatch.Append("\n");
            //
            StringBatch.Append(Setup_LD_LIBRARY_PATH + "\n");
            StringBatch.Append("\n");
            //
            StringBatch.Append(Setup_PATH + "\n");
            StringBatch.Append("\n");
            //
            StringBatch.Append(Setup_Logos_TVD + "\n");
            //
            return StringBatch.ToString();
        }
        //---------------------------------------------------------------------
    }
}

/*
#!/usr/bin/env bash

# Экспорт общих переменных
if [ -f /home/software/VNIIEF/LOGOS-5.3.21.725/logosvars.sh ] ; then 
    . /home/software/VNIIEF/LOGOS-5.3.21.725/logosvars.sh
fi

# Установка переменных для компонента
TVDROOT=/home/software/VNIIEF/LOGOS-5.3.21.725/LOGOS-CFD/Bin/common/tvd
COMMONBIN=/home/software/VNIIEF/LOGOS-5.3.21.725/LOGOS-COMMON/Bin
OMPI_LIB=/apps/openmpi-4.0.5_v3/lib

if [ x"$LD_LIBRARY_PATH" = x ] ; then
    export LD_LIBRARY_PATH=$TVDROOT:$COMMONBIN:$OMPI_LIB
else
    export LD_LIBRARY_PATH=$TVDROOT:$COMMONBIN:$LD_LIBRARY_PATH:$OMPI_LIB
fi

export PATH=$TVDROOT:$PATH:$OMPI_LIB

$TVDROOT/Logos_TVD $@

*/