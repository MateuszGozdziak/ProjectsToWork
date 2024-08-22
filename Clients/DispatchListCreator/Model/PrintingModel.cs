using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchListCreator.Model
{
    public class PrintingModel
    {
        public string Country { get; set; }
        public string Delivery { get; set; }
        public string PostalBarcode { get; set; }
        public string ShipmentType { get; set; }
        public int BatchNumber { get; set; }
        public string Removed { get; set; }
        public string SapBatchNumber { get; set; }
        public List<SapBatchInfo> SapBatchInfo { get; set; }

    }
}
