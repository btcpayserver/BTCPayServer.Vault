using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;
using NBitcoin.DataEncoders;

namespace BTCPayServer.Helpers
{
    internal class NBitcoinHelpers
    {
        public static BitcoinExtPubKey BetterParseExtPubKey(string extPubKeyString, Network network, bool ignoreInvalidNetwork)
        {
            extPubKeyString = Guard.NotNullOrEmptyOrWhitespace(nameof(extPubKeyString), extPubKeyString, trim: true);

            ExtPubKey epk;
            try
            {
                epk = ExtPubKey.Parse(extPubKeyString, network); // Starts with "ExtPubKey": "xpub...
            }
            catch
            {
                try
                {
                    // Try hex, Old wallet format was like this.
                    epk = new ExtPubKey(ByteHelpers.FromHex(extPubKeyString)); // Starts with "ExtPubKey": "hexbytes...
                }
                catch when (ignoreInvalidNetwork)
                {
                    // Let's replace the version prefix
                    var data = Encoders.Base58Check.DecodeData(extPubKeyString);
                    var versionBytes = network.GetVersionBytes(Base58Type.EXT_PUBLIC_KEY, true);
                    if (versionBytes.Length > data.Length)
                        throw;
                    for (int i = 0; i < versionBytes.Length; i++)
                    {
                        data[i] = versionBytes[i];
                    }
                    extPubKeyString = Encoders.Base58Check.EncodeData(data);
                    epk = ExtPubKey.Parse(extPubKeyString, network);
                }
            }
            return epk.GetWif(network);
        }
    }
}
