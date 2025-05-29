using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProxyHunter
{
    /// <summary>
    /// Checks proxies for responsiveness, latency, and anonymity level.
    /// </summary>
    public static class ProxyChecker
    {
        private static readonly HttpClient Client = new();

        /// <summary>
        /// Represents proxy check results including anonymity and latency.
        /// </summary>
        public record ProxyResult(string Proxy, string Anonymity, double LatencyMs);

        /// <summary>
        /// Contains counts of proxies by anonymity level.
        /// </summary>
        public class AnonymityStats
        {
            public int Elite { get; set; }
            public int Anonymous { get; set; }
            public int Transparent { get; set; }
        }

        /// <summary>
        /// Checks all proxies concurrently and returns valid proxies and anonymity statistics.
        /// </summary>
        /// <param name="proxies">Dictionary of proxy type to list of proxies.</param>
        /// <returns>Tuple of valid proxies and anonymity stats.</returns>
        public static (Dictionary<string, List<ProxyResult>> validProxies, Dictionary<string, AnonymityStats> anonymityStats)
            CheckProxies(Dictionary<string, List<string>> proxies)
        {
            var validProxies = new Dictionary<string, List<ProxyResult>>();
            var anonymityStats = new Dictionary<string, AnonymityStats>();

            foreach (var type in proxies.Keys)
            {
                validProxies[type] = new List<ProxyResult>();
                anonymityStats[type] = new AnonymityStats();
            }

            var tasks = new List<Task>();

            foreach (var (type, list) in proxies)
            {
                foreach (var proxy in list)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var result = await CheckProxyAsync(proxy, type);
                        if (result != null)
                        {
                            lock (validProxies)
                            {
                                validProxies[type].Add(result);
                                switch (result.Anonymity.ToLower())
                                {
                                    case "elite":
                                        anonymityStats[type].Elite++;
                                        break;
                                    case "anonymous":
                                        anonymityStats[type].Anonymous++;
                                        break;
                                    case "transparent":
                                        anonymityStats[type].Transparent++;
                                        break;
                                }
                            }
                        }
                    }));
                }
            }

            Task.WaitAll(tasks.ToArray());

            return (validProxies, anonymityStats);
        }

        /// <summary>
        /// Checks a single proxy for latency and anonymity.
        /// </summary>
        /// <param name="proxy">Proxy string</param>
        /// <param name="proxyType">Proxy type</param>
        /// <returns>ProxyResult or null if not valid</returns>
        private static async Task<ProxyResult?> CheckProxyAsync(string proxy, string proxyType)
        {
            string testUrl = (proxyType == "http" || proxyType == "https")
                ? "https://httpbin.org/headers"
                : "http://httpbin.org/ip";

            var handler = new HttpClientHandler();

            try
            {
                WebProxy webProxy = new($"http://{proxy}");
                handler.Proxy = webProxy;
                handler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;

                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(5);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ProxyHunter/1.0");

                var start = DateTime.UtcNow;
                var response = await client.GetAsync(testUrl);
                var latency = (DateTime.UtcNow - start).TotalMilliseconds;

                if (!response.IsSuccessStatusCode)
                    return null;

                string anonymity = "anonymous";

                if (proxyType == "http" || proxyType == "https")
                {
                    var json = await response.Content.ReadAsStringAsync();
                    if (json.Contains("X-Forwarded-For"))
                        anonymity = "transparent";
                    else if (!json.Contains("Via"))
                        anonymity = "elite";
                    else
                        anonymity = "anonymous";
                }

                Console.WriteLine($"[{proxyType.ToUpper()}] {anonymity.ToUpper()} {proxy} ({latency:F2} ms)");

                return new ProxyResult(proxy, anonymity, latency);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Calculates average latency from a list of ProxyResults.
        /// </summary>
        public static double CalculateAverageLatency(List<ProxyResult> proxyResults)
        {
            double total = 0;
            foreach (var result in proxyResults)
            {
                total += result.LatencyMs;
            }
            return proxyResults.Count > 0 ? total / proxyResults.Count : 0;
        }
    }
}
