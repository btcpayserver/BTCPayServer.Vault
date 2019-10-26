using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NBitcoin;

namespace System.Diagnostics
{
    internal static class ProcessExtensions
    {
        public static Task WaitForExitAsync(this Process process, CancellationToken cancel)
        {
            return Task.Factory.StartNew((_) =>
            {
                while (!cancel.IsCancellationRequested)
                {
                    if (process.WaitForExit(100))
                        break;
                }
            }, TaskCreationOptions.LongRunning, cancellationToken: cancel);
        }
    }
}
