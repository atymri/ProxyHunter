namespace ProxyHunter
{
    /// <summary>
    /// Proxy data model (can be extended if needed).
    /// </summary>
    public class Proxy
    {
        public string Address { get; set; }
        public string Type { get; set; }

        public Proxy(string address, string type)
        {
            Address = address;
            Type = type;
        }
    }
}
