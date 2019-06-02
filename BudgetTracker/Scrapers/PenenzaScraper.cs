using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetTracker.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class PenenzaScraper : GenericScraper
    {
        private ILogger<PenenzaScraper> _logger;

        public PenenzaScraper(ObjectRepository repository, ILoggerFactory factory) : base(repository, factory)
        {
            _logger = factory.CreateLogger<PenenzaScraper>();
        }

        public override string ProviderName => "Penenza";
        
        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;
            driver.Navigate().GoToUrl(@"https://my.penenza.ru/main/sso/Login.aspx");
            var name = GetElement(driver, By.Id("MainContent_txtUserName"));
            var pass = GetElement(driver, By.Id("MainContent_txtUserPassword"));
            name.Click();
            chrome.SendKeys(configuration.Login);
            pass.Click();
            chrome.SendKeys(configuration.Password);
            chrome.SendKeys(Keys.Return);

            WaitForPageLoad(driver, 10);
            driver.Navigate().GoToUrl("https://my.penenza.ru/main/tradefinancemvc/?forcedshow=1");
            WaitForPageLoad(driver, 10);

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
                    catch (Exception ex)
                    {
                        _logger.LogError("Penenza scraping error", ex);
                    }

                    var acc = td.Text;
                    var text = value.Text;
                    var doubleValue = ParseDouble(text);
                    var mm = Money(acc, doubleValue, CurrencyExtensions.RUB);
                    result.Add(mm);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Penenza scraping error", ex);
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
                    catch (Exception ex)
                    {
                        _logger.LogError("Penenza scraping error", ex);
                    }

                    var acc = td.Text;
                    var text = value.Text;

                    if (acc.ToLower().Contains("просрочено"))
                    {
                        debtValue += ParseDouble(text);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Penenza scraping error", ex);
                }
            }

            result.Add(Money("Просрочка", debtValue, CurrencyExtensions.RUB));
            
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