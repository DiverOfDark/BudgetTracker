using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetTracker.Model;
using JetBrains.Annotations;
using OpenQA.Selenium;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class AlfaCapitalScraper : GenericScraper
    {
        public AlfaCapitalScraper(ObjectRepository repository) : base(repository)
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

            var now = DateTime.UtcNow;
            GetElement(driver, By.Id("smsCode")).Click();
            bool success = false;
            while (DateTime.UtcNow - now < TimeSpan.FromMinutes(15))
            {
                var lastSms = Repository.Set<SmsModel>().Where(v=>v.When > now.AddMinutes(-3)).OrderByDescending(v => v.When).FirstOrDefault();
                if (lastSms?.Message.Contains("Код для входа:") == true)
                {
                    var code = new string(lastSms.Message.Where(char.IsDigit).ToArray());
                    chrome.SendKeys(code);
                    chrome.SendKeys(Keys.Return);
                    success = true;
                    break;
                }
                WaitForPageLoad(driver);
            }

            if (!success)
                throw new Exception();

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
                    if (title.Contains("\r"))
                    {
                        title = title.Remove(title.IndexOf("\r"));
                    }
                    var valueAmount = double.Parse(new string(value.Where(v=>char.IsDigit(v) || v == ',').ToArray()), new NumberFormatInfo{NumberDecimalSeparator = ","});
                    result.Add(Money(title, valueAmount, "RUB"));
                }
            }
            
            return result;
        }
    }
}