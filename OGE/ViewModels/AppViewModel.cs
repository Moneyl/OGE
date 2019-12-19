using System.Reactive;
using System.Windows.Forms;
using OGE.Editor.Managers;
using ReactiveUI;

namespace OGE.ViewModels
{
    public class AppViewModel : ReactiveObject
    {
        private OpenFileDialog _openFileDialog = new OpenFileDialog
        {
            Filter = "OGE project file (*.oge_proj)|*.oge_proj",
            Title = "Select an OGE project file (*.oge_proj)"
        };

        public ReactiveCommand<Unit, Unit> ShowAboutMessageCommand;
        public ReactiveCommand<Unit, Unit> OpenProjectCommand;
        public ReactiveCommand<Unit, Unit> CloseProjectCommand;
        public ReactiveCommand<Unit, Unit> SaveProjectCommand;

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
                _openFileDialog.ShowDialog();
                ProjectManager.OpenProject(_openFileDialog.FileName);
            });
            SaveProjectCommand = ReactiveCommand.Create(() =>
            {
                ProjectManager.SaveCurrentProject();
            });
            CloseProjectCommand = ReactiveCommand.Create(() =>
            {
                //Todo: Add option to save unsaved changes
                var result = MessageBox.Show("Are you sure you'd like to close the project? Any unsaved changes will be lost.",
                    "Confirm closing project", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    ProjectManager.CloseCurrentProject();
                }
            });
        }
    }
}
