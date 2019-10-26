using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;

namespace NBitcoin
{
    internal static class NBitcoinExtensions
    {
        /// <param name="startWithM">The keypath will start with m/ or not.</param>
        /// <param name="format">h or ', eg.: m/84h/0h/0 or m/84'/0'/0</param>
        public static string ToString(this KeyPath me, bool startWithM, string format)
        {
            var toStringBuilder = new StringBuilder(me.ToString());

            if (startWithM)
            {
                toStringBuilder.Insert(0, "m/");
            }

            if (format == "h")
            {
                toStringBuilder.Replace('\'', 'h');
            }

            return toStringBuilder.ToString();
        }
    }
}
