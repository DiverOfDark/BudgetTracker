using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using BudgetTracker.Model;
using JetBrains.Annotations;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class ModulbankScraper : GenericScraper
    {
        public ModulbankScraper(ObjectRepository repository) : base(repository)
        {
        }

        public override string ProviderName => "МодульБанк";
        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;
            driver.Navigate().GoToUrl(@"https://my.modulbank.ru/");
            var name = GetElement(driver, By.Name("tel"));
            var pass = GetElement(driver, By.Name("password"));
            name.Click();
            foreach (var k in configuration.Login) 
            {
                chrome.SendKeys(k.ToString());
                WaitForPageLoad(driver);
            }
            pass.Click();
            chrome.SendKeys(configuration.Password);

            var now = DateTime.UtcNow;
            chrome.SendKeys(Keys.Return);
            
            GetElement(driver, By.Name("smsCode")).Click();

            bool success = false;
            while (DateTime.UtcNow - now < TimeSpan.FromMinutes(15))
            {
                var lastSms = Repository.Set<SmsModel>().Where(v=>v.When > now.AddMinutes(-3)).OrderByDescending(v => v.When).FirstOrDefault();
                if (lastSms?.Message.Contains("Код подтверждения") == true)
                {
                    var code = new string(lastSms.Message.Where(char.IsDigit).ToArray());
                    chrome.SendKeys(code);
                    success = true;
                    break;
                }
                WaitForPageLoad(driver);
            }

            if (!success)
                throw new Exception();

            WaitForPageLoad(driver, 15);

            var accounts = driver.FindElementsByClassName("bank_account");

            var result = new List<MoneyStateModel>();

            foreach (var acc in accounts)
            {
                var title = acc.FindElement(By.ClassName("bank_account_name")).Text;

                var amount = acc.FindElement(By.ClassName("bank_account_money")).Text;

                amount = new string(amount.Where(v => char.IsDigit(v) || v == ',').ToArray());

                var doubleAmount = double.Parse(amount, new NumberFormatInfo() {NumberDecimalSeparator = ","});

                result.Add(Money(title, doubleAmount, CurrencyExtensions.RUB));
            }
            return result;
        }
    }
}