using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BTCPayServer.Helpers;
using System.Text.RegularExpressions;

namespace BTCPayServer.Hwi
{
	public static class HwiParser
	{
		public static bool TryParseErrors(string text, out HwiException error)
		{
			error = null;
			if (JsonHelpers.TryParseJToken(text, out JToken token) && TryParseError(token, out HwiException e))
			{
				error = e;
			}
			else
			{
				var subString = "error:";
				if (text.Contains(subString, StringComparison.OrdinalIgnoreCase))
				{
					int startIndex = text.IndexOf(subString, StringComparison.OrdinalIgnoreCase) + subString.Length;
					var err = text.Substring(startIndex);
					error = new HwiException(HwiErrorCode.UnknownError, err);
				}
			}
			return error != null;
		}

		public static bool TryParseError(JToken token, out HwiException error)
		{
			error = null;
			if (token is JArray)
			{
				return false;
			}

			var errToken = token["error"];
			var codeToken = token["code"];
			var successToken = token["success"];

			string err = "";
			if (errToken != null)
			{
				err = Guard.Correct(errToken.Value<string>());
			}

			HwiErrorCode? code = null;
			if (TryParseErrorCode(codeToken, out HwiErrorCode c))
			{
				code = c;
			}
			// HWI bug: it does not give error code.
			// https://github.com/bitcoin-core/HWI/issues/216
			else if (err == "Not initialized")
			{
				code = HwiErrorCode.DeviceNotInitialized;
			}

			if (code.HasValue)
			{
				error = new HwiException(code.Value, err);
			}
			else if (err.Length != 0)
			{
				error = new HwiException(HwiErrorCode.UnknownError, err);
			}
			else if (successToken != null && successToken.Value<bool>() == false)
			{
				error = new HwiException(HwiErrorCode.UnknownError, "");
			}

			return error != null;
		}

		public static bool TryParseErrorCode(JToken codeToken, out HwiErrorCode code)
		{
			code = default;

			if (codeToken is null)
			{
				return false;
			}

			try
			{
				var codeInt = codeToken.Value<int>();
				if (Enum.IsDefined(typeof(HwiErrorCode), codeInt))
				{
					code = (HwiErrorCode)codeInt;
					return true;
				}
			}
			catch
			{
				return false;
			}

			return false;
		}

		public static IEnumerable<HwiEnumerateEntry> ParseHwiEnumerateResponse(string responseString)
		{
			var jarr = JArray.Parse(responseString);

			var response = new List<HwiEnumerateEntry>();
			foreach (JObject json in jarr)
			{
				var hwiEntry = ParseHwiEnumerateEntry(json);
				response.Add(hwiEntry);
			}

			return response;
		}

		public static PSBT ParsePsbt(string json, Network network)
		{
			// HWI does not support regtest, so the parsing would fail here.
			if (network == Network.RegTest)
			{
				network = Network.TestNet;
			}

			if (JsonHelpers.TryParseJToken(json, out JToken token))
			{
				var psbtString = token["psbt"]?.ToString()?.Trim() ?? null;
				var psbt = PSBT.Parse(psbtString, network);
				return psbt;
			}
			else
			{
				throw new FormatException($"Could not parse PSBT: {json}.");
			}
		}

		public static HwiEnumerateEntry ParseHwiEnumerateEntry(JObject json)
		{
			string model = json["model"]?.Value<string>();
			var pathString = json["path"]?.ToString()?.Trim();
			var serialNumberString = json["serial_number"]?.ToString()?.Trim();
			var fingerprintString = json["fingerprint"]?.ToString()?.Trim();
			var needsPinSentString = json["needs_pin_sent"]?.ToString()?.Trim();
			var needsPassphraseSentString = json["needs_passphrase_sent"]?.ToString()?.Trim();

			HDFingerprint? fingerprint = null;
			if (fingerprintString != null)
			{
				if (HDFingerprint.TryParse(fingerprintString, out HDFingerprint fp))
				{
					fingerprint = fp;
				}
				else
				{
					throw new FormatException($"Could not parse fingerprint: {fingerprintString}");
				}
			}

			bool? needsPinSent = null;
			if (!string.IsNullOrEmpty(needsPinSentString))
			{
				needsPinSent = bool.Parse(needsPinSentString);
			}

			bool? needsPassphraseSent = null;
			if (!string.IsNullOrEmpty(needsPassphraseSentString))
			{
				needsPassphraseSent = bool.Parse(needsPassphraseSentString);
			}

			HwiErrorCode? code = null;
			string errorString = null;
			if (TryParseError(json, out HwiException err))
			{
				code = err.ErrorCode;
				errorString = err.Message;
			}

			return new HwiEnumerateEntry(
				model: model,
				path: pathString,
				serialNumber: serialNumberString,
				fingerprint: fingerprint,
				needsPinSent: needsPinSent,
				needsPassphraseSent: needsPassphraseSent,
				error: errorString,
				code: code);
		}

		public static string NormalizeRawDevicePath(string rawPath)
		{
			// There's some strangeness going on here.
			// Seems like when we get a complex path like: "hid:\\\\\\\\?\\\\hid#vid_534c&pid_0001&mi_00#7&6f0b727&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}"
			// While reading it out as the json, the duplicated \s are removed magically by newtonsoft.json.
			// However the normalized path is accepted by HWI (not sure if the raw path is accepted also.)
			return rawPath.Replace(@"\\", @"\");
		}

        static Regex VersionRegex = new Regex(@"(\d+)\.(\d+)(\.(\d+))?");
		public static bool TryParseVersion(string hwiResponse, out Version version)
		{
			version = null;
            var m = VersionRegex.Match(hwiResponse);
            if (!m.Success || m.Groups.Count < 5)
                return false;
            int.TryParse(m.Groups[1].Value, out var major);
            int.TryParse(m.Groups[2].Value, out var minor);
            int.TryParse(m.Groups[4].Value, out var build);
            version = new Version(major, minor, build);
            return true;
		}

		public static Version ParseVersion(string hwiResponse)
		{
			if (TryParseVersion(hwiResponse, out Version version))
			{
				return version;
			}

			throw new FormatException($"Cannot parse version from HWI's response. Response: {hwiResponse}.");
		}
        static Dictionary<ChainName, string> chainNames = new Dictionary<ChainName, string>()
            {
                { ChainName.Mainnet, "main" },
                { ChainName.Testnet, "test" },
                { ChainName.Regtest, "regtest" },
                { Bitcoin.Instance.Signet.ChainName, "signet" },
            };
        internal static string[] ToArgumentString(DeviceSelector deviceSelector, Network network, IEnumerable<HwiOption> options, HwiCommands? command, string[] commandArguments)
		{
            List<string> arguments = new List<string>();
			options ??= Enumerable.Empty<HwiOption>();
			var fullOptions = new List<HwiOption>(options);

            chainNames.TryGetValue(network.ChainName, out var val);
            val ??= network.ChainName.ToString().ToLowerInvariant();
            arguments.Add("--chain");
            arguments.Add(val);

            foreach (var option in fullOptions)
            {
                arguments.Add($"--{option.Type.ToString().ToLowerInvariant()}");
                if (!string.IsNullOrEmpty(option.Arguments))
                    arguments.Add(option.Arguments);
            }

            deviceSelector?.AddArgs(arguments);
			if (command != null)
			{
				arguments.Add(command.ToString().ToLowerInvariant());
			}

            if (commandArguments != null)
            {
                foreach (var commandArgument in commandArguments)
                {
                    arguments.Add(commandArgument);
                }
            }
			return arguments.ToArray();
		}
	}
}
