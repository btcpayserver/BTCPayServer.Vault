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

namespace BTCPayServer.Vault
{
    public class PermissionPrompt : IPermissionPrompt
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly MainWindow _mainWindow;
        HttpContext httpContext;
        public PermissionPrompt(LinkGenerator linkGenerator,
                                IHttpContextAccessor httpContextAccessor,
                                MainWindow mainWindow)
        {
            _linkGenerator = linkGenerator;
            _mainWindow = mainWindow;
            httpContext = httpContextAccessor.HttpContext;
        }
        public async Task<bool> AskPermission(string origin, CancellationToken cancellationToken)
        {
            return await _mainWindow.Authorize(origin);
        }
    }
}
