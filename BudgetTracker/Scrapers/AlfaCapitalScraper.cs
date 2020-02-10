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

            var inputs = GetElements(driver, By.TagName("input"));
            var login = inputs.First(v => v.GetAttribute("name") == "alfa-login");
            var password = inputs.First(v => v.GetAttribute("name") == "alfa-password");

            login.Click();
            chrome.SendKeys(configuration.Login);

            password.Click();
            chrome.SendKeys(configuration.Password);
            chrome.SendKeys(Keys.Return);

            var smsModel = WaitForSms(() => {}, s => s.Message.Contains("Код для входа:"));

            GetElement(driver, By.TagName("input")).Click();
            
            var code = new string(smsModel.Message.Where(char.IsDigit).ToArray());
            chrome.SendKeys(code);
            chrome.SendKeys(Keys.Return);

            WaitForPageLoad(driver);

            var result = new List<MoneyStateModel>();

            foreach (var tr in GetElements(driver, By.TagName("tr")))
            {
                try
                {
                    var tds = tr.FindElements(By.TagName("td")).ToList();
                    var title = tds[1].Text;
                    var value = tds[2].Text;
                    if (title.Contains("\n"))
                    {
                        title = title.Remove(title.IndexOf("\n")).Trim();
                    }

                    if (title.Equals("Итого", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }

                    var valueAmount = double.Parse(new string(value.Where(v => char.IsDigit(v) || v == ',').ToArray()),
                        new NumberFormatInfo {NumberDecimalSeparator = ","});
                    result.Add(Money(title, valueAmount, CurrencyExtensions.RUB));
                }
                catch(Exception ex)
                {}
            }
            
            return result;
        }
    }
}