using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Controls;

namespace BTCPayServer.Vault
{
    public static class AvaloniaHackExtensions
    {
        private static readonly bool IsWin32NT = Environment.OSVersion.Platform == PlatformID.Win32NT;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindow(IntPtr hWnd, bool bInvert);


        /// <summary>
        /// Workaround https://github.com/AvaloniaUI/Avalonia/issues/2975
        /// </summary>
        /// <param name="window"></param>
        public static void ActivateHack(this Window window)
        {
            if (ReferenceEquals(window, null))
                throw new ArgumentNullException(nameof(window));

            // Call default Activate() anyway.
            window.Activate();

            // Skip workaround for non-windows platforms.
            if (!IsWin32NT)
                return;

            var platformImpl = window.PlatformImpl;
            if (ReferenceEquals(platformImpl, null))
                return;

            var platformHandle = window.TryGetPlatformHandle();
            if (ReferenceEquals(platformHandle, null))
                return;

            var handle = platformHandle.Handle;
            if (IntPtr.Zero == handle)
                return;

            try
            {
                SetForegroundWindow(handle);
            }
            catch
            {
                // ignored
            }
        }

        public static void Blink(this Window window)
        {
            if (!IsWin32NT)
                return;
            var platformImpl = window.PlatformImpl;
            if (ReferenceEquals(platformImpl, null))
                return;

            var platformHandle = window.TryGetPlatformHandle();
            if (ReferenceEquals(platformHandle, null))
                return;
            var handle = platformHandle.Handle;
            if (IntPtr.Zero == handle)
                return;
            FlashWindow(handle, true);
        }
    }
}
