﻿using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;
using BudgetTracker.Scrapers;
using Microsoft.Extensions.Logging;

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
        }

        public void Scrape()
        {
            lock (_chrome)
            {
                _chrome.Reset();
                var logger = _logger;
                var scrapeConfigs = _objectRepository.Set<ScraperConfigurationModel>().ToList();
                
                foreach (var scraperConfig in scrapeConfigs)
                {
                    var scraper = _scrapers.FirstOrDefault(v => v.ProviderName == scraperConfig.ScraperName);

                    if (scraper == null)
                    {
                        logger.LogError($"Failed to find scraper {scraperConfig.ScraperName}");
                        continue;
                    }

                    try
                    {
                        logger.LogInformation($"Scraping {scraper.ProviderName} current state");
                        ScrapeCurrentState(scraper, logger, scraperConfig);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Failed to get state for {scraper.ProviderName}...");
                    }
                    finally
                    {
                        _chrome.Reset();
                    }

                    try
                    {
                        logger.LogInformation($"Scraping {scraper.ProviderName} statements");
                        ScrapeStatement(scraper, scraperConfig, logger);
                    }
                    catch(Exception ex)
                    {
                        logger.LogError(ex, $"Failed to get statement for {scraper.ProviderName}...");
                    }
                    finally
                    {
                        _chrome.Reset();
                    }
                }
            }

            // default strategy - generated money statements from diff between states.
            foreach (var column in _objectRepository.Set<MoneyColumnMetadataModel>().Where(v => v.AutogenerateStatements))
            {
                var payments = _objectRepository.Set<PaymentModel>()
                    .Where(v => v.Column == column)
                    .ToList();

                _objectRepository.Set<MoneyStateModel>()
                    .Where(v => v.Provider == column.Provider && v.AccountName == column.AccountName)
                    .OrderBy(v => v.When)
                    .Aggregate((a, b) =>
                {
                    var delta = b.Amount - a.Amount;

                    var appliedPayments = payments.Where(v =>
                        v.Column.AccountName == column.AccountName && v.When >= a.When && v.When <= b.When).ToList();

                    foreach (var item in appliedPayments)
                    {
                        switch (item.Kind)
                        {
                            case PaymentKind.Expense:
                                delta -= Math.Abs(item.Amount);
                                break;
                            case PaymentKind.Income:
                                delta += Math.Abs(item.Amount);
                                break;
                            case PaymentKind.Transfer:
                                delta += item.Amount;
                                break;
                        }
                    }

                    if (Math.Abs(delta) >= 0.01)
                    {
                        var when = a.When + (b.When - a.When) / 2;
                        _objectRepository.Add(new PaymentModel(when, "Коррекция баланса " + column.Provider + " " + column.AccountName, -delta, a.Ccy, "N/A-" + DateTime.Now.Ticks, column));
                    }

                    return b;
                });
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

        private void ScrapeStatement(GenericScraper scraper, ScraperConfigurationModel scraperConfig, ILogger<ScrapeService> logger)
        {
            var minDates = new[]
            {
                _objectRepository.Set<PaymentModel>()
                    .Where(v => v.Column?.Provider == scraper.ProviderName)
                    .OrderByDescending(v => v.When)
                    .FirstOrDefault()?.When,
                _objectRepository.Set<MoneyStateModel>().OrderBy(v => v.When)
                    .FirstOrDefault()?.When,
                _objectRepository.Set<PaymentModel>().OrderBy(v => v.When)
                    .FirstOrDefault()?.When
            };

            var lastPayment = minDates.Where(v => v != null).OrderBy(v => v).FirstOrDefault() ??
                              DateTime.MinValue;

            if (scraperConfig.LastSuccessfulStatementScraping != default)
                lastPayment = scraperConfig.LastSuccessfulStatementScraping;

            // Let's not scrape statements too often - it's hard
            if (lastPayment.AddHours(24) < DateTime.Now)
            {
                logger.LogInformation(
                    $"Scraping statement for {scraper.ProviderName} since {lastPayment.AddDays(-4)}...");

                var statements = scraper.ScrapeStatement(scraperConfig, _chrome, lastPayment.AddDays(-4))
                    .ToList();

                logger.LogInformation($"Got statement of {statements.Count} items...");

                foreach (var s in statements)
                {
                    var existingItem = _objectRepository.Set<PaymentModel>().OrderBy(v => v.When)
                        .FirstOrDefault(v =>
                            Math.Abs((v.When.Date - s.When.Date).TotalDays) <= 4 &&
                            Math.Abs(v.Amount - s.Amount) < 0.01 &&
                            v.Ccy == s.Ccy &&
                            v.StatementReference == null ||
                            v.StatementReference == s.StatementReference);

                    if (existingItem == null)
                    {
                        _objectRepository.Add(s);
                    }
                    else
                    {
                        if (existingItem.Column == null)
                        {
                            existingItem.Column = s.Column;
                        }

                        if (existingItem.StatementReference == null)
                        {
                            existingItem.StatementReference = s.StatementReference;
                        }
                    }
                }

                scraperConfig.LastSuccessfulStatementScraping = DateTime.Now;
            }
        }

        private void ScrapeCurrentState(GenericScraper scraper, ILogger<ScrapeService> logger, ScraperConfigurationModel scraperConfig)
        {
            var currentState = _objectRepository.Set<MoneyStateModel>();

            var accountCount = currentState.Where(s =>
                    s.Provider == scraper.ProviderName && s.When.Date == DateTime.UtcNow.Date.AddDays(-1))
                .Select(s => s.AccountName).Distinct()
                .ToList();

            var todayState = currentState.Where(s =>
                    s.Provider == scraper.ProviderName && s.When.Date == DateTime.UtcNow.Date)
                .Select(s => s.AccountName).Distinct()
                .ToList();

            var toScrape = accountCount.Count == 0 || accountCount.Except(todayState).Any();

            if (toScrape)
            {
                logger.LogInformation("No cached items, scraping...");

                var items = scraper.Scrape(scraperConfig, _chrome.Driver);

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

                scraperConfig.LastSuccessfulBalanceScraping = DateTime.Now;
                logger.LogInformation("Indexed...");
            }
            else
            {
                logger.LogInformation("For today there are already scraped items, continuing...");
            }
        }
    }
}