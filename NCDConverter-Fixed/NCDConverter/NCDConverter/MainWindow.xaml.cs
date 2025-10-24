using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using RespondClient.DomiKnow.NinjaTrader;
using NinjaTrader = RespondClient.DomiKnow.NinjaTrader;
using Parquet;
using Parquet.Data;
using Parquet.Schema;

namespace NCDConverter
{
    public partial class MainWindow : Window
    {
        // Constructor
        public MainWindow()
        {
            InitializeComponent();
            
            // Set default NT8 path
            string defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "NinjaTrader 8", "db");
            
            if (Directory.Exists(defaultPath))
            {
                NinjaTrader.GlobalOptions.HistoricalDataPath = defaultPath;
            }
        }
        private List<string> selectedFiles = new List<string>();

        private void BrowseFiles_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select NCD Files",
                Filter = "NCD Files (*.ncd)|*.ncd|All Files (*.*)|*.*",
                Multiselect = true,
                CheckFileExists = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFiles = openFileDialog.FileNames.ToList();
                UpdateFileList();
                UpdateConvertButton();
            }
        }

        private void BrowseOutputPath_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Output Folder for Parquet Files",
                ShowNewFolderButton = true
            };

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OutputPathTextBox.Text = folderDialog.SelectedPath;
                UpdateConvertButton();
            }
        }

        private void UpdateFileList()
        {
            FileListBox.Items.Clear();
            
            foreach (var file in selectedFiles)
            {
                FileListBox.Items.Add(Path.GetFileName(file));
            }

            FilesSelectedLabel.Text = $"{selectedFiles.Count} file(s) selected";
        }

        private void UpdateConvertButton()
        {
            ConvertButton.IsEnabled = selectedFiles.Count > 0 && 
                                      !string.IsNullOrWhiteSpace(OutputPathTextBox.Text);
        }

        private async void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFiles.Count == 0)
            {
                MessageBox.Show("Please select at least one NCD file.", "No Files Selected", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string outputPath = OutputPathTextBox.Text;
            if (string.IsNullOrWhiteSpace(outputPath) || !Directory.Exists(outputPath))
            {
                MessageBox.Show("Please select a valid output folder.", "Invalid Output Path", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Disable UI during conversion
            ConvertButton.IsEnabled = false;
            BrowseFilesButton.IsEnabled = false;
            BrowseOutputButton.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.Value = 0;

            try
            {
                await Task.Run(() => ConvertFilesAsync(outputPath));
                MessageBox.Show("Conversion completed successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during conversion:\n{ex.Message}", "Conversion Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                AppendLog($"ERROR: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                // Re-enable UI
                ConvertButton.IsEnabled = true;
                BrowseFilesButton.IsEnabled = true;
                BrowseOutputButton.IsEnabled = true;
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private async Task ConvertFilesAsync(string outputPath)
        {
            int totalFiles = selectedFiles.Count;
            int currentFile = 0;

            foreach (var filePath in selectedFiles)
            {
                currentFile++;
                string fileName = Path.GetFileName(filePath);
                string outputFileName = Path.GetFileNameWithoutExtension(filePath) + ".parquet";
                string outputFilePath = Path.Combine(outputPath, outputFileName);

                Dispatcher.Invoke(() =>
                {
                    AppendLog($"\n[{currentFile}/{totalFiles}] Processing: {fileName}");
                    ProgressBar.Value = (currentFile - 1) * 100.0 / totalFiles;
                });

                try
                {
                    // Determine file type from path
                    NCDFileType fileType = DetermineFileType(filePath);
                    
                    // Count records first
                    int recordCount = CountRecords(filePath, fileType);
                    
                    Dispatcher.Invoke(() =>
                    {
                        AppendLog($"  Records to convert: {recordCount:N0}");
                    });

                    // Convert to Parquet
                    await ConvertNCDToParquetAsync(filePath, outputFilePath, fileType);

                    Dispatcher.Invoke(() =>
                    {
                        AppendLog($"  ✓ Saved: {outputFileName}");
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        AppendLog($"  ✗ ERROR: {ex.Message}");
                    });
                }
            }

            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = 100;
                AppendLog($"\n=== Conversion Complete ===\n");
            });
        }

        private NCDFileType DetermineFileType(string filePath)
        {
            string pathLower = filePath.ToLower();
            
            if (pathLower.Contains("\\minute\\"))
                return NCDFileType.Minute;
            else if (pathLower.Contains("\\tick\\"))
                return NCDFileType.Tick;
            else if (pathLower.Contains("\\day\\"))
                return NCDFileType.Day;
            
            // Default to Minute if can't determine
            return NCDFileType.Minute;
        }

        private int CountRecords(string filePath, NCDFileType fileType)
        {
            int count = 0;
            
            try
            {
                INCDFile file = null;
                
                if (fileType == NCDFileType.Minute)
                    file = new NCDMinuteFile(filePath);
                else if (fileType == NCDFileType.Tick)
                    file = new NCDTickFile(filePath);
                else
                    throw new NotSupportedException($"File type {fileType} not supported");

                while (!file.EndOfFile)
                {
                    file.ReadNextRecord();
                    count++;
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    AppendLog($"  Warning: Could not count records - {ex.Message}");
                });
            }

            return count;
        }

        private async Task ConvertNCDToParquetAsync(string ncdPath, string parquetPath, NCDFileType fileType)
        {
            if (fileType == NCDFileType.Minute)
                await ConvertMinuteFileAsync(ncdPath, parquetPath);
            else if (fileType == NCDFileType.Tick)
                await ConvertTickFileAsync(ncdPath, parquetPath);
            else
                throw new NotSupportedException($"File type {fileType} not supported");
        }

        private async Task ConvertMinuteFileAsync(string ncdPath, string parquetPath)
        {
            var file = new NCDMinuteFile(ncdPath);
            var records = new List<MinuteRecord>();

            while (!file.EndOfFile)
            {
                var record = (MinuteRecord)file.ReadNextRecord();
                records.Add(record);
            }

            // Write to Parquet using Parquet.Net
            await WriteMinuteRecordsToParquetAsync(records, parquetPath);
        }

        private async Task ConvertTickFileAsync(string ncdPath, string parquetPath)
        {
            var file = new NCDTickFile(ncdPath);
            var records = new List<TickRecord>();

            while (!file.EndOfFile)
            {
                var record = (TickRecord)file.ReadNextRecord();
                records.Add(record);
            }

            // Write to Parquet using Parquet.Net
            await WriteTickRecordsToParquetAsync(records, parquetPath);
        }

        private async Task WriteMinuteRecordsToParquetAsync(List<MinuteRecord> records, string filePath)
        {
            // Create schema
            var schema = new ParquetSchema(
                new DataField<DateTime>("timestamp"),
                new DataField<double>("open"),
                new DataField<double>("high"),
                new DataField<double>("low"),
                new DataField<double>("close"),
                new DataField<long>("volume")
            );

            // Create data columns
            var timestampColumn = new DataColumn(
                (DataField)schema[0],
                records.Select(r => r.DateTime).ToArray());
            
            var openColumn = new DataColumn(
                (DataField)schema[1],
                records.Select(r => r.Open).ToArray());
            
            var highColumn = new DataColumn(
                (DataField)schema[2],
                records.Select(r => r.High).ToArray());
            
            var lowColumn = new DataColumn(
                (DataField)schema[3],
                records.Select(r => r.Low).ToArray());
            
            var closeColumn = new DataColumn(
                (DataField)schema[4],
                records.Select(r => r.Close).ToArray());
            
            var volumeColumn = new DataColumn(
                (DataField)schema[5],
                records.Select(r => r.Volume).ToArray());

            using (var fileStream = File.Create(filePath))
            using (var parquetWriter = await ParquetWriter.CreateAsync(schema, fileStream))
            using (var groupWriter = parquetWriter.CreateRowGroup())
            {
                await groupWriter.WriteColumnAsync(timestampColumn);
                await groupWriter.WriteColumnAsync(openColumn);
                await groupWriter.WriteColumnAsync(highColumn);
                await groupWriter.WriteColumnAsync(lowColumn);
                await groupWriter.WriteColumnAsync(closeColumn);
                await groupWriter.WriteColumnAsync(volumeColumn);
            }
        }

        private async Task WriteTickRecordsToParquetAsync(List<TickRecord> records, string filePath)
        {
            // Create schema
            var schema = new ParquetSchema(
                new DataField<DateTime>("timestamp"),
                new DataField<double>("price"),
                new DataField<double>("bid"),
                new DataField<double>("ask"),
                new DataField<long>("volume")
            );

            // Create data columns
            var timestampColumn = new DataColumn(
                (DataField)schema[0],
                records.Select(r => r.DateTime).ToArray());
            
            var priceColumn = new DataColumn(
                (DataField)schema[1],
                records.Select(r => r.Price).ToArray());
            
            var bidColumn = new DataColumn(
                (DataField)schema[2],
                records.Select(r => r.Bid).ToArray());
            
            var askColumn = new DataColumn(
                (DataField)schema[3],
                records.Select(r => r.Ask).ToArray());
            
            var volumeColumn = new DataColumn(
                (DataField)schema[4],
                records.Select(r => r.Volume).ToArray());

            using (var fileStream = File.Create(filePath))
            using (var parquetWriter = await ParquetWriter.CreateAsync(schema, fileStream))
            using (var groupWriter = parquetWriter.CreateRowGroup())
            {
                await groupWriter.WriteColumnAsync(timestampColumn);
                await groupWriter.WriteColumnAsync(priceColumn);
                await groupWriter.WriteColumnAsync(bidColumn);
                await groupWriter.WriteColumnAsync(askColumn);
                await groupWriter.WriteColumnAsync(volumeColumn);
            }
        }

        private void AppendLog(string message)
        {
            LogTextBox.AppendText(message + "\n");
            LogTextBox.ScrollToEnd();
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
        }

        private void OutputPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateConvertButton();
        }
    }
}
