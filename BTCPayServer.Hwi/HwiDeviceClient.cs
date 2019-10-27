using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NBitcoin;

namespace BTCPayServer.Hwi
{
    public class HwiDeviceClient
    {
        public HwiDeviceClient(HwiClient hwiClient, DeviceSelector deviceSelector, HardwareWalletModels model, HDFingerprint? fingerprint)
        {
            HwiClient = hwiClient ?? throw new ArgumentNullException(nameof(hwiClient));
            DeviceSelector = deviceSelector ?? throw new ArgumentNullException(nameof(deviceSelector));
            Model = model;
            Fingerprint = fingerprint;
        }

        public HwiClient HwiClient { get; }
        public DeviceSelector DeviceSelector { get; }
        public HardwareWalletModels Model { get; }
        public HDFingerprint? Fingerprint { get; }

        public Task PromptPin(CancellationToken cancellationToken = default)
        {
            return SendCommandAsync(
                command: HwiCommands.PromptPin,
                cancellationToken: cancellationToken);
        }

        public Task SendPin(int pin, CancellationToken cancellationToken = default)
        {
            return SendCommandAsync(
                command: HwiCommands.SendPin,
                commandArguments: new[] { pin.ToString() },
                cancellationToken);
        }

        public async Task<BitcoinExtPubKey> GetXpubAsync(KeyPath keyPath, CancellationToken cancellationToken = default)
        {
            if (keyPath == null)
                throw new ArgumentNullException(nameof(keyPath));
            string keyPathString = keyPath.ToString(true, "h");
            var response = await SendCommandAsync(
                command: HwiCommands.GetXpub,
                commandArguments: new[] { keyPathString },
                cancellationToken).ConfigureAwait(false);

            return HwiParser.ParseExtPubKey(response, HwiClient.Network);
        }

        public async Task<BitcoinAddress> DisplayAddress(ScriptPubKeyType addressType, KeyPath keyPath, CancellationToken cancellationToken = default)
        {
            if (keyPath == null)
                throw new ArgumentNullException(nameof(keyPath));
            var response = await SendCommandAsync(
                command: HwiCommands.DisplayAddress,
                commandArguments: new[] { "--path", keyPath.ToString(true, "h"), ToString(addressType) },
                cancellationToken).ConfigureAwait(false);

            return HwiParser.ParseAddress(response, HwiClient.Network);
        }

        public async Task<PSBT> SignTx(PSBT psbt, CancellationToken cancellationToken = default)
        {
            if (psbt == null)
                throw new ArgumentNullException(nameof(psbt));
            var psbtString = psbt.ToBase64();

            var response = await SendCommandAsync(
                command: HwiCommands.SignTx,
                commandArguments: new string[] { psbtString },
                cancellationToken: cancellationToken).ConfigureAwait(false);

            PSBT signedPsbt = HwiParser.ParsePsbt(response, HwiClient.Network);
            signedPsbt.TryFinalize(out var e);
            return signedPsbt;
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

        private string ToString(ScriptPubKeyType addressType)
        {
            switch (addressType)
            {
                case ScriptPubKeyType.Legacy:
                    return "";
                case ScriptPubKeyType.Segwit:
                    return " --wpkh";
                case ScriptPubKeyType.SegwitP2SH:
                    return " --sh_wpkh";
                default:
                    throw new NotSupportedException($"AddressType not supported {addressType}");
            }
        }
        private Task<string> SendCommandAsync(HwiCommands? command = null, string[] commandArguments = null, CancellationToken cancellationToken = default)
        {
            return HwiClient.SendCommandAsync(DeviceSelector, null, command, commandArguments, cancellationToken);
        }
    }
}
