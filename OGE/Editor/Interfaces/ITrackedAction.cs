using System.Xml.Linq;

namespace OGE.Editor.Interfaces
{
    /// <summary>
    /// Used to help track changes to files for undo/redo,
    /// and modinfo generation. The initial action should
    /// occur before or when the concrete type is constructed.
    /// </summary>
    public interface ITrackedAction
    {
        /// <summary>
        /// Undo the changes made by the action.
        /// </summary>
        void Undo();
        /// <summary>
        /// Redo the changes made by the action.
        /// </summary>
        void Redo();

        void WriteToProjectFile(XElement changeNode);
        void ReadFromProjectFile(XElement changeNode);
    }
}
