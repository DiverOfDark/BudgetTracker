using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using BudgetTracker.Model;
using Microsoft.EntityFrameworkCore.Internal;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace BudgetTracker.Scrapers
{
    internal class AlfabankScraper : GenericScraper, IStatementScraper
    {
        public override string ProviderName => "Альфа-Банк";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, ChromeDriver driver)
        {
            Login(configuration, driver);

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

        public IEnumerable<PaymentModel> ScrapeStatement(ScraperConfigurationModel configuration, Chrome chromeDriver,
            DateTime startFrom)
        {
            if (startFrom < DateTime.Now.AddYears(-2).AddDays(1))
            {
                startFrom = DateTime.Now.AddYears(-2).AddDays(1);
            }

            var driver = chromeDriver.Driver;
            
            Login(configuration, driver);

            var existingLinks = driver.FindElementsByPartialLinkText("История операций").ToList();

            var link1 = (RemoteWebElement) driver.FindElementByLinkText("Счета");
            driver.Mouse.MouseMove(link1.Coordinates, 1, 1);
            
            Thread.Sleep(500); 
            
            existingLinks = driver.FindElementsByPartialLinkText("История операций").Except(existingLinks).ToList();
            existingLinks.Single().Click();

            Thread.Sleep(2000); 
            
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

                Thread.Sleep(2000); 
            
                tds[0].Click();

                Thread.Sleep(2000); 


                var inputDate = driver.FindElementById("pt1:id1::fd");
                inputDate.Click();

                Thread.Sleep(2000); 
            
                driver.Keyboard.SendKeys(Enumerable.Repeat(Keys.Delete, 20).Join(""));
                Thread.Sleep(500);
                driver.Keyboard.SendKeys(Enumerable.Repeat(Keys.Backspace, 20).Join(""));
                Thread.Sleep(500);
                driver.Keyboard.SendKeys(Enumerable.Repeat(Keys.Delete, 20).Join(""));
                Thread.Sleep(500);
                driver.Keyboard.SendKeys(Enumerable.Repeat(Keys.Backspace, 20).Join(""));
                Thread.Sleep(500);
                driver.Keyboard.SendKeys(startFrom.ToString("ddMMyyyy"));

                var submit = driver.FindElementById("pt1:showButton::button");
                submit.Click();

                Thread.Sleep(2000); 

                var csv = driver.FindElementById("pt1:downloadCSVLink");
                csv.Click();

                int waited = 0;
                while (chromeDriver.GetDownloads().Count < 1 && waited < 300)
                {
                    Thread.Sleep(1000);
                    waited++;
                }

                var files = chromeDriver.GetDownloads();
                if (files.Count == 1)
                {
                    var csvFile = files.First();
                    var csvContent = File.ReadAllLines(csvFile.FullName, Encoding.GetEncoding(1251)).Skip(1).Select(v=>new AlphaStatement(v)).ToList();
                    var payments = csvContent.Select(v =>
                        Statement(v.Date, v.AccountName, v.What, v.Outcome - v.Income, v.Ccy, v.Reference)).ToList();

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
        }

        private void Login(ScraperConfigurationModel configuration, ChromeDriver driver)
        {
            driver.Navigate().GoToUrl(@"https://click.alfabank.ru/");
            var name = GetElement(driver, By.Name("username"));
            var pass = GetElement(driver, By.Name("password"));
            name.Click();
            driver.Keyboard.SendKeys(configuration.Login);
            pass.Click();
            driver.Keyboard.SendKeys(configuration.Password);
            driver.Keyboard.PressKey(Keys.Return);
        }
    }
}