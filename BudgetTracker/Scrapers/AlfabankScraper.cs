using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BudgetTracker.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class AlfabankScraper : GenericScraper
    {
        public AlfabankScraper(ObjectRepository repository, ILoggerFactory factory) : base(repository, factory)
        {
        }

        public override string ProviderName => "Альфа-Банк";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;
            Login(configuration, chrome);

            var link = GetElement(driver, By.PartialLinkText("Все счета"));
            link.Click();

            var result = new List<MoneyStateModel>();

            var wt = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            wt.Until(x =>
            {
                try
                {
                    return driver.FindElement(By.ClassName("interactiveTable")).FindElements(By.TagName("tr")).Count > 3;
                }
                catch
                {
                    return false;
                }
            });

            foreach (var row in GetElement(driver, By.ClassName("interactiveTable")).FindElements(By.TagName("tr")))
            {
                var cells = row.FindElements(By.TagName("td"));

                var label = cells[0];
                var amount = cells[2];
                var ccy = cells[3];

                var labelText = label.Text;
                var amountText = amount.Text;
                var ccyText = ccy.Text;

                result.Add(Money(labelText,
                    double.Parse(amountText.Replace(" ", ""), new NumberFormatInfo() {NumberDecimalSeparator = "."}),
                    ccyText));
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

            var existingLinks = driver.FindElementsByPartialLinkText("История операций").ToList();

            var link1 = (RemoteWebElement) driver.FindElementByLinkText("Счета");
            chromeDriver.MoveToElement(link1, 1, 1);
            
            WaitForPageLoad(driver); 
            
            existingLinks = driver.FindElementsByPartialLinkText("История операций").Except(existingLinks).ToList();
            existingLinks.Single().Click();

            WaitForPageLoad(driver, 2); 
            
            var selectBtn = driver.FindElementById("pt1:soc1::button");

            var accountsChooser = driver.FindElementById("pt1:soc1::pop");

            var accs = accountsChooser.FindElements(By.TagName("tr"));

            var result = new List<PaymentModel>();

            foreach (var acc in accs)
            {
                var tds = acc.FindElements(By.TagName("td"));
                if (tds.Count < 4)
                    continue;

                selectBtn.Click();

                WaitForPageLoad(driver, 2); 
            
                tds[0].Click();

                WaitForPageLoad(driver, 2); 


                var inputDate = driver.FindElementById("pt1:id1::fd");
                inputDate.Click();

                WaitForPageLoad(driver, 2); 
            
                chromeDriver.SendKeys(Enumerable.Repeat(Keys.Delete, 20).Join(""));
                WaitForPageLoad(driver);
                chromeDriver.SendKeys(Enumerable.Repeat(Keys.Backspace, 20).Join(""));
                WaitForPageLoad(driver);
                chromeDriver.SendKeys(Enumerable.Repeat(Keys.Delete, 20).Join(""));
                WaitForPageLoad(driver);
                chromeDriver.SendKeys(Enumerable.Repeat(Keys.Backspace, 20).Join(""));
                WaitForPageLoad(driver);
                chromeDriver.SendKeys(startFrom.ToString("ddMMyyyy"));

                var submit = driver.FindElementById("pt1:showButton::button");
                submit.Click();

                WaitForPageLoad(driver, 2); 

                var csv = driver.FindElementById("pt1:downloadCSVLink");
                csv.Click();

                int waited = 0;
                while (chromeDriver.GetDownloads().Count < 1 && waited < 300)
                {
                    WaitForPageLoad(driver);
                    waited++;
                }

                var files = chromeDriver.GetDownloads();
                if (files.Count == 1)
                {
                    var csvFile = files.First();
                    var csvContent = File.ReadAllLines(csvFile.FullName, Encoding.GetEncoding(1251)).Skip(1).Select(v=>new AlphaStatement(v)).ToList();
                    var payments = csvContent.Select(v => Statement(v.Date, v.AccountName, v.What, v.Income - v.Outcome, v.Kind, v.Ccy, v.Reference)).ToList();

                    var holdPayments = payments.Where(v => v.StatementReference == "HOLD").ToList();
                    payments = payments.Except(holdPayments).ToList();
                    
                    result.AddRange(payments);
                    csvFile.Delete();
                }

                chromeDriver.CleanupDownloads();
            }
            
            
            return result;
        }

        internal class AlphaStatement
        {
            public AlphaStatement(string csvLine)
            {
                var fields = csvLine.Split(";");

                AccountName = fields[0];
                AccountNumber = fields[1];
                Ccy = fields[2];
                Date = DateTime.ParseExact(fields[3], "dd.MM.yy", CultureInfo.InvariantCulture);
                Reference = fields[4];
                What = fields[5];
                Income = ConvertToDouble(fields[6]);
                Outcome = ConvertToDouble(fields[7]);

                var match = Regex.Match(What, @"\d{2}\.\d{2}\.\d{2} (?<when>\d{2}\.\d{2}\.\d{2})"); // Because CC operations in statement are shown in transaction posted date, not when transaction began. 
                if (match.Success)
                {
                    Date = DateTime.ParseExact(match.Groups["when"].Value, "dd.MM.yy", CultureInfo.InvariantCulture);
                }
            }

            private double ConvertToDouble(string p0) => double.Parse(p0.Replace(",", "."), new NumberFormatInfo() {NumberDecimalSeparator = "."});

            public string AccountName { get; }
            public string AccountNumber { get; set; }
            public string Ccy { get; set; }
            public DateTime Date { get; set; }
            public string Reference { get; set; }
            public string What { get; set; }
            public double Income { get; set; }
            public double Outcome { get; set; }

            public PaymentKind Kind => Outcome - Income > 0 ? PaymentKind.Expense : PaymentKind.Income;
        }

        private void Login(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;
            driver.Navigate().GoToUrl(@"https://click.alfabank.ru/");
            WaitForPageLoad(chrome.Driver, 2);
            var name = GetElement(driver, By.TagName("input"));
            name.Click();
            chrome.SendKeys(configuration.Login);
            chrome.SendKeys(Keys.Return);
            
            var pass = GetElement(driver, By.TagName("input"));
            pass.Click();
            chrome.SendKeys(configuration.Password);
            chrome.SendKeys(Keys.Return);
            
            WaitForPageLoad(chrome.Driver);
        }
    }
}