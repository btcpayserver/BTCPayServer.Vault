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

            try
            {
                return new BitcoinExtPubKey(extPubKeyString, network); // Starts with "ExtPubKey": "xpub...
            }
            catch
            {
                try
                {
                    // Try hex, Old wallet format was like this.
                    return new ExtPubKey(ByteHelpers.FromHex(extPubKeyString)).GetWif(network); // Starts with "ExtPubKey": "hexbytes...
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
                    return new BitcoinExtPubKey(extPubKeyString, network);
                }
                catch
                {
                }
                throw;
            }
        }
    }
}
