using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using BudgetTracker.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;

namespace BudgetTracker.Scrapers
{
    public abstract class GenericScraper
    {
        public GenericScraper(ObjectRepository repository)
        {
            Repository = repository;
        }
        
        public abstract string ProviderName { get; }

        public ObjectRepository Repository { get; }
        
        public abstract IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome driver);

        public virtual IList<PaymentModel> ScrapeStatement(ScraperConfigurationModel configuration, Chrome driver, DateTime startFrom)
        {
            return new List<PaymentModel>();
        }

        protected void WaitForPageLoad(ChromeDriver driver, int times = 5)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(0.5));
            wait.Until(wd => wd.ExecuteJavaScript<string>("return document.readyState") == "complete");
            Thread.Sleep(times * 1000);
        }
        
        protected IWebElement GetElement(ChromeDriver driver, By currencySpan)
        {
            var wt = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            var amountWait = wt.Until(d => d.FindElement(currencySpan));
            return amountWait;
        }

        protected IEnumerable<IWebElement> GetElements(ChromeDriver driver, By by)
        {
            ReadOnlyCollection<IWebElement> elements = null;

            for (int count = 0; count < 20 && (elements?.Count ?? 0) == 0; count++)
            {
                WaitForPageLoad(driver);
                elements = driver.FindElements(by);
            }

            return elements;
        }

        protected MoneyStateModel Money(string account, double amount, string ccy) => new MoneyStateModel
        {
            Provider = ProviderName,
            AccountName = account,
            When = DateTime.UtcNow.Date,
            Ccy = ccy,
            Amount = amount
        };

        protected PaymentModel Statement(DateTime when, string account, string what, double amount, PaymentKind kind, string ccy,
            string statementReference)
        {
            var column = Repository.Set<MoneyColumnMetadataModel>().FirstOrDefault(v => v.Provider == ProviderName && v.AccountName == account);

            if (column == null)
            {
                column = new MoneyColumnMetadataModel(ProviderName, account)
                {
                    UserFriendlyName = account
                };
                Repository.Add(column);
            }

            return new PaymentModel(when, what, amount, kind, ccy, statementReference, column);
        }
    }
}