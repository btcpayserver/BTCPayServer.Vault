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
using System.Runtime.InteropServices;

namespace BTCPayServer.Vault
{
    public class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            Title = Extensions.GetTitle();
        }

        private DispatcherTimer _BlinkTimer;
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            if (Program.CurrentServiceProvider is { } serviceProvider)
            {
                ServiceProvider = serviceProvider;
                Indicator = ServiceProvider.GetRequiredService<IRunningIndicator>();
                Indicator.Running += OnRunning;
                Indicator.StoppedRunning += OnStoppedRunning;
                DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>();
                MainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
                _BlinkTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, (_, __) =>
                {
                    if (MainViewModel.IsVisible && this.WindowState == WindowState.Minimized)
                        this.Blink();
                });
                _BlinkTimer.Start();
            }
        }
        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsVisible))
            {
                Context.Post(_ =>
                {
                    this.ActivateHack();
                }, null);
            }
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            if (Indicator != null)
            {
                Indicator.Running -= OnRunning;
                Indicator.StoppedRunning -= OnStoppedRunning;
                MainViewModel.PropertyChanged -= MainViewModel_PropertyChanged;
                _BlinkTimer.Stop();
            }
        }

        void OnRunning(object sender, string operation)
        {
            Context.Post(_ =>
            {
                MainViewModel.CurrentOperation = operation + "...";
            }, null);
        }

        void OnStoppedRunning(object sender, EventArgs _)
        {
            Context.Post(_ =>
            {
                MainViewModel.CurrentOperation = null;
            }, null);
        }

        MainWindowViewModel MainViewModel
        {
            get
            {
                return this.DataContext as MainWindowViewModel;
            }
        }

        public IServiceProvider ServiceProvider { get; private set; }
        public IRunningIndicator Indicator { get; private set; }

        AvaloniaSynchronizationContext Context = new AvaloniaSynchronizationContext();

        internal async Task<bool> Authorize(OriginReason originReason)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Context.Post((state) =>
            {
                MainViewModel.Authorize(originReason, tcs);
            }, null);
            return await tcs.Task;
        }
    }
}
