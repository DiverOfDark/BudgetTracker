using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using BudgetTracker.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class AlfaDirectScraper : GenericScraper
    {
        public AlfaDirectScraper(ObjectRepository repository, ILoggerFactory factory) : base(repository, factory)
        {
        }

        public override string ProviderName => "Альфа-Директ";
        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;
            
            driver.Navigate().GoToUrl("https://lk.alfadirect.ru/");

            var fields = GetElements(driver, By.TagName("input")).ToList();
            var name = fields[0];
            var pass = fields[1];

            name.Click();
            chrome.SendKeys(configuration.Login);
            pass.Click();
            chrome.SendKeys(configuration.Password);
            
            chrome.SendKeys(Keys.Return);
            WaitForPageLoad(driver);
            driver.Navigate().GoToUrl("https://lk.alfadirect.ru/reports/MyPortfolio");
            WaitForPageLoad(driver);

            var lnk = GetElement(driver, By.LinkText("Просмотреть"));

            var link = lnk.GetAttribute("href");
            link = link.Replace("HTML", "XML");

            driver.Navigate().GoToUrl(link);
            
            int waited = 0;
            while (chrome.GetDownloads().Count < 1 && waited < 300)
            {
                WaitForPageLoad(driver);
                waited++;
            }

            var file = chrome.GetDownloads()[0].FullName;

            var contents = File.ReadAllText(file);

            var xDoc = XDocument.Parse(contents);

            var el = xDoc.Root;
            el = el.Element(XName.Get("Financial_results","MyPortfolio"));
            el = el.Element(XName.Get("Report", "MyPortfolio"));
            var positions = el.Descendants(XName.Get("Details", "MyPortfolio")).ToList();

            var result = new List<MoneyStateModel>();
            foreach (var item in positions)
            {
                var ccy = item.Attribute("code_curr")?.Value;
                var activeType = item.Attribute("active_type")?.Value;
                var activeName = item.Attributes().FirstOrDefault(s =>
                    s.Name.LocalName.StartsWith("p_name") && !string.IsNullOrWhiteSpace(s.Value))?.Value;

                var activeCurrentPrice = item.Attribute("CostOpenPosEnd8")?.Value;

                if (!double.TryParse(activeCurrentPrice, NumberStyles.Any, new NumberFormatInfo{NumberDecimalSeparator = "."}, out var amount))
                    continue;

                result.Add(Money(activeName ?? (activeType + " " + ccy), amount, ccy));
            }

            var totals = result.GroupBy(v => v.Ccy).Select(s => Money("Итого " + s.Key, s.Sum(v => v.Amount), s.Key)).ToList();

            result.AddRange(totals);
            return result;
        }
    }
}