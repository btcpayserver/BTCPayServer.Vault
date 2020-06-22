using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NBitcoin.DataEncoders;
using System.Security.Cryptography;
using System.Security;
using System.Threading;

namespace BTCPayServer.Hwi.Deployment
{
    public class HwiVersions
    {
        public static HwiVersion v1_0_3 { get; } = new HwiVersion()
        {
            Windows = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1.0.3/hwi-1.0.3-windows-amd64.zip",
                Hash = "f52ec4c8dd2dbef4aabe28d8a49580bceb54fd609b84c753d6354eeecbd6dc7a",
                Extractor = new ZipExtractor()
            },
            Linux = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1.0.3/hwi-1.0.3-linux-amd64.tar.gz",
                Hash = "00cb4b2c6eb78d848124e1c3707bdae9c95667f1397dd32cf3b51b579b3a010a",
                Extractor = new TarExtractor()
            },
            Mac = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1.0.3/hwi-1.0.3-mac-amd64.tar.gz",
                Hash = "b0219f4f51d74e4525dd57a19f1aee9df409a879e041ea65f2d70cf90ac48a32",
                Extractor = new TarExtractor()
            }
        };

        public static HwiVersion v1_1_0 { get; } = new HwiVersion()
        {
            Windows = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1.1.0/hwi-1.1.0-windows-amd64.zip",
                Hash = "cabf83aad91c44c78f6830c31309b9cfa61b900d27c1beb5ee07152e66167853",
                Extractor = new ZipExtractor()
            },
            Linux = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1.1.0/hwi-1.1.0-linux-amd64.tar.gz",
                Hash = "1e98a59ee0b99ccac7ec6a62e55bf9fa88650250009aecba50fd10468031ed01",
                Extractor = new TarExtractor()
            },
            Mac = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1.1.0/hwi-1.1.0-mac-amd64.tar.gz",
                Hash = "195d61bb941b6e2e6aab229f16a039f207407f80e628297b8a0cb85228e754ea",
                Extractor = new TarExtractor()
            }
        };
        public static HwiVersion v1_1_1 { get; } = new HwiVersion()
        {
            Windows = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1.1.1/hwi-1.1.1-windows-amd64.zip",
                Hash = "c36bd39635097c4fa952aceca3f4c7c74be2035a31c39a10a33dae53996630aa",
                Extractor = new ZipExtractor()
            },
            Linux = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1.1.1/hwi-1.1.1-linux-amd64.tar.gz",
                Hash = "e786797701e454415ed170ee9aed4c81a33f1adef6821bb4bd0f92d1df9d3b23",
                Extractor = new TarExtractor()
            },
            Mac = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1.1.1/hwi-1.1.1-mac-amd64.tar.gz",
                Hash = "1f48ac21c42579aa88c98e02571ed4d2dfa48f973cd6904984bc9a8b304816ad",
                Extractor = new TarExtractor()
            }
        };
    }

    public class HwiVersion
    {
        public HwiDownloadInfo Windows { get; set; }
        public HwiDownloadInfo Linux { get; set; }
        public HwiDownloadInfo Mac { get; set; }
        public HwiDownloadInfo Current
        {
            get
            {
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Windows :
                   RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Linux :
                   RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Mac :
                   throw new NotSupportedException();
            }
        }
    }

    public class HwiDownloadInfo
    {
        static HttpClient HttpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(10.0) };
        public string Link { get; set; }
        public string Hash { get; set; }
        public IExtractor Extractor { get; set; }
        private static string GetFileHash(string processName)
        {
            byte[] checksum;
            using (var stream = File.Open(processName, FileMode.Open, FileAccess.Read))
            using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
            {
                var sha = new SHA256Managed();
                checksum = sha.ComputeHash(bufferedStream);
            }
            return Encoders.Hex.EncodeData(checksum);
        }

        /// <summary>
        /// Download HWI, extract, check the hash and returns the full path to the executable
        /// </summary>
        /// <param name="destinationDirectory">Destination where to put the executable</param>
        /// <returns>The full path to the hwi executable</returns>
        public async Task<string> EnsureIsDeployed(string destinationDirectory = null, bool enforceHash = true, CancellationToken cancellationToken = default)
        {
            destinationDirectory = string.IsNullOrEmpty(destinationDirectory) ? "." : destinationDirectory;
            var processName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "hwi.exe" : "hwi";
            var processFullPath = Path.Combine(destinationDirectory, processName);
            bool hasDownloaded = false;

download:
            if (!File.Exists(processFullPath))
            {
                var data = await HttpClient.GetStreamAsync(Link);
                var downloadedFile = Path.Combine(destinationDirectory, Link.Split('/').Last());
                try
                {
                    using (var fs = File.Open(downloadedFile, FileMode.Create, FileAccess.ReadWrite))
                    {
                        await data.CopyToAsync(fs, cancellationToken);
                    }
                    await Extractor.Extract(downloadedFile, processFullPath);
                    hasDownloaded = true;
                }
                finally
                {
                    if (File.Exists(downloadedFile))
                        File.Delete(downloadedFile);
                }
            }
            if (File.Exists(processFullPath))
            {
                if (Hash != GetFileHash(processFullPath))
                {
                    if (hasDownloaded)
                    {
                        throw new SecurityException($"Incorrect hash for {processFullPath}");
                    }
                    else if (enforceHash)
                    {
                        File.Delete(processFullPath);
                        goto download;
                    }
                }
            }
            return processFullPath;
        }
    }
}
