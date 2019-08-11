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

            driver.Navigate().GoToUrl("https://ru.investing.com/currencies/usd-rub");
            sps = GetElement(driver, By.Id("last_last")).Text;
            var usdRub = CurrencyExtensions.USD + "/" + CurrencyExtensions.RUB;
            result.Add(Money(usdRub,
                double.Parse(sps, new NumberFormatInfo() { NumberDecimalSeparator = ",", NumberGroupSeparator = "." }),usdRub));

            driver.Navigate().GoToUrl("https://ru.investing.com/currencies/eur-rub");
            sps = GetElement(driver, By.Id("last_last")).Text;
            var eurRub = CurrencyExtensions.EUR + "/" + CurrencyExtensions.RUB;
            result.Add(Money(eurRub, double.Parse(sps,
                new NumberFormatInfo() {NumberDecimalSeparator = ",", NumberGroupSeparator = "."}), eurRub));

            return result;
        }
    }
}