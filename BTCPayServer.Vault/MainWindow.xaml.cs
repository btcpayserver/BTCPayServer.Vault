using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using BTCPayServer.Vault.HWI;

namespace BTCPayServer.Vault
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Title = Extensions.GetTitle();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Context = AvaloniaSynchronizationContext.Current as AvaloniaSynchronizationContext;
            if (AvaloniaLocator.CurrentMutable?.GetService<IServiceProvider>() is IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
                var indicator = ServiceProvider.GetRequiredService<IRunningIndicator>();
                indicator.Running += (_, op) => Context.Post((___) => MainViewModel.CurrentOperation = op + "...", null);
                indicator.StoppedRunning += (_, __) => Context.Post((___) => MainViewModel.CurrentOperation = null, null);
                DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>();
            }
        }

        MainWindowViewModel MainViewModel
        {
            get
            {
                return this.DataContext as MainWindowViewModel;
            }
        }

        public IServiceProvider ServiceProvider { get; private set; }

        AvaloniaSynchronizationContext Context;

        internal async Task<bool> Authorize(string origin)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Context.Post((state) =>
            {
                MainViewModel.Authorize(origin, tcs);
            }, null);
            return await tcs.Task;
        }
    }
}
