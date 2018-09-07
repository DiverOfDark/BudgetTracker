using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using BudgetTracker.Model;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class PenenzaScraper : GenericScraper
    {
        public PenenzaScraper(ObjectRepository repository) : base(repository)
        {
        }

        public override string ProviderName => "Penenza";
        
        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome chrome)
        {
            var driver = chrome.Driver;
            driver.Navigate().GoToUrl(@"https://my.penenza.ru/main/sso/Login.aspx");
            var name = GetElement(driver, By.Id("MainContent_txtUserName"));
            var pass = GetElement(driver, By.Id("MainContent_txtUserPassword"));
            name.Click();
            chrome.SendKeys(configuration.Login);
            pass.Click();
            chrome.SendKeys(configuration.Password);
            chrome.SendKeys(Keys.Return);

            var firstPortlet = GetElement(driver, By.ClassName("investor-dashboard__portlet-wrapper"));

            var rows = firstPortlet.FindElements(By.TagName("tr"));
            WaitForPageLoad(driver, 10);
            var result = new List<MoneyStateModel>();

            foreach (var row in rows)
            {
                try
                {
                    var td = row.FindElement(By.ClassName("investor-dashboard__table-title-col"));
                    var value = row.FindElement(By.ClassName("investor-dashboard__table-value-col"));

                    try
                    {
                        var subTd = td.FindElement(By.TagName("a"));
                        if (subTd != null)
                            td = subTd;
                    }
                    catch
                    {
                    }

                    var acc = td.Text;
                    var text = value.Text;
                    var doubleValue = ParseDouble(text);
                    var mm = Money(acc, doubleValue, "RUB");
                    result.Add(mm);
                }
                catch
                {
                }
            }

            var formattedUrl = string.Format("https://my.penenza.ru/main/tradefinancemvc/Credit/GetTotalCreditSum?clientName=&clientInn=&tradeId=&creditStateIds=1%2C2%2C4%2C6&lawTypes=&creditOrganizationIds=&planingPaymentDateFrom=&planingPaymentDateTo=&issueDateFrom=&issueDateTo=&planingReturnDateFrom=&planingReturnDateTo={0}&actualReturnDateFrom=&actualReturnDateTo=&isBorrower=false&IsExpertVisibility=false&creditSumFrom=&creditSumTo=&serviceTypeIds=", DateTime.Now.ToString("dd.MM.yyyy"));
            
            driver.Navigate().GoToUrl(formattedUrl);
            
            WaitForPageLoad(driver);

            var response = GetElement(driver, By.TagName("body")).Text;

            var debtValueString = JObject.Parse(response)["totalCreditSum"].Value<string>();

            var debtValue = ParseDouble(debtValueString);
            
            result.Add(Money("Просрочка", debtValue, "RUB"));
            
            return result;
        }

        private static double ParseDouble(string text)
        {
            var valueText = new string(text.Where(v => char.IsDigit(v) || v == ',').ToArray());
            var doubleValue = double.Parse(valueText, new NumberFormatInfo() {NumberDecimalSeparator = ","});
            return doubleValue;
        }
    }
}