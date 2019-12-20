using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using OGE.Editor.Events;
using OGE.ViewModels;
using OGE.ViewModels.FileExplorer;
using ReactiveUI;
using RfgTools.Formats.Asm;
using RfgTools.Formats.Packfiles;
using RfgTools.Formats.Textures;

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
            //Ensure file is loaded and read so panel info is available on first open
            if (selectedItem.File == null)
                selectedItem.GetCacheFile(true, true);

            string extension = Path.GetExtension(selectedItem.Filename);
            DataPanel.Children.Clear();

            if (extension == ".vpp_pc")
            {
                var packfile = selectedItem.File?.PackfileData;
                if (packfile == null)
                    GenerateNullData(selectedItem);
                else
                    GeneratePackfileData(packfile);
            }
            else if (extension == ".str2_pc")
            {
                var container = selectedItem.File?.PackfileData;
                if (container == null)
                    GenerateNullData(selectedItem);
                else
                    GeneratePackfileData(container);
            }
            else if (extension == ".asm_pc")
            {
                var asmData = selectedItem.File?.AsmData;
                if (asmData == null)
                    GenerateNullData(selectedItem);
                else
                    GenerateAsmData(asmData);
            }
            else if (extension == ".cpeg_pc" || extension == ".cvbm_pc")
            {
                var pegData = selectedItem.File?.PegData;
                if(pegData == null)
                    GenerateNullData(selectedItem);
                else
                    GenerateTextureData(pegData);
            }
            else if (extension == ".rfgzone_pc")
                GenerateNullData(selectedItem);
            else if (extension == ".layer_pc")
                GenerateNullData(selectedItem);
            else if (extension == ".scriptx") 
                GenerateNullData(selectedItem);
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
                    new Run {Text = selectedItem.Filename},
                    new LineBreak()
                },
                TextWrapping = TextWrapping.Wrap
            });
        }

        private void GeneratePackfileData(Packfile packfile)
        {
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

        private void GenerateAsmData(AsmFile asmData)
        {

        }

        private void GenerateTextureData(PegFile pegData)
        {
            //Add peg info
            DataPanel.Children.Add(new TextBlock
            {
                Inlines =
                {
                    new Run
                    {
                        Text = "Peg info", FontWeight = FontWeights.Bold,
                        FontSize = 14
                    },
                    new LineBreak(),

                    new Run {Text = "Name: ", FontWeight = FontWeights.Bold},
                    new Run {Text = pegData.cpuFileName},
                    new LineBreak(),

                    new Run {Text = "Version: ", FontWeight = FontWeights.Bold},
                    new Run {Text = pegData.Version.ToString()},
                    new LineBreak(),

                    new Run {Text = "Platform: ", FontWeight = FontWeights.Bold},
                    new Run {Text = pegData.Platform.ToString()},
                    new LineBreak(),

                    new Run {Text = "Cpeg size: ", FontWeight = FontWeights.Bold},
                    new Run {Text = pegData.DirectoryBlockSize.ToString()},
                    new LineBreak(),

                    new Run {Text = "Gpeg size: ", FontWeight = FontWeights.Bold},
                    new Run {Text = pegData.DataBlockSize.ToString()},
                    new LineBreak(),

                    new Run {Text = "Number of sub-textures: ", FontWeight = FontWeights.Bold},
                    new Run {Text = pegData.NumberOfBitmaps.ToString()},
                    new LineBreak(),

                    new Run {Text = "Flags: ", FontWeight = FontWeights.Bold},
                    new Run {Text = pegData.Flags.ToString()},
                    new LineBreak(),

                    new Run {Text = "Total entries: ", FontWeight = FontWeights.Bold},
                    new Run {Text = pegData.TotalEntries.ToString()},
                    new LineBreak(),

                    new Run {Text = "Align value: ", FontWeight = FontWeights.Bold},
                    new Run {Text = pegData.AlignValue.ToString()},
                    new LineBreak()
                },
                TextWrapping = TextWrapping.Wrap
            });

            //Add sub-texture info
            var subTextures = new TreeView();
            var rootNode = new TreeViewItem {Header = "Texture info", IsExpanded = true};

            foreach(var entry in pegData.Entries)
            {
                var entryNode = new TreeViewItem {Header = entry.Name, IsExpanded = true};
                //Add basic values
                entryNode.Items.Add(new TextBlock
                {
                    Margin = new Thickness(0.0, 0.0, 0.0, -15.0),
                    Inlines =
                    {
                        new Run {Text = "Width: ", FontWeight = FontWeights.Bold},
                        new Run {Text = entry.width.ToString()},
                        new LineBreak(),

                        new Run {Text = "Height: ", FontWeight = FontWeights.Bold},
                        new Run {Text = entry.height.ToString()},
                        new LineBreak(),

                        new Run {Text = "Format: ", FontWeight = FontWeights.Bold},
                        new Run {Text = entry.bitmap_format.ToString()},
                        new LineBreak()
                    }
                });

                //Add advanced values //Todo: Add some helpers to shorten this code
                entryNode.Items.Add(new TreeViewItem
                {
                    Header = "Advanced values",
                    Items =
                    {
                        new TextBlock
                            {
                                Inlines =
                                {
                                    new Run {Text = "Data offset: ", FontWeight = FontWeights.Bold},
                                    new Run {Text = entry.data.ToString()},
                                    new LineBreak(),

                                    new Run {Text = "Source width: ", FontWeight = FontWeights.Bold},
                                    new Run {Text = entry.source_width.ToString()},
                                    new LineBreak(),

                                    new Run {Text = "Source height: ", FontWeight = FontWeights.Bold},
                                    new Run {Text = entry.source_height.ToString()},
                                    new LineBreak(),

                                    new Run {Text = "Anim tiles width: ", FontWeight = FontWeights.Bold},
                                    new Run {Text = entry.anim_tiles_width.ToString()},
                                    new LineBreak(),

                                    new Run {Text = "Anim tiles height: ", FontWeight = FontWeights.Bold},
                                    new Run {Text = entry.anim_tiles_height.ToString()},
                                    new LineBreak(),

                                    new Run {Text = "Number of frames: ", FontWeight = FontWeights.Bold},
                                    new Run {Text = entry.num_frames.ToString()},
                                    new LineBreak(),

                                    new Run {Text = "Flags: ", FontWeight = FontWeights.Bold},
                                    new Run {Text = entry.flags.ToString()},
                                    new LineBreak(),

                                    new Run {Text = "Filename hash: ", FontWeight = FontWeights.Bold},
                                    new Run {Text = entry.filename.ToString()},
                                    new LineBreak(),

                                    new Run {Text = "FPS: ", FontWeight = FontWeights.Bold},
                                    new Run {Text = entry.fps.ToString()},
                                    new LineBreak(),

                                    new Run {Text = "Mip levels: ", FontWeight = FontWeights.Bold},
                                    new Run {Text = entry.mip_levels.ToString()},
                                    new LineBreak(),

                                    new Run {Text = "Frame size: ", FontWeight = FontWeights.Bold},
                                    new Run {Text = entry.frame_size.ToString()},
                                    new LineBreak(),

                                    new Run {Text = "Next: ", FontWeight = FontWeights.Bold},
                                    new Run {Text = entry.next.ToString()},
                                    new LineBreak(),

                                    new Run {Text = "Previous: ", FontWeight = FontWeights.Bold},
                                    new Run {Text = entry.previous.ToString()},
                                    new LineBreak(),

                                    new Run {Text = "Cache 0: ", FontWeight = FontWeights.Bold},
                                    new Run {Text = entry.cache0.ToString()},
                                    new LineBreak(),

                                    new Run {Text = "Cache 1: ", FontWeight = FontWeights.Bold},
                                    new Run {Text = entry.cache1.ToString()},
                                    new LineBreak(),
                                }
                            }
                    }
                });
                rootNode.Items.Add(entryNode);
            }

            subTextures.Items.Add(rootNode);
            DataPanel.Children.Add(subTextures);
        }
    }
}
