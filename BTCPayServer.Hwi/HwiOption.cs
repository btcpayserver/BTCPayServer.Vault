using NBitcoin;
using System;
using System.Collections.Generic;
using System.Text;
using BTCPayServer.Helpers;

namespace BTCPayServer.Hwi
{
	internal class HwiOption : IEquatable<HwiOption>
	{
		public static HwiOption Debug => new HwiOption(HwiOptions.Debug);

		public static HwiOption Help => new HwiOption(HwiOptions.Help);
		public static HwiOption Interactive => new HwiOption(HwiOptions.Interactive);

		public static HwiOption Password(string password) => new HwiOption(HwiOptions.Password, password);

		public static HwiOption Version => new HwiOption(HwiOptions.Version);

		private HwiOption(HwiOptions type, string argument = null)
		{
			Type = type;
			Arguments = argument;
		}

		public HwiOptions Type { get; }
		public string Arguments { get; }

		#region Equality

		public override bool Equals(object obj) => obj is HwiOption pubKey && this == pubKey;

		public bool Equals(HwiOption other) => this == other;

		public override int GetHashCode() => Type.GetHashCode() ^ Arguments.GetHashCode();

		public static bool operator ==(HwiOption x, HwiOption y) => x?.Type == y?.Type && x?.Arguments == y?.Arguments;

		public static bool operator !=(HwiOption x, HwiOption y) => !(x == y);

		#endregion Equality
	}
}
