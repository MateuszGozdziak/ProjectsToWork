using DispatchListCreator.Contracts;
using DispatchListCreator.Model;
using DMS.Database.DbTransporty;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DispatchListCreator.Logic
{
    public class ExportFileGenerator : IExportFileGenerator
    {
        private readonly TransportyContext _transportyContext;

        public ExportFileGenerator(TransportyContext transportyContext)
        {
            _transportyContext = transportyContext;
        }
        public void GenerateCsvFile(
            List<CsvValues> dataToWrite,
            List<int> batchNumbers,
            DateTime MergeDateTimeTo,
            DateTime MergeDateTimeFrom)
        {
            var timeOfCreatingFile = DateTime.Now;
            if (!dataToWrite.Any())
            {
                MessageBox.Show("Brak danych");
                return;
            }
            string date = MergeDateTimeTo.ToString("yyyyMMdd");

            if ((MergeDateTimeTo - MergeDateTimeFrom).TotalDays >= 1)
            {
                date = $"{MergeDateTimeFrom:yyyyMMdd}_{MergeDateTimeTo:yyyyMMdd}";
            }

            File.WriteAllLines
                ($@"\{date}_{timeOfCreatingFile:HHmmss}.txt", dataToWrite.Select(x => $"{x.Delivery};{x.PostalBarcode};{x.DispatchDate:yyyyMMdd}"));
            UpdateTransportyExportedToSap(batchNumbers);

        }
        private void UpdateTransportyExportedToSap(List<int> batchNumbers)
        {
            if (!batchNumbers.Any())
            {
                MessageBox.Show("Nie można wygenerować pliku, brak danych");
                return;
            }

            var timeOfCreatingFile = DateTime.Now;

            var results = _transportyContext.Batches
                .Where(x
                    => batchNumbers.Contains(x.BatchNumber))
                .ExecuteUpdate(x => x
                    .SetProperty(e => e.ExportedToSap, 2)
                    .SetProperty(e => e.ExportedToSapDateTime, timeOfCreatingFile));

            _transportyContext.SaveChanges();

        }
    }
}
