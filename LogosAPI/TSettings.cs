using System.IO;
using System.Reflection;

namespace Logos_TVD
{
    public class TSettings
    {
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Путь к локальной рабочей папке. Должен заканчиваться на слеш.
        /// </summary>
        public string LocalWorkFolder 
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\L_API_Tmp\\";
            }
        }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public string YamlMeshFolder { get { return LocalWorkFolder + "YamlMesh\\"; } }
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public string PathToMesh
        {
            get { return YamlMeshFolder + MeshFile; }
        }
        public string PathToYaml
        {
            get { return YamlMeshFolder + YamlFile; }
        }
        public string PathToTaskName
        {
            get { return YamlMeshFolder + "task.name"; }
        }
        public string MeshFile = "";
        public string YamlFile = "";
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public int NumberOfNodes = 1;
        public int NumberOfTasksPerNode = 1;
        public string PartitionName = "";
    }
}
