using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using BudgetTracker.Model;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class AlfaPotokScraper : GenericScraper
    {
        private readonly ILogger<AlfaPotokScraper> _logger;

        public AlfaPotokScraper(ObjectRepository repository, ILoggerFactory factory) : base(repository)
        {
            _logger = factory.CreateLogger<AlfaPotokScraper>();
        }

        public override string ProviderName => "Альфа-Поток";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;
            driver.Navigate().GoToUrl(@"https://potok.digital/potok");
            
            var name = GetElement(driver, By.Name("username"));
            var pass = GetElement(driver, By.Name("password"));
            name.Click();
            chrome.SendKeys(configuration.Login);
            pass.Click();
            chrome.SendKeys(configuration.Password);
            chrome.SendKeys(Keys.Return);

            var result = new List<MoneyStateModel>();

            var accountTab = GetElement(driver, By.Id("account-tab"));

            var trs = accountTab.FindElements(By.ClassName("row"));

            foreach (var item in trs)
            {
                try
                {
                    var keyObj = item.FindElement(By.ClassName("col-sm-9"));
                    var valueObj = item.FindElement(By.ClassName("col-sm-3"));

                    keyObj = keyObj.FindElement(By.ClassName("font-bigger"));
                    valueObj = valueObj.FindElement(By.ClassName("font-bigger"));

                    var key = keyObj.Text;
                    var value = valueObj.Text;
                    
                    if (key.Contains("("))
                    {
                        key = key.Remove(key.IndexOf("(", StringComparison.Ordinal),
                            key.IndexOf(")", StringComparison.Ordinal) - key.IndexOf("(", StringComparison.Ordinal) +
                            1);
                    }

                    key = key.Trim();

                    var doubleValue = ParseDouble(value);

                    result.Add(Money(key, doubleValue, "RUB"));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse");
                }
            }

            var btn = GetElement(driver, By.PartialLinkText("Мои инвестиции"));
            btn.Click();
            
            WaitForPageLoad(driver, 10);
            
            var table = GetElement(driver, By.ClassName("table--investment"));
            var tbody = table.FindElement(By.TagName("tbody"));

            var rows = tbody.FindElements(By.TagName("tr"));

            var risked = 0.0;
            var totalInvested = 0.0;

            for (var index = 0; index < rows.Count; index++)
            {
                if (index % 10 == 0)
                {
                    _logger.LogInformation($"Parsing status for AlfaPotok: {((double)index / rows.Count):P2}");
                }
                
                var v = rows[index];
                var cells = v.FindElements(By.TagName("td"));

                var status = cells[11].Text;
                if (status.ToLower() == "выплачено")
                    continue;

                var when = cells[1].Text;
                var duration = cells[10].Text;

                when = new String(when.TakeWhile(s => s != '\n').Where(s => char.IsDigit(s) || s == '.').ToArray());
                
                var whenDate = DateTime.ParseExact(when, "dd.MM.yyyy", CultureInfo.CurrentCulture);
                var durationDays = ParseDouble(duration);

                var expirationDate = whenDate.AddDays(durationDays);

                var agreedPercentage = cells[2].Text;

                var invested = cells[3].Text;
                var returned = cells[4].Text;
                var returnedBody = cells[5].Text;

                var agreedPercentageDouble = ParseDouble(agreedPercentage);
                
                var investedDouble = ParseDouble(invested);
                var returnedDouble = ParseDouble(returned);
                var returnedBodyDouble = ParseDouble(returnedBody);
                
                var delta = investedDouble - returnedDouble;
                var bodyDelta = investedDouble - returnedBodyDouble;
                
                if (delta > 0)
                {
                    totalInvested += delta;
                }
                if (expirationDate < DateTime.Now && bodyDelta > 1) // Ignore all sums < 1 RUB.
                {
                    risked += bodyDelta * (1.0 + agreedPercentageDouble / 100.0);
                }
            }

            result.Add(Money("Просрочка", risked, "RUB"));            
            result.Add(Money("Инвестировано", totalInvested, "RUB"));            
            
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