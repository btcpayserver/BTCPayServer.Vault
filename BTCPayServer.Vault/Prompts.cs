using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BTCPayServer.Vault
{
    public class Prompt
    {
        public Prompt(string origin)
        {
            Origin = origin;
        }
        public string Origin { get; }
        internal TaskCompletionSource<bool> _Cts = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    public class Prompts
    {
        ConcurrentDictionary<uint, Prompt> _Prompts = new ConcurrentDictionary<uint, Prompt>();
        public void CreatePrompt(uint id, string origin)
        {
            if (origin == null)
                throw new ArgumentNullException(nameof(origin));
            _Prompts.GetOrAdd(
                id,
                (_) => new Prompt(origin));
        }

        public bool TryGetPrompt(uint id, out Prompt prompt)
        {
            return _Prompts.TryGetValue(id, out prompt);
        }

        public async Task<bool> WaitPrompt(uint id, CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(() =>
            {
                _Prompts.TryRemove(id, out var p);
                p._Cts.TrySetCanceled();
            }))
            {
                return await _Prompts[id]._Cts.Task;
            }
        }

        public bool TrySetResult(uint id, bool result)
        {
            if (_Prompts.TryRemove(id, out var p))
            {
                p._Cts.TrySetResult(result);
                return true;
            }
            return false;
        }
    }
}
