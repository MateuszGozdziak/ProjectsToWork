using DispatchListCreator.Model;

namespace DispatchListCreator.Contracts
{
    public interface ISummaryContentCollector
    {
        SummaryBatchResult CollectBatchSummary(DateTime MergeDateTimeFrom, DateTime MergeDateTimeTo, List<string> countries);
    }
}