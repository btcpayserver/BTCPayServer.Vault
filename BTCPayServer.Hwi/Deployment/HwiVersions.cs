﻿using System;
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
        public static HwiVersion v2_1_1 { get; } = new HwiVersion()
        {
            Windows = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/2.1.1/hwi-2.1.1-windows-amd64.zip",
                Hash = "3efa5bcde386ca5523a4127f3a9802a7e9ef5320c2a8910ead343386c0b7dbfc",
                Extractor = new ZipExtractor()
            },
            Linux = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/2.1.1/hwi-2.1.1-linux-amd64.tar.gz",
                Hash = "7f4cbe4e5c2cd1ac892f9bd8ac35fb1f837b6a547b528b61aca895a212a90062",
                Extractor = new TarExtractor()
            },
            Mac = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/2.1.1/hwi-2.1.1-mac-amd64.tar.gz",
                Hash = "1b1a903b4a9884aa06593356e7a958c19ccb56a5bc97e0c6075f968310640fd2",
                Extractor = new TarExtractor()
            }
        };

        public static HwiVersion v2_3_1 { get; } = new HwiVersion()
        {
            Windows = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/2.3.1/hwi-2.3.1-windows-x86_64.zip",
                Hash = "460c8b83a9d8888ad769ffdc34dbe3ad7ecd27b22035494bdeb268d943be1791",
                Extractor = new ZipExtractor()
            },
            Linux = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/2.3.1/hwi-2.3.1-linux-x86_64.tar.gz",
                Hash = "9519023b3a485b68668675db8ab70be2e338be100fd2731eeddd6d34fc440580",
                Extractor = new TarExtractor()
            },
            Mac = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/2.3.1/hwi-2.3.1-hwi-2.3.1-mac-x86_64.tar.gz",
                Hash = "9059b8f7cf6fe42f6e37cd8015cd11cb8fb736650797b25da849c625ed61ea62",
                Extractor = new TarExtractor()
            }
        };

        public static HwiVersion v3_0_0 { get; } = new HwiVersion()
        {
            Windows = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/3.0.0/hwi-3.0.0-windows-x86_64.zip",
                Hash = "38b3f02374c300516b4583a1195ffe1cac1159f9885b8ab434fd450e290c907a",
                Extractor = new ZipExtractor()
            },
            Linux = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/3.0.0/hwi-3.0.0-linux-x86_64.tar.gz",
                Hash = "9b70aab37a1265457de4aaa242bd24a0abef5056357d8337bd79232e9b85bc1c",
                Extractor = new TarExtractor()
            },
            Mac = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/3.0.0/hwi-3.0.0-mac-x86_64.tar.gz",
                Hash = "d05c046d5718bf92b348a786aad15cb0f0132fcccf57a646758610240327a977",
                Extractor = new TarExtractor()
            }
        };
        
        public static HwiVersion v3_1_0 { get; } = new HwiVersion()
        {
            Version = "3.1.0",
            Windows = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/{0}/hwi-{0}-windows-x86_64.zip",
                Hash = "42b491941d26b41ca51a671eb8d533557ba7261d32a70f843bbe6a5af997fb25",
                Extractor = new ZipExtractor()
            },
            Linux = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/{0}/hwi-{0}-linux-x86_64.tar.gz",
                Hash = "4732e06e879b51eb5b42ce334c54373d7901556227dca4a63d342190f04b22c9",
                Extractor = new TarExtractor()
            },
            Mac = new HwiDownloadInfo()
            {
                Link = "https://github.com/bitcoin-core/HWI/releases/download/{0}/hwi-{0}-mac-x86_64.tar.gz",
                Hash = "bf5079c8899ca62bb9c48f78b53368cd43525e4900474db7d47f7c251eeaacde",
                Extractor = new TarExtractor()
            }
        };

        public static HwiVersion Latest => v3_1_0;
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

        public string Version { get; set; }
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
                var sha = SHA256.Create();
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
                var link = HwiVersions.Latest.Version is null ? Link : Link.Replace("{0}", HwiVersions.Latest.Version);
                var data = await HttpClient.GetStreamAsync(link);
                var downloadedFile = Path.Combine(destinationDirectory, link.Split('/').Last());
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
