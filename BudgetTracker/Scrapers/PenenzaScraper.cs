using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using BudgetTracker.Model;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BudgetTracker.Scrapers
{
    internal class PenenzaScraper : GenericScraper
    {
        public PenenzaScraper(ObjectRepository repository) : base(repository)
        {
        }

        public override string ProviderName => "Penenza";
        
        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, ChromeDriver driver)
        {
            driver.Navigate().GoToUrl(@"https://my.penenza.ru/main/sso/Login.aspx");
            var name = GetElement(driver, By.Id("MainContent_txtUserName"));
            var pass = GetElement(driver, By.Id("MainContent_txtUserPassword"));
            name.Click();
            driver.Keyboard.SendKeys(configuration.Login);
            pass.Click();
            driver.Keyboard.SendKeys(configuration.Password);
            driver.Keyboard.PressKey(Keys.Return);

            var portlets = GetElements(driver, By.ClassName("investor-dashboard__portlet-wrapper"));

            var firstPortlet = portlets.First();

            var rows = firstPortlet.FindElements(By.TagName("tr"));
            WaitForPageLoad(driver, 10);
            var result = new List<MoneyStateModel>();

            foreach (var row in rows)
            {
                try
                {
                    var td = row.FindElement(By.ClassName("investor-dashboard__table-title-col"));
                    var value = row.FindElement(By.ClassName("investor-dashboard__table-value-col"));

                    try
                    {
                        var subTd = td.FindElement(By.TagName("a"));
                        if (subTd != null)
                            td = subTd;
                    }
                    catch
                    {
                    }

                    var acc = td.Text;
                    var text = value.Text;
                    var doubleValue = ParseDouble(text);
                    var mm = Money(acc, doubleValue, "RUB");
                    result.Add(mm);
                }
                catch
                {
                }
            }

            var lastPortlet = portlets.Last();
            
            rows = lastPortlet.FindElements(By.TagName("tr"));

            double debtValue = 0;
            
            foreach (var row in rows)
            {
                try
                {
                    var td = row.FindElement(By.ClassName("investor-dashboard__table-title-col"));
                    var value = row.FindElement(By.ClassName("investor-dashboard__table-value-col"));

                    try
                    {
                        var subTd = td.FindElement(By.TagName("a"));
                        if (subTd != null)
                            td = subTd;
                    }
                    catch
                    {
                    }

                    var acc = td.Text;
                    var text = value.Text;

                    if (acc.ToLower().Contains("просрочено"))
                    {
                        debtValue += ParseDouble(text);
                    }
                }
                catch
                {
                }
            }

            result.Add(Money("Просрочка", debtValue, "RUB"));
            
            return result;
        }

        private static double ParseDouble(string text)
        {
            var valueText = new string(text.Where(v => char.IsDigit(v) || v == ',').ToArray());
            var doubleValue = double.Parse(valueText, new NumberFormatInfo() {NumberDecimalSeparator = ","});
            return doubleValue;
        }
    }
}