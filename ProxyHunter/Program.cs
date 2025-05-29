using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ProxyHunter
{
    /// <summary>
    /// Main entry point for the ProxyHunter application.
    /// Coordinates fetching, validating, and checking proxies.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Utils.ClearScreen();
            Console.WriteLine(Banner.Text);

            Console.WriteLine("[*] Fetching proxies...");
            var proxies = await ProxyFetcher.FetchAllProxiesAsync();

            foreach (var (type, list) in proxies)
            {
                Console.WriteLine($"[{type.ToUpper()}] Found {list.Count} proxies.");
            }

            Console.WriteLine("[*] Checking proxies...");
            var (validProxies, anonymityStats) = await ProxyChecker.CheckProxiesAsync(proxies);


            foreach (var type in validProxies.Keys)
            {
                var working = validProxies[type];
                Console.WriteLine($"\n[{type.ToUpper()}] Working proxies: {working.Count}");
                Console.WriteLine($"  - Elite: {anonymityStats[type].Elite}");
                Console.WriteLine($"  - Anonymous: {anonymityStats[type].Anonymous}");
                Console.WriteLine($"  - Transparent: {anonymityStats[type].Transparent}");

                double avgLatency = working.Count > 0
                    ? ProxyChecker.CalculateAverageLatency(working)
                    : 0;
                Console.WriteLine($"  - Average latency: {avgLatency:F2} ms");

                if (working.Count > 0)
                {
                    string filename = $"{type}_ProxyHunter.txt";
                    File.WriteAllLines(filename, working.ConvertAll(p => p.Proxy));
                    Console.WriteLine($"Saved to {filename}");
                }
            }
        }
    }
}
