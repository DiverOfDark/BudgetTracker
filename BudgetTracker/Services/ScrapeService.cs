using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BudgetTracker.Model;
using BudgetTracker.Scrapers;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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

                    if (scraper is IStatementScraper ss)
                    {
                        try
                        {
                            var minDates = new[]
                            {
                                _objectRepository.Set<PaymentModel>()
                                    .Where(v => v.Provider == scraper.ProviderName)
                                    .OrderByDescending(v => v.When)
                                    .FirstOrDefault()?.When,
                                _objectRepository.Set<MoneyStateModel>().OrderBy(v => v.When)
                                    .FirstOrDefault()?.When,
                                _objectRepository.Set<PaymentModel>().OrderBy(v => v.When)
                                    .FirstOrDefault()?.When
                            };

                            var lastPayment = minDates.Where(v => v != null).OrderBy(v => v).FirstOrDefault() ??
                                              DateTime.MinValue;

                            if (lastPayment.AddHours(24) > DateTime.Now)
                                continue; // Let's not scrape statements too often - it's hard

                            logger.LogInformation($"Scraping statement for {scraper.ProviderName} since {lastPayment}...");
                            
                            var statements = ss.ScrapeStatement(scraperConfig, _chrome, lastPayment).ToList();
                            
                            logger.LogInformation($"Got statement of {statements.Count} items...");
                            
                            foreach (var s in statements)
                            {
                                var existingItem = _objectRepository.Set<PaymentModel>().FirstOrDefault(v =>
                                    v.When.Date == s.When.Date &&
                                    Math.Abs(v.Amount - s.Amount) < 0.01 &&
                                    v.Ccy == s.Ccy &&
                                    v.StatementReference == null || v.StatementReference == s.StatementReference);
                                
                                if (existingItem == null)
                                {
                                    _objectRepository.Add(s);
                                }
                                else
                                {
                                    if (existingItem.Provider == null)
                                    {
                                        existingItem.Provider = s.Provider;
                                    }

                                    if (existingItem.Account == null)
                                    {
                                        existingItem.Account = s.Account;
                                    }

                                    if (existingItem.StatementReference == null)
                                    {
                                        existingItem.StatementReference = s.StatementReference;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError($"Failed to get statement for {scraper.ProviderName}...", ex);
                        }
                    }

                    var accountCount = currentState.Where(s => s.Provider == scraper.ProviderName && s.When.Date == DateTime.UtcNow.Date.AddDays(-1))
                        .Select(s => s.AccountName).Distinct()
                        .ToList();

                    var todayState = currentState.Where(s => s.Provider == scraper.ProviderName && s.When.Date == DateTime.UtcNow.Date)
                        .Select(s => s.AccountName).Distinct()
                        .ToList();

                    var toScrape = accountCount.Count == 0 || accountCount.Except(todayState).Any();

                    if (toScrape)
                    {
                        try
                        {
                            logger.LogInformation("No cached items, scraping...");

                            var items = scraper.Scrape(scraperConfig, _chrome.Driver);

                            logger.LogInformation($"Found {items.Count()} items, indexing...");

                            foreach (var item in items)
                            {
                                logger.LogInformation($" - {item.Provider} / {item.AccountName}: {item.Amount} ({item.Ccy})");
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