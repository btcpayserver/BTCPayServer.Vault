[![NuGet](https://img.shields.io/nuget/v/BTCPayServer.Hwi.svg)](https://www.nuget.org/packages/BTCPayServer.Hwi) 

## How to use BTCPayServer.Hwi

First, you need to reference the [nuget package](https://www.nuget.org/packages/BTCPayServer.Hwi) in your project.

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Hwi;
using BTCPayServer.Hwi.Deployment;
using NBitcoin;

namespace BTCPayServer.Vault
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // This line will download hwi program in the process current directory
            await HwiVersions.v1_1_2.Current.EnsureIsDeployed();

            var hwiClient = new HwiClient(Network.Main);

            // Enumerate the harware wallets on this computer
            // If your device is not detected and you are on linux,
            // make sure that you properly applied udev rules.
            // These are necessary for the devices to be reachable on Linux environments.
            // See https://github.com/bitcoin-core/HWI/tree/master/hwilib/udev
            var device = (await hwiClient.EnumerateDevicesAsync()).First();

            // Ask the device to display the segwit address on the BIP32 path "84'/0'/0'/0/0"
            await device.DisplayAddressAsync(ScriptPubKeyType.Segwit, new KeyPath("84'/0'/0'/0/0"));
        }
    }
}

```

You can find some other example on how to use this library in [BTCPayServer.Vault.Tests/HwiTests.cs](BTCPayServer.Vault.Tests/HwiTests.cs).

## Licence

This project is under MIT License.

## Special thanks

Special thanks to [Wasabi Wallet](https://github.com/zkSNACKs/WalletWasabi), this code is based on their work, and as well to the bitcoin developers and [achow101](https://github.com/achow101) for the [HWI Project](https://github.com/bitcoin-core/HWI).
