using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace BTCPayServer.Vault
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public override void OnFrameworkInitializationCompleted()
        {
            var serviceProvider = AvaloniaLocator.CurrentMutable.GetService<IServiceProvider>();
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = (MainWindow)serviceProvider.GetService(typeof(MainWindow));
            base.OnFrameworkInitializationCompleted();
        }
    }
}
