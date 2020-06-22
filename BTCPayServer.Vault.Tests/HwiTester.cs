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

namespace BTCPayServer.Vault.Tests
{
    public class HwiTester
    {
        static HttpClient HttpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(10.0) };
        private ILogger _logger;
        private ILogger _HwiLogger;

        public HwiTester(ILoggerFactory loggerFactory, string hwiPath)
        {
            if (hwiPath == null)
                throw new ArgumentNullException(nameof(hwiPath));
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger("HwiTester");
            _HwiLogger = loggerFactory.CreateLogger("CliTransport");
            Client = new HwiClient(Network)
            {
                IgnoreInvalidNetwork = true,
                Transport = new CliTransport(hwiPath)
                {
                    Logger = _HwiLogger
                }
            };
        }

        public static async Task<HwiTester> CreateAsync(ILoggerFactory loggerFactory)
        {
            var hwi = await HwiVersions.v1_1_1.Current.EnsureIsDeployed();
            return new HwiTester(loggerFactory, hwi);
        }

        public async Task EnsureHasDevice()
        {
            Device = (await Client.EnumerateDevicesAsync()).FirstOrDefault();
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
    }
}
