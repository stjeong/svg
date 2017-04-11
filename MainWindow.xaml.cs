using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System;
using System.IO;
using System.Collections.ObjectModel;
using SvgConverter.Properties;
using System.Diagnostics;

namespace SvgConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<FileItem> _svgList = new ObservableCollection<FileItem>();
        ObservableCollection<FileItem> _convertedList = new ObservableCollection<FileItem>();

        public MainWindow()
        {
            InitializeComponent();

            this.lvSVG.ItemsSource = _svgList;
            this.lvConverted.ItemsSource = _convertedList;

            LoadFromContext();
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
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.EnsurePathExists = true;

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

            string exePath = typeof(MainWindow).Assembly.Location;
            string binFolderPath = Path.Combine(Path.GetDirectoryName(exePath), "Inkscape");

            string inkscapePath = Path.Combine(binFolderPath, "inkscape.exe");
            string arg = string.Format("\"{0}\" --export-wmf=\"{1}\"", filePath, wmfFilePath);
            ProcessStartInfo psi = new ProcessStartInfo(inkscapePath, arg);
            psi.WorkingDirectory = binFolderPath;

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
                throw new ApplicationException(exePath, e);
            }
        }

    }
}
