using System.Collections.Generic;
using System.Globalization;
using BudgetTracker.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class FxRateScraper : GenericScraper
    {
        public FxRateScraper(ObjectRepository repository, ILoggerFactory factory) : base(repository, factory)
        {
        }

        public override string ProviderName => "FX";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;

            var result = new List<MoneyStateModel>();

            driver.Navigate().GoToUrl("https://ru.investing.com/indices/us-spx-500");
            var sps = GetElement(driver, By.Id("last_last")).Text;
            result.Add(Money("SP500",
                double.Parse(sps, new NumberFormatInfo() {NumberDecimalSeparator = ",", NumberGroupSeparator = "."}),
                CurrencyExtensions.USD));

            foreach (var item in CurrencyExtensions.KnownCurrencies)
            {
                driver.Navigate().GoToUrl($"https://ru.investing.com/currencies/{item.ToLower()}-rub");
                sps = GetElement(driver, By.Id("last_last")).Text;
                var itemRub = item + "/" + CurrencyExtensions.RUB;
                result.Add(Money(itemRub,
                    double.Parse(sps, new NumberFormatInfo { NumberDecimalSeparator = ",", NumberGroupSeparator = "." }),itemRub));
            }
                
            return result;
        }
    }
}