using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Vault.HWI;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using BTCPayServer.Vault.Services;

namespace BTCPayServer.Vault
{
    public class PermissionPrompt : IPermissionPrompt
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly PermissionsService _permissionsService;
        private readonly MainWindow _mainWindow;
        public PermissionPrompt(LinkGenerator linkGenerator,
                                PermissionsService permissionsService,
                                MainWindow mainWindow)
        {
            _linkGenerator = linkGenerator;
            _permissionsService = permissionsService;
            _mainWindow = mainWindow;
        }
        public async Task<bool> AskPermission(OriginReason originReason, CancellationToken cancellationToken)
        {
            var result = await _mainWindow.Authorize(originReason);
            if (result)
                await _permissionsService.Grant(originReason);
            return result;
        }
    }
}
