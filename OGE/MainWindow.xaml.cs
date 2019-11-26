using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MahApps.Metro.Controls;
using MLib.Interfaces;
using OGE.Editor;
using OGE.Events;
using OGE.Helpers;
using OGE.Utility;
using OGE.ViewModels;
using ReactiveUI;
using Xceed.Wpf.AvalonDock.Layout;

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
        public IAppearanceManager AppearanceManager { get; set; }
        public IThemeInfos Themes { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            //SetupAppearanceInfo();

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

        private void SetupAppearanceInfo()
        {
            AppearanceManager = MLib.AppearanceManager.GetInstance();
            Themes = AppearanceManager.CreateThemeInfos();
            Themes.RemoveAllThemeInfos();
            AppearanceManager.SetDefaultThemes(Themes, false);

            // Adding Generic theme (which is really based on Light theme in MLib)
            // but other components may have another theme definition for Generic
            // so this is how it can be tested ...
            //var uri1 = new Uri("/MLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute);
            //var uri2 = new Uri("/MWindowLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute);
            //var uri3 = 

            //AppearanceManager.AddThemeResources("Generic", new List<Uri>
            //{
            //    new Uri("/MLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute),
            //    new Uri("/MWindowLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute),
            //    //new Uri("/MWindowDialogLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute),
            //    new Uri("/BindToMLib;component/LightBrushs.xaml", UriKind.RelativeOrAbsolute)
            //}, Themes);
            AppearanceManager.AddThemeResources("Generic", new List<Uri>
            {
                new Uri("/MLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute),
                new Uri("/MWindowLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute),
                new Uri("/MWindowDialogLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute),
                new Uri("/BindToMLib;component/LightBrushs.xaml", UriKind.RelativeOrAbsolute)
            }, Themes);

            // Add additional Dark and Light resources to those theme resources added above
            AppearanceManager.AddThemeResources("Dark", new List<Uri>
            {
                //new Uri("/MWindowDialogLib;component/Themes/DarkIcons.xaml", UriKind.RelativeOrAbsolute),
                //new Uri("/MWindowDialogLib;component/Themes/DarkBrushs.xaml", UriKind.RelativeOrAbsolute),
                new Uri("/BindToMLib;component/DarkBrushs.xaml", UriKind.RelativeOrAbsolute),

                ////new Uri("/MWindowLib;component/Themes/DarkBrushs.xaml", UriKind.RelativeOrAbsolute),
                ////new Uri("/MDemo;component/Themes/MWindowLib/DarkBrushs.xaml", UriKind.RelativeOrAbsolute),

                new Uri("/MDemo;component/Themes/Dark/DarkIcons.xaml", UriKind.RelativeOrAbsolute),
                ////new Uri("/MDemo;component/Themes/MWindowDialogLib/DarkBrushs.xaml", UriKind.RelativeOrAbsolute).
                new Uri("/MDemo;component/Demos/Views/Dialogs.xaml", UriKind.RelativeOrAbsolute)

            }, Themes);
        }
    }
}
