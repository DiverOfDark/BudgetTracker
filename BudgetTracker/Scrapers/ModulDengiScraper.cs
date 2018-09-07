using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetTracker.Model;
using JetBrains.Annotations;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class ModulDengiScraper : GenericScraper
    {
        public ModulDengiScraper(ObjectRepository repository) : base(repository)
        {
        }

        public override string ProviderName => "МодульДеньги";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;
            driver.Navigate().GoToUrl("https://cabinet.moduldengi.ru/#/");

            var auth = GetElement(driver, By.ClassName("auth"));
            var phone = auth.FindElement(By.TagName("input"));
            phone.Click();
            foreach (var ch in configuration.Login)
            {
                chrome.SendKeys(ch.ToString());
                WaitForPageLoad(driver);
            }

            chrome.SendKeys(Keys.Enter);
            WaitForPageLoad(driver, 5);

            var inputs = auth.FindElements(By.TagName("input"));
            var pass = inputs.First(v => v.GetAttribute("id") != phone.GetAttribute("id"));
            pass.Click();
            chrome.SendKeys(configuration.Password);
            chrome.SendKeys(Keys.Return);

            WaitForPageLoad(driver, 5);
            
            var row = GetElement(driver, By.ClassName("balances-row"));

            var cells = row.FindElements(By.ClassName("balances-item"));

            var result = new List<MoneyStateModel>();

            foreach (var cell in cells)
            {
                var label = cell.FindElement(By.ClassName("property")).Text;
                var value = cell.FindElement(By.ClassName("value")).Text;

                var cleanValue = ParseDouble(value);

                result.Add(Money(label, cleanValue, "RUB"));
            }

            var investmentsSection = GetElement(driver, By.ClassName("investments"));

            while (investmentsSection.Text.ToLower().Contains("загрузка"))
            {
                WaitForPageLoad(driver);
            }

            var blockMenu = investmentsSection.FindElement(By.ClassName("block-menu"));

            var links = blockMenu.FindElements(By.TagName("a"));

            var badOnesButton = links.First(v => v.Text.ToLower() == "просроченные");
            badOnesButton.Click();

            WaitForPageLoad(driver);
            
            var badProjects = investmentsSection.FindElements(By.ClassName("project-item-wrapper"));
            var titles = badProjects.Select(v => v.FindElement(By.ClassName("project-table"))).ToList();
            var spans = titles.Select(v =>
            {
                var rows = v.FindElements(By.TagName("tr")).Select(s=>s.FindElements(By.TagName("td")).ToList()).ToList();

                var parsed = rows.Select(s =>
                {
                    var key = s[0].Text;
                    var value = s[1];

                    var spanValue = ParseDouble(value.FindElement(By.TagName("span")).Text);

                    return new
                    {
                        key,
                        spanValue
                    };
                }).ToList();

                var inv = parsed.First(s => s.key.ToLower().Contains("инвестиция")).spanValue;
                var ok  = parsed.First(s => s.key.ToLower().Contains("погашено")).spanValue;
                
                return inv - ok;
            }).ToList();

            result.Add(Money("Просрочки", spans.Sum(), "RUB"));

            return result;
        }

        private static double ParseDouble(string value)
        {
            var cleanValueString = new string(value.Where(v => char.IsDigit(v) || v == '.').ToArray());

            var cleanValue = double.Parse(cleanValueString, new NumberFormatInfo
            {
                NumberDecimalSeparator = "."
            });
            return cleanValue;
        }
    }
}