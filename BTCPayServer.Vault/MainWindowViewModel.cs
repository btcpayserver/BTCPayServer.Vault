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
                this.AuthorizedOrigins.Add(OriginReason);
                OriginReason = null;
                this.taskCompletionSource.TrySetResult(true);
                this.taskCompletionSource = null;
            });
            this.Reject = new LambdaCommand(() =>
            {
                this.OriginReason = null;
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


        private bool _HWIVisible;
        public bool HWIVisible
        {
            get
            {
                return _HWIVisible;
            }
            set
            {
                if (value != _HWIVisible)
                {
                    _HWIVisible = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("HWIVisible"));
                }
            }
        }


        private bool _NFCVisible;
        public bool NFCVisible
        {
            get
            {
                return _NFCVisible;
            }
            set
            {
                if (value != _NFCVisible)
                {
                    _NFCVisible = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("NFCVisible"));
                }
            }
        }

        public List<OriginReason> AuthorizedOrigins { get; set; } = new List<OriginReason>();
        OriginReason _OriginReason;

        public OriginReason OriginReason
        {
            get
            {
                return _OriginReason;
            }
            set
            {
                if (_OriginReason != value)
                {
                    _OriginReason = value;
                    if (value is null)
                    {
                        Origin = null;
                        IsVisible = false;
                        HWIVisible = false;
                        NFCVisible = false;
                    }
                    else
                    {
                        Origin = value.Origin;
                        IsVisible = true;
                        HWIVisible = value.Reason == "hwi";
                        NFCVisible = value.Reason == "nfc";
                    }
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("OriginReason"));
                }
            }
        }


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


        private string _CurrentOperation;
        public string CurrentOperation
        {
            get
            {
                return _CurrentOperation;
            }
            set
            {
                if (value != _CurrentOperation)
                {
                    _CurrentOperation = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("CurrentOperation"));
                        PropertyChanged(this, new PropertyChangedEventArgs("IsLoading"));
                    }
                }
            }
        }

        public bool IsLoading
        {
            get
            {
                return CurrentOperation != null;
            }
        }

        TaskCompletionSource<bool> taskCompletionSource;
        internal void Authorize(OriginReason originReason, TaskCompletionSource<bool> tcs)
        {
            if (AuthorizedOrigins.Contains(originReason))
            {
                tcs.TrySetResult(true);
                return;
            }

            if (taskCompletionSource != null)
            {
                if (_OriginReason != originReason)
                    taskCompletionSource.TrySetResult(false);
                else
                    taskCompletionSource.Task.ContinueWith(result => taskCompletionSource?.TrySetResult(result.Result));
                return;
            }
            else
            {
                OriginReason = originReason;
                taskCompletionSource = tcs;
            }
        }
    }
}
