using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetTracker.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BudgetTracker.Scrapers
{
    internal class RaiffeisenScraper : GenericScraper
    {
        public override string ProviderName => "Райффайзен";
        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, ChromeDriver driver)
        {
            driver.Navigate().GoToUrl(@"https://online.raiffeisen.ru/");
            var name = GetElement(driver, By.Name("username"));
            var pass = GetElement(driver, By.Name("password"));
            name.Click();
            driver.Keyboard.SendKeys(configuration.Login);
            pass.Click();
            driver.Keyboard.SendKeys(configuration.Password);
            driver.Keyboard.PressKey(Keys.Return);

            var amountWait = GetElement(driver, By.ClassName("rc-currency"));

            var amount = amountWait.Text;

            var amountClear = new string(amount.Where(v=>char.IsDigit(v) || v == ',').ToArray());

            var amountNumber = double.Parse(amountClear, new NumberFormatInfo()
            {
                NumberDecimalSeparator = ","
            });

            return new[]
            {
                Money("ЗП", amountNumber, "RUB")
            };
        }
    }
}