using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Hwi.Transports;
using System.Linq;

namespace BTCPayServer.Vault.HWI
{
    internal class InternalTransport : ITransport, IRunningIndicator
    {
        private readonly ITransport _inner;
        public InternalTransport(ITransport inner)
        {
            _inner = inner;
        }
        public event Action<object, string> Running;
        public event EventHandler StoppedRunning;
        readonly static string[] commands = "enumerate,getmasterxpub,signtx,getxpub,signmessage,getkeypool,getdescriptors,displayaddress,setup,wipe,restore,backup,promptpin,sendpin".Split(',');
        public async Task<string> SendCommandAsync(string[] arguments, CancellationToken cancel)
        {
            try
            {
                Running?.Invoke(this, arguments.Where(a => commands.Contains(a.ToLowerInvariant().Trim())).FirstOrDefault() ?? string.Empty);
                return await _inner.SendCommandAsync(arguments, cancel);
            }
            finally
            {
                StoppedRunning?.Invoke(this, new EventArgs());
            }
        }
    }
}
