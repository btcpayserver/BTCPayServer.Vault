using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Helpers;
using BTCPayServer.Hwi.Transports;

namespace BTCPayServer.Hwi
{
	public class HwiClient
	{
		#region PropertiesAndMembers

		public Network Network { get; }
        public ITransport Transport { get; set; } = new CliTransport();
        public bool IgnoreInvalidNetwork { get; set; }

        #endregion PropertiesAndMembers

        #region ConstructorsAndInitializers

        public HwiClient(Network network)
		{
			Network = Guard.NotNull(nameof(network), network);
		}

		#endregion ConstructorsAndInitializers

		#region Commands

		internal async Task<string> SendCommandAsync(DeviceSelector deviceSelector, IEnumerable<HwiOption> options, HwiCommands? command, string[] commandArguments, CancellationToken cancel)
		{

			try
            {
                return await SendCommandCoreAsync(deviceSelector, options, command, commandArguments, cancel).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException || ex is TimeoutException)
			{
				throw new OperationCanceledException($"'hwi operation is canceled.", ex);
			}
			// HWI is inconsistent with error codes here.
			catch (HwiException ex) when (ex.ErrorCode == HwiErrorCode.DeviceConnError || ex.ErrorCode == HwiErrorCode.DeviceNotReady)
			{
				// Probably didn't find device with specified fingerprint.
				// Enumerate and call again, but not forever.
				if (!(deviceSelector is FingerprintDeviceSelector))
				{
					throw;
				}

				IEnumerable<HwiEnumerateEntry> hwiEntries = await EnumerateEntriesAsync(cancel);

				// Trezor T won't give Fingerprint info so we'll assume that the first device that doesn't give fingerprint is what we need.
				HwiEnumerateEntry firstNoFingerprintEntry = hwiEntries.Where(x => x.Fingerprint is null).FirstOrDefault();
				if (firstNoFingerprintEntry is null)
				{
					throw;
				}
                deviceSelector = DeviceSelectors.FromDeviceType(firstNoFingerprintEntry.Model, firstNoFingerprintEntry.Path);
                return await SendCommandCoreAsync(deviceSelector, options, command, commandArguments, cancel).ConfigureAwait(false);
			}
		}

        private async Task<string> SendCommandCoreAsync(DeviceSelector deviceSelector = null, 
                                                        IEnumerable<HwiOption> options = null, 
                                                        HwiCommands? command = null, 
                                                        string[] commandArguments = null,
                                                        CancellationToken cancellationToken = default)
        {
            var arguments = HwiParser.ToArgumentString(deviceSelector, Network, options, command, commandArguments);
            var response = await Transport.SendCommandAsync(arguments, cancellationToken).ConfigureAwait(false);
            ThrowIfError(response);
            return response;
        }

        public static void ThrowIfError(string responseString)
        {
            if (HwiParser.TryParseErrors(responseString, out HwiException error))
            {
                throw error;
            }
        }


        public async Task<Version> GetVersionAsync(CancellationToken cancellationToken = default)
		{
			string responseString = await SendCommandCoreAsync(
				options: new[] { HwiOption.Version },
				cancellationToken: cancellationToken).ConfigureAwait(false);

			var version = HwiParser.ParseVersion(responseString);
			return version;
		}

        public async Task<IEnumerable<HwiDeviceClient>> EnumerateDevicesAsync(CancellationToken cancellationToken = default)
        {
            var entries = await EnumerateEntriesAsync(cancellationToken).ConfigureAwait(false);
            return entries.Select(e => new HwiDeviceClient(this, e.DeviceSelector, e.Model, e.Fingerprint)).ToArray();
        }

        public async Task<IEnumerable<HwiEnumerateEntry>> EnumerateEntriesAsync(CancellationToken cancellationToken = default)
		{
			string responseString = await SendCommandCoreAsync(
                command: HwiCommands.Enumerate,
				cancellationToken: cancellationToken).ConfigureAwait(false);
			IEnumerable<HwiEnumerateEntry> response = HwiParser.ParseHwiEnumerateResponse(responseString);

			return response;
		}

		#endregion Commands
	}
}
