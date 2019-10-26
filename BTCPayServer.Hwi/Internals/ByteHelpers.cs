using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    internal class ByteHelpers
    {
        // https://stackoverflow.com/a/5919521/2061103
        // https://stackoverflow.com/a/10048895/2061103
        /// <summary>
        /// Fastest hex to byte array implementation in C#
        /// </summary>
        public static byte[] FromHex(string hex)
        {
            if (hex is null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(hex))
            {
                return Array.Empty<byte>();
            }

            var bytes = new byte[hex.Length / 2];
            var hexValue = new int[]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05,
                0x06, 0x07, 0x08, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F
            };

            for (int x = 0, i = 0; i < hex.Length; i += 2, x += 1)
            {
                bytes[x] = (byte)((hexValue[char.ToUpper(hex[i + 0]) - '0'] << 4) |
                    hexValue[char.ToUpper(hex[i + 1]) - '0']);
            }

            return bytes;
        }
    }
}
