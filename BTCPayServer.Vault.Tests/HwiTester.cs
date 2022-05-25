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

        public HwiTester(Network network, ILoggerFactory loggerFactory, string hwiPath)
        {
            if (hwiPath == null)
                throw new ArgumentNullException(nameof(hwiPath));
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger("HwiTester");
            _HwiLogger = loggerFactory.CreateLogger("CliTransport");
            Network = network;
            Client = new HwiClient(network)
            {
                IgnoreInvalidNetwork = true,
                Transport = new LegacyCompatibilityTransport(new CliTransport(Path.GetDirectoryName(hwiPath))
                {
                    Logger = _HwiLogger
                })
            };
        }

        public static async Task<HwiTester> CreateAsync(Network network, ILoggerFactory loggerFactory)
        {
            var hwi = await HwiVersions.Latest.Current.EnsureIsDeployed();
            return new HwiTester(network, loggerFactory, hwi);
        }

        public async Task EnsureHasDevice()
        {
            Device = (await Client.EnumerateDevicesAsync()).FirstOrDefault();
            if (Device == null)
                throw new InvalidOperationException("No device supported by HWI has been plugged");
        }

        public Network Network { get; }

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

        public KeyPath GetKeyPath(ScriptPubKeyType addressType)
        {
            var network = Network.ChainName == ChainName.Mainnet ? "0'" : "1'";
            switch (addressType)
            {
                case ScriptPubKeyType.Legacy:
                    return new KeyPath($"44'/{network}/0'");
                case ScriptPubKeyType.Segwit:
                    return new KeyPath($"84'/{network}/0'");
                case ScriptPubKeyType.SegwitP2SH:
                    return new KeyPath($"49'/{network}/0'");
                default:
                    throw new NotSupportedException(addressType.ToString());
            }
        }
    }
}
