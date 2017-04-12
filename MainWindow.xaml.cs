using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System;
using System.IO;
using System.Collections.ObjectModel;
using SvgConverter.Properties;
using System.Diagnostics;
using System.Resources;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Windows.Controls;

namespace SvgConverter
{
    /// <summary>
    /// SVG Splash & Icons: designed by {dimitry-miroliubov} from Flaticon
    ///                     http://www.flaticon.com/authors/dimitry-miroliubov
    ///                     http://www.flaticon.com/free-icon/svg_337954#term=SVG&page=1&position=2
    /// </summary>
    
    /// <summary>
    /// PNG to Icon
    /// http://convertico.com/
    /// </summary>

    public partial class MainWindow : Window
    {
        ObservableCollection<FileItem> _svgList = new ObservableCollection<FileItem>();
        ObservableCollection<FileItem> _convertedList = new ObservableCollection<FileItem>();

        public string InkscapePath
        {
            get
            {
                string exePath = typeof(MainWindow).Assembly.Location;
                string binFolderPath = Path.Combine(Path.GetDirectoryName(exePath), "Inkscape");

                return binFolderPath;
            }
        }

        public string ZipFilePath
        {
            get
            {
                string exePath = typeof(MainWindow).Assembly.Location;
                string zipFilePath = Path.Combine(Path.GetDirectoryName(exePath), "Inkscape.zip");

                return zipFilePath;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            this.lvSVG.ItemsSource = _svgList;
            this.lvConverted.ItemsSource = _convertedList;

            LoadFromContext();

            ExtractResources();
        }

        private void LoadFromContext()
        {
            if (string.IsNullOrEmpty(Settings.Default.Path) == false)
            {
                this.txtFolderPath.Text = Settings.Default.Path;
            }
        }

        void SaveToContext()
        {
            Settings.Default.Path = this.txtFolderPath.Text;
            Settings.Default.Save();
        }

        private void ExtractResources()
        {
            // https://github.com/icsharpcode/SharpZipLib/wiki/Zip-Samples#unpack-a-zip-using-zipinputstream-eg-for-unseekable-input-streams
            if (Directory.Exists(InkscapePath) == false)
            {
                using (FileStream stream = File.OpenRead(ZipFilePath))
                {
                    ZipInputStream zipInputStream = new ZipInputStream(stream);
                    ZipEntry entry = zipInputStream.GetNextEntry();
                    while (entry != null)
                    {
                        string entryFileName = entry.Name;

                        byte[] buffer = new byte[4096];     // 4K is optimum

                        String fullZipToPath = Path.Combine(InkscapePath, entryFileName);
                        string directoryName = Path.GetDirectoryName(fullZipToPath);
                        if (directoryName.Length > 0)
                            Directory.CreateDirectory(directoryName);

                        using (FileStream streamWriter = File.Create(fullZipToPath))
                        {
                            StreamUtils.Copy(zipInputStream, streamWriter, buffer);
                        }

                        entry = zipInputStream.GetNextEntry();
                    }
                }
            }
        }

        private void txtFolderPathChanged(object sender, RoutedEventArgs e)
        {
            string folderPath = txtFolderPath.Text;

            RefreshFolderList(folderPath);
        }

        private void RefreshFolderList(string folderPath)
        {
            if (Directory.Exists(folderPath) == true)
            {
                ListFiles(_svgList, folderPath, "*.svg");
                ListFiles(_convertedList, folderPath, "*.wmf");
            }
        }

        private void btnFolderClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                EnsurePathExists = true,
                EnsureFileExists = true,
                AllowNonFileSystemItems = false,
                IsFolderPicker = true
            };

            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                txtFolderPath.Text = dialog.FileName;

                SaveToContext();
                RefreshFolderList(dialog.FileName);
            }
        }

        private void ListFiles(ObservableCollection<FileItem> list, string filePath, string pattern)
        {
            list.Clear();

            foreach (string item in Directory.GetFiles(filePath, pattern))
            {
                FileInfo fi = new FileInfo(item);

                FileItem fileItem = new FileItem { Path = item, Size = fi.Length };
                list.Add(fileItem);
            }
        }

        private void lvSVGSelectionChanged(object sender, RoutedEventArgs e)
        {
            btnConvert.IsEnabled = lvSVG.SelectedItems.Count != 0;
        }
            
        private void btnConvertClicked(object sender, RoutedEventArgs e)
        {
            foreach (FileItem fileItem in this.lvSVG.SelectedItems)
            {
                CovnertFile(fileItem.Path);
            }

            RefreshFolderList(this.txtFolderPath.Text);
        }

        private void btnRefreshClicked(object sender, RoutedEventArgs e)
        {
            RefreshFolderList(this.txtFolderPath.Text);
        }

        void CovnertFile(string filePath)
        {
            string wmfFilePath = Path.ChangeExtension(filePath, ".wmf");

            string inkscapePath = Path.Combine(InkscapePath, "inkscape.exe");
            string arg = string.Format("\"{0}\" --export-wmf=\"{1}\"", filePath, wmfFilePath);
            ProcessStartInfo psi = new ProcessStartInfo(inkscapePath, arg);
            psi.WorkingDirectory = InkscapePath;

            try
            {
                using (Process process = Process.Start(psi))
                {
                    process.Start();
                    process.WaitForExit();
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(inkscapePath, e);
            }
        }
    }
}
