using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BudgetTracker.Model;
using Microsoft.AspNetCore.WebUtilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.Scrapers
{
    internal class RaiffeisenScraper : GenericScraper, IStatementScraper
    {
        public RaiffeisenScraper(ObjectRepository repository) : base(repository)
        {
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

        public IEnumerable<PaymentModel> ScrapeStatement(ScraperConfigurationModel configuration, Chrome chrome, DateTime startFrom)
        {
            var driver = chrome.Driver;
            Login(configuration, driver);

            driver.Navigate().GoToUrl(@"https://online.raiffeisen.ru/#/statement");

            var accounts = GetElements(driver, By.TagName("c-select-option-account"));

            var accountDetails = accounts.Select(v =>
            {
                var id = v.FindElement(By.TagName("div")).GetAttribute("data-account-id");
                var textElement = v.FindElement(By.TagName("account-logo")).FindElement(By.XPath(".."));
                var name = textElement.GetAttribute("textContent").Trim();
                return (id, name);
            }).Distinct().ToList();

            var link = GetElements(driver, By.TagName("a")).First(v => v.GetAttribute("href")?.Contains("/transaction.csv?") == true);
            var linkText = link.GetAttribute("href");
            
            var build = new Uri(linkText);

            var result = new List<PaymentModel>();
                
            var urlFormat = @"https://online.raiffeisen.ru/rest/account/{accountId}/transaction.csv?from={from}&to={to}&sort=date&order=desc&access_token={token}";

            var originalQuery = QueryHelpers.ParseQuery(build.Query);

            var accessToken = originalQuery["access_token"].First();
            
            foreach (var account in accountDetails)
            {
                var accountId = account.id;
                var accountName = account.name;
                var url = urlFormat.Replace("{accountId}", accountId)
                    .Replace("{from}", startFrom.ToString("yyyy-MM-ddTHH:mm"))
                    .Replace("{to}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm"))
                    .Replace("{token}", accessToken);
                
                driver.Navigate().GoToUrl(url);
                
                int waited = 0;
                while (chrome.GetDownloads().Count < 1 && waited < 300)
                {
                    Thread.Sleep(1000);
                    waited++;
                }

                var files = chrome.GetDownloads();
                if (files.Count == 1)
                {
                    var csvFile = files.First();
                    var csvContent = File.ReadAllLines(csvFile.FullName, Encoding.GetEncoding(1251)).Skip(1).Select(v=>new RaiffeisenStatement(v)).ToList();
                    var payments = csvContent.Select(v =>
                        Statement(v.When, accountName, v.What, -v.Amount, v.Ccy, v.Reference)).ToList();

                    var holdPayments = payments.Where(v => v.StatementReference == "HOLD").ToList();
                    payments = payments.Except(holdPayments).ToList();
                    
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
            
            Thread.Sleep(5000);
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
    }
}