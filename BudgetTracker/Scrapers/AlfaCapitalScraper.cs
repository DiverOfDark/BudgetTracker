using System;
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
    internal class AlfaCapitalScraper : GenericScraper
    {
        public AlfaCapitalScraper(ObjectRepository repository, ILoggerFactory factory) : base(repository, factory)
        {
        }

        public override string ProviderName => "Альфа-Капитал";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;
            
            driver.Navigate().GoToUrl(@"https://my.alfacapital.ru/#/");

            driver.Navigate().Refresh();
            
            GetElement(driver, By.Id("username")).Click();
            
            chrome.SendKeys(configuration.Login);

            GetElement(driver, By.Id("password")).Click();
            chrome.SendKeys(configuration.Password);
            chrome.SendKeys(Keys.Return);

            var smsModel = WaitForSms(() => GetElement(driver, By.Id("smsCode")).Click(),
                s => s.Message.Contains("Код для входа:"));

            var code = new string(smsModel.Message.Where(char.IsDigit).ToArray());
            chrome.SendKeys(code);
            chrome.SendKeys(Keys.Return);

            WaitForPageLoad(driver, 5);

            var result = new List<MoneyStateModel>();

            var assetClasses = new[] {"fund-table-component", "am-table-component"};

            foreach (var assetClass in assetClasses)
            {
                var fund = GetElement(driver, By.ClassName(assetClass));

                foreach (var tr in fund.FindElements(By.TagName("tr")).ToList())
                {
                    var tds = tr.FindElements(By.TagName("td")).ToList();
                    var title = tds[0].Text;
                    var value = tds[1].Text;
                    if (title.Contains("\n"))
                    {
                        title = title.Remove(title.IndexOf("\n")).Trim();
                    }
                    var valueAmount = double.Parse(new string(value.Where(v=>char.IsDigit(v) || v == ',').ToArray()), new NumberFormatInfo{NumberDecimalSeparator = ","});
                    result.Add(Money(title, valueAmount, CurrencyExtensions.RUB));
                }
            }
            
            return result;
        }
    }
}