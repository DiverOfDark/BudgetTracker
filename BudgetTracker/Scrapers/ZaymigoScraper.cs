using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetTracker.Model;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class ZaymigoScraper : GenericScraper
    {
        public ZaymigoScraper(ObjectRepository repository) : base(repository)
        {
        }

        public override string ProviderName => "Займиго";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;
            driver.Navigate().GoToUrl(@"https://zaymigo.ru/");
            
            GetElement(driver, By.LinkText("Войти")).Click();

            WaitForPageLoad(driver);

            GetElement(driver, By.Id("login_form_email")).Click();
            chrome.SendKeys(configuration.Login);
            
            GetElement(driver, By.Id("login_form_password")).Click();
            chrome.SendKeys(configuration.Password);
            
            chrome.SendKeys(Keys.Return);
            
            WaitForPageLoad(driver);

            var response = driver.ExecuteScript("return loansData");

            var result = new List<MoneyStateModel>();

            foreach (Dictionary<string, object> obj in ((IEnumerable) response))
            {
                var item = (string)obj["type"];

                var pairs = item.Split("\n");

                var name = pairs[0].Trim().TrimEnd(':');
                var value = Double.Parse(new string(pairs[1].Where(char.IsDigit).ToArray()), CultureInfo.InvariantCulture);
                
                result.Add(Money(name, value, CurrencyExtensions.RUB));
            }

            var fundsReports = GetElements(driver, By.ClassName("funds_report"));

            foreach (var item in fundsReports)
            {
                var tds = item.FindElements(By.TagName("td"));

                foreach (var td in tds)
                {
                    try
                    {
                        var samps = td.FindElements(By.TagName("samp"));

                        if (samps.Count != 2)
                        {
                            continue;
                        }

                        var name = samps[0].Text;

                        var valueStr = samps[1].Text;

                        var value = Double.Parse(new string(valueStr.Where(char.IsDigit).ToArray()),
                            CultureInfo.InvariantCulture);

                        result.Add(Money(name, value, CurrencyExtensions.RUB));
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            
            return result;
            
            
        }
    }
}