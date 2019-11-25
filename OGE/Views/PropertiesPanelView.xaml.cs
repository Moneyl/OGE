using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using OGE.Events;
using OGE.ViewModels;
using ReactiveUI;
using RfgTools.Formats.Packfiles;

namespace OGE.Views
{
    public partial class PropertiesPanelView : ReactiveUserControl<PropertiesPanelViewModel>
    {
        public PropertiesPanelView()
        {
            InitializeComponent();
            ViewModel = new PropertiesPanelViewModel();

            this.WhenActivated(disposable =>
            {
                MessageBus.Current.Listen<SelectedItemChangedEventArgs>()
                    .Where(args => args.SelectedItem != null)
                    .Subscribe(action =>
                    {
                        GenerateDataView(action.SelectedItem);
                    })
                    .DisposeWith(disposable);
            });
        }

        //Todo: Change to support multiple file types (peg, packfile, zone file, etc)
        private void GenerateDataView(FileExplorerItemViewModel selectedItem)
        {
            var packfile = selectedItem.Packfile;
            if (packfile == null)
            {
                GenerateNullData(selectedItem);
                return;
            }

            DataPanel.Children.Clear();
            DataPanel.Children.Add(new TextBlock
            {
                Inlines =
                {
                    new Run {Text = "Name: ", FontWeight = FontWeights.Bold},
                    new Run {Text = packfile.Filename},
                    new LineBreak(),

                    new Run {Text = "Version: ", FontWeight = FontWeights.Bold},
                    new Run {Text = packfile.Header.Version.ToString()},
                    new LineBreak(),

                    new Run {Text = "Flags: ", FontWeight = FontWeights.Bold},
                    new Run {Text = packfile.Header.Flags.ToString()},
                    new LineBreak(),

                    new Run {Text = "Compressed: ", FontWeight = FontWeights.Bold},
                    new Run {Text = packfile.Header.Compressed.ToString()},
                    new LineBreak(),

                    new Run {Text = "Condensed: ", FontWeight = FontWeights.Bold},
                    new Run {Text = packfile.Header.Condensed.ToString()},
                    new LineBreak(),

                    new Run {Text = "Num subfiles: ", FontWeight = FontWeights.Bold},
                    new Run {Text = packfile.Header.NumberOfFiles.ToString()},
                    new LineBreak(),

                    new Run {Text = "Total size: ", FontWeight = FontWeights.Bold},
                    new Run {Text = $"{(float)packfile.Header.FileSize / 1000}KB"},
                    new LineBreak(),

                    new Run {Text = "Subfile data size: ", FontWeight = FontWeights.Bold},
                    new Run {Text = $"{(float)packfile.Header.DataSize / 1000}KB"},
                    new LineBreak(),

                    new Run {Text = "Compressed subfile data size: ", FontWeight = FontWeights.Bold},
                    new Run {Text = GetCompressedDataSizeString(packfile.Header.CompressedDataSize)},
                    new LineBreak(),

                    new Run {Text = "Short note: ", FontWeight = FontWeights.Bold},
                    new Run {Text = new string(packfile.Header.ShortName)},
                    new LineBreak(),

                    new Run {Text = "Long note: ", FontWeight = FontWeights.Bold},
                    new Run {Text = new string(packfile.Header.PathName)},
                    new LineBreak(),
                },
                TextWrapping = TextWrapping.Wrap
            });
        }

        private string GetCompressedDataSizeString(uint compressedDataSizeInBytes)
        {
            if (compressedDataSizeInBytes == 4294967295)
            {
                return "N/A";
            }
            float compressedDataSizeInKiloBytes = (float)compressedDataSizeInBytes / 1000.0f;
            return $"{compressedDataSizeInKiloBytes}KB";
        }

        private void GenerateNullData(FileExplorerItemViewModel selectedItem)
        {
            DataPanel.Children.Clear();
            DataPanel.Children.Add(new TextBlock
            {
                Inlines =
                {
                    new Run {Text = "Name: ", FontWeight = FontWeights.Bold},
                    new Run {Text = selectedItem.ShortName},
                    new LineBreak()
                },
                TextWrapping = TextWrapping.Wrap
            });
        }
    }
}
