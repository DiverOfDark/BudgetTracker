using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using BudgetTracker.Model;
using JetBrains.Annotations;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class RaiffeisenScraper : GenericScraper
    {
        public RaiffeisenScraper(ObjectRepository repository, ILoggerFactory logger) : base(repository, logger)
        {
        }

        public override string ProviderName => "Райффайзен";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;
            Login(configuration, chrome);

            driver.Navigate().GoToUrl(@"https://online.raiffeisen.ru/#/accounts");
            
            var accounts = GetElements(driver, By.TagName("account-widget"));

            var result = new List<MoneyStateModel>();
            
            foreach (var acc in accounts)
            {
                try
                {
                    var titleElement = acc.FindElement(By.ClassName("product-header-title__name-text"));
                    var text = titleElement.GetAttribute("textContent");

                    var amountWait = acc.FindElement(By.ClassName("product-header-info__value"));

                    var amount = amountWait.GetAttribute("textContent");
                    var amountClear = new string(amount.Where(v => char.IsDigit(v) || v == ',').ToArray());

                    var amountNumber = double.Parse(amountClear, new NumberFormatInfo()
                    {
                        NumberDecimalSeparator = ","
                    });

                    var ccySign = acc.FindElement(By.ClassName("amount__symbol"));
                    var ccyText = ccySign.GetAttribute("textContent");

                    result.Add(Money(text, amountNumber, ccyText));
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to parse row, continue", ex);
                } 
            }

            return result;
        }

        public override IList<PaymentModel> ScrapeStatement(ScraperConfigurationModel configuration, Chrome chrome, DateTime startFrom)
        {
            var driver = chrome.Driver;
            Login(configuration, chrome);

            driver.Navigate().GoToUrl(@"https://online.raiffeisen.ru/#/history/statement");

            var accounts = GetElements(driver, By.TagName("c-select-option-account"));

            var link = GetElements(driver, By.TagName("a")).First(v => v.GetAttribute("href")?.Contains("/transaction.ofx?") == true);
            var linkText = link.GetAttribute("href");
            
            var build = new Uri(linkText);

            var result = new List<PaymentModel>();
                
            var urlFormat = @"https://online.raiffeisen.ru/rest/account/{accountId}/transaction.ofx?from={from}&to={to}&sort=date&order=desc&access_token={token}";

            var originalQuery = QueryHelpers.ParseQuery(build.Query);

            var accessToken = originalQuery["access_token"].First();
            
            var accountDetails = accounts.Select(v =>
            {
                var id = v.FindElement(By.TagName("div")).GetAttribute("data-account-id");
                var textElement = v.FindElement(By.TagName("account-logo")).FindElement(By.XPath(".."));
                var name = textElement.GetAttribute("textContent").Trim();
                return (id, name);
            }).Distinct().ToList();

            Logger.LogInformation($"Found {accountDetails.Count} Raiffeisen accounts");
            
            foreach (var account in accountDetails)
            {
                var accountId = account.id;
                var accountName = account.name;
                var url = urlFormat.Replace("{accountId}", accountId)
                    .Replace("{from}", startFrom.ToString("yyyy-MM-ddTHH:mm"))
                    .Replace("{to}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm"))
                    .Replace("{token}", accessToken);
                
                driver.Navigate().GoToUrl(url);
                
                Logger.LogInformation($"Getting statement for {account.name} at {url}");

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
                    var ofxFile = files.First();

                    var doc = File.ReadAllText(ofxFile.FullName);

                    var xdoc = XDocument.Parse(doc);
                    
                    var statements = xdoc.XPathSelectElements("OFX/BANKMSGSRSV1/STMTTRNRS/STMTRS/BANKTRANLIST/STMTTRN");

                    var ccyNode = xdoc.XPathSelectElement("OFX/BANKMSGSRSV1/STMTTRNRS/STMTRS/CURDEF");

                    var ccy = ccyNode.Value;
                    
                    var payments = new List<PaymentModel>();
            
                    foreach (var st in statements)
                    {
                        var timeStr = st.Element("DTPOSTED").Value;
                        var time = DateTime.ParseExact(timeStr, "yyyyMMddhhmmss", CultureInfo.CurrentCulture);
                        var amount = double.Parse(st.Element("TRNAMT").Value, new NumberFormatInfo {NumberDecimalSeparator = "."});
                        var name = st.Element("MEMO").Value;
                        var id = st.Element("FITID").Value;

                        var kind = amount < 0 ? PaymentKind.Expense : PaymentKind.Income; 
                        
                        payments.Add(Statement(time, accountName, name, amount, kind, ccy, id));
                    }
                    
                    Logger.LogInformation($"Got {payments.Count} payments from {url}");
                    result.AddRange(payments);
                    ofxFile.Delete();
                }
            }

            return result;
        }

        private void Login(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;
            driver.Navigate().GoToUrl(@"https://online.raiffeisen.ru/");

            var name = GetElement(driver, By.ClassName("login-form__username-wrap")).FindElement(By.TagName("input"));
            var pass = GetElement(driver, By.ClassName("login-form__password-wrap")).FindElement(By.TagName("input"));
            name.Click();
            chrome.SendKeys(configuration.Login);
            pass.Click();
            chrome.SendKeys(configuration.Password);
            chrome.SendKeys(Keys.Return);
            
            WaitForPageLoad(driver, 5);
        }
    }
}