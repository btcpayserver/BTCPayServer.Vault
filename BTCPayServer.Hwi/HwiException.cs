using System;
using System.Collections.Generic;
using System.Text;

namespace BTCPayServer.Hwi
{
	public class HwiException : Exception
	{
        public HwiErrorCode ErrorCode { get; }

		public HwiException(HwiErrorCode errorCode, string message) : base(message)
		{
			ErrorCode = errorCode;
		}
	}
}
