using System.Collections.Generic;
using System.Globalization;
using BudgetTracker.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BudgetTracker.Scrapers
{
    internal class FxRateScraper : GenericScraper
    {
        public FxRateScraper(ObjectRepository repository) : base(repository)
        {
        }

        public override string ProviderName => "FX";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, ChromeDriver driver)
        {
            var result = new List<MoneyStateModel>();
            driver.Navigate().GoToUrl("https://ru.investing.com/indices/us-spx-500");
            var sps = GetElement(driver, By.Id("last_last")).Text;
            result.Add(Money("SP500",
                double.Parse(sps, new NumberFormatInfo() {NumberDecimalSeparator = ",", NumberGroupSeparator = "."}),
                "USD"));

            driver.Navigate().GoToUrl("https://ru.investing.com/currencies/usd-rub");
            sps = GetElement(driver, By.Id("last_last")).Text;
            result.Add(Money("USD/RUB",
                double.Parse(sps, new NumberFormatInfo() { NumberDecimalSeparator = ",", NumberGroupSeparator = "." }),

                "USD/RUB"));

            driver.Navigate().GoToUrl("https://ru.investing.com/currencies/eur-rub");
            sps = GetElement(driver, By.Id("last_last")).Text;
            result.Add(Money("EUR/RUB", double.Parse(sps,
                new NumberFormatInfo() {NumberDecimalSeparator = ",", NumberGroupSeparator = "."}), "EUR/RUB"));

            return result;
        }
    }
}