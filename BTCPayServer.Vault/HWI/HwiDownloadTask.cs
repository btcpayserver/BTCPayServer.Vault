using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Hwi.Deployment;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BTCPayServer.Vault.HWI
{
    internal class HwiDownloadTask : IHostedService
    {
        private readonly HwiVersion _hwiVersion;
        private ILogger _logger;
        private string _deployementDirectory;

        public HwiDownloadTask(ILoggerFactory loggerFactory, HwiVersion hwiVersion, IOptions<HwiServerOptions> option)
        {
            if (hwiVersion == null)
                throw new ArgumentNullException(nameof(hwiVersion));
            _hwiVersion = hwiVersion;
            _logger = loggerFactory.CreateLogger(LoggerNames.HwiServer);
            _deployementDirectory = option.Value.HwiDeploymentDirectory;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Ensuring hwi program is deployed...");
            var path = await _hwiVersion.Current.EnsureIsDeployed(_deployementDirectory, enforceHash:false, cancellationToken: cancellationToken);
            _logger.LogInformation($"Hwi program deployed to {path}");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
