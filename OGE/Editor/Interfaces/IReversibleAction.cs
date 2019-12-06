using System;
using System.Collections.Generic;
using System.Text;

namespace OGE.Editor.Interfaces
{
    /// <summary>
    /// Used to help track changes to files for undo/redo,
    /// and modinfo generation. The initial action should
    /// occur before or when the concrete type is constructed.
    /// </summary>
    public interface IReversibleAction
    {
        /// <summary>
        /// Undo the changes made by the action.
        /// </summary>
        void Undo();
        /// <summary>
        /// Redo the changes made by the action.
        /// </summary>
        void Redo();
    }
}
