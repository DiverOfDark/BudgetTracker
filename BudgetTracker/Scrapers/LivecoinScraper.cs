using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using BudgetTracker.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace BudgetTracker.Scrapers
{
    internal class LivecoinScraper : GenericScraper
    {
        private HttpClient _client;

        public LivecoinScraper(ObjectRepository repo, ILoggerFactory logger) : base(repo, logger)
        {
            _client = new HttpClient();
        }

        public override string ProviderName => "LiveCoin";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome driver)
        {
            var apiKey = configuration.Login;
            var password = configuration.Password;
            
            var param = "".Replace("/", "%2F").Replace("@", "%40").Replace(";", "%3B");
            var sign = HashHmac(password, param).ToUpper();
            var uri = "https://api.livecoin.net/payment/balances";

            _client.DefaultRequestHeaders.Remove("Api-Key");            
            _client.DefaultRequestHeaders.Add("Api-Key", apiKey);
            _client.DefaultRequestHeaders.Remove("Sign");
            _client.DefaultRequestHeaders.Add("Sign", sign);
            var response = _client.GetAsync(uri).GetAwaiter().GetResult();
            var responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            try
            {
                var jobj = JArray.Parse(responseText);
                var result = new List<MoneyStateModel>();

                foreach (var item in jobj)
                {
                    var type = item["type"].Value<string>();
                    var amount = item["value"].Value<double>();
                    var ccy = item["currency"].Value<string>();

                    if (amount > 0 && type == "total")
                    {
                        result.Add(Money(ccy, amount, ccy));
                    }
                }

                var tickers = result.Select(v => v.Ccy).Distinct().Where(v => v != CurrencyExtensions.USD).Select(v => v + "/" + CurrencyExtensions.USD)
                    .ToList();
                uri = "https://api.livecoin.net/exchange/ticker";
                response = _client.GetAsync(uri).GetAwaiter().GetResult();
                responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                jobj = JArray.Parse(responseText);

                foreach (var item in jobj)
                {
                    var symbol = item["symbol"].Value<string>();

                    if (!tickers.Contains(symbol))
                        continue;

                    var maxbid = item["max_bid"].Value<double>();
                    var minask = item["min_ask"].Value<double>();

                    var rate = (maxbid + minask) / 2;

                    result.Add(Money(symbol, rate, symbol));
                }

                return result;

            }
            catch
            {
                Logger.LogInformation($"LiveCoin response:\n{responseText}");
                throw;
            }
        }

        private static string HashHmac(string key, string message)
        {
            var encoding = new UTF8Encoding();
            var keyByte = encoding.GetBytes(key);

            var hmacsha256 = new HMACSHA256(keyByte);

            var messageBytes = encoding.GetBytes(message);
            var hashmessage = hmacsha256.ComputeHash(messageBytes);
            return ByteArrayToString(hashmessage);
        }

        private static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}