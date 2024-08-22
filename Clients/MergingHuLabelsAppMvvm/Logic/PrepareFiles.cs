using MergingHuLabelsAppMvvm.Models;
using Microsoft.Win32;
using NLog;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MergingHuLabelsAppMvvm.Logic
{
    public class PrepareFiles
    {
        private string DefaultSavePath { get; set; } = "";
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private GetVertical _getVertical;
        private GeneratorLabel _generatorLabel;

        private List<(LabelsData TopLabel, LabelsData BottomLabel)> Labels { get; set; } = new List<(LabelsData TopLabel, LabelsData BottomLabel)>();
        public string MergedPDF { get; set; } = "";
        public List<string> FilesToProcessList { get; private set; } = new List<string>();
        public List<string> NoMatchDelivery { get; set; } = new List<string>();
        public List<DirectoryInfo> ListOfDirectories = new List<DirectoryInfo>();

        //PrepareLabelData prepareLabelData = new PrepareLabelData();

        //PrintersConfig printersConfig = new PrintersConfig();
        public PrepareFiles(GetVertical getVertical, GeneratorLabel generatorLabel)
        {
            _getVertical = getVertical;
            _generatorLabel = generatorLabel;
            CheckDirectory();
        }
        public void GetFiles()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                string directoryPath = Path.GetDirectoryName(dialog.FileName);

                DefaultSavePath = Path.Combine(directoryPath, $"Etykiety\\");

                if(!Directory.Exists(DefaultSavePath)) 
                    Directory.CreateDirectory(DefaultSavePath);

                FilesToProcessList = dialog.FileNames.ToList();


                MessageBox.Show($"Wczytano '{dialog.FileNames.Count()}' etykiet pdf.");
                Logger.Info($"Wczytano '{dialog.FileNames.Count()}' etykiet pdf.");
                
            }
            
        }
        public void GetFilesFromPath(string folderPath)
        { 
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            FileInfo[] pdfFiles = directoryInfo.GetFiles("*.pdf");

            DefaultSavePath = Path.Combine(folderPath, $"Etykiety\\");

            if (!Directory.Exists(DefaultSavePath))
                Directory.CreateDirectory(DefaultSavePath);
            FilesToProcessList.Clear();
            foreach (var item in pdfFiles)
            {
                FilesToProcessList.Add(item.FullName);
            }
        }
        //List<DirectoryInfo> directoryList = new List<DirectoryInfo>();
        public void CheckDirectory()
        {
            var defaultPath = @"";
            var directoryList = new DirectoryInfo(defaultPath).GetDirectories();
            var except = new List<DirectoryInfo>();
            foreach (var directory in directoryList)
            {
                //directory.Exists
                var subdirectories = new DirectoryInfo(directory.FullName).GetDirectories();
                if (subdirectories.Any())
                except.Add(directory);
            }

            //var x = directoryList.Except(except);

            ListOfDirectories.Clear();
            ListOfDirectories.AddRange(directoryList.Except(except));
            
        }

        public List<LabelsData> ProcessingMergingLabels()
        {
            var namesFromFile = _getVertical.GetFileNames(FilesToProcessList);

            var onlyLabelWithVert = namesFromFile
                                        .Where(x => x.FullVertical != null)
                                        .OrderBy(x=> x.FullVertical)
                                        .ToList();
            
            NoMatchDelivery = _getVertical.NoMatchDelivery;

            if (NoMatchDelivery.Any())
            {
                string x = "";
                foreach (var delivery in NoMatchDelivery)
                {
                    x += delivery + "\n";
                }
                File.WriteAllText
                (DefaultSavePath+"file.txt", x);
                
                System.Windows.Clipboard.SetText(x);
            }

            Labels = Enumerable.Range(0, (onlyLabelWithVert.Count + 1) / 2)
                       .Select(i => (TopLabel: i * 2 < onlyLabelWithVert.Count ? onlyLabelWithVert[i * 2] : null,
                       BottomLabel: i * 2 + 1 < onlyLabelWithVert.Count ? onlyLabelWithVert[i * 2 + 1] : null))
                       .ToList();

            string batch = onlyLabelWithVert.FirstOrDefault().BatchNumber.ToString();
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_hh_mm_ss");

            MergedPDF = DefaultSavePath + $"EtykietaPak_{batch}_{timestamp}.pdf";

            _generatorLabel.GenerateLabelFile(MergedPDF, Labels);

            return namesFromFile;
        }

        public void OpenPdfFile()
        {
            if (File.Exists(MergedPDF))
                Process.Start(new ProcessStartInfo
                {
                    FileName = MergedPDF,
                    UseShellExecute = true
                });
        }

        //public void GenerateLabelExecute()
        //{
        //    if (!Directory.Exists(DefaultSavePath))
        //    {
        //        Directory.CreateDirectory(DefaultSavePath);
        //    }

        //    var newList = prepareLabelData.GetFileNames(FilesToProcessList);
        //    prepareLabelData.SearchInDatabase(newList);

        //    var sortedList = newList.OrderBy(x => x.VerticalNumber).ToList();

        //    string batch = sortedList.FirstOrDefault().BatchNumber;
        //    string timestamp = DateTime.Now.ToString("yyyy-MM-dd_hh_mm_ss");

        //    //Labels = Enumerable.Range(0, sortedList.Count +1 / 2)
        //    //           .Select(i => (TopLabel: sortedList[i * 2], BottomLabel: sortedList[i * 2 + 1]))
        //    //           .ToList();
        //    Labels = Enumerable.Range(0, (sortedList.Count + 1) / 2)
        //               .Select(i => (TopLabel: i * 2 < sortedList.Count ? sortedList[i * 2] : null,
        //               BottomLabel: i * 2 + 1 < sortedList.Count ? sortedList[i * 2 + 1] : null))
        //               .ToList();

        //    MergedPDF = DefaultSavePath + $"EtykietaPak_{batch}_{timestamp}.pdf";

        //    GenerateLabelFile(MergedPDF);

        //    MessageBox.Show($"Scalono '{sortedList.Count()}' etykiet \n{DefaultSavePath}\n");
        //}
    }
}
