using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackingPlPrototype.Entities.SBGV_PL;

namespace DispatchListCreator.Model
{
    public class SummaryBatches
    {
        public string Country { get; set; }
        public int CountDeliveries { get; set; }
        public string BatchNumber { get; set; }
        public string SapBatchNumber { get; set; }
        public int FilledTracking { get; set; }
        public int ExpectedTracking { get; set; }
        public int RemovedPacks { get; set; }
        public int SuspendPacks { get; set; }
    }
}
