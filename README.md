# ProxyHunter

ProxyHunter is a C# console application to fetch, validate, and check proxies for latency and anonymity from multiple online sources.

## Features

- Fetch HTTP, HTTPS, SOCKS4, SOCKS5 proxies from various free proxy lists.
- Validate proxy format.
- Check proxy response time and anonymity level.
- Save working proxies into separate files per proxy type.

## Usage

1. Clone this repo.
2. Build with .NET 6.0 or higher.
3. Run the application: `dotnet run`
4. The app fetches proxies, validates, checks, and saves working proxies as `{proxyType}_ProxyHunter.txt`.

## Project Structure

- `Program.cs`: Main runner.
- `ProxySources.cs`: Proxy source URLs.
- `ProxyFetcher.cs`: Async fetch logic.
- `ProxyValidator.cs`: Regex validation.
- `ProxyChecker.cs`: Proxy checking & stats.
- `Utils.cs`: Helper functions.


