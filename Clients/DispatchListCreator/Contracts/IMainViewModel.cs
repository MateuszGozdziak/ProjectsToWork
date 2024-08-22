using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using DispatchListCreator.Model;

namespace DispatchListCreator.Contracts
{
    public interface IMainViewModel
    {
        DateTime MergeDateTimeFrom { get; set; }
        DateTime MergeDateTimeTo { get; set; }
        RelayCommand GetDispatchDataCommand { get; set; }
        RelayCommand GenerateCsvFileCommand { get; set; }

        void GetDispatchDataCommandExecute();
        void GenerateCsvFileCommandExecute();
    }
}