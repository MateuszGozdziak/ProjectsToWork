using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchListCreator.Model
{
    public class SummaryBatchResult
    {
        public SummaryBatchResult()
        {
            SummaryData = new List<SummaryBatches>();
            BatchNumbers = new List<int>();
            CsvValuesList = new List<CsvValues>();
        }

        public List<SummaryBatches> SummaryData { get; set; }
        public List<int> BatchNumbers { get; set; }
        public List<CsvValues> CsvValuesList { get; set; }
    }
}
