using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MergingHuLabelsAppMvvm.Logic
{
    public class PrintersConfig
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        private string serverIpAdam = "";
        private string serverIpEwa = "";
        private string serverIpFiery = "";
        private string smbShareName = "";
        private string smbShareNameVipp = "vipp";
        private string smbShareNamePdf = "pdf";
        private string directoryForLabels = "";
        private string userFtp = "";
        private string passwordFtp = "";
        private string testDirView = "";

        private void CopyDirectoryToSmb(string sourcePath, string smbServerIp, string smbShareName)
        {
            var directoryInfo = new DirectoryInfo(sourcePath);
            var fileName = directoryInfo.Name;

            var destinationPath = Path.Combine($@"\\{smbServerIp}\{smbShareName}\{fileName}");

            if (!string.IsNullOrEmpty(destinationPath))
            {
                File.Copy(sourcePath, destinationPath);
            }
            else
            {
                Logger.Error("Wystapił bład podczas przesyłania pliku");
            }
            //foreach (var file in directoryInfo.GetFiles("*.pdf"))
            //{
            //    var destinationPath = Path.Combine($"\\\\{smbServerIp}\\{smbShareName}", file.Name);

            //    var destinationDirectory = new DirectoryInfo(Path.GetDirectoryName(destinationPath));

            //    if (!destinationDirectory.Exists)
            //    {
            //        destinationDirectory.Create();
            //    }
            //    if (!File.Exists(destinationPath))
            //    {
            //        File.Copy(file.FullName, destinationPath);
            //    }
            //    else
            //    {
            //        MessageBox.Show($"Plik {file.Name} już istnieje na serwerze.");
            //    }
            //}
        }

        public void adamPrinterSend(string sourceDirectory)
        {
            CopyDirectoryToSmb(sourceDirectory, serverIpAdam, smbShareNameVipp);
            ConfirmedSend(sourceDirectory);
        }

        public void ewaPrinterSend(string sourceDirectory)
        {
            CopyDirectoryToSmb(sourceDirectory, serverIpEwa, smbShareNameVipp);
            ConfirmedSend(sourceDirectory);
        }

        public void fieryPrinterSend(string sourceDirectory)
        {
            //tutaj funkcja do przesyłania plików na fiery
            CopyDirectoryToSmb(sourceDirectory, serverIpFiery, smbShareNamePdf);
            ConfirmedSend(sourceDirectory);
        }
        private void ConfirmedSend(string sourceDirectory)
        {
            Logger.Info($"Przesłano plik z lokalizacji {sourceDirectory}");
            MessageBox.Show($"Przesłano na drukarkę z lokalizacji {sourceDirectory}");
            //this.Close();
        }
    }
}
