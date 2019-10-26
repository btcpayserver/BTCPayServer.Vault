using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;

namespace BTCPayServer.Helpers
{
    internal class NBitcoinHelpers
    {
        public static ExtPubKey BetterParseExtPubKey(string extPubKeyString, Network network)
        {
            extPubKeyString = Guard.NotNullOrEmptyOrWhitespace(nameof(extPubKeyString), extPubKeyString, trim: true);

            ExtPubKey epk;
            try
            {
                epk = ExtPubKey.Parse(extPubKeyString, network); // Starts with "ExtPubKey": "xpub...
            }
            catch
            {
                // Try hex, Old wallet format was like this.
                epk = new ExtPubKey(ByteHelpers.FromHex(extPubKeyString)); // Starts with "ExtPubKey": "hexbytes...
            }
            return epk;
        }
    }
}
