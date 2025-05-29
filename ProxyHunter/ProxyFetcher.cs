using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyHunter
{
    public static class ProxyFetcher
    {
        private static readonly HttpClient Client = new()
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        private const int MaxConcurrency = 10;

        public static async Task<Dictionary<string, List<string>>> FetchAllProxiesAsync()
        {
            var result = new ConcurrentDictionary<string, ConcurrentBag<string>>();
            using var semaphore = new SemaphoreSlim(MaxConcurrency);

            var tasks = new List<Task>();

            foreach (var (type, urls) in ProxySources.Sources)
            {
                foreach (var url in urls)
                {
                    tasks.Add(ProcessUrlAsync(type, url, result, semaphore));
                }
            }

            await Task.WhenAll(tasks);

            // Convert result to standard Dictionary<string, List<string>>
            var finalResult = new Dictionary<string, List<string>>();
            foreach (var (type, proxies) in result)
            {
                finalResult[type] = new List<string>(proxies);
            }

            return finalResult;
        }

        private static async Task ProcessUrlAsync(string type, string url, ConcurrentDictionary<string, ConcurrentBag<string>> result, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            try
            {
                var response = await Client.GetStringAsync(url);
                foreach (var line in response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var proxy = line.Trim();
                    if (ProxyValidator.IsValidProxy(proxy))
                    {
                        result.AddOrUpdate(
                            type,
                            _ => new ConcurrentBag<string> { proxy },
                            (_, bag) => { bag.Add(proxy); return bag; });
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"Timeout: {url} - {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {url} - {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {url} - {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
