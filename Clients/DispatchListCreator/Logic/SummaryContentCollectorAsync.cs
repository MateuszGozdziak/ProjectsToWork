using DispatchListCreator.Model;
using DMS.Database.DbTransporty;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackingPlPrototype.Entities.SBGV_PL;
using TrackingPlPrototype.Entities.SGBV_Cz;
using TrackingPlPrototype.Entities.SGBV_HU;
using TrackingPlPrototype.Entities.SGBV_SK;

namespace DispatchListCreator.Logic
{
    public class SummaryContentCollectorAsync
    {
        private const string RemovedValue = "1";
        private const string HaltedValue = "2";

        private SgbvPlContext _sgbvPlContext;
        private SgbvCzContext _sgbvCzContext;
        private SgbvHuContext _sgbvHuContext;
        private SgbvContext _sgbvContext;
        private TransportyContext _transportyContext;

        public SummaryContentCollectorAsync(SgbvPlContext sgbvPlContext, SgbvCzContext sgbvCzContext, SgbvHuContext sgbvHuContext, SgbvContext sgbvContext, TransportyContext transportyContext)
        {
            _sgbvPlContext = sgbvPlContext;
            _sgbvCzContext = sgbvCzContext;
            _sgbvHuContext = sgbvHuContext;
            _sgbvContext = sgbvContext;
            _transportyContext = transportyContext;
        }

        private List<CsvValues> DataForDispatchFile(List<PrintingModel> printings)
        {
            var result = printings
                .Select(b => new CsvValues
                {
                    Delivery = b.Delivery,
                    PostalBarcode = b.PostalBarcode,
                    DispatchDate = b.SapBatchInfo
                            .Where(x => x.SapBatchNumber == b.SapBatchNumber)
                            .Select(x => x.DispatchDate)
                            .FirstOrDefault()

                }).ToList();
            return result;
        }

        public async Task<SummaryBatchResult> CollectBatchSummaryAsync(
            DateTime MergeDateTimeFrom,
            DateTime MergeDateTimeTo,
            List<string> countries)
        {
            var combinedResults = new List<PrintingModel>();

            var dataFromTransporty = await _transportyContext.Batches
                .Where(d =>
                         d.MergeDateTime >= MergeDateTimeFrom && d.MergeDateTime <= MergeDateTimeTo
                      && d.DispatchDate >= DateOnly.FromDateTime(MergeDateTimeFrom)
                      && d.BatchType == "PICK"
                      && d.IsSapBatch == true)
                .GroupBy(d => new { d.Market, d.BatchNumber })
                .Select(group => new InnerBatchNumber
                {
                    Market = group.Key.Market,
                    BatchNumber = group.Key.BatchNumber,
                    SapBatchNumbers = group.Select(x => new SapBatchInfo
                    {
                        SapBatchNumber = x.SapBatchNumber,
                        DispatchDate = x.DispatchDate.ToString("yyyyMMdd")
                    }).ToList(),

                })
                .ToListAsync();

            if (!dataFromTransporty.Any())
                return new SummaryBatchResult();

            combinedResults.Clear();

            var tasks = countries.Select(async country =>
            {
                switch (country)
                {
                    case "CZ":
                        combinedResults.AddRange(await GetResultsCzAsync(dataFromTransporty));
                        break;
                    case "PL":
                        combinedResults.AddRange(await GetResultsPlAsync(dataFromTransporty));
                        break;
                    case "SK":
                        combinedResults.AddRange(await GetResultsSkAsync(dataFromTransporty));
                        break;
                    case "HU":
                        combinedResults.AddRange(await GetResultsHuAsync(dataFromTransporty));
                        break;
                }
            });

            await Task.WhenAll(tasks);

            var printingModels = (from print in combinedResults
                                  join batch in dataFromTransporty on print.BatchNumber.ToString() equals batch.BatchNumber.ToString()
                                  select new PrintingModel
                                  {
                                      Country = print.Country,
                                      Delivery = print.Delivery,
                                      PostalBarcode = print.PostalBarcode,
                                      ShipmentType = print.ShipmentType,
                                      BatchNumber = print.BatchNumber,
                                      Removed = print.Removed,
                                      SapBatchNumber = print.SapBatchNumber,
                                      SapBatchInfo = batch.SapBatchNumbers
                                  }).ToList();

            var dataForCsv = DataForDispatchFile(printingModels);

            var removedCount = combinedResults
                .Where(x => x.Removed != null)
                .GroupBy(x => new { x.BatchNumber, x.Removed })
                .Select(group => new
                {
                    group.Key.BatchNumber,
                    group.Key.Removed,
                    Count = group.Count(),
                }).ToList();

            var trackingsCount = combinedResults
                .Where(x => x.PostalBarcode != null && x.Removed == null)
                .GroupBy(x => x.BatchNumber)
                .Select(group => new
                {
                    BatchNumber = group.Key,
                    Count = group.Count(),
                }).ToList();

            var expectedTrackings = ExpectedTrackings(combinedResults);

            var summaryData = combinedResults
                .GroupBy(d => new { d.BatchNumber, d.Country })
                .Select(group => new SummaryBatches
                {
                    BatchNumber = group.Key.BatchNumber.ToString(),
                    Country = group.Key.Country,
                    SapBatchNumber = string.Join(",", dataFromTransporty
                        .FirstOrDefault(x => x.BatchNumber == group.Key.BatchNumber)?
                        .SapBatchNumbers
                        .Select(s => s.SapBatchNumber) ?? new List<string>()),
                    CountDeliveries = group.Count(),
                    RemovedPacks = removedCount.FirstOrDefault(x => x.BatchNumber == group.Key.BatchNumber && x.Removed.Contains(RemovedValue))?.Count ?? 0,
                    SuspendPacks = removedCount.FirstOrDefault(x => x.BatchNumber == group.Key.BatchNumber && x.Removed.Contains(HaltedValue))?.Count ?? 0,
                    FilledTracking = trackingsCount.FirstOrDefault(x => x.BatchNumber == group.Key.BatchNumber)?.Count ?? 0,
                    ExpectedTracking = expectedTrackings.FirstOrDefault(x => x.BatchNumber == group.Key.BatchNumber)?.Count ?? 0
                })
                .ToList();

            var batchNumbers = printingModels.DistinctBy(x => x.BatchNumber).Select(x => x.BatchNumber).ToList();

            return new SummaryBatchResult
            {
                SummaryData = summaryData,
                BatchNumbers = batchNumbers,
                CsvValuesList = dataForCsv
            };
        }

        private List<TrackingCount> ExpectedTrackings(List<PrintingModel> printings)
        {
            var result = new List<TrackingCount>();

            var pl = printings
               .Where(x =>
                   (x.ShipmentType.Contains("K48") ||
                    x.ShipmentType.Contains("COU")) &&
                    x.Removed == null &&
                    x.Country.Equals("PL"))
               .GroupBy(x => x.BatchNumber)
               .Select(group => new TrackingCount
               {
                   BatchNumber = group.Key,
                   Count = group.Count(),
               }).ToList();

            var cz = printings
                .Where(x =>
                    (x.ShipmentType.Contains("BI") ||
                     x.ShipmentType.Contains("RL") ||
                     x.ShipmentType.Contains("COU") ||
                     x.ShipmentType.Contains("PACK_DR")) &&
                     x.Removed == null &&
                     x.Country.Equals("CZ"))
                .GroupBy(x => x.BatchNumber)
                .Select(group => new TrackingCount
                {
                    BatchNumber = group.Key,
                    Count = group.Count(),
                }).ToList();

            var hu = printings
                .Where(x =>
                    x.ShipmentType.Contains("COU") &&
                    x.Removed == null &&
                     x.Country.Equals("HU"))
                .GroupBy(x => x.BatchNumber)
                .Select(group => new TrackingCount
                {
                    BatchNumber = group.Key,
                    Count = group.Count(),
                }).ToList();

            var sk = printings
                .Where(x =>
                    (x.ShipmentType.Contains("BI") ||
                     x.ShipmentType.Contains("PI") ||
                     x.ShipmentType.Contains("PE") ||
                     x.ShipmentType.Contains("COU")) &&
                     x.Removed == null &&
                     x.Country.Equals("SK"))
                .GroupBy(x => x.BatchNumber)
                .Select(group => new TrackingCount
                {
                    BatchNumber = group.Key,
                    Count = group.Count(),
                }).ToList();

            result.AddRange(pl);
            result.AddRange(cz);
            result.AddRange(hu);
            result.AddRange(sk);
            return result;
        }

        private async Task<List<PrintingModel>> GetResultsPlAsync(List<InnerBatchNumber> batchNumbersTotal)
        {
            var batchNumbersPl = GroupCountries(batchNumbersTotal, "PL");
            _sgbvPlContext.Database.SetCommandTimeout(360);
            var results = await _sgbvPlContext.TblCsvPrinting2s
                .Where(x => x.BatchNumber.HasValue && batchNumbersPl.Contains(x.BatchNumber.Value))
                .Select(x => new PrintingModel
                {
                    Country = x.Country,
                    Delivery = x.Delivery,
                    PostalBarcode = x.PostalBarcode,
                    ShipmentType = x.ShipmentType,
                    BatchNumber = x.BatchNumber.Value,
                    Removed = x.Removed,
                    SapBatchNumber = x.SapBatchNumber

                })
                .ToListAsync();

            return results;
        }

        private async Task<List<PrintingModel>> GetResultsCzAsync(List<InnerBatchNumber> batchNumbersTotal)
        {
            var batchNumbersCz = GroupCountries(batchNumbersTotal, "CZ");
            _sgbvCzContext.Database.SetCommandTimeout(360);
            var resultsCz = await _sgbvCzContext.TblCsvPrinting2s
                .Where(x => x.BatchNumber.HasValue && batchNumbersCz.Contains(x.BatchNumber.Value))
                .Select(x => new PrintingModel
                {
                    Country = x.Country,
                    Delivery = x.Delivery,
                    PostalBarcode = x.PostalBarcode,
                    ShipmentType = x.ShipmentType,
                    BatchNumber = x.BatchNumber.Value,
                    Removed = x.Removed,
                    SapBatchNumber = x.SapBatchNumber
                })
                .ToListAsync();

            return resultsCz;
        }

        private async Task<List<PrintingModel>> GetResultsHuAsync(List<InnerBatchNumber> batchNumbersTotal)
        {
            var batchNumbersHu = GroupCountries(batchNumbersTotal, "HU");
            _sgbvHuContext.Database.SetCommandTimeout(360);
            var resultsHu = await _sgbvHuContext.TblCsvPrinting2s
                .Where(x => x.BatchNumber.HasValue && batchNumbersHu.Contains(x.BatchNumber.Value))
                .Select(x => new PrintingModel
                {
                    Country = x.Country,
                    Delivery = x.Delivery,
                    PostalBarcode = x.PostalBarcode,
                    ShipmentType = x.ShipmentType,
                    BatchNumber = x.BatchNumber.Value,
                    Removed = x.Removed,
                    SapBatchNumber = x.SapBatchNumber
                })
                .ToListAsync();

            return resultsHu;
        }

        private async Task<List<PrintingModel>> GetResultsSkAsync(List<InnerBatchNumber> batchNumbersTotal)
        {
            var batchNumbers = GroupCountries(batchNumbersTotal, "SK");
            _sgbvContext.Database.SetCommandTimeout(360);

            var results = await _sgbvContext.TblCsvPrinting2s
                .Where(x => x.BatchNumber.HasValue && batchNumbers.Contains(x.BatchNumber.Value))
                .Select(x => new PrintingModel
                {
                    Country = x.Country,
                    Delivery = x.Delivery,
                    PostalBarcode = x.PostalBarcode,
                    ShipmentType = x.ShipmentType,
                    BatchNumber = x.BatchNumber.Value,
                    Removed = x.Removed,
                    SapBatchNumber = x.SapBatchNumber
                })
                .ToListAsync();

            return results;
        }

        private List<int> GroupCountries(List<InnerBatchNumber> batchNumbersTotal, string market)
            => batchNumbersTotal
                .Where(x => x.Market == market)
                .Select(x => x.BatchNumber).ToList();
    }
}

