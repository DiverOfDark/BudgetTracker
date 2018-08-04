﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BudgetTracker.Model;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.Scrapers
{
    internal class RaiffeisenScraper : GenericScraper
    {
        private readonly ILogger<RaiffeisenScraper> _logger;

        public RaiffeisenScraper(ObjectRepository repository, ILogger<RaiffeisenScraper> logger) : base(repository)
        {
            _logger = logger;
        }

        public override string ProviderName => "Райффайзен";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, ChromeDriver driver)
        {
            Login(configuration, driver);

            driver.Navigate().GoToUrl(@"https://online.raiffeisen.ru/#/accounts");
            
            var accounts = GetElements(driver, By.TagName("account-widget"));

            var result = new List<MoneyStateModel>();
            
            foreach (var acc in accounts)
            {
                var titleElement = acc.FindElement(By.ClassName("product-header-title__name-text"));
                var text = titleElement.Text;
                
                var amountWait = acc.FindElement(By.ClassName("rc-currency"));
            
                var amount = amountWait.Text;
                var amountClear = new string(amount.Where(v=>char.IsDigit(v) || v == ',').ToArray());

                var amountNumber = double.Parse(amountClear, new NumberFormatInfo()
                {
                    NumberDecimalSeparator = ","
                });

                var ccySign = acc.FindElement(By.ClassName("rc-currency__sign"));
                var ccyText = ccySign.Text;
                
                result.Add(Money(text, amountNumber, ccyText));
            }

            return result;
        }

        public override IList<PaymentModel> ScrapeStatement(ScraperConfigurationModel configuration, Chrome chrome, DateTime startFrom)
        {
            var driver = chrome.Driver;
            Login(configuration, driver);

            driver.Navigate().GoToUrl(@"https://online.raiffeisen.ru/#/statement");

            var accounts = GetElements(driver, By.TagName("c-select-option-account"));

            var link = GetElements(driver, By.TagName("a")).First(v => v.GetAttribute("href")?.Contains("/transaction.csv?") == true);
            var linkText = link.GetAttribute("href");
            
            var build = new Uri(linkText);

            var result = new List<PaymentModel>();
                
            var urlFormat = @"https://online.raiffeisen.ru/rest/account/{accountId}/transaction.csv?from={from}&to={to}&sort=date&order=desc&access_token={token}";

            var originalQuery = QueryHelpers.ParseQuery(build.Query);

            var accessToken = originalQuery["access_token"].First();
            
            var accountDetails = accounts.Select(v =>
            {
                var id = v.FindElement(By.TagName("div")).GetAttribute("data-account-id");
                var textElement = v.FindElement(By.TagName("account-logo")).FindElement(By.XPath(".."));
                var name = textElement.GetAttribute("textContent").Trim();
                return (id, name);
            }).Distinct().ToList();

            _logger.LogInformation($"Found {accountDetails.Count} Raiffeisen accounts");
            
            foreach (var account in accountDetails)
            {
                var accountId = account.id;
                var accountName = account.name;
                var url = urlFormat.Replace("{accountId}", accountId)
                    .Replace("{from}", startFrom.ToString("yyyy-MM-ddTHH:mm"))
                    .Replace("{to}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm"))
                    .Replace("{token}", accessToken);
                
                driver.Navigate().GoToUrl(url);
                
                _logger.LogInformation($"Getting statement for {account.name} at {url}");

                int waited = 0;
                while (chrome.GetDownloads().Count < 1 && waited < 300)
                {
                    WaitForPageLoad(driver);
                    waited++;
                }
                
                Thread.Sleep(10000);

                var files = chrome.GetDownloads();
                if (files.Count == 1)
                {
                    var csvFile = files.First();
                    var csvContent = File.ReadAllLines(csvFile.FullName, Encoding.GetEncoding(1251)).Skip(1).Select(v=>new RaiffeisenStatement(v)).ToList();
                    var payments = csvContent.Select(v =>
                        Statement(v.When, accountName, v.What, v.Amount, v.Kind, v.Ccy, v.Reference)).ToList();

                    foreach (var group in payments.GroupBy(v => v.StatementReference).Where(v => v.Count() > 1))
                    {
                        var list = group.ToList();
                        for (var index  = 0; index < list.Count; index++)
                        {
                            list[index].StatementReference += "." + index;
                        }
                    }                    
                    
                    _logger.LogInformation($"Got {payments.Count} payments from {url}");

                    result.AddRange(payments);
                    csvFile.Delete();
                }

            }
            return result;
        }

        private void Login(ScraperConfigurationModel configuration, ChromeDriver driver)
        {
            driver.Navigate().GoToUrl(@"https://online.raiffeisen.ru/");
            var name = GetElement(driver, By.Name("username"));
            var pass = GetElement(driver, By.Name("password"));
            name.Click();
            driver.Keyboard.SendKeys(configuration.Login);
            pass.Click();
            driver.Keyboard.SendKeys(configuration.Password);
            driver.Keyboard.PressKey(Keys.Return);
            
            WaitForPageLoad(driver, 5);
        }
    }

    internal class RaiffeisenStatement
    {
        public RaiffeisenStatement(string s)
        {
            var values = s.Split(";");
            When = DateTime.ParseExact(values[0], "dd.MM.yyyy HH:mm", CultureInfo.CurrentCulture);
            What = values[1];
            Ccy = values[2];
            Amount = double.Parse(values[3].Replace(" ", "").Replace(".", ","),
                new NumberFormatInfo() {NumberDecimalSeparator = ","});
        }
        
        public DateTime When { get; }
        public string What { get; }
        public string Ccy { get; }
        public double Amount { get; }

        public string Reference => (When.Ticks + What + Ccy + Amount).ToMD5();
        public PaymentKind Kind => Amount < 0 ? PaymentKind.Expense : PaymentKind.Income;
    }
}