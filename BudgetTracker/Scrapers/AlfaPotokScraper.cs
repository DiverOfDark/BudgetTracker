using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using BudgetTracker.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

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

                var doubleValue = ParseDouble(value);

                result.Add(Money(key, doubleValue, "RUB"));
            }

            var btn = GetElement(driver, By.PartialLinkText("Мои инвестиции"));
            btn.Click();
            
            WaitForPageLoad(driver);

            var select = GetElement(driver, By.TagName("select"));
            var sel = new SelectElement(select);
            sel.SelectByText("Текущие");
            
            WaitForPageLoad(driver);
            
            var table = GetElement(driver, By.ClassName("table--investment"));
            var tbody = table.FindElement(By.TagName("tbody"));

            var rows = tbody.FindElements(By.TagName("tr"));

            var risked = 0.0;

            for (var index = 0; index < rows.Count; index++)
            {
                if (index % 10 == 0)
                {
                    Console.WriteLine($"Parsing expiring status for AlfaPotok: {((double)index / rows.Count):P2}%");
                }
                
                var v = rows[index];
                var cells = v.FindElements(By.TagName("td"));

                var status = cells[11].Text;
                if (status.ToLower() == "выплачено")
                    continue;

                var when = cells[1].Text;
                var duration = cells[10].Text;

                var whenDate = DateTime.ParseExact(when, "dd.MM.yyyy", CultureInfo.CurrentCulture);
                var durationDays = ParseDouble(duration);

                var expirationDate = whenDate.AddDays(durationDays);

                if (expirationDate > DateTime.Now)
                    continue;

                var invested = cells[3].Text;
                var returned = cells[4].Text;

                var investedDouble = ParseDouble(invested);
                var returnedDouble = ParseDouble(returned);

                var delta = investedDouble - returnedDouble;
                if (delta > 0)
                    risked += delta;
            }

            result.Add(Money("Просрочка", risked, "RUB"));            
            
            return result;
        }

        private static double ParseDouble(string value)
        {
            value = new string(value.Where(v => char.IsDigit(v) || v == ',').ToArray());

            var doubleValue = double.Parse(value, new NumberFormatInfo() {NumberDecimalSeparator = ","});
            return doubleValue;
        }
    }
}