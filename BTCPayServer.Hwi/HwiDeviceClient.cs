using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Helpers;
using NBitcoin;
using NBitcoin.DataEncoders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BTCPayServer.Hwi
{
    public class HwiDeviceClient
    {
        public HwiDeviceClient(HwiClient hwiClient, DeviceSelector deviceSelector, string model, HDFingerprint? fingerprint)
        {
            HwiClient = hwiClient ?? throw new ArgumentNullException(nameof(hwiClient));
            DeviceSelector = deviceSelector ?? throw new ArgumentNullException(nameof(deviceSelector));
            Model = model;
            Fingerprint = fingerprint;
        }

        /// <summary>
        /// Password to send along any requests
        /// </summary>
        public string Password { get; set; }
        public HwiClient HwiClient { get; }
        public DeviceSelector DeviceSelector { get; }
        public string Model { get; }
        public HDFingerprint? Fingerprint { get; }

        public Task PromptPinAsync(CancellationToken cancellationToken = default)
        {
            return SendCommandAsync(
                command: HwiCommands.PromptPin,
                cancellationToken: cancellationToken);
        }

        public async Task<bool> SendPinAsync(int pin, CancellationToken cancellationToken = default)
        {
            try
            {
                await SendCommandAsync(
                    command: HwiCommands.SendPin,
                    commandArguments: new[] { pin.ToString() },
                    cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (HwiException ex) when (ex.ErrorCode == HwiErrorCode.UnknownError)
            {
                return false;
            }
        }

        public async Task<BitcoinExtPubKey> GetXPubAsync(KeyPath keyPath, CancellationToken cancellationToken = default)
        {
            if (keyPath == null)
                throw new ArgumentNullException(nameof(keyPath));
            string keyPathString = keyPath.ToString(true, "h");
            var response = await SendCommandAsync(
                command: HwiCommands.GetXpub,
                commandArguments: new[] { keyPathString },
                cancellationToken).ConfigureAwait(false);

            return ParseExtPubKey(response);
        }

        public async Task<string> SignMessageAsync(string message, KeyPath keyPath, CancellationToken cancellationToken = default)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (keyPath == null)
                throw new ArgumentNullException(nameof(keyPath));
            var response = await SendCommandAsync(
                command: HwiCommands.SignMessage,
                commandArguments: new[] { message, keyPath.ToString(true, "h") },
                cancellationToken).ConfigureAwait(false);
            if (!JsonHelpers.TryParseJToken(response, out JToken token))
                throw new InvalidOperationException($"Invalid response from hwi");
            var signature = token["signature"]?.ToString().Trim();
            if (signature == null)
                throw new InvalidOperationException($"Invalid response from hwi");
            return signature;
        }

        private BitcoinExtPubKey ParseExtPubKey(string response)
        {
            if (!JsonHelpers.TryParseJToken(response, out JToken token))
                throw new InvalidOperationException($"Invalid response from hwi");
            var extPubKeyString = token["xpub"]?.ToString().Trim();
            if (extPubKeyString == null)
                throw new InvalidOperationException($"Invalid response from hwi");
            return NBitcoinHelpers.BetterParseExtPubKey(extPubKeyString, this.HwiClient.Network, HwiClient.IgnoreInvalidNetwork);
        }

        public async Task<BitcoinAddress> DisplayAddressAsync(ScriptPubKeyType addressType, KeyPath keyPath, CancellationToken cancellationToken = default)
        {
            if (keyPath == null)
                throw new ArgumentNullException(nameof(keyPath));
            List<string> commandArguments = new List<string>();
            commandArguments.Add("--path");
            commandArguments.Add(keyPath.ToString(true, "h"));
            commandArguments.Add("--addr-type");
            switch (addressType)
            {
                case ScriptPubKeyType.Legacy:
                    commandArguments.Add("legacy");
                    break;
                case ScriptPubKeyType.Segwit:
                    commandArguments.Add("wit");
                    break;
                case ScriptPubKeyType.SegwitP2SH:
                    commandArguments.Add("sh_wit");
                    break;
            }

            var response = await SendCommandAsync(
                command: HwiCommands.DisplayAddress,
                commandArguments: commandArguments.ToArray(),
                cancellationToken).ConfigureAwait(false);

            return ParseAddress(response, HwiClient.Network, HwiClient.IgnoreInvalidNetwork);
        }

        private static BitcoinAddress ParseAddress(string response, Network expectedNetwork, bool ignoreInvalidNetwork)
        {
            if (JsonHelpers.TryParseJToken(response, out JToken token) &&
                            token["address"]?.ToString()?.Trim() is String address)
            {
                try
                {
                    return BitcoinAddress.Create(address, expectedNetwork);
                }
                catch when (ignoreInvalidNetwork)
                {
                    var set = expectedNetwork.NetworkSet;
                    // Some wallet does not really support --chain parameter. So we need to bruteforce the proper format
                    foreach (var network in new[]
                    {
                        set.Mainnet,
                        set.Testnet,
                        set.Regtest,
                        set.GetNetwork(new ChainName("Signet"))
                    })
                    {
                        if (network is null)
                            continue;
                        if (network == expectedNetwork)
                            continue;
                        try
                        {
                            return BitcoinAddress.Create(address, network).ToNetwork(expectedNetwork);
                        }
                        catch
                        {

                        }
                    }
                    throw new FormatException(CantParseAddress);
                }
                catch (Exception ex)
                {
                    throw new FormatException(CantParseAddress, ex);
                }
            }
            throw new FormatException(CantParseAddress);
        }

        const string CantParseAddress = "The device returned an address which can't be parsed. Please use HwiClient.IgnoreInvalidNetwork=true to ignore.";

        public async Task<PSBT> SignPSBTAsync(PSBT psbt, CancellationToken cancellationToken = default)
        {
            if (psbt == null)
                throw new ArgumentNullException(nameof(psbt));
            var psbtString = psbt.ToBase64();

            var response = await SendCommandAsync(
                command: HwiCommands.SignTx,
                commandArguments: new string[] { psbtString },
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return HwiParser.ParsePsbt(response, HwiClient.Network);
        }

        public async Task WipeAsync(CancellationToken cancellationToken = default)
        {
            await SendCommandAsync(
                command: HwiCommands.Wipe,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task SetupAsync(CancellationToken cancellationToken = default)
        {
            await SendCommandAsync(
                command: HwiCommands.Setup,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task RestoreAsync(CancellationToken cancellationToken = default)
        {
            await SendCommandAsync(
                command: HwiCommands.Restore,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        private Task<string> SendCommandAsync(HwiCommands? command = null, string[] commandArguments = null, CancellationToken cancellationToken = default)
        {
            List<HwiOption> options = new List<HwiOption>();
            if (!string.IsNullOrEmpty(Password))
                options.Add(HwiOption.Password(Password));
            return HwiClient.SendCommandAsync(DeviceSelector, options, command, commandArguments, cancellationToken);
        }
    }
}
