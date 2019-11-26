using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using OGE.Editor;
using OGE.Events;
using OGE.Helpers;
using OGE.Utility;
using OGE.ViewModels;
using ReactiveUI;
using Xceed.Wpf.AvalonDock.Layout;

namespace OGE
{
    public partial class MainWindow : ReactiveWindow<AppViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

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

                var document = new LayoutDocument
                {
                    Title = args.TargetItem.ShortName,
                    Content = new TextEditor
                    {
                        Document = new TextDocument(docString),
                        SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(targetItem.FileExtension),
                        Margin = new Thickness(5, 0, 0, 0)
                    }
                };
                DocumentPane.Children.Add(document);
                DocumentPane.SelectedContentIndex = DocumentPane.ChildrenCount - 1;
            }
        }

        private void LoadAdditionalHighlightingDefinitions()
        {
            var highlightingManager = HighlightingManager.Instance;

            using (var reader = new XmlTextReader(new FileStream(@".\Highlighting\Lua-Mode.xshd", FileMode.Open)))
            {
                IHighlightingDefinition definition = HighlightingLoader.Load(reader, highlightingManager);
                highlightingManager.RegisterHighlighting("Lua-Mode", new []{".lua"}, definition);
            }
        }
    }
}
