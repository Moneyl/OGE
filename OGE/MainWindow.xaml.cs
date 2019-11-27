using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using DynamicData.Kernel;
using HL.HighlightingTheme;
using HL.Manager;
using HL.Xshtd.interfaces;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using MahApps.Metro.Controls;
using MLib.Interfaces;
using OGE.Editor;
using OGE.Events;
using OGE.Helpers;
using OGE.Utility;
using OGE.ViewModels;
using ReactiveUI;
using Xceed.Wpf.AvalonDock.Layout;
using HighlightingLoader = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader;

namespace OGE
{
    public partial class MainWindow : MetroWindow, IViewFor<AppViewModel> //ReactiveWindow<AppViewModel>
    {
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AppViewModel)value;
        }
        public AppViewModel ViewModel { get; set; }
        //public ThemedHighlightingManager highlightingManager { get; set; }
        public HighlightingManager highlightingManager { get; set; }
        public IAppearanceManager AppearanceManager { get; set; }
        public IThemeInfos Themes { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            //highlightingManager = new ThemedHighlightingManager();
            highlightingManager = new HighlightingManager();
            SetupAppearanceInfo();
            //var theme = highlightingManager.CurrentTheme;
            //highlightingManager.SetCurrentTheme("VS2019_Dark");

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

                //this.OneWayBind(ViewModel,
                //        vm => vm.FileExplorerVm,
                //        v => v.fileExplorerView.ViewModel)
                //    .DisposeWith(disposable);
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

            if (PathHelpers.IsTextExtension(args.TargetItem.FileExtension))
            {
                string docString;
                using (StreamReader reader = new StreamReader(docStream))
                {
                    docString = reader.ReadToEnd();
                }

                var definition = highlightingManager.GetDefinitionByExtension(targetItem.FileExtension);
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
        }

        private void LoadAdditionalHighlightingDefinitions()
        {
            LoadHighlightingDefinition(@".\Highlighting\Lua-Mode.xshd", "Lua-Mode", new []{".lua"});
            LoadHighlightingDefinition(@".\Highlighting\XML-Mode.xshd", "Xml", PathHelpers.XmlExtensions.AsArray());

        }

        private void LoadHighlightingDefinition(string definitionPath, string definitionName, string[] extensions)
        {
            using (var reader = new XmlTextReader(new FileStream(definitionPath, FileMode.Open)))
            {
                IHighlightingDefinition definition = HighlightingLoader.Load(reader, highlightingManager);
                highlightingManager.RegisterHighlighting(definitionName, extensions, definition);
            }
        }

        private void SetupAppearanceInfo()
        {

        }
    }
}
