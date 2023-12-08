using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls.Platform;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using System.Threading;
using System.Runtime.CompilerServices;

namespace BTCPayServer.Vault
{
    public static class Extensions
    {
        public static void AddAvalonia<TApp>(this IServiceCollection services) where TApp : Application, new()
        {
            var result = AppBuilder.Configure<TApp>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                result
                    .UseWin32()
                    .UseSkia();
            }
            else
            {
                result.UsePlatformDetect();
            }

            var title = GetTitle();

            result = result
                .With(new Win32PlatformOptions())
                .With(new X11PlatformOptions {  WmClass = title })
                .With(new AvaloniaNativePlatformOptions())
                .With(new MacOSPlatformOptions { ShowInDock = true });
            services.AddSingleton(result);
            services.AddSingleton<MainWindow>();
        }

        public static void AddViewModels(this IServiceCollection services)
        {
            services.AddSingleton<MainWindowViewModel>();
        }

        public static string GetTitle(bool withVersion = true)
        {
            var title = typeof(Extensions).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)
                                              .OfType<AssemblyTitleAttribute>().Select(s => s.Title).Single();
            if (withVersion)
            {
                var version = typeof(Extensions).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                                                  .OfType<AssemblyInformationalVersionAttribute>().Select(s => s.InformationalVersion).Single();

                return $"{title} (Version: {version})";
            }
            else
            {
                return title;
            }
        }

        static bool DetectLLVMPipeRasterizer()
        {
            try
            {
                List<string> args = new List<string>();
                args.Add("-c");
                args.Add("glxinfo | grep renderer");
                var psi = new ProcessStartInfo("bash")
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                psi.ArgumentList.Add("-c");
                psi.ArgumentList.Add("glxinfo | grep renderer");
                using var process = Process.Start(psi);
                var output = process.StandardOutput.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(output) && output.Contains("llvmpipe"))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // do nothing
            }

            return false;
        }
        public static string ToHex(this byte[] data)
        {
            return Convert.ToHexString(data, 0, data.Length).ToLowerInvariant();
        }
        public static byte[] HexToBytes(this string hex)
        {
            if (hex == null)
                throw new ArgumentNullException(nameof(hex));
            if (hex.Length % 2 == 1)
                throw new FormatException("Invalid Hex String");
            if (hex.Length < (hex.Length >> 1))
                throw new ArgumentException("output should be bigger", nameof(hex));
            var output = new byte[hex.Length / 2];
            try
            {
                for (int i = 0, j = 0; i < hex.Length; i += 2, j++)
                {
                    var a = IsDigitCore(hex[i]);
                    var b = IsDigitCore(hex[i + 1]);
                    if (a == 0xff || b == 0xff)
                        throw new FormatException("Invalid Hex String");
                    output[j] = (byte)(((uint)a << 4) | (uint)b);
                }
            }
            catch (IndexOutOfRangeException) { throw new FormatException("Invalid Hex String"); }
            return output;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static byte IsDigitCore(char c)
        {
            return CharToHexLookup[c];
        }
        static byte[] CharToHexLookup => new byte[]
       {
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 15
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 31
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 47
        0x0,  0x1,  0x2,  0x3,  0x4,  0x5,  0x6,  0x7,  0x8,  0x9,  0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 63
        0xFF, 0xA,  0xB,  0xC,  0xD,  0xE,  0xF,  0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 79
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 95
        0xFF, 0xa,  0xb,  0xc,  0xd,  0xe,  0xf,  0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 111
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 127
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 143
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 159
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 175
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 191
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 207
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 223
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 239
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF  // 255
    };
    }
}
