using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using BudgetTracker.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class ModulbankScraper : GenericScraper
    {
        public ModulbankScraper(ObjectRepository repository, ILoggerFactory factory) : base(repository, factory)
        {
        }

        public override string ProviderName => "МодульБанк";

        private void DoLogin(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;
            driver.Navigate().GoToUrl(@"https://my.modulbank.ru/");
            var name = GetElement(driver, By.Name("tel"));
            var pass = GetElement(driver, By.Name("password"));
            name.Click();
            foreach (var k in configuration.Login) 
            {
                chrome.SendKeys(k.ToString());
                WaitForPageLoad(driver);
            }
            pass.Click();
            chrome.SendKeys(configuration.Password);

            chrome.SendKeys(Keys.Return);

            var smsButton = GetElement(driver, By.Name("smsCode"));

            var sms = WaitForSms(() => smsButton.Click(),
                s => s.Message.Contains("Код подтверждения"));
            
            var code = new string(sms.Message.Where(char.IsDigit).ToArray());
            chrome.SendKeys(code);

            WaitForPageLoad(driver, 15);

            IEnumerable<IWebElement> popups;
            do
            {
                popups = GetElements(driver, By.ClassName("popup_close"));
                foreach (var p in popups)
                {
                    try
                    {
                        p.Click();
                    }
                    catch
                    {
                        // Ignore underneath popup
                    }
                }
            } while (popups.Any());
        }
        
        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome chrome)
        {
            DoLogin(configuration, chrome);
            
            var driver = chrome.Driver;
            
            var accounts = GetElements(driver, By.ClassName("bank_account"));

            var result = new List<MoneyStateModel>();

            foreach (var acc in accounts)
            {
                var title = acc.FindElement(By.ClassName("bank_account_name")).Text;

                var amount = acc.FindElement(By.ClassName("bank_account_money")).Text;

                amount = new string(amount.Where(v => char.IsDigit(v) || v == ',').ToArray());

                var doubleAmount = double.Parse(amount, new NumberFormatInfo() {NumberDecimalSeparator = ","});

                result.Add(Money(title, doubleAmount, CurrencyExtensions.RUB));
            }

            return result;
        }

        public override IList<PaymentModel> ScrapeStatement(ScraperConfigurationModel configuration, Chrome chrome, DateTime startFrom)
        {
            DoLogin(configuration, chrome);
            
            var driver = chrome.Driver;

            var btns = GetElements(driver, By.TagName("button"));

            var rightBtn = btns.First(s => s.Text.ToLower().Contains("выписка"));
            rightBtn.Click();

            var form = GetElement(driver, By.ClassName("address_form"));
            
            var switches = form.FindElements(By.ClassName("switch_item"));

            var htmlSwitch = switches.First(s => s.Text == "1C");
            htmlSwitch.Click();

            var buttons = form.FindElements(By.TagName("button"));
            var dlButton = buttons.First(s => s.Text.ToLower().Contains("получить"));

            chrome.CleanupDownloads();
            
            dlButton.Click();

            var waited = 0;
            while (chrome.GetDownloads().Count == 0 && waited < 300)
            {
                WaitForPageLoad(driver);
                waited++;
            }

            var dlItem = chrome.GetDownloads().First().FullName;

            var docs = ParseOdinAssFile(dlItem);

            var config = docs.Single(v => !v.ContainsKey("СекцияДокумент"));

            var account = config["РасчСчет"];
            
            var goodDocs = docs.Where(v =>v.ContainsKey("СекцияДокумент"));

            var statements = goodDocs.Select(v =>
            {
                var whenString = v["Дата"];
                var when = DateTime.ParseExact(whenString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                var what = v["НазначениеПлатежа"];
                var amountText = v["Сумма"];
                var amount = double.Parse(amountText, new NumberFormatInfo() {NumberDecimalSeparator = "."});
                
                var isIncome = v["ПолучательСчет"] == account;
                var kind = isIncome ? PaymentKind.Income : PaymentKind.Expense;
                var ccy = "RUB";
                var reference = v["Номер"];

                return Statement(when, account, what, amount, kind, ccy, reference);
            }).ToList();

            return statements;
        }

        private List<Dictionary<string, string>> ParseOdinAssFile(string filename)
        {
            var odinAssFile = File.ReadAllLines(filename, Encoding.GetEncoding(1251)).Select(FormatLine).ToList();
                
            if (odinAssFile[0].Key != "1CClientBankExchange")
                throw new NotSupportedException();

            var keyValuePairs = new Queue<KeyValuePair<string, string>>();
            foreach (var pair in odinAssFile)
            {
                keyValuePairs.Enqueue(pair);
            }

            Dictionary<string, string> currentDocument = new Dictionary<string, string>();
            var docs = new List<Dictionary<string, string>> {currentDocument};
            while (keyValuePairs.Any())
            {
                var current = keyValuePairs.Dequeue();

                if (current.Key == "СекцияДокумент")
                {
                    currentDocument = new Dictionary<string, string>();
                    docs.Add(currentDocument);
                }

                currentDocument[current.Key] = current.Value;
            }

            return docs;
        }

        private static KeyValuePair<string, string> FormatLine(String line)
        {
            if (line.Contains("="))
            {
                var strings = line.Split("=", 2);
                return new KeyValuePair<string, string>(strings[0], strings[1]);
            }

            return new KeyValuePair<string, string>(line, null);
        }
    }
}