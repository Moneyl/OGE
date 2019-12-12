using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using DynamicData.Kernel;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using OGE.Editor.Events;
using OGE.Editor.Managers;
using OGE.Utility;
using OGE.Utility.Helpers;
using OGE.ViewModels;
using OGE.Views.Dialogs;
using ReactiveUI;
using RfgTools.Formats.Textures;
using Xceed.Wpf.AvalonDock.Layout;
using HighlightingLoader = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader;
using TextureViewerView = OGE.Views.TextureViewer.TextureViewerView;

namespace OGE
{
    public partial class MainWindow : MetroWindow, IViewFor<AppViewModel>
    {
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AppViewModel)value;
        }
        public AppViewModel ViewModel { get; set; }
        public HighlightingManager HighlightingManager { get; set; }

        public MainWindow()
        {
            SettingsManager.Init();
            ProjectManager.Init(SettingsManager.DataFolderPath);
            InitializeComponent();

            HighlightingManager = new HighlightingManager();
            LoadAdditionalHighlightingDefinitions();
            WindowLogger.SetLogPanel(LogStackPanel);

            ViewModel = new AppViewModel();

            this.WhenActivated(disposable =>
            {
                this.BindCommand(ViewModel,
                        vm => vm.ShowAboutMessageCommand,
                        v => v.MenuAboutButton)
                    .DisposeWith(disposable);

                this.BindCommand(ViewModel,
                        vm => vm.OpenProjectCommand,
                        v => v.MenuOpenProjectButton)
                    .DisposeWith(disposable);

                this.BindCommand(ViewModel,
                        vm => vm.SaveProjectCommand,
                        v => v.MenuSaveProjectButton)
                    .DisposeWith(disposable);

                this.BindCommand(ViewModel,
                        vm => vm.CloseProjectCommand,
                        v => v.MenuCloseProjectButton)
                    .DisposeWith(disposable);
            });

            MessageBus.Current.Listen<OpenFileEventArgs>()
                .Throttle(TimeSpan.FromMilliseconds(100))
                .Where(args => args.TargetItem?.Parent != null)
                .Subscribe(HandleOpenFileEvent);
        }

        private void MenuExit_OnClick(object sender, RoutedEventArgs e)
        {
#if DEBUG
            var debug = 2; //Set breakpoint here for easy debugging
#else
            Close();
#endif
        }

        private void HandleOpenFileEvent(OpenFileEventArgs args)
        {
            //This syntax is to fix this issue: https://stackoverflow.com/questions/2329978/the-calling-thread-must-be-sta-because-many-ui-components-require-this
            Application.Current.Dispatcher?.Invoke(() =>
            {
                var targetItem = args.TargetItem;

                if (PathHelpers.IsTextExtension(targetItem.FileExtension))
                {
                    if (!ProjectManager.TryGetFile(targetItem.Filename, targetItem.Parent, out Stream docStream, true))
                        return;

                    using StreamReader reader = new StreamReader(docStream);
                    string docString = reader.ReadToEnd();

                    var definition = HighlightingManager.GetDefinitionByExtension(targetItem.FileExtension);
                    var document = new LayoutDocument
                    {
                        Title = args.TargetItem.Filename,
                        Content = new TextEditor
                        {
                            Document = new TextDocument(docString),
                            SyntaxHighlighting = definition,
                            Margin = new Thickness(5, 0, 0, 0),
                            ShowLineNumbers = true,
                            LineNumbersForeground = Brushes.SteelBlue,
                            Foreground = Brushes.LightGray
                        }
                    };
                    DocumentPane.Children.Add(document);
                    DocumentPane.SelectedContentIndex = DocumentPane.ChildrenCount - 1;
                }
                else if (PathHelpers.IsTextureHeaderExtension(targetItem.FileExtension)) //Todo: Support opening via gpu files 
                {
                    if (targetItem.Parent == null)
                        return;

                    //Get gpu filename
                    if (!PathHelpers.TryGetGpuFileNameFromCpuFile(targetItem.FilePath, out string gpuFileName))
                        return;
                    //Get cpu file stream from cache
                    if (!ProjectManager.TryGetFile(targetItem.Filename, targetItem.Parent, out Stream cpuFileStream, true))
                        return;
                    //Get gpu file stream from cache
                    if(!ProjectManager.TryGetFile(gpuFileName, targetItem.Parent, out Stream gpuFileStream, true))
                        return;

                    var pegFile = new PegFile();
                    try
                    {
                        pegFile.Read(cpuFileStream, gpuFileStream);
                        cpuFileStream.Close();
                        gpuFileStream.Close();
                    }
                    catch (Exception e)
                    {
                        WindowLogger.Log($"Failed to load peg file! Filename: \"{targetItem.FilePath}\". Message: \"{e.Message}\"");
                        return;
                    }

                    var document = new LayoutDocument
                    {
                        Title = args.TargetItem.Filename,
                        Content = new TextureViewerView(pegFile)
                    };
                    DocumentPane.Children.Add(document);
                    DocumentPane.SelectedContentIndex = DocumentPane.ChildrenCount - 1;
                }
            });
        }

        private void LoadAdditionalHighlightingDefinitions()
        {
            LoadHighlightingDefinition(@".\Resources\Themes\Highlighting\Lua-Mode.xshd", 
                "Lua-Mode", new []{".lua"});
            LoadHighlightingDefinition(@".\Resources\Themes\Highlighting\XML-Mode.xshd", 
                "Xml", PathHelpers.XmlExtensions.AsArray());
        }

        private void LoadHighlightingDefinition(string definitionPath, string definitionName, string[] extensions)
        {
            using var stream = new FileStream(definitionPath, FileMode.Open);
            using var reader = new XmlTextReader(stream);
            IHighlightingDefinition definition = HighlightingLoader.Load(reader, HighlightingManager);
            HighlightingManager.RegisterHighlighting(definitionName, extensions, definition);
        }

        private async void MenuNewProjectButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new NewProjectDialog {IsModal = true};
            await this.ShowChildWindowAsync(dialog);
            
            //Todo: Ask user if they want to save current project if one is open
            if(dialog.Result == NewProjectDialogResult.Create)
              ProjectManager.CreateNewProject(dialog.Path, dialog.ProjectName, dialog.Author, dialog.Description, dialog.Version);
        }
    }
}
