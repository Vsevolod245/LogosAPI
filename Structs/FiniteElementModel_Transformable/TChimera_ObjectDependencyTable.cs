// Таблица соответствия химер с объектами и привязывания объектов друг с другом
//******************************************************************
namespace Example
{
    public class TChimera_ObjectDependencyTable
    {
        public string[] PathsToSTL;
        public int[] ChimeraID;
        public int[] ParentID;

        public TChimera_ObjectDependencyTable(string[] pathsToSTL)
        {
            PathsToSTL = pathsToSTL;
            ChimeraID = new int[pathsToSTL.Length];
            ParentID = new int[pathsToSTL.Length];
        }
    }
}
