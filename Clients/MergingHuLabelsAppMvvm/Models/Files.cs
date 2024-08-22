using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergingHuLabelsAppMvvm.Models
{
    public class Files
    {
        public List<LabelsData> EvenElements { get; set; } = new List<LabelsData>();

        public List<LabelsData> NoEvenElements { get; set; } = new List<LabelsData>();

        public Files DivideElement(List<LabelsData> file)
        {

            for (int i = 0; i < file.Count; i++)
            {
                if (i % 2 == 0)
                {
                    EvenElements.Add(file[i]);
                }
                else if (i % 2 == 1)
                {
                    NoEvenElements.Add(file[i]);
                }
            }
            return this;
        }
    }
}
