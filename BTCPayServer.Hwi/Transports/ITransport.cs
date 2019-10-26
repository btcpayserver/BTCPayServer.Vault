using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BTCPayServer.Hwi.Transports
{
    public interface ITransport
    {
        Task<string> SendCommandAsync(string[] arguments, CancellationToken cancel);
    }
}
