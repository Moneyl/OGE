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
using OGE.Editor;
using OGE.Events;
using OGE.Helpers;
using OGE.Utility;
using OGE.ViewModels;
using ReactiveUI;
using RfgTools.Formats.Textures;
using Xceed.Wpf.AvalonDock.Layout;
using HighlightingLoader = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader;

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
            InitializeComponent();

            HighlightingManager = new HighlightingManager();
            LoadAdditionalHighlightingDefinitions();
            WindowLogger.SetLogPanel(LogStackPanel);
            ProjectManager.Init();

            ViewModel = new AppViewModel();

            this.WhenActivated(disposable =>
            {
                this.BindCommand(ViewModel,
                        vm => vm.OpenWorkingFolder,
                        v => v.MenuOpenFolderButton)
                    .DisposeWith(disposable);
            });

            MessageBus.Current.Listen<OpenFileEventArgs>()
                .Where(args => args.TargetItem != null && args.TargetItem.Parent != null)
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
            var targetItem = args.TargetItem;
            var parent = targetItem.Parent;
            if (!ProjectManager.TryGetFile(targetItem, out Stream docStream, true))
                return;

            if (PathHelpers.IsTextExtension(targetItem.FileExtension))
            {
                string docString;
                using (StreamReader reader = new StreamReader(docStream))
                {
                    docString = reader.ReadToEnd();
                }

                var definition = HighlightingManager.GetDefinitionByExtension(targetItem.FileExtension);
                var document = new LayoutDocument
                {
                    Title = args.TargetItem.ShortName,
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
                //Make sure both texture files are in cache, get their paths
                //Load texture
                //Create view/viewmodel for texture and pass texture to it
                //Stick that view/viewmodel into a LayoutDocument

                //Get gpu file name
                if(!PathHelpers.TryGetGpuFileNameFromCpuFile(targetItem.FilePath, out string gpuFileName))
                    return;

                //Get cpu file stream from cache
                if (!ProjectManager.TryGetFile(targetItem.FilePath, out Stream cpuFileStream, targetItem.Parent, true))
                    return;

                //Get gpu file stream from cache
                if (!ProjectManager.TryGetFile(gpuFileName, out Stream gpuFileStream, targetItem.Parent, true))
                    return;

                var pegFile = new PegFile();
                pegFile.Read(cpuFileStream, gpuFileStream);
                var a = 2;
            }
        }

        private void LoadAdditionalHighlightingDefinitions()
        {
            LoadHighlightingDefinition(@".\Themes\Highlighting\Lua-Mode.xshd", "Lua-Mode", new []{".lua"});
            LoadHighlightingDefinition(@".\Themes\Highlighting\XML-Mode.xshd", "Xml", PathHelpers.XmlExtensions.AsArray());

        }

        private void LoadHighlightingDefinition(string definitionPath, string definitionName, string[] extensions)
        {
            using (var reader = new XmlTextReader(new FileStream(definitionPath, FileMode.Open)))
            {
                IHighlightingDefinition definition = HighlightingLoader.Load(reader, HighlightingManager);
                HighlightingManager.RegisterHighlighting(definitionName, extensions, definition);
            }
        }
    }
}
