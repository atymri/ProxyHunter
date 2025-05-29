using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProxyHunter
{
    /// <summary>
    /// Fetches proxy lists asynchronously from multiple URLs.
    /// </summary>
    public static class ProxyFetcher
    {
        private static readonly HttpClient Client = new();

        /// <summary>
        /// Fetches all proxies from all sources asynchronously.
        /// </summary>
        /// <returns>A dictionary mapping proxy type to a list of proxy strings.</returns>
        public static async Task<Dictionary<string, List<string>>> FetchAllProxiesAsync()
        {
            var proxies = new Dictionary<string, HashSet<string>>();

            foreach (var key in ProxySources.Sources.Keys)
            {
                proxies[key] = new HashSet<string>();
            }

            var tasks = new List<Task>();

            foreach (var (type, urls) in ProxySources.Sources)
            {
                foreach (var url in urls)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var response = await Client.GetStringAsync(url);
                            foreach (var line in response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                string proxy = line.Trim();
                                if (ProxyValidator.IsValidProxy(proxy))
                                {
                                    lock (proxies)
                                    {
                                        proxies[type].Add(proxy);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Ignore failed fetches silently
                        }
                    }));
                }
            }

            await Task.WhenAll(tasks);

            // Convert HashSet to List
            var result = new Dictionary<string, List<string>>();
            foreach (var (type, set) in proxies)
            {
                result[type] = new List<string>(set);
            }

            return result;
        }
    }
}
