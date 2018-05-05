using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using BudgetTracker.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BudgetTracker.Scrapers
{
    internal class ModulbankScraper : GenericScraper
    {
        private readonly ObjectRepository _repository;

        public ModulbankScraper(ObjectRepository repository)
        {
            _repository = repository;
        }

        public override string ProviderName => "МодульБанк";
        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, ChromeDriver driver)
        {
            driver.Navigate().GoToUrl(@"https://my.modulbank.ru/");
            var name = GetElement(driver, By.Name("tel"));
            var pass = GetElement(driver, By.Name("password"));
            name.Click();
            foreach (var k in configuration.Login) 
            {
                driver.Keyboard.SendKeys(k.ToString());
                Thread.Sleep(100);
            }
            pass.Click();
            driver.Keyboard.SendKeys(configuration.Password);

            var now = DateTime.UtcNow;
            driver.Keyboard.PressKey(Keys.Return);
            
            GetElement(driver, By.Name("smsCode")).Click();

            bool success = false;
            while (DateTime.UtcNow - now < TimeSpan.FromMinutes(15))
            {
                var lastSms = _repository.Set<SmsModel>().Where(v=>v.When > now.AddMinutes(-3)).OrderByDescending(v => v.When).FirstOrDefault();
                if (lastSms?.Message.Contains("Код подтверждения") == true)
                {
                    var code = new string(lastSms.Message.Where(char.IsDigit).ToArray());
                    driver.Keyboard.SendKeys(code);
                    success = true;
                    break;
                }
                Thread.Sleep(100);
            }

            if (!success)
                throw new Exception();

            Thread.Sleep(15000); // this should be enough for all data to load...

            var accounts = driver.FindElementsByClassName("bank_account");

            var result = new List<MoneyStateModel>();

            foreach (var acc in accounts)
            {
                var title = acc.FindElement(By.ClassName("bank_account_name")).Text;

                var amount = acc.FindElement(By.ClassName("bank_account_money")).Text;

                amount = new string(amount.Where(v => char.IsDigit(v) || v == ',').ToArray());

                var doubleAmount = double.Parse(amount, new NumberFormatInfo() {NumberDecimalSeparator = ","});

                result.Add(Money(title, doubleAmount, "RUB"));
            }
            return result;
        }
    }
}