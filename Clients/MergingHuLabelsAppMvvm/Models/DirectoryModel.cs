using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergingHuLabelsAppMvvm.Models
{
    public class DirectoryModel
    {
        public DirectoryInfo DirectoryInfo { get; set; }
        public DirectoryModel(DirectoryInfo directoryInfo)
        {
            DirectoryInfo = directoryInfo;
        }

        public string Name => DirectoryInfo.Name;
        public string FullName => DirectoryInfo.FullName;
    }
}
