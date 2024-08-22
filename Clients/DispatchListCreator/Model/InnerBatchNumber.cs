using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchListCreator.Model
{
    public  class InnerBatchNumber
    {
        public string Market { get; set; }
        public int BatchNumber { get; set; }
        public List<SapBatchInfo> SapBatchNumbers { get; set; }

    }

    public class SapBatchInfo
    {
        public string SapBatchNumber { get; set; }    
        public string DispatchDate { get; set; }    
    }

}
