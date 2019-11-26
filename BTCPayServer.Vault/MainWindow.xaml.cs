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

        DispatcherTimer _ResizeHackTimer;
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Context = AvaloniaSynchronizationContext.Current as AvaloniaSynchronizationContext;
            if (AvaloniaLocator.CurrentMutable?.GetService<IServiceProvider>() is IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
                Indicator = ServiceProvider.GetRequiredService<IRunningIndicator>();
                Indicator.Running += OnRunning;
                Indicator.StoppedRunning += OnStoppedRunning;
                DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>();
                MainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
                this.Opened += MainWindow_Opened;
                _ResizeHackTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, (_, __) =>
                {
                    ResizeHack();
                    if (MainViewModel.IsVisible && this.WindowState == WindowState.Minimized)
                        this.Blink();
                });
                _ResizeHackTimer.Start();
            }
            PermissionPanel = this.Get<Panel>("PermissionPanel");
        }

        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsVisible))
            {
                Context.Post(_ =>
                {
                    this.ResizeHack();
                    this.ActivateHack();
                }, null);
            }
        }
        /// <summary>
        /// Workaround https://github.com/AvaloniaUI/Avalonia/issues/3290 and https://github.com/AvaloniaUI/Avalonia/issues/3291
        /// </summary>
        void ResizeHack()
        {
            //Console.WriteLine("PanelDesired:" + PermissionPanel.DesiredSize);

            // We hardcode here the PermissionPanel size change
            if (MainViewModel.IsVisible)
            {
                this.ClientSize = new Size(originalSize.Width, originalSize.Height + 213);
            }
            else
            {
                this.ClientSize = originalSize;
            }
        }
        private Size originalSize;
        private void MainWindow_Opened(object sender, EventArgs e)
        {
            originalSize = this.DesiredSize;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (Indicator != null)
            {
                Indicator.Running -= OnRunning;
                Indicator.StoppedRunning -= OnStoppedRunning;
                MainViewModel.PropertyChanged -= MainViewModel_PropertyChanged;
                _ResizeHackTimer.Stop();
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
        public Panel PermissionPanel { get; private set; }

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
