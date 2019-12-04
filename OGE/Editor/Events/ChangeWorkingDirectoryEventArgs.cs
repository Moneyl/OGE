
namespace OGE.Editor.Events
{
    /// <summary>
    /// Triggered when the working directory for the file explorer changes
    /// </summary>
    public class ChangeWorkingDirectoryEventArgs
    {
        public string NewWorkingDirectory { get; }

        public ChangeWorkingDirectoryEventArgs(string newWorkingDirectory)
        {
            NewWorkingDirectory = newWorkingDirectory;
        }
    }
}
