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
using AvalonStudio.Shell;
using AvalonStudio.Shell.Extensibility.Platforms;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls.Platform;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using System.Threading;

namespace BTCPayServer.Vault
{
    public static class Extensions
    {
        public static void AddAvalonia<TApp>(this IServiceCollection services) where TApp : Application, new()
        {
            bool useGpuLinux = true;

            var result = AppBuilder.Configure<TApp>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                result
                    .UseWin32()
                    .UseSkia();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (DetectLLVMPipeRasterizer())
                {
                    useGpuLinux = false;
                }

                result.UsePlatformDetect();
            }
            else
            {
                result.UsePlatformDetect();
            }

            // TODO remove this overriding of RenderTimer when Avalonia 0.9 is released.
            // fixes "Thread Leak" issue in 0.8.1 Avalonia.
            var old = result.WindowingSubsystemInitializer;

            result.UseWindowingSubsystem(() =>
            {
                old();

                AvaloniaLocator.CurrentMutable.Bind<IRenderTimer>().ToConstant(new DefaultRenderTimer(60));
            });
            var title = GetTitle();

            result = result
                .With(new Win32PlatformOptions { AllowEglInitialization = true, UseDeferredRendering = true })
                .With(new X11PlatformOptions { UseGpu = useGpuLinux, WmClass = title })
                .With(new AvaloniaNativePlatformOptions { UseDeferredRendering = true, UseGpu = true })
                .With(new MacOSPlatformOptions { ShowInDock = true });
            services.AddSingleton(result);
            services.AddSingleton<MainWindow>();
        }

        public static void AddViewModels(this IServiceCollection services)
        {
            services.AddSingleton<MainWindowViewModel>();
        }

        public static string GetTitle()
        {
            return typeof(Extensions).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)
                                              .OfType<AssemblyTitleAttribute>().Select(s => s.Title).Single();
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
    }
}
