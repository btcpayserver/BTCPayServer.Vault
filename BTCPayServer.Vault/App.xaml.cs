using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BTCPayServer.Vault
{
    public class App : Application
    {
        public App()
        {

        }
        public IServiceProvider ServiceProvider { get; private set; }
        public IHostApplicationLifetime HostApplicationLifetime { get; private set; }
        public IHost Host { get; private set; }
        public IClassicDesktopStyleApplicationLifetime Desktop { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public override void OnFrameworkInitializationCompleted()
        {
            ServiceProvider = AvaloniaLocator.CurrentMutable.GetService<IServiceProvider>();
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Desktop = desktop;
                Desktop.MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                Desktop.Exit += Desktop_Exit;
                Desktop.Startup += Desktop_Startup;
                HostApplicationLifetime = ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
                Host = ServiceProvider.GetRequiredService<IHost>();
            }
            base.OnFrameworkInitializationCompleted();
        }

        private void Desktop_Startup(object sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            new Thread(_ =>
            {
                // Every other solution make deadlocks when the host is disposed
                using (Host)
                {
                    try
                    {
                        Host.Start();
                        Host.WaitForShutdown();
                    }
                    catch
                    {
                        AvaloniaSynchronizationContext.Current.Post(_ => { Desktop.Shutdown(1); }, null);
                    }
                }
            })
            {
                IsBackground = false,
                Name = "WebHost thread"
            }.Start();
        }

        private void Desktop_Exit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            HostApplicationLifetime.StopApplication();
        }
    }
}
