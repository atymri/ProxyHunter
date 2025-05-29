using System.Collections.Generic;

namespace ProxyHunter
{
    /// <summary>
    /// Stores proxy source URLs categorized by proxy types.
    /// </summary>
    public static class ProxySources
    {
        public static readonly Dictionary<string, List<string>> Sources = new()
        {
            ["http"] = new()
            {
                "https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/http.txt",
                "https://api.proxyscrape.com/v2/?request=displayproxies&protocol=http",
                "https://www.proxy-list.download/api/v1/get?type=http"
            },
            ["https"] = new()
            {
                "https://api.proxyscrape.com/v3/?request=displayproxies&protocol=https&timeout=10000&country=All",
                "https://raw.githubusercontent.com/roosterkid/openproxylist/main/HTTPS_RAW.txt",
                "https://www.proxy-list.download/api/v1/get?type=https"
            },
            ["socks4"] = new()
            {
                "https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks4.txt",
                "https://api.proxyscrape.com/v2/?request=displayproxies&protocol=socks4"
            },
            ["socks5"] = new()
            {
                "https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks5.txt",
                "https://api.proxyscrape.com/v2/?request=displayproxies&protocol=socks5"
            }
        };
    }
}
