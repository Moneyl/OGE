using System.IO;
using System.Reactive;
using System.Windows.Forms;
using OGE.Editor.Events;
using OGE.Utility;
using ReactiveUI;

namespace OGE.ViewModels
{
    public class AppViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> ShowAboutMessageCommand;

        public AppViewModel()
        {
            ShowAboutMessageCommand = ReactiveCommand.Create(() =>
            {
                MessageBox.Show("OGE is an open source modding tool for Red Faction Guerrilla Re-Mars-tered. " + 
                                "For more info, see it's github page: https://github.com/Moneyl/OGE", "About OGE",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }
    }
}
