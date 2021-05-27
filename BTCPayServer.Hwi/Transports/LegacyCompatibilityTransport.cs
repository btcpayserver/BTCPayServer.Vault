using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BTCPayServer.Hwi.Transports
{
    /// <summary>
    /// Attempt to modify the args from legacy clients so hwi does not error
    /// </summary>
    public class LegacyCompatibilityTransport : ITransport
    {
        public LegacyCompatibilityTransport(ITransport innerTransport)
        {
            if (innerTransport == null)
                throw new ArgumentNullException(nameof(innerTransport));
            InnerTransport = innerTransport;
        }

        public ITransport InnerTransport { get; }
        static Dictionary<string, string[]> replaceMap = new Dictionary<string, string[]>()
        {
            { "--sh_wpkh", new[] {"--addr-type", "sh_wit" } },
            { "--wpkh", new[] {"--addr-type", "wit" } },
            { "--testnet", new[] {"--chain", "test" } },
        };
        public Task<string> SendCommandAsync(string[] arguments, CancellationToken cancel)
        {
            if (arguments.Any(a => replaceMap.ContainsKey(a)))
            {
                List<string> newArgs = new List<string>(arguments.Length);
                foreach (var a in arguments)
                {
                    if (replaceMap.TryGetValue(a, out var replacement))
                    {
                        newArgs.AddRange(replacement);
                    }
                    else
                    {
                        newArgs.Add(a);
                    }
                }
                arguments = newArgs.ToArray();
            }
            return InnerTransport.SendCommandAsync(arguments, cancel);
        }
    }
}
