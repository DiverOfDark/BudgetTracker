using System;
using System.Collections.Generic;
using System.Globalization;
using BudgetTracker.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace BudgetTracker.Scrapers
{
    internal class AlfabankScraper : GenericScraper
    {
        public override string ProviderName => "Альфа-Банк";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, ChromeDriver driver)
        {
            driver.Navigate().GoToUrl(@"https://click.alfabank.ru/");
            var name = GetElement(driver, By.Name("username"));
            var pass = GetElement(driver, By.Name("password"));
            name.Click();
            driver.Keyboard.SendKeys(configuration.Login);
            pass.Click();
            driver.Keyboard.SendKeys(configuration.Password);
            driver.Keyboard.PressKey(Keys.Return);

            var link = GetElement(driver, By.PartialLinkText("Все счета"));
            link.Click();

            var result = new List<MoneyStateModel>();

            var wt = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            wt.Until(x =>
            {
                try
                {
                    return driver.FindElement(By.ClassName("interactiveTable")).FindElements(By.TagName("tr")).Count > 3;
                }
                catch
                {
                    return false;
                }
            });

            foreach (var row in GetElement(driver, By.ClassName("interactiveTable")).FindElements(By.TagName("tr")))
            {
                var cells = row.FindElements(By.TagName("td"));

                var label = cells[0];
                var amount = cells[2];
                var ccy = cells[3];

                var labelText = label.Text;
                var amountText = amount.Text;
                var ccyText = ccy.Text;

                result.Add(Money(labelText,
                    double.Parse(amountText.Replace(" ", ""), new NumberFormatInfo() {NumberDecimalSeparator = "."}),
                    ccyText));
            }

            return result;
        }
    }
}