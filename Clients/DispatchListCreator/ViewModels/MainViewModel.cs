using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DispatchListCreator.Contracts;
using DispatchListCreator.Logic;
using DispatchListCreator.Model;
using DMS.Database.DbTransporty;
using iText.Layout.Element;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrackingPlPrototype.Entities.SBGV_PL;
using TrackingPlPrototype.Entities.SGBV_Cz;
using TrackingPlPrototype.Entities.SGBV_HU;
using TrackingPlPrototype.Entities.SGBV_SK;

namespace DispatchListCreator.ViewModels
{
    public class MainViewModel : ObservableRecipient, IMainViewModel, INotifyPropertyChanged
    {
        private ISummaryContentCollector _summaryContentWorker;
        private IExportFileGenerator _csvGenerator;
        private SummaryContentCollectorAsync _summaryContentCollectorAsync;
        private List<CsvValues> _csvValues;
        private ObservableCollection<SummaryBatches> _summaryBatches;
        private ObservableCollection<Market> _markets;
        private List<PrintingModel> _printingModel;
        private List<string> _selectedCountries;
        private DateTime _mergeDateTimeFrom;
        private DateTime _mergeDateTimeTo;

        private bool _isPlChecked;
        private bool _isCzChecked;
        private bool _isSkChecked;
        private bool _isHuChecked;
        public RelayCommand GetDispatchDataCommand { get; set; }
        public RelayCommand GenerateCsvFileCommand { get; set; }
        public AsyncRelayCommand GetDispatchDataCommandAsync { get; set; }

        public RelayCommand<object> MarketsSelectionChangedCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler? CanExecuteChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainViewModel(ISummaryContentCollector summaryContent, IExportFileGenerator csvGenerator, SummaryContentCollectorAsync summaryContentCollectorAsync)
        {
            _summaryContentWorker = summaryContent;
            _csvGenerator = csvGenerator;
            _summaryContentCollectorAsync = summaryContentCollectorAsync;
            Init();
            MarketsSelectionChangedCommand = new RelayCommand<object>(MarketsSelectionChangedCommandExecute);


        }
        private void MarketsSelectionChangedCommandExecute(object param)
        {
            var listView = param as System.Windows.Controls.ListView;
            if (listView != null)
            {
                foreach (var item in listView.SelectedItems)
                {
                    Markets.Add((Market)item);
                }
            }
        }
        private void Init()
        {

            SummaryBatches = new ObservableCollection<SummaryBatches>();
            Markets = new ObservableCollection<Market>();
            GetDispatchDataCommand = new RelayCommand(GetDispatchDataCommandExecute);
            GenerateCsvFileCommand = new RelayCommand(GenerateCsvFileCommandExecute);
            GetDispatchDataCommandAsync = new AsyncRelayCommand(GetDispatchDataCommandExecuteAsync);

            _mergeDateTimeFrom = DateTime.Today;
            _mergeDateTimeTo = DateTime.Now;
            SelectedCountries = new List<string>();
            IsCzChecked = true;
            IsPlChecked = true;
            IsSkChecked = true;
            IsHuChecked = true;
                        

        }
        
        public List<string> SelectedCountries
        {
            get => _selectedCountries;
            private set
            {
                if (_selectedCountries != value)
                {
                    _selectedCountries = value;
                    OnPropertyChanged(nameof(SelectedCountries));
                }
            }
        }

        public bool IsPlChecked
        {
            get => _isPlChecked;
            set
            {
                if (_isPlChecked != value)
                {
                    _isPlChecked = value;
                    OnPropertyChanged(nameof(IsPlChecked));
                    UpdateCountrySelect();
                }
            }
        }
        public bool IsCzChecked
        {
            get => _isCzChecked;
            set
            {
                if (_isCzChecked != value)
                {
                    _isCzChecked = value;
                    OnPropertyChanged(nameof(IsCzChecked));
                    UpdateCountrySelect();
                }
            }
        }

        public bool IsSkChecked
        {
            get => _isSkChecked;
            set
            {
                if (_isSkChecked != value)
                {
                    _isSkChecked = value;
                    OnPropertyChanged(nameof(IsSkChecked));
                    UpdateCountrySelect();
                }
            }
        }
        public bool IsHuChecked
        {
            get => _isHuChecked;
            set
            {
                if (_isHuChecked != value)
                {
                    _isHuChecked = value;
                    OnPropertyChanged(nameof(IsHuChecked));
                    UpdateCountrySelect();
                }
            }
        }
        public ObservableCollection<Market> Markets
        {
            get { return _markets; }
            set
            {
                //_summaryBatches = value;
                SetProperty(ref _markets, value, true);
            }
        }
        public ObservableCollection<SummaryBatches> SummaryBatches
        {
            get { return _summaryBatches; }
            set
            {
                //_summaryBatches = value;
                SetProperty(ref _summaryBatches, value, true);
            }
        }

        public DateTime MergeDateTimeFrom
        {
            get { return _mergeDateTimeFrom; }
            set
            {
                //_mergeDateTimeFrom = value;
                SetProperty(ref _mergeDateTimeFrom, value, true);
            }
        }

        public DateTime MergeDateTimeTo
        {
            get { return _mergeDateTimeTo; }
            set
            {
                //_mergeDateTimeTo = value;
                SetProperty(ref _mergeDateTimeTo, value, true);

            }
        }
        private void UpdateCountrySelect()
        {
            SelectedCountries.Clear();
            if (IsPlChecked) SelectedCountries.Add("PL");
            if (IsCzChecked) SelectedCountries.Add("CZ");
            if (IsSkChecked) SelectedCountries.Add("SK");
            if (IsHuChecked) SelectedCountries.Add("HU");

            OnPropertyChanged(nameof(SelectedCountries));
        }
        public void GetDispatchDataCommandExecute()
        {
            SummaryBatches.Clear();

            if (!SelectedCountries.Any())
            {
                MessageBox.Show("Nie wybrano rynku!");
                return;
            }

            _summaryBatchResult = _summaryContentWorker.CollectBatchSummary(MergeDateTimeFrom, MergeDateTimeTo, SelectedCountries);
            
            foreach (var summary in _summaryBatchResult.SummaryData)
            {
                SummaryBatches.Add(summary);
            }

            OnPropertyChanged(nameof(SummaryBatches));
            
        }
        public async Task GetDispatchDataCommandExecuteAsync()
        {

            Stopwatch st = Stopwatch.StartNew();

            if (!SelectedCountries.Any())
            {
                MessageBox.Show("Nie wybrano rynku!");
                return;
            }
            SummaryBatches.Clear();
            IsLoading = true;

            _summaryBatchResult = await _summaryContentCollectorAsync.CollectBatchSummaryAsync(MergeDateTimeFrom, MergeDateTimeTo, SelectedCountries);

            foreach (var summary in _summaryBatchResult.SummaryData)
            {
                SummaryBatches.Add(summary);
            }

            OnPropertyChanged(nameof(SummaryBatches));
            IsLoading = false;
            st.Stop();
            MessageBox.Show($"{st.ElapsedMilliseconds}");
        }
        public void GenerateCsvFileCommandExecute()
        {
            _csvGenerator.GenerateCsvFile(_summaryBatchResult.CsvValuesList, _summaryBatchResult.BatchNumbers, MergeDateTimeTo,MergeDateTimeFrom);
        }

        private SummaryBatchResult _summaryBatchResult = new SummaryBatchResult();
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
    }
}
