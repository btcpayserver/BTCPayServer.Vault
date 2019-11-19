using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebWindows;

namespace BTCPayServer.Vault
{
    public class WebWindowBrowser : IBrowser
    {
        private readonly WebWindow _webWindow;
        public WebWindowBrowser(): this(null)
        {

        }
        public WebWindowBrowser(WebWindow webWindow)
        {
            _webWindow = webWindow ?? new WebWindow("BTCPayServer Vault");
        }
        public void OpenBrowser(string url)
        {
            _webWindow.NavigateToUrl(url);
            _webWindow.Show();
        }
    }
}
