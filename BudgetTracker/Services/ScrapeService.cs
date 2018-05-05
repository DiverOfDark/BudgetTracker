using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BudgetTracker.Model;
using BudgetTracker.Scrapers;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;

namespace BudgetTracker.Services
{
    public class ScrapeService
    {
        private readonly ObjectRepository _objectRepository;
        private readonly ILogger<ScrapeService> _logger;
        private readonly List<GenericScraper> _scrapers;
        private readonly Chrome _chrome;

        public ScrapeService(Chrome chrome, ObjectRepository objectRepository, IEnumerable<GenericScraper> scrapers, ILogger<ScrapeService> logger)
        {
            _objectRepository = objectRepository;
            _logger = logger;
            _chrome = chrome;

            _scrapers = scrapers.ToList();
            
            /*
             *  new GenericScraper[]
            {
                new AlfabankScraper("***REMOVED***", "***REMOVED***"),
                new AlfaPotokScraper("***REMOVED***", "***REMOVED***"),
                new ModulDengiScraper("***REMOVED***", "***REMOVED***"), 
                new RaiffeisenScraper("***REMOVED***", "***REMOVED***"),
                new LivecoinScraper("***REMOVED***", "***REMOVED***", logger),
                new PenenzaScraper("***REMOVED***", "***REMOVED***"),
                new FxRateScraper(),
                new AlfaCapitalScraper("***REMOVED***", "***REMOVED***", _objectRepository), 
                new ModulbankScraper("***REMOVED***", "***REMOVED***", _objectRepository), 
            }
             */
        }

        public void Scrape()
        {
            lock (_chrome)
            {
                _chrome.Reset();
                var logger = _logger;
                var currentState = _objectRepository.Set<MoneyStateModel>();

                var scrapeConfigs = _objectRepository.Set<ScraperConfigurationModel>().ToList();
                
                foreach (var scraperConfig in scrapeConfigs)
                {
                    var scraper = _scrapers.FirstOrDefault(v => v.ProviderName == scraperConfig.ScraperName);

                    if (scraper == null)
                    {
                        logger.LogError($"Failed to find scraper {scraperConfig.ScraperName}");
                        continue;
                    }
                    
                    logger.LogInformation($"Scraping {scraper.ProviderName}");

                    var accountCount = currentState.Where(s => s.Provider == scraper.ProviderName && s.When.Date == DateTime.UtcNow.Date.AddDays(-1))
                        .Select(s => s.AccountName).Distinct()
                        .ToList();

                    var todayState = currentState.Where(s => s.Provider == scraper.ProviderName && s.When.Date == DateTime.UtcNow.Date)
                        .Select(s => s.AccountName).Distinct()
                        .ToList();

                    var toScrape = accountCount.Count == 0 || accountCount.Except(todayState).Any();

                    if (toScrape)
                    {
                        logger.LogInformation("No cached items, scraping...");

                        var driver = _chrome.Driver;

                        try
                        {
                            var items = scraper.Scrape(scraperConfig, driver);

                            logger.LogInformation($"Found {items.Count()} items, indexing...");

                            foreach (var item in items)
                            {
                                logger.LogInformation(
                                    $" - {item.Provider} / {item.AccountName}: {item.Amount} ({item.Ccy})");
                                if (!string.IsNullOrWhiteSpace(item.Provider))
                                {
                                    if (item.Amount <= 0.001)
                                    {
                                        if (!_objectRepository.Set<MoneyStateModel>().Any(s =>
                                            s.Provider == item.Provider
                                            && s.AccountName == item.AccountName
                                            && s.Amount > 0
                                        ))
                                            continue;
                                    }

                                    if (todayState.Contains(item.AccountName))
                                        continue;

                                    _objectRepository.Add(item);
                                }
                            }

                            logger.LogInformation("Indexed...");
                        }
                        catch (Exception ex)
                        {
                            if (!Startup.IsProduction)
                            {
                                driver.TakeScreenshot().SaveAsFile("/tmp/screenshots/err.png", ScreenshotImageFormat.Png);
                                var body = driver.FindElement(By.TagName("body")).GetAttribute("innerHtml");
                                File.WriteAllText("/tmp/screenshots/doc.html", body);
                                File.WriteAllText("/tmp/screenshots/doc2.html", driver.PageSource);
                            }

                            logger.LogError(ex, "There were an issue scraping");
                        }
                    }
                    else
                    {
                        logger.LogInformation("For today there are already scraped items, continuing...");
                    }
                }
            }
            
            foreach(var item in _objectRepository.Set<MoneyStateModel>().GroupBy(v => v.Provider, v => v.AccountName))
            foreach (var sub in item.Distinct())
            {
                var existing = _objectRepository.Set<MoneyColumnMetadataModel>()
                    .FirstOrDefault(v => v.Provider == item.Key && v.AccountName == sub);

                if (existing == null)
                {
                    existing = new MoneyColumnMetadataModel(item.Key, sub)
                    {
                        UserFriendlyName = sub,
                        IsVisible = true
                    };
                    _objectRepository.Add(existing);
                }
            }
        }
    }
}