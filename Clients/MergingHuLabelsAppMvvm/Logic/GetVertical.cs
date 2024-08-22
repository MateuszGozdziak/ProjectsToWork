using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MergingHuLabelsAppMvvm.Models;
using TrackingPlPrototype.Entities.SGBV_HU;
namespace MergingHuLabelsAppMvvm.Logic
{
    public class GetVertical
    {
        private readonly SgbvHuContext _sgbvHuContext;
        private List<LabelsData> workedClasses;
        public List<string?> NoMatchDelivery { get; set; }
        
        public GetVertical(SgbvHuContext homedbContext)
        {
            _sgbvHuContext = homedbContext;
        }
        public List<LabelsData> GetFileNames(List<string> dialogPaths)
        {
            workedClasses = new List<LabelsData>();
            foreach (string file in dialogPaths)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string[] splitedName = fileName.Split('_');

                workedClasses.Add(new LabelsData
                {
                    FilePath = file,
                    FileName = Path.GetFileName(file),
                    BatchNumber = Int32.Parse(splitedName[0]),
                    Delivery = splitedName[1],
                });
            }
            var fullVerticals = SearchDb();

            foreach (var labelData in workedClasses)
            {
                var fullVertical = fullVerticals.FirstOrDefault(fv => fv.Delivery == labelData.Delivery);
                if (fullVertical != null)
                {
                    labelData.FullVertical = fullVertical.FullVertical;
                    labelData.VerticalNumber = fullVertical.FullVertical;
                }
            }
            var x = workedClasses.Where(x => x.FullVertical is null).Select(x => x.Delivery);
            return workedClasses;
        }

        private List<LabelDetails> SearchDb()
        {
            var batch = workedClasses.Take(1).Select(b => b.BatchNumber).ToList();
            
            var deliveryValues = workedClasses.Select(b => b.Delivery.ToString()).ToList();

            int hh = 15060;

            var x = _sgbvHuContext.TblCsvPrinting2s
                .Where(x => 
                    x.BatchNumber == batch.First()
                    && deliveryValues.Contains(x.Delivery))
                .OrderBy(x => x.VerticalNo)
                .Select(x => new LabelDetails
                {
                    Delivery = x.Delivery,
                    FullVertical = $"HU_{x.BatchNumber}_{x.ShipmentType}_{x.VerticalNo}"
                }).ToList();

            var deliveries = x.Select(x => x.Delivery).ToList();
            NoMatchDelivery = deliveryValues.Except(deliveries).ToList();

            return x;
        }
        private void MergeList()
        {

        }
    }
}
