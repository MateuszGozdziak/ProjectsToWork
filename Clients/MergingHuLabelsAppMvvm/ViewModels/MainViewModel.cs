using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MergingHuLabelsAppMvvm.Contracts;
using MergingHuLabelsAppMvvm.Logic;
using MergingHuLabelsAppMvvm.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace MergingHuLabelsAppMvvm.ViewModels
{
    public class MainViewModel : ObservableRecipient, IMainViewModel, INotifyPropertyChanged
    {
        private readonly PrepareFiles _prepareFiles;
        private readonly PrintersConfig _printersConfig;
        private ObservableCollection<string> _deliveriesWithoutMatch;
        private ObservableCollection<DirectoryInfo> _listOfDirectories;
        private int _selectedPrinterIndex;
        private int _selectedDirectoryIndex;

        public RelayCommand OpenFileDialogCommand { get; set; }
        public RelayCommand GenerateLabelsCommand { get; set; }
        public RelayCommand OpenPdfInBroswerCommand { get; set; }
        public RelayCommand SendToPrinterCommand { get; set; }
        public RelayCommand OpenExtraPrintCommand { get; set; }

        public MainViewModel(PrepareFiles prepareFiles, PrintersConfig printersConfig)
        {
            _prepareFiles = prepareFiles;
            _printersConfig = printersConfig;
            Init();
        }

        private void Init()
        {
            OpenFileDialogCommand = new RelayCommand(StartOpenDialogFile);
            GenerateLabelsCommand = new RelayCommand(GenerateLabelsCommandExecute);
            OpenPdfInBroswerCommand = new RelayCommand(_prepareFiles.OpenPdfFile);
            DeliveriesWithoutMatch = new ObservableCollection<string>();
            ListOfDirectories = new ObservableCollection<DirectoryInfo>();
            SendToPrinterCommand = new RelayCommand(SendToPrinter);
            OpenExtraPrintCommand = new RelayCommand(OpenExtraPrintExecute);
            SelectedPrinterIndex = 1;
            SelectedDirectoryIndex = -1;
            SetList();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler? CanExecuteChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ObservableCollection<string> DeliveriesWithoutMatch
        {
            get { return _deliveriesWithoutMatch; }
            set
            {
                _deliveriesWithoutMatch = value;
                SetProperty(ref _deliveriesWithoutMatch, value, true);
            }
        }
        public ObservableCollection<DirectoryInfo> ListOfDirectories
        {
            get { return _listOfDirectories; }
            set
            {
                _listOfDirectories = value;
                SetProperty(ref _listOfDirectories, value, true);
            }
        }
        public void SetList()
        {
            ListOfDirectories.Clear();
            OnPropertyChanged(nameof(ListOfDirectories));

            foreach (var item in _prepareFiles.ListOfDirectories)
            {
                ListOfDirectories.Add(item);
            }
            OnPropertyChanged(nameof(ListOfDirectories));
        }
        public int SelectedPrinterIndex
        {
            get => _selectedPrinterIndex;
            set => SetProperty(ref _selectedPrinterIndex, value, true);
        }
        public int SelectedDirectoryIndex
        {
            get => _selectedDirectoryIndex;
            set => SetProperty(ref _selectedDirectoryIndex, value, true);
        }
        public void StartOpenDialogFile()
        {
            _prepareFiles.GetFiles();
        }
        public void GenerateLabelsCommandExecute()
        {
            var xx = SelectedDirectoryIndex;
            if (SelectedDirectoryIndex != -1)
            {
                var selectedPath = ListOfDirectories[SelectedDirectoryIndex];
                if (selectedPath != null)
                {
                    _prepareFiles.GetFilesFromPath(selectedPath.FullName);
                }
                else
                    return;
            }

            var x = _prepareFiles.ProcessingMergingLabels();
            DeliveriesWithoutMatch.Clear();
            foreach (var item in _prepareFiles.NoMatchDelivery)
            {
                DeliveriesWithoutMatch.Add(item);
            }
            OnPropertyChanged(nameof(DeliveriesWithoutMatch));
            SelectedDirectoryIndex = -1;
            SetList();
        }
        public void OpenExtraPrintWindow()
        {

        }
        private void SendToPrinter()
        {
            if (string.IsNullOrEmpty(_prepareFiles.MergedPDF))
            {
                MessageBox.Show("Proszę wybrać drukarkę.");
                return;
            }

            switch (SelectedPrinterIndex)
            {
                case 0: // Adam Printer
                    _printersConfig.adamPrinterSend(_prepareFiles.MergedPDF);
                    break;
                case 1: // Ewa Printer
                    _printersConfig.ewaPrinterSend(_prepareFiles.MergedPDF);
                    break;
                case 2: // Fiery Printer
                    _printersConfig.fieryPrinterSend(_prepareFiles.MergedPDF);
                    break;
            }
        }
        private void OpenExtraPrintExecute()
        {
            ExtraPrint extraPrintControl = new ExtraPrint();
        }
    }
}
