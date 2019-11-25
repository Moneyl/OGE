using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
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
            if (!ProjectManager.TryGetFile(targetItem.FilePath, parent.FilePath, out Stream docStream, true))
                return;

            if (PathHelpers.IsXmlExtension(args.TargetItem.FileExtension))
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
                        SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XmlDoc")
                    }
                };
                DocumentPane.Children.Add(document);
                DocumentPane.SelectedContentIndex = DocumentPane.ChildrenCount - 1;
            }
        }
    }
}
