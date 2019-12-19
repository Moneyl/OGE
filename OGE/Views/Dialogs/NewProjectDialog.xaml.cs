using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using MahApps.Metro.SimpleChildWindow;

namespace OGE.Views.Dialogs
{
    public enum NewProjectDialogResult
    {
        Create,
        Cancel
    }

    public partial class NewProjectDialog : ChildWindow, INotifyPropertyChanged
    {
        //Note: Didn't use any ReactiveUI features here for now since it was simpler to implement.
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _path;

        public string ProjectName { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged("Path");
            }
        }
        public NewProjectDialogResult Result { get; set; }

        public NewProjectDialog()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void CreateButton_OnClick(object sender, RoutedEventArgs e)
        {
            Result = NewProjectDialogResult.Create;
            Close();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Result = NewProjectDialogResult.Cancel;
            Close();
        }

        private void BrowsePathButton_OnClick(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowDialog();
            Path = folderBrowserDialog.SelectedPath;
        }
    }
}
