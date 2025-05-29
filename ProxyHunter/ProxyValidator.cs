using System.Text.RegularExpressions;

namespace ProxyHunter
{
    /// <summary>
    /// Validates proxy strings for proper IP:Port format.
    /// </summary>
    public static class ProxyValidator
    {
        private static readonly Regex ProxyRegex = new(@"^(\d{1,3}\.){3}\d{1,3}:\d{2,5}$");

        /// <summary>
        /// Checks if the proxy string is valid (IP range and port).
        /// </summary>
        /// <param name="proxy">Proxy string in format IP:Port</param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool IsValidProxy(string proxy)
        {
            if (!ProxyRegex.IsMatch(proxy))
                return false;

            var parts = proxy.Split(':');
            var ipParts = parts[0].Split('.');

            foreach (var part in ipParts)
            {
                if (!int.TryParse(part, out int octet) || octet < 0 || octet > 255)
                    return false;
            }

            if (!int.TryParse(parts[1], out int port) || port <= 0 || port > 65535)
                return false;

            return true;
        }
    }
}
