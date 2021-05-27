using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace BTCPayServer.Hwi.Deployment
{
    public interface IExtractor
    {
        Task Extract(string inputFileName, string outputFileName);
    }

    public class ZipExtractor : IExtractor
    {
        public Task Extract(string inputFileName, string outputFileName)
        {
            using (var zip = ZipFile.Open(inputFileName, ZipArchiveMode.Read))
            {
                foreach (var element in zip.Entries)
                {
                    if (element.Name == "hwi" || element.Name == "hwi.exe")
                    {
                        element.ExtractToFile(outputFileName);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }

    public class TarExtractor : IExtractor
    {
        public Task Extract(string inputFileName, string outputFileName)
        {
            var process = new ProcessStartInfo("tar")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            process.ArgumentList.Add("-zxvf");
            process.ArgumentList.Add(inputFileName);
            process.ArgumentList.Add("-C");
            var directory = Path.GetDirectoryName(outputFileName);
            if (string.IsNullOrEmpty(directory))
                directory = ".";
            process.ArgumentList.Add(directory);
            process.ArgumentList.Add("hwi");
            var extractedPath = Path.Combine(directory, "hwi");
            System.Diagnostics.Process.Start(process).WaitForExit();
            if (!File.Exists(extractedPath))
                throw new InvalidOperationException($"hwi was not extracted properly to {extractedPath}");
            if (extractedPath != outputFileName)
                File.Move(extractedPath, outputFileName);
            return Task.CompletedTask;
        }
    }

    public class NoOpExtractor : IExtractor
    {
        public Task Extract(string inputFileName, string outputFileName)
        {
            if (inputFileName != outputFileName)
            {
                File.Move(inputFileName, outputFileName);
            }
            return Task.CompletedTask;
        }
    }
}
