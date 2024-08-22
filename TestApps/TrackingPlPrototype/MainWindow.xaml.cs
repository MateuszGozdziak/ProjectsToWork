using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrackingPlPrototype.Entities.SBGV_PL;

namespace TrackingPlPrototype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string filePath = $@"";
        string filePathDhl = $@"";
        List<LabelDetails> listOfTracking = new List<LabelDetails>();
        private readonly SgbvPlContext dbContext;
        public MainWindow()
        {
            InitializeComponent();
            dbContext = new SgbvPlContext();
            
        }
        public void ReadFiles(string filePath)
        {
            using (PdfReader pdfReader = new PdfReader(filePath))
            {
                using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
                {
                    int pages = pdfDocument.GetNumberOfPages();
                    string postalBarcode = "";
                    string invoiceNo = "";
                    for (int i = 1; i <= pages; i++)
                    {
                        PdfPage page = pdfDocument.GetPage(i);
                        LocationTextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                        string pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                        var lines = pageText.Split('\n');

                        for (int j = 0; j < lines.Length; j++)
                        {
                            var element = lines[j];
                            if (element.StartsWith("PX") && element.Replace(" ", "").Length == 12)
                            {
                                postalBarcode = element.Replace(" ", "");
                            }
                            if (element.StartsWith("Opis/Uwagi") && j + 2 < lines.Length)
                            {
                                invoiceNo = lines[j + 2];
                            }
                        }
                        listOfTracking.Add(new LabelDetails { TrackingNumber = postalBarcode, Vertical = invoiceNo });
                    }
                }
            }
        }

        public void ReadFilesDhl(string pdfPath)
        {
            using (PdfReader pdfReader = new PdfReader(pdfPath))
            using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
            {
                int pages = pdfDocument.GetNumberOfPages();
                string postalBarcode = "";
                string invoiceNo = "";

                for (int i = 1; i <= pages; i++)
                {
                    PdfPage page = pdfDocument.GetPage(i);
                    LocationTextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                    string pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                    var lines = pageText.Split('\n');


                    for (int j = 0; j < lines.Length; j++)
                    {
                        var element = lines[j];
                        if (element.StartsWith("00"))//&& element.Replace(" ", "").Length == 12)
                        {
                            postalBarcode = lines[1];
                        }
                        if (element.StartsWith("MPK") && j + 2 < lines.Length)
                        {
                            invoiceNo = element.Replace(" ", "");
                        }
                    }
                    listOfTracking.Add(new LabelDetails { TrackingNumber = postalBarcode, Vertical = invoiceNo });
                }
            }
        }
        public async Task SearchDb(List<LabelDetails> listOfTracking)
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            List<string> lista = new List<string>();
            foreach (var item in listOfTracking)
            {
                var invoiceNumber = await dbContext.TblCsvPrinting2s
                                .Where(c => c.PostalBarcode == item.TrackingNumber)
                                .Select(c => c.InvoiceNumber)
                                .FirstOrDefaultAsync();
                
                lista.Add(invoiceNumber);
                InvoiceList.Items.Add(invoiceNumber);
            }
            st.Stop();
            decimal sec = st.ElapsedMilliseconds / 1000M;
            lblSt2.Content = $"{sec.ToString("F3")}";
            
        }

        private void SearchPdf_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            ReadFiles(filePath);
            st.Stop();
            decimal sec = st.ElapsedMilliseconds / 1000M;
            lblSt1.Content = $"{sec.ToString("F3")}";

        }
        private void SearchDb_Click(object sender, RoutedEventArgs e)
        {
            SearchDb(listOfTracking);
        }

        private void SearchPdfDhl_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            ReadFilesDhl(filePathDhl);
            st.Stop();
            decimal sec = st.ElapsedMilliseconds / 1000M;
            lblStDhl.Content = $"{sec.ToString("F3")}";

            MessageBox.Show($"{listOfTracking.Count().ToString()}");
        }
    }
}