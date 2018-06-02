using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetTracker.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BudgetTracker.Scrapers
{
    internal class AlfaPotokScraper : GenericScraper
    {
        public AlfaPotokScraper(ObjectRepository repository) : base(repository)
        {
        }

        public override string ProviderName => "Альфа-Поток";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, ChromeDriver driver)
        {
            driver.Navigate().GoToUrl(@"https://potok.digital/potok");
            
            var name = GetElement(driver, By.Name("username"));
            var pass = GetElement(driver, By.Name("password"));
            name.Click();
            driver.Keyboard.SendKeys(configuration.Login);
            pass.Click();
            driver.Keyboard.SendKeys(configuration.Password);
            driver.Keyboard.PressKey(Keys.Return);

            var result = new List<MoneyStateModel>();

            var accountTab = GetElement(driver, By.Id("account-tab"));

            var trs = accountTab.FindElements(By.TagName("tr"));

            var items = trs.Select(v => v.Text).Where(v => v.Any(char.IsDigit)).ToList();
            foreach (var item in items)
            {
                var splitPlace = item.IndexOfAny("0123456789".ToCharArray());
                var key = item.Remove(splitPlace);
                var value = item.Substring(splitPlace);

                if (key.Contains("("))
                {
                    key = key.Remove(key.IndexOf("(", StringComparison.Ordinal), key.IndexOf(")", StringComparison.Ordinal) - key.IndexOf("(", StringComparison.Ordinal) + 1);
                }

                key = key.Trim();

                value = new string(value.Where(v => char.IsDigit(v) || v == ',').ToArray());

                var doubleValue = double.Parse(value, new NumberFormatInfo() {NumberDecimalSeparator = ","});

                result.Add(Money(key, doubleValue, "RUB"));
            }

            return result;
        }

        public override IList<PaymentModel> ScrapeStatement(ScraperConfigurationModel configuration, Chrome driver, DateTime startFrom)
        {
            // Will be handled by AlfaBankScraper as AlfaPotok uses AlfaBank infrastructure for statements.
            return new List<PaymentModel>();
        }
    }
}