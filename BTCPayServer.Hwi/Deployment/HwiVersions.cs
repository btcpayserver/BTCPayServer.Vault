using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

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
        public string Link { get; set; }
        public string Hash { get; set; }
        public IExtractor Extractor { get; set; }
    }
}
