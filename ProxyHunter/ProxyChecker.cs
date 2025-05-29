using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyHunter
{
    public static class ProxyChecker
    {
        private const int MaxConcurrency = 50;
        private static readonly SemaphoreSlim Semaphore = new(MaxConcurrency);

        public record ProxyResult(string Proxy, string Anonymity, double LatencyMs);

        public class AnonymityStats
        {
            public int Elite { get; set; }
            public int Anonymous { get; set; }
            public int Transparent { get; set; }
        }

        public static async Task<(Dictionary<string, List<ProxyResult>> validProxies, Dictionary<string, AnonymityStats> anonymityStats)>
            CheckProxiesAsync(Dictionary<string, List<string>> proxies)
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

            await Task.WhenAll(tasks);

            return (validProxies, anonymityStats);
        }

        private static async Task<ProxyResult?> CheckProxyAsync(string proxy, string proxyType)
        {
            await Semaphore.WaitAsync();
            try
            {
                string testUrl = (proxyType == "http" || proxyType == "https")
                    ? "https://httpbin.org/headers"
                    : "http://httpbin.org/ip";

                // Use correct proxy scheme or just ip:port, assuming HTTP proxy
                // You can adjust here for socks proxies if you add socks support
                string proxyAddress = proxy;

                var handler = new HttpClientHandler()
                {
                    Proxy = new WebProxy(proxyAddress),
                    UseProxy = true,
                    DefaultProxyCredentials = CredentialCache.DefaultCredentials
                };

                using var client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(8)  // Increased timeout
                };
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
            finally
            {
                Semaphore.Release();
            }
        }

        public static double CalculateAverageLatency(List<ProxyResult> proxyResults)
        {
            if (proxyResults.Count == 0) return 0;
            double total = 0;
            foreach (var r in proxyResults)
                total += r.LatencyMs;
            return total / proxyResults.Count;
        }
    }
}
