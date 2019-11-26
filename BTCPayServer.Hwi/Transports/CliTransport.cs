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

namespace BTCPayServer.Hwi.Transports
{
    public class CliTransport : ITransport
    {
        protected SemaphoreSlim _SemaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly string hwiFolder;

        public CliTransport() : this(null)
        {

        }
        public CliTransport(string hwiFolder)
        {
            this.hwiFolder = hwiFolder;
        }
        public ILogger Logger { get; set; } = NullLogger.Instance;
        public async Task<string> SendCommandAsync(string[] arguments, CancellationToken cancel)
        {
            string responseString;
            int exitCode;
            var fileName = Path.Combine(hwiFolder, "hwi");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                !fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".exe";
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            foreach (var arg in arguments)
                startInfo.ArgumentList.Add(arg);

            Process process = null;
            try
            {
                process = await StartProcess(startInfo, cancel);

                exitCode = process.ExitCode;
                responseString = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
                Logger.LogDebug($"Exit code: exit code: {exitCode}, Output: {responseString}");
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

        private async Task<Process> StartProcess(ProcessStartInfo startInfo, CancellationToken cancel)
        {
            await _SemaphoreSlim.WaitAsync(cancel).ConfigureAwait(false);
            try
            {
                if (Logger.IsEnabled(LogLevel.Debug))
                {
                    Logger.LogDebug($"{startInfo.FileName} {string.Join(' ', startInfo.ArgumentList)}");
                }
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
