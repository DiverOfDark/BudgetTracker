using System;
using System.Collections.Generic;
using BudgetTracker.Model;
using OpenQA.Selenium.Chrome;

namespace BudgetTracker.Scrapers
{
    public interface IStatementScraper
    {
        IEnumerable<PaymentModel> ScrapeStatement(ScraperConfigurationModel configuration, Chrome driver,
            DateTime startFrom);
    }
}