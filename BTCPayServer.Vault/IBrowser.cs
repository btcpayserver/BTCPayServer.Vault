using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BTCPayServer.Vault
{
    public interface IBrowser
    {
        void OpenBrowser(string url);
    }
}
