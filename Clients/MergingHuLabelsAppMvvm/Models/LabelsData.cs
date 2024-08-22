using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergingHuLabelsAppMvvm.Models
{
    public class LabelsData
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int? BatchNumber { get; set; }
        public string Delivery { get; set; }
        public string InvoiceNo { get; set; }

        public string VerticalNumber { get; set; }
        public string SortedVertical { get; set; }
        public string FullVertical { get; set; }
    }
}
