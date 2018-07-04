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
    internal class ModulDengiScraper : GenericScraper
    {
        public ModulDengiScraper(ObjectRepository repository) : base(repository)
        {
        }

        public override string ProviderName => "МодульДеньги";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, ChromeDriver driver)
        {
            driver.Navigate().GoToUrl("https://cabinet.moduldengi.ru/#/");

            var auth = GetElement(driver, By.ClassName("auth"));
            var phone = auth.FindElement(By.TagName("input"));
            phone.Click();
            foreach (var ch in configuration.Login)
            {
                driver.Keyboard.SendKeys(ch.ToString());
                WaitForPageLoad(driver);
            }

            driver.Keyboard.PressKey(Keys.Enter);
            WaitForPageLoad(driver, 5);

            var inputs = auth.FindElements(By.TagName("input"));
            var pass = inputs.First(v => v.GetAttribute("id") != phone.GetAttribute("id"));
            pass.Click();
            driver.Keyboard.SendKeys(configuration.Password);
            driver.Keyboard.PressKey(Keys.Return);

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
                WaitForPageLoad(driver,5);
            }

            var blockMenu = investmentsSection.FindElement(By.ClassName("block-menu"));

            var links = blockMenu.FindElements(By.TagName("a"));

            var badOnesButton = links.First(v => v.Text.ToLower() == "просроченные");
            badOnesButton.Click();

            WaitForPageLoad(driver);
            
            var badProjects = investmentsSection.FindElements(By.ClassName("project-item-wrapper"));
            var titles = badProjects.Select(v => v.FindElement(By.ClassName("project-details"))).ToList();
            var spans = titles.Select(v => v.FindElement(By.TagName("span"))).ToList();
            var innerTexts = spans.Select(v => v.Text).ToList();

            var innerValues = innerTexts.Select(ParseDouble).ToList();
            
            result.Add(Money("Просрочки", innerValues.Sum(), "RUB"));

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