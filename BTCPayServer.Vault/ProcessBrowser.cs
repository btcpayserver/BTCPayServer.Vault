using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BTCPayServer.Vault
{
    public class ProcessBrowser : IBrowser
    {
        private readonly Func<string, ProcessStartInfo> _createProcessInfo;

        internal ProcessBrowser(Func<string, ProcessStartInfo> createProcessInfo)
        {
            if (createProcessInfo == null)
                throw new ArgumentNullException(nameof(createProcessInfo));
            _createProcessInfo = createProcessInfo;
        }

        static Lazy<ProcessBrowser> _Instance = new Lazy<ProcessBrowser>(() =>
        {
            var plateform = new[]
            {
                OSPlatform.Windows,
                OSPlatform.Linux,
                OSPlatform.OSX,
                OSPlatform.FreeBSD
            }.FirstOrDefault(p => RuntimeInformation.IsOSPlatform(p));
            return CreateForPlatform(plateform);
        });
        public static ProcessBrowser Instance
        {
            get
            {
                return _Instance.Value;
            }
        }

        public static ProcessBrowser CreateForPlatform(OSPlatform platform)
        {
            if (platform == OSPlatform.Windows)
            {
                return new ProcessBrowser((url) =>
                {
                    return new ProcessStartInfo
                    {
                        FileName = url,
                        CreateNoWindow = true,
                        UseShellExecute = true,
                    };
                });
            }
            else if (platform == OSPlatform.Linux)
            {
                return new ProcessBrowser((url) =>
                {
                    // If no associated application/json MimeType is found xdg-open opens retrun error
                    // but it tries to open it anyway using the console editor (nano, vim, other..)
                    var escapedArgs = $"xdg-open {url}".Replace("\"", "\\\"");
                    return new ProcessStartInfo
                    {
                        FileName = "/bin/sh",
                        Arguments = $"-c \"{escapedArgs}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                });
            }
            else if (platform == OSPlatform.OSX)
            {
                return new ProcessBrowser((url) => new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? url : "open",
                    Arguments = $"-e {url}",
                    CreateNoWindow = true,
                    UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                });
            }
            else
                throw new NotSupportedException(platform.ToString());
        }

        public void OpenBrowser(string url)
        {
            var processInfo = _createProcessInfo(url);
            Process.Start(processInfo).Dispose();
        }
    }
}
