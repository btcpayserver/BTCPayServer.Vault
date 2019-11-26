using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BTCPayServer.Vault
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        class LambdaCommand : ICommand
        {
            private readonly Action act;

            public LambdaCommand(Action act)
            {
                this.act = act;
            }
#pragma warning disable CS0067
            public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                act();
            }
        }

        public MainWindowViewModel()
        {
            this.Accept = new LambdaCommand(() =>
            {
                this.IsVisible = false;
                this.AuthorizedOrigins.Add(this.Origin);
                this.Origin = null;
                this.taskCompletionSource.TrySetResult(true);
                this.taskCompletionSource = null;
            });
            this.Reject = new LambdaCommand(() =>
            {
                this.IsVisible = false;
                this.Origin = null;
                this.taskCompletionSource.TrySetResult(false);
                this.taskCompletionSource = null;
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _IsVisible;
        public bool IsVisible
        {
            get
            {
                return _IsVisible;
            }
            set
            {
                if (value != _IsVisible)
                {
                    _IsVisible = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsVisible"));
                }
            }
        }

        public List<string> AuthorizedOrigins { get; set; } = new List<string>();

        private string _Origin;
        public string Origin
        {
            get
            {
                return _Origin;
            }
            set
            {
                if (value != _Origin)
                {
                    _Origin = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Origin"));
                }
            }
        }

        public ICommand Accept { get; }
        public ICommand Reject { get; }

        private bool _IsLoading;
        public bool IsLoading
        {
            get
            {
                return _IsLoading;
            }
            set
            {
                if (value != _IsLoading)
                {
                    _IsLoading = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsLoading"));
                }
            }
        }

        TaskCompletionSource<bool> taskCompletionSource;
        internal void Authorize(string origin, TaskCompletionSource<bool> tcs)
        {
            if (AuthorizedOrigins.Contains(origin))
            {
                tcs.TrySetResult(true);
                return;
            }
            if (taskCompletionSource != null)
            {
                if (Origin != origin)
                    taskCompletionSource.TrySetResult(false);
                else
                    taskCompletionSource.Task.ContinueWith(result => taskCompletionSource.TrySetResult(result.Result));
                return;
            }
            else
            {
                IsVisible = true;
                Origin = origin;
                taskCompletionSource = tcs;
            }
        }
    }
}
