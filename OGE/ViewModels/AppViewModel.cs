using System.IO;
using System.Reactive;
using System.Windows.Forms;
using OGE.Editor.Events;
using OGE.Editor.Managers;
using OGE.Utility;
using ReactiveUI;

namespace OGE.ViewModels
{
    public class AppViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> ShowAboutMessageCommand;
        public ReactiveCommand<Unit, Unit> OpenProjectCommand;
        public ReactiveCommand<Unit, Unit> NewProjectCommand;
        public ReactiveCommand<Unit, Unit> CloseProjectCommand;

        public AppViewModel()
        {
            ShowAboutMessageCommand = ReactiveCommand.Create(() =>
            {
                MessageBox.Show("OGE is an open source modding tool for Red Faction Guerrilla Re-Mars-tered. " + 
                                "For more info, see it's github page: https://github.com/Moneyl/OGE", "About OGE",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
            OpenProjectCommand = ReactiveCommand.Create(() =>
            {
                //Get project file path
                //Check for valid path
                //Pass to ProjectManager
                //ProjectManager.OpenProject();
            });
            NewProjectCommand = ReactiveCommand.Create(() =>
            {
                //Create dialog requesting user input: project folder path, project name, templates, settings, etc
                //Pass to project manager
                //ProjectManager.CreateNewProject();
            });
            CloseProjectCommand = ReactiveCommand.Create(() =>
            {
                //If project is open, ask user to confirm if they want to close it
                //Ask if they want to save any unsaved changes, warn about data loss if not
                //Pass to project manager
                ProjectManager.CloseCurrentProject();
            });
        }
    }
}
