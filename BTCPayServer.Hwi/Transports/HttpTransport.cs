using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BTCPayServer.Hwi.Transports
{
    public class HttpTransport : ITransport
    {
        static HttpClient HttpClientSingleton = new HttpClient();
        private readonly string _url;
        private HttpClient httpClient;
        public const string LocalHwiServerUri = "http://127.0.0.1:65092";
        public const int LocalHwiDefaultPort = 65092;
        /// <summary>
        /// Create a new instance of HttpTransport
        /// </summary>
        /// <param name="url">The endpoint of the HWI server (default: http://127.0.0.1:65092)</param>
        /// <param name="httpClient">The HttpClient to use (default: A singleton)</param>
        public HttpTransport(string url = LocalHwiServerUri, HttpClient httpClient = null)
        {
            _url = url ?? LocalHwiServerUri;
            if (!_url.EndsWith("/"))
                _url += "/";
            _url += "hwi-bridge/v1";
            this.httpClient = httpClient ?? HttpClientSingleton;
        }

        public async Task<string> SendCommandAsync(string[] arguments, CancellationToken cancel)
        {
            JObject request = new JObject();
            request.Add("params", new JArray(arguments));
            var response = await this.httpClient.PostAsync(_url, new StringContent(request.ToString(), Encoding.UTF8, "application/json"), cancel);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
