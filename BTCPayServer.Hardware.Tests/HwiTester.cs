using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BTCPayServer.Hwi.Deployment;
using NBitcoin.DataEncoders;
using BTCPayServer.Hwi;
using Microsoft.Extensions.Logging;
using BTCPayServer.Hwi.Transports;
using NBitcoin;

namespace BTCPayServer.Hardware.Tests
{
    public class HwiTester
    {
        static HttpClient HttpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(10.0) };
        private ILoggerFactory _loggerFactory;
        private ILogger _logger;
        private ILogger _HwiLogger;

        public HwiTester(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("HwiTester");
            _HwiLogger = loggerFactory.CreateLogger("CliTransport");
        }

        public static async Task<HwiTester> CreateAsync(ILoggerFactory loggerFactory)
        {
            var tester = new HwiTester(loggerFactory);
            await tester.EnsureDownloaded(HwiVersions.v1_0_3.Current);
            return tester;
        }

        private async Task EnsureDownloaded(HwiDownloadInfo current)
        {
            var processName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "hwi.exe" : "hwi";
            bool hasDownloaded = false;

            download:
            if (!File.Exists(processName))
            {
                HttpClient.Timeout = TimeSpan.FromMinutes(10.0);
                _logger.LogInformation($"Downloading {current.Link}...");
                var data = await HttpClient.GetStreamAsync(current.Link);
                var fileName = current.Link.Split('/').Last();
                try
                {
                    using (var fs = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite))
                    {
                        await data.CopyToAsync(fs);
                    }
                    _logger.LogInformation($"Downloaded, now extracting...");
                    await current.Extractor.Extract(fileName, processName);
                    _logger.LogInformation($"Extracted");
                    hasDownloaded = true;
                }
                finally
                {
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                }
            }
            if (File.Exists(processName))
            {
                if (current.Hash != GetFileHash(processName))
                {
                    if (hasDownloaded)
                    {
                        throw new SecurityException($"Incorrect hash for {processName}");
                    }
                    else
                    {
                        File.Delete(processName);
                        _logger.LogInformation($"Hash of {processName} does not match the expected one.");
                        goto download;
                    }
                }
            }
            Client = new HwiClient(Network)
            {
                Bridge = new CliTransport(processName)
                {
                    Logger = _HwiLogger
                }
            };
        }

        public async Task EnsureHasDevice()
        {
            Device = (await Client.EnumerateDevices()).FirstOrDefault();
            if (Device == null)
                throw new InvalidOperationException("No device supported by HWI has been plugged");
        }

        public Network Network => NBitcoin.Network.RegTest;

        public HwiClient Client
        {
            get;
            set;
        }
        public HwiDeviceClient Device
        {
            get;
            set;
        }

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
    }
}
