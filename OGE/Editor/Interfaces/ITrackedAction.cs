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
        /// <summary>
        /// Write the changes and any data needed by them
        /// to the project file.
        /// </summary>
        /// <param name="changeNode">The xml node containing the change data</param>
        void WriteToProjectFile(XElement changeNode);
        /// <summary>
        /// Read the changes and any data needed by them
        /// to the project file.
        /// </summary>
        /// <param name="changeNode">The xml node containing the change data</param>
        void ReadFromProjectFile(XElement changeNode);
    }
}