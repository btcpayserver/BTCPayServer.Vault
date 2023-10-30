using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BTCPayServer.Vault.HWI
{
    public interface IPermissionPrompt
    {
        Task<bool> AskPermission(OriginReason originReason, CancellationToken cancellationToken);
    }
}
