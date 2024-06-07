// Описание элемента конечно-элементной модели
//******************************************************************
namespace Example
{
    /// <summary>
    /// Описание элемента конечно-элементной модели
    /// </summary>
    public class TFemElement_Transformable
    {
        /// <summary>
        /// Массив граней(сторон)
        /// </summary>
        public TFemFace_Transformable[] Faces = new TFemFace_Transformable[0];
        /// <summary>
        /// 
        /// </summary>
        public int ChimeraId = -1;
    }
}