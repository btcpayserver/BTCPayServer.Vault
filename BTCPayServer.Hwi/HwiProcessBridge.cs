using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NBitcoin.Logging;
using Microsoft.Extensions;

namespace BTCPayServer.Hwi
{
	public class HwiProcessBridge : IHWIProcess
	{
        protected SemaphoreSlim _SemaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly string hwiPath;

        public HwiProcessBridge(): this(null)
        {

        }
        public HwiProcessBridge(string hwiPath)
        {
            this.hwiPath = hwiPath ?? "hwi";
        }
        public bool OpenConsole { get; set; }
        public ILogger Logger { get; set; } = NullLogger.Instance;

        public async Task<string> SendCommandAsync(string[] arguments, CancellationToken cancel)
		{
			string responseString;
			int exitCode;

			var fileName = hwiPath;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && 
                !fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".exe";
            }
			var redirectStandardOutput = !OpenConsole;
			var useShellExecute = OpenConsole;
			var createNoWindow = !OpenConsole;
			var windowStyle = OpenConsole ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;

			if (OpenConsole && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				throw new PlatformNotSupportedException($"{RuntimeInformation.OSDescription} is not supported.");
				//var escapedArguments = (hwiPath + " " + arguments).Replace("\"", "\\\"");
				//useShellExecute = false;
				//if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				//{
				//	fileName = "xterm";
				//	finalArguments = $"-e \"{escapedArguments}\"";
				//}
				//else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				//{
				//	fileName = "osascript";
				//	finalArguments = $"-e 'tell application \"Terminal\" to do script \"{escapedArguments}\"'";
				//}
			}

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                RedirectStandardOutput = redirectStandardOutput,
                UseShellExecute = useShellExecute,
                CreateNoWindow = createNoWindow,
                WindowStyle = windowStyle
            };

            foreach (var arg in arguments)
                startInfo.ArgumentList.Add(arg);

            Process process = null;
			try
            {
                process = await StartProcess(startInfo, cancel);

                exitCode = process.ExitCode;
                if (redirectStandardOutput)
                {
                    responseString = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
                }
                else
                {
                    responseString = exitCode == 0
                        ? "{\"success\":\"true\"}"
                        : $"{{\"success\":\"false\",\"error\":\"Process terminated with exit code: {exitCode}.\"}}";
                }
                Logger.LogDebug($"Output: {responseString}");
                ThrowIfError(responseString);
                if (exitCode != 0)
                {
                    throw new HwiException(HwiErrorCode.UnknownError, $"'hwi {arguments}' exited with incorrect exit code: {exitCode}.");
                }
            }
            catch (Exception ex)
			{
                Logger.LogError(default, ex, "Failed to call hwi");
				throw;
			}
            finally
            {
                try
                {
                    if (!process.HasExited)
                        process.Kill();
                }
                catch { }
                finally { process?.Dispose(); }
            }

			return responseString;
		}

        public static void ThrowIfError(string responseString)
        {
            if (HwiParser.TryParseErrors(responseString, out HwiException error))
            {
                throw error;
            }
        }

        private async Task<Process> StartProcess(ProcessStartInfo startInfo, CancellationToken cancel)
        {
            await _SemaphoreSlim.WaitAsync(cancel).ConfigureAwait(false);
            try
            {
                Logger.LogDebug($"{startInfo.FileName} {startInfo.Arguments}");
                Process process = Process.Start(startInfo);
                await process.WaitForExitAsync(cancel).ConfigureAwait(false);
                return process;
            }
            finally
            {
                _SemaphoreSlim.Release();
            }
        }
    }
}
