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
            var hwi = await HwiVersions.v1_0_3.Current.EnsureIsDeployed();
            tester.Client = new HwiClient(tester.Network)
            {
                Bridge = new CliTransport(hwi)
                {
                    Logger = tester._HwiLogger
                }
            };
            return tester;
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
    }
}
