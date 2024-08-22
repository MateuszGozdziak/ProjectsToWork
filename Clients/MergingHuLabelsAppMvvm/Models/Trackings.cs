using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergingHuLabelsApp
{
    public class Trackings
    {
        public string PostalBarcode { get; set; }
        public string InvoiceNumber { get; set; }
        
        List<Trackings> listOfTracking = new List<Trackings>();

        public void GetTrackings(string filePath)
        {
            using (PdfReader pdfReader = new PdfReader(filePath))
            {
                using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
                {
                    string postalBarcode = "";
                    string invoiceNo = "";

                    PdfPage page = pdfDocument.GetPage(1);
                    LocationTextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                    string pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                    var lines = pageText.Split('\n');

                    for (int j = 0; j < lines.Length; j++)
                    {
                        var element = lines[j];
                        if ((element.StartsWith("PN") || element.StartsWith("PB")) && element.Replace(" ", "").Length == 26)
                        {
                            postalBarcode = element.Replace(" ", "");
                        }
                        if (element.StartsWith("Információk:") && j + 1 < lines.Length)
                        {
                            invoiceNo = lines[j + 1];
                        }
                    }

                    listOfTracking.Add(new Trackings { PostalBarcode = postalBarcode, InvoiceNumber = invoiceNo });
                }
            }
        }
    }
}
