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
using BTCPayServer.Hwi.Process;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

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
            var fileName = Path.Combine(hwiFolder, "hwi");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                !fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".exe";
            }

            ProcessSpec processSpec = new ProcessSpec()
            {
                Executable = fileName,
                OutputCapture = new OutputCapture(),
            };
            if (arguments.Contains("signtx", StringComparer.OrdinalIgnoreCase))
            {
                processSpec.Arguments = new ReadOnlyCollection<string>(new string[] { "--stdin" });
                processSpec.Stdin = arguments.Concat(new[] { string.Empty }).ToArray();
            }
            else
            {
                processSpec.Arguments = new ReadOnlyCollection<string>(arguments);
            }

            try
            {
                ProcessRunner processRunner = new ProcessRunner();
                var exitCode = await processRunner.RunAsync(processSpec, cancel);
                responseString = string.Concat(processSpec.OutputCapture.Lines);
                Logger.LogDebug($"Exit code: exit code: {exitCode}, Output: {responseString}");
            }
            catch (Exception ex)
            {
                Logger.LogError(default, ex, "Failed to call hwi");
                throw;
            }
            return responseString;
        }
    }
}
