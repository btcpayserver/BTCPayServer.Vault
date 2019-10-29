using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using Xunit;
using Xunit.Abstractions;
using BTCPayServer.Hwi;
using BTCPayServer.Hwi.Transports;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Builder;

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
            var tester = await CreateTester(false);
            Logger.LogInformation((await tester.Client.GetVersionAsync()).ToString());
        }

        [Fact]
        public async Task CanGetVersionViaHttpTransport()
        {
            var host = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ILoggerFactory>(LoggerFactory);
                    services.AddHwiServer();
                })
                .Configure(app =>
                {
                    app.UseHwiServer();
                })
                .UseKestrel(kestrel =>
                {
                    kestrel.Listen(IPAddress.Loopback, 0);
                })
                .Build();
            try
            {
                await host.StartAsync();
                var address = host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.First();
                var version = await new HwiClient(Network.Main)
                {
                    Transport = new HttpTransport(address)
                }.GetVersionAsync();
                Logger.LogInformation(version.ToString());
            }
            finally
            {
                await host.StopAsync();
            }
        }

        [Fact]
        [Trait("Device", "Device")]
        public async Task CanGetXPub()
        {
            var tester = await CreateTester();
            await tester.Device.GetXPubAsync(new KeyPath("1'"));
        }


        [Fact]
        [Trait("Device", "Device")]
        public async Task CanSignMessage()
        {
            var tester = await CreateTester();
            var signature = await tester.Device.SignMessageAsync("I am satoshi", new KeyPath("44'/1'/0'/0/0"));
            var xpub = await tester.Device.GetXPubAsync(new KeyPath("44'/1'/0'/0/0"));
            Assert.True(xpub.GetPublicKey().VerifyMessage("I am satoshi", signature));
        }

        [Fact]
        [Trait("Device", "Device")]
        public async Task CanDisplayAddress()
        {
            var tester = await CreateTester();
            await tester.Device.DisplayAddressAsync(ScriptPubKeyType.Legacy, GetKeyPath(ScriptPubKeyType.Legacy).Derive("0/1"));
            if (tester.Network.Consensus.SupportSegwit)
            {
                await tester.Device.DisplayAddressAsync(ScriptPubKeyType.Segwit, GetKeyPath(ScriptPubKeyType.Segwit).Derive("0/1"));
                await tester.Device.DisplayAddressAsync(ScriptPubKeyType.SegwitP2SH, GetKeyPath(ScriptPubKeyType.SegwitP2SH).Derive("0/1"));
            }
        }

        [Fact]
        [Trait("Device", "Device")]
        public async Task CanSign()
        {
            var tester = await CreateTester();

            // Should show we are sending 2.0 BTC three time
            var psbt = await tester.Device.SignPSBTAsync(await CreatePSBT(tester, ScriptPubKeyType.Legacy));
            AssertFullySigned(tester, psbt);
            if (tester.Network.Consensus.SupportSegwit)
            {
                psbt = await tester.Device.SignPSBTAsync(await CreatePSBT(tester, ScriptPubKeyType.Segwit));
                AssertFullySigned(tester, psbt);
                psbt = await tester.Device.SignPSBTAsync(await CreatePSBT(tester, ScriptPubKeyType.SegwitP2SH));
                AssertFullySigned(tester, psbt);
            }
        }

        private static void AssertFullySigned(HwiTester tester, PSBT psbt)
        {
            var txbuilder = tester.Network.CreateTransactionBuilder();
            txbuilder.AddCoins(psbt.Inputs.Select(i => i.GetCoin()));
            Assert.True(txbuilder.Verify(psbt.ExtractTransaction()), "The transaction should be fully signed");
        }

        private async Task<PSBT> CreatePSBT(HwiTester tester, ScriptPubKeyType addressType)
        {
            var accountKeyPath = new RootedKeyPath(tester.Device.Fingerprint.Value, GetKeyPath(addressType));
            var accountKey = await tester.Device.GetXPubAsync(accountKeyPath.KeyPath);
            Logger.LogInformation($"Signing with xpub {accountKeyPath}: {accountKey}...");
            List<Transaction> knownTransactions = new List<Transaction>();
            TransactionBuilder builder = accountKey.Network.CreateTransactionBuilder();
            CreateCoin(builder, knownTransactions, addressType, Money.Coins(1.0m), accountKey, "0/0");
            CreateCoin(builder, knownTransactions, addressType, Money.Coins(1.2m), accountKey, "0/1");
            builder.Send(new Key().PubKey.GetScriptPubKey(addressType), Money.Coins(2.0m));
            builder.SetChange(accountKey.Derive(new KeyPath("1/0")).ExtPubKey.PubKey.GetScriptPubKey(addressType));
            builder.SendEstimatedFees(new FeeRate(1.0m));
            var psbt = builder.BuildPSBT(false);
            psbt.AddTransactions(knownTransactions.ToArray());
            psbt.AddKeyPath(accountKey, new KeyPath[] { new KeyPath("0/0"), new KeyPath("0/1"), new KeyPath("1/0") });
            psbt.RebaseKeyPaths(accountKey, accountKeyPath);
            return psbt;
        }

        private KeyPath GetKeyPath(ScriptPubKeyType addressType)
        {
            switch (addressType)
            {
                case ScriptPubKeyType.Legacy:
                    return new KeyPath("44'/1'/0'");
                case ScriptPubKeyType.Segwit:
                    return new KeyPath("84'/1'/0'");
                case ScriptPubKeyType.SegwitP2SH:
                    return new KeyPath("49'/1'/0'");
                default:
                    throw new NotSupportedException(addressType.ToString());
            }
        }

        private void CreateCoin(TransactionBuilder builder, List<Transaction> knownTransactions, ScriptPubKeyType addressType, Money money, BitcoinExtPubKey xpub, string path)
        {
            var pubkey = xpub.Derive(new KeyPath(path)).ExtPubKey.PubKey;
            if (addressType == ScriptPubKeyType.Legacy)
            {
                var prevTx = xpub.Network.CreateTransaction();
                prevTx.Inputs.Add(RandomOutpoint(), new Key().ScriptPubKey);
                var txout = prevTx.Outputs.Add(money, pubkey.GetScriptPubKey(addressType));
                var coin = new Coin(new OutPoint(prevTx, 0), txout);
                builder.AddCoins(coin);
                knownTransactions.Add(prevTx);
            }
            if (addressType == ScriptPubKeyType.Segwit)
            {
                var outpoint = RandomOutpoint();
                var txout = xpub.Network.Consensus.ConsensusFactory.CreateTxOut();
                txout.Value = money;
                txout.ScriptPubKey = pubkey.GetScriptPubKey(addressType);
                var coin = new Coin(outpoint, txout);
                builder.AddCoins(coin);
            }
            if (addressType == ScriptPubKeyType.SegwitP2SH)
            {
                var outpoint = RandomOutpoint();
                var txout = xpub.Network.Consensus.ConsensusFactory.CreateTxOut();
                txout.Value = money;
                txout.ScriptPubKey = pubkey.GetScriptPubKey(addressType);
                var coin = new Coin(outpoint, txout).ToScriptCoin(pubkey.GetScriptPubKey(ScriptPubKeyType.Segwit));
                builder.AddCoins(coin);
            }
        }

        private static OutPoint RandomOutpoint()
        {
            return new OutPoint(RandomUtils.GetUInt256(), 0);
        }

        async Task<HwiTester> CreateTester(bool needDevice = true)
        {
            var tester = await HwiTester.CreateAsync(LoggerFactory);
            if (needDevice)
                await tester.EnsureHasDevice();
            return tester;
        }
    }
}
