using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using BudgetTracker.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
        
        public abstract IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, ChromeDriver driver);

        public virtual IList<PaymentModel> ScrapeStatement(ScraperConfigurationModel configuration, Chrome driver, DateTime startFrom)
        {
            // default strategy - generated money statements from diff between states.
            var states = Repository.Set<MoneyStateModel>().Where(v => v.Provider == configuration.ScraperName && v.When >= startFrom)
                .GroupBy(v => v.AccountName).ToList();

            var payments = Repository.Set<PaymentModel>()
                .Where(v => v.Column != null && v.Column?.Provider == configuration.ScraperName && v.When >= startFrom)
                .GroupBy(v => v.Column.AccountName)
                .ToDictionary(v => v.Key, v => v.ToList());

            var result = new List<PaymentModel>();
            
            foreach (var accountState in states)
            {
                var accountName = accountState.Key;
                var accountStates = accountState.OrderBy(v => v.When).ToList();
                var existingPayments = payments.GetValueOrDefault(accountName, new List<PaymentModel>());

                accountStates.Aggregate((a, b) =>
                {
                    var delta = b.Amount - a.Amount;

                    var appliedPayments = existingPayments.Where(v =>
                        v.Column.AccountName == accountName && v.When >= a.When && v.When <= b.When).ToList();

                    foreach (var item in appliedPayments)
                    {
                        switch (item.Kind)
                        {
                            case PaymentKind.Expense:
                                delta -= Math.Abs(item.Amount);
                                break;
                            case PaymentKind.Income:
                                delta += Math.Abs(item.Amount);
                                break;
                            case PaymentKind.Transfer:
                                delta += item.Amount;
                                break;
                        }
                    }

                    if (Math.Abs(delta) >= 0.01)
                    {
                        var when = a.When + (b.When - a.When) / 2;
                        result.Add(Statement(when, accountName, "Коррекция баланса " + ProviderName + " " + accountName, delta, a.Ccy, "N/A-" + DateTime.Now.Ticks));
                    }
                    
                    return b;
                });
            }

            return result;
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
                Thread.Sleep(500);
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

        protected PaymentModel Statement(DateTime when, string account, string what, double amount, string ccy,
            string statementReference)
        {
            var column = Repository.Set<MoneyColumnMetadataModel>().FirstOrDefault(v => v.Provider == ProviderName && v.AccountName == account);

            if (column == null)
            {
                column = new MoneyColumnMetadataModel(ProviderName, account)
                {
                    UserFriendlyName = account,
                    IsVisible = true
                };
                Repository.Add(column);
            }

            return new PaymentModel(when, what, amount, ccy, statementReference, column);
        }
    }
}