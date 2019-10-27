using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions;
using NBitcoin;
using Xunit;
using Xunit.Abstractions;
using BTCPayServer.Hwi;
using BTCPayServer.Hwi.Transports;

namespace BTCPayServer.Hardware.Tests
{
    public class HwiTests
    {
        public HwiTests(ITestOutputHelper testOutput)
        {
            LoggerFactory = new XUnitLoggerFactory(testOutput);
            Logger = LoggerFactory.CreateLogger("Tests");
        }

        ILoggerFactory LoggerFactory;
        ILogger Logger;


        [Fact]
        public async Task CanGetVersion()
        {
            var tester = await CreateTester();
            await tester.Client.GetVersionAsync();
        }

        [Fact]
        [Trait("Device", "Device")]
        public async Task CanGetXPub()
        {
            var tester = await CreateTester();
            await tester.EnsureHasDevice();
            await tester.Device.GetXpubAsync(new KeyPath("1'"));
        }

        Task<HwiTester> CreateTester()
        {
            return HwiTester.CreateAsync(LoggerFactory);
        }
    }
}
