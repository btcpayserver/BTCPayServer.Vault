using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NBitcoin;
using BTCPayServer.Helpers;

namespace BTCPayServer.Hwi
{
    public interface DeviceSelector
    {
        void AddArgs(List<string> args);
    }
    internal class LambdaDeviceSelector : DeviceSelector
    {
        private readonly Action<List<string>> _writer;

        public LambdaDeviceSelector(Action<List<string>> writer)
        {
            _writer = Guard.NotNull(nameof(writer), writer);
        }

        public void AddArgs(List<string> args)
        {
            _writer(args);
        }
    }
    internal class FingerprintDeviceSelector : LambdaDeviceSelector
    {
        public FingerprintDeviceSelector(HDFingerprint fingerprint) : base(w =>
        {
            w.Add("--fingerprint");
            w.Add(fingerprint.ToString());
        })
        {
            Fingerprint = fingerprint;
        }
        public HDFingerprint Fingerprint { get; }
    }
    public class DeviceSelectors
    {
        public static DeviceSelector FromFingerprint(HDFingerprint fingerprint)
        {
            return new FingerprintDeviceSelector(fingerprint);
        }
        public static DeviceSelector FromDevicePath(string devicePath)
        {
            if (devicePath == null)
                throw new ArgumentNullException(nameof(devicePath));
            return new LambdaDeviceSelector(w =>
            {
                w.Add("--device-path");
                w.Add(devicePath);
            });
        }
        public static DeviceSelector FromDeviceType(string deviceType, string devicePath = null)
        {
            return new LambdaDeviceSelector(w =>
            {
                w.Add($"--device-type"); 
                w.Add(deviceType.ToString().ToLowerInvariant());
                if (!string.IsNullOrEmpty(devicePath))
                {
                    w.Add($"--device-path");
                    w.Add(devicePath);
                }
            });
        }
    }
}
