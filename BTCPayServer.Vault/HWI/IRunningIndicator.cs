using System;
using System.Collections.Generic;
using System.Text;

namespace BTCPayServer.Vault.HWI
{
    public interface IRunningIndicator
    {
        public event Action<object, string> Running;
        public event EventHandler StoppedRunning;
    }
}
