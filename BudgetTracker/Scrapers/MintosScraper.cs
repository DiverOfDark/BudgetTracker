using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetTracker.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class MintosScraper : GenericScraper
    {
        public MintosScraper(ObjectRepository repository, ILoggerFactory factory) : base(repository, factory)
        {
        }

        public override string ProviderName => "Mintos";
        
        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;

            driver.Navigate().GoToUrl("https://www.mintos.com/en/login");
            WaitForPageLoad(driver);

            GetElement(driver, By.Id("login-username")).Click();
            chrome.SendKeys(configuration.Login);
            GetElement(driver, By.Id("login-password")).Click();
            chrome.SendKeys(configuration.Password);
            chrome.SendKeys(Keys.Enter);
            
            WaitForPageLoad(driver);

            var interestingPart = GetElement(driver, By.Id("mintos-boxes"));

            var boxes = interestingPart.FindElements(By.ClassName("overview-box"));

            var result = new List<MoneyStateModel>();

            foreach (var box in boxes)
            {
                var header = box.FindElement(By.ClassName("header"));

                var originalTitle = header.FindElement(By.ClassName("title")).Text;
                var originalValue = header.FindElement(By.ClassName("value")).Text;

                var value = new string(originalValue.Where(v => char.IsNumber(v) || v == '.').ToArray());

                var valueAmount = double.Parse(value, NumberStyles.Any,
                    new NumberFormatInfo {NumberDecimalSeparator = "."});
                
                result.Add(Money(originalTitle, valueAmount, GuessCcy(originalValue)));

                var rows = box.FindElements(By.TagName("tr"));
                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));

                    var title = cells[0].Text;
                    
                    if (string.IsNullOrWhiteSpace(title))
                        continue;

                    title = originalTitle + " - " + title.Replace("\n", "")
                        .Replace("\r", "")
                        .Replace("\t", "")
                        .Trim();
                    
                    originalValue = cells[1].Text;
                    value = new string(originalValue.Where(v => char.IsNumber(v) || v == '.').ToArray());

                    valueAmount = double.Parse(value, NumberStyles.Any,
                        new NumberFormatInfo {NumberDecimalSeparator = "."});

                    result.Add(Money(title, valueAmount, GuessCcy(originalValue)));
                }
            }

            return result;
        }

        private static string GuessCcy(string from)
        {
            var normalized = CurrencyExtensions.NormalizeCcy(from);
            return normalized == from ? string.Empty : normalized;
        }
    }
}