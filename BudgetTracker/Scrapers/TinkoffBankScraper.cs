using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using BudgetTracker.Model;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class TinkoffBankScraper : GenericScraper
    {
        public TinkoffBankScraper(ObjectRepository repository, ILoggerFactory factory) : base (repository, factory)
        {
            
        }

        public override string ProviderName => "Тинькофф Банк";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;
            Login(configuration, chrome);

            var links = GetElements(driver, By.TagName("a")).ToList();
            var matchingLinks = links.Where(v => v.GetProperty("href").Contains("/events/account/")).ToList();

            var result = new List<MoneyStateModel>();

            foreach (var link in matchingLinks)
            {
                var children = link.FindElements(By.TagName("div"));
                var name = children.FirstOrDefault(v => v.GetAttribute("class").Contains("Item__name"));
                var balance = children.FirstOrDefault(v => v.GetAttribute("class").Contains("Item__balance"));

                if (name == null || balance == null)
                    continue;
                
                var nameText = name.Text;
                var balanceText = balance.Text;
                var balanceTextClear = new string(balanceText.Where(v => char.IsDigit(v) || v == ',').ToArray());
                var balanceValue = double.Parse(balanceTextClear.Replace(" ", ""));

                var ccy = "RUB";
                if (balanceText.Contains("$"))
                    ccy = "USD";
                if (balanceText.Contains("€"))
                    ccy = "EUR";
                
                result.Add(Money(nameText, balanceValue, ccy));
            }

            return result;
        }
        
         public override IList<PaymentModel> ScrapeStatement(ScraperConfigurationModel configuration, Chrome chromeDriver,
            DateTime startFrom)
        {
            if (startFrom < DateTime.Now.AddYears(-2).AddDays(1))
            {
                startFrom = DateTime.Now.AddYears(-2).AddDays(1);
            }

            var driver = chromeDriver.Driver;
            
            Login(configuration, chromeDriver);

            var unixTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var startTime = (long)(startFrom - unixTime).TotalMilliseconds; 
            var endTime = (long)(DateTime.UtcNow - unixTime).TotalMilliseconds; 
            var cardId = 0;

            var sessionId = driver.Manage().Cookies.GetCookieNamed("psid").Value;
            
            var links = GetElements(driver, By.TagName("a")).ToList();
            var matchingLinks = links.Where(v => v.GetProperty("href").Contains("/events/account/")).ToList();

            var result = new List<PaymentModel>();

            var accountRegex = new Regex(@"\/events\/account\/([^\/]+)\/(?<accountId>[0-9]+)");
            foreach (var link in matchingLinks)
            {
                var href = link.GetAttribute("href");
                if (href  == null)
                    continue;
                
                var match = accountRegex.Match(href);
                if (match.Success)
                {
                    var children = link.FindElements(By.TagName("div"));
                    var name = children.First(v => v.GetAttribute("class").Contains("Item__name"));
                    var accountName = name.Text;
                    
                    var accountId = match.Groups["accountId"].Value;

                    var url = $"https://www.tinkoff.ru/api/common/v1/export_operations/?format=ofx&sessionid={sessionId}&start={startTime}&end={endTime}&card={cardId}&account={accountId}";

                    driver.Navigate().GoToUrl(url);

                    int waited = 0;
                    while (chromeDriver.GetDownloads().Count < 1 && waited < 300)
                    {
                        WaitForPageLoad(driver);
                        waited++;
                    }

                    var file = chromeDriver.GetDownloads()[0].FullName;

                    var contents = File.ReadAllText(file);
                    var ofx = XDocument.Parse(contents);
                    var payments = ParseOfx(ofx, accountName);
                    result.AddRange(payments);
                    File.Delete(file);
                }
            }

            return result;
        }
         
         private void Login(ScraperConfigurationModel configuration, Chrome chrome)
         {
             var driver = chrome.Driver;
             driver.Navigate().GoToUrl(@"https://www.tinkoff.ru/login/");
             var name = GetElement(driver, By.Name("login"));
             name.Click();
             chrome.SendKeys(configuration.Login);
             chrome.SendKeys(Keys.Enter);
             var smsCode = GetElement(driver, By.Name("code"));
             
             var smsModel = WaitForSms(() => {}, s => s.Message.Contains("код") && s.Message.Contains("Tinkoff.ru"));
             smsCode.Click();
             var code = new string(smsModel.Message.Where(char.IsDigit).Take(4).ToArray());

             chrome.SendKeys(code);

             var password = GetElement(driver, By.Name("password")); 
             password.Click();
             chrome.SendKeys(configuration.Password);
             chrome.SendKeys(Keys.Return);
            
             WaitForPageLoad(chrome.Driver);
         }
    }
}