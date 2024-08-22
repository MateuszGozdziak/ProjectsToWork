using DispatchListCreator.Model;

namespace DispatchListCreator.Contracts
{
    public interface IExportFileGenerator
    {
        void GenerateCsvFile(List<CsvValues> dataToWrite, List<int> batchNumbers, DateTime MergeDateTimeTo, DateTime MergeDateTimeFrom);
    }
}