using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.WindowsAPICodePack.Dialogs;
using OGE.Formats.Asm;

namespace OGE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AsmFile _asmFile;
        private CommonOpenFileDialog _fileBrowser;

        public MainWindow()
        {
            InitializeComponent();

            _fileBrowser = new CommonOpenFileDialog();
            _fileBrowser.Filters.Add(new CommonFileDialogFilter("Asm_pc files", "*.asm_pc"));
        }

        private void OpenAsm_OnClick(object sender, RoutedEventArgs e)
        {
            _fileBrowser.IsFolderPicker = false;
            if (_fileBrowser.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string asmFilePath = _fileBrowser.FileName;
                if (!File.Exists(asmFilePath))
                {
                    return;
                }

                _asmFile = new AsmFile(asmFilePath);
                _asmFile.Read();

                UpdateInfoList();
                CurrentFileLabel.Content = new TextBlock() {Text = _asmFile.AsmFileName};
            }
        }

        private void UpdateInfoList()
        {
            FileInfoTree.Items.Clear();

            if (_asmFile != null)
            {
                foreach (var container in _asmFile.Containers)
                {
                    var containerNode = new TreeViewItem {Header = container.Name + ".str2_pc"};

                    var wrapPanel = new WrapPanel {Margin = new Thickness(-10, 0, 0, 0), Width = 250};
                    var label = new Label {Content = "Type:"};
                    var input = new TextBox {HorizontalAlignment = HorizontalAlignment.Stretch, Width = 150};
                    wrapPanel.Children.Add(label);
                    wrapPanel.Children.Add(input);
                    containerNode.Items.Add(wrapPanel);

                    var binding = new Binding("Text") {Source = container.Type};
                    input.SetBinding(TextBox.TextProperty, binding);

                    var primitivesRootNode = new TreeViewItem { Header = "Primitives" };
                    containerNode.Items.Add(primitivesRootNode);

                    foreach (var primitive in container.Primitives)
                    {
                        var primitiveNode = new TreeViewItem {Header = primitive.Name, Margin = new Thickness(-10, 0, 0,0)};
                        primitivesRootNode.Items.Add(primitiveNode);
                    }
                    FileInfoTree.Items.Add(containerNode);
                }
            }
        }
    }
}
