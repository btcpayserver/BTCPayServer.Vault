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
        public static HwiVersion v1_1_2 { get; } = new HwiVersion()
        {
            Windows = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1.1.2/hwi-1.1.2-windows-amd64.zip",
                Hash = "0f3fb7c89740ac2cf245bb8e743c5dd7e686efbda8c4a288869621a63bc32ced",
                Extractor = new ZipExtractor()
            },
            Linux = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1.1.2/hwi-1.1.2-linux-amd64.tar.gz",
                Hash = "fd6cca20aaa24f4ae4332ca01f1d4c2711247e3ccb8bbea44ee93456f211ea4b",
                Extractor = new TarExtractor()
            },
            Mac = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1.1.2/hwi-1.1.2-mac-amd64.tar.gz",
                Hash = "630aef7a02cbc08fae95e79bb9c01684650426a6f8e5383cfb040093b05aa0f1",
                Extractor = new TarExtractor()
            }
        };
        public static HwiVersion v1_2_0 { get; } = new HwiVersion()
        {
            Windows = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1_2_0/hwi-1_2_0-windows-amd64.zip",
                Hash = "599dde27eb97cf48d9fe6395e1158cc471bdf6168228facbb9d7090ce9e14634",
                Extractor = new ZipExtractor()
            },
            Linux = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1_2_0/hwi-1_2_0-linux-amd64.tar.gz",
                Hash = "92c263bd2e5c41a533972900e856e0ee9a004ad507024b38462c69afae361cea",
                Extractor = new TarExtractor()
            },
            Mac = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1_2_0/hwi-1_2_0-mac-amd64.tar.gz",
                Hash = "96437674a1bec7ee87aced6f429c9adcf74a749f41f3355cf1d5adb859fa4304",
                Extractor = new TarExtractor()
            }
        };
        public static HwiVersion v1_2_1 { get; } = new HwiVersion()
        {
            Windows = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1_2_1/hwi-1_2_1-windows-amd64.zip",
                Hash = "b8b21499592a311cfaa18676280807d6bf674d72cef21409ed265069f6582c1b",
                Extractor = new ZipExtractor()
            },
            Linux = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1_2_1/hwi-1_2_1-linux-amd64.tar.gz",
                Hash = "23ea301117f74561294b5b3ebe1eeb461004aff7e479c4b90a0aaec5924cc677",
                Extractor = new TarExtractor()
            },
            Mac = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/1_2_1/hwi-1_2_1-mac-amd64.tar.gz",
                Hash = "dc516e563db7c0f21b3f017313fc93a2a57f8d614822b8c71f1467a4e5f59dbb",
                Extractor = new TarExtractor()
            }
        };

        public static HwiVersion v2_0_1 { get; } = new HwiVersion()
        {
            Windows = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/2.0.1/hwi-2.0.1-windows-amd64.zip",
                Hash = "2cfdd6ae51e345f8c70214d626430c8d236336688a87f7d85fc6f3d6a8392da8",
                Extractor = new ZipExtractor()
            },
            Linux = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/2.0.1/hwi-2.0.1-linux-amd64.tar.gz",
                Hash = "ca1f91593b3c0a99269ecbc0f85aced08e2dec4bf263cfb25429e047e63e38d5",
                Extractor = new TarExtractor()
            },
            Mac = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/2.0.1/hwi-2.0.1-mac-amd64.tar.gz",
                Hash = "389afc3927cbc6ce01f464d8d6fa66bf050d2b7d17d7127d1c1e6ee89c5b5ec1",
                Extractor = new TarExtractor()
            }
        };
        public static HwiVersion Latest => v2_0_1;
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
