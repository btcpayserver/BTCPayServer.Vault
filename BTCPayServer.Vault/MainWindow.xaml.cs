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
        static Size NormalSize = new Size(622, 220);
        static Size ExpandedSize = new Size(622, 433);
        /// <summary>
        /// Workaround https://github.com/AvaloniaUI/Avalonia/issues/3290 and https://github.com/AvaloniaUI/Avalonia/issues/3291
        /// Because our app can only have two size we just resize based on the state of IsVisible of the MainViewModel
        /// </summary>
        void ResizeHack()
        {
            Size newSize = MainViewModel.IsVisible ? ExpandedSize : NormalSize;

            if (newSize != this.ClientSize)
            {
                this.ClientSize = newSize;
                // On Mac, resizing down will make the windows jump down, so we need to set it back up
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && newSize == NormalSize)
                {
                    this.Position = this.Position.WithY(this.Position.Y - (int)(ExpandedSize.Height - NormalSize.Height));
                }
            }
        }

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
                _ResizeHackTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, (_, __) =>
                {
                    ResizeHack();
                    if (MainViewModel.IsVisible && this.WindowState == WindowState.Minimized)
                        this.Blink();
                });
                _ResizeHackTimer.Start();
                this.ResizeHack();
            }
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
