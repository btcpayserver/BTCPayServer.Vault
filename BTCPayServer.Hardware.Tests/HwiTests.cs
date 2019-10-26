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
            CliTransport bridge = new CliTransport();
            Client = new HwiClient(Network.RegTest)
            {
                Bridge = new CliTransport(@"hwi.exe")
                {
                    Logger = new XUnitLogger("Bridge", testOutput)
                }
            };
            Logger = new XUnitLogger("Test", testOutput);
        }

        ILogger Logger;
        HwiClient Client; 
        [Fact]
        public async Task Test1()
        {
            var version = await Client.GetVersionAsync();
            Logger.LogInformation("Version: " + version);
            var device = (await Client.EnumerateDevices()).First();
            await device.PromptPin();
            var pin = 0;
            await device.SendPin(pin);
            var xpub = await (device.GetXpubAsync(new KeyPath("1'")));
            Logger.LogInformation("XPub: " + xpub.Network);
        }
    }
}
