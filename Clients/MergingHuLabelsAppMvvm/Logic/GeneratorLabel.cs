using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MergingHuLabelsAppMvvm.Models;
using iText.Layout.Element;

namespace MergingHuLabelsAppMvvm.Logic
{
    public class GeneratorLabel
    {

        public void GenerateLabelFile(string outputFilePath, List<(LabelsData TopLabel, LabelsData BottomLabel)> Labels)
        {
            int sum = Labels.Count();

            using (PdfWriter writer = new PdfWriter(outputFilePath))
            {

                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    for (int i = 0; i < sum; i++)
                    {
                        PdfPage page = pdf.AddNewPage();

                        using (PdfDocument sourcePdf1 = new PdfDocument(new PdfReader(Labels[i].TopLabel.FilePath)))
                        {
                            PdfPage firstPage = sourcePdf1.GetFirstPage();

                            PdfFormXObject page1XObject = firstPage.CopyAsFormXObject(pdf);
                            PdfCanvas canvas = new PdfCanvas(page);
                            canvas.AddXObjectAt(page1XObject, 0, -7);

                            //canvas.AddXObject(page1XObject);
                        }

                        Paragraph paragraph = new Paragraph($"{Labels[i].TopLabel.VerticalNumber}")
                        .SetFixedPosition(i + 1, 110, 653, 230)
                        .SetFontSize(9);

                        var doc = new Document(pdf);
                        doc.Add(paragraph);

                        //Druga część etykieta - dolna

                        if (Labels[i].BottomLabel != null)
                        {

                            using (PdfDocument sourcePdf2 = new PdfDocument(new PdfReader(Labels[i].BottomLabel.FilePath)))
                            {
                                PdfPage secondPage = sourcePdf2.GetFirstPage();
                                PdfFormXObject page2XObject = secondPage.CopyAsFormXObject(pdf);

                                PdfCanvas canvas = new PdfCanvas(page);
                                //canvas.AddXObject(page2XObject);
                                canvas.AddXObjectAt(page2XObject, 0, -1 * page.GetPageSize().GetHeight() / 2 - 7);


                            }

                            paragraph = new Paragraph($"{Labels[i].BottomLabel.VerticalNumber}")
                            .SetFixedPosition(i + 1, 110, 231, 230)
                            .SetFontSize(9);
                            doc.Add(paragraph);
                        }

                    }
                }
            }

        }
    }
}
