using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tinkoff.Trading.OpenApi.Network;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class TinkoffInvestmentScraper : GenericScraper
    {
        public TinkoffInvestmentScraper(ObjectRepository repository, ILoggerFactory factory) : base (repository, factory)
        {
        }

        public override string ProviderName => "Тинькофф-Инвестиции";
        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome driver)
        {
            using var connection = ConnectionFactory.GetConnection(configuration.Password);
            using var context = connection.Context;

            var accounts = context.AccountsAsync().GetAwaiter().GetResult();

            var result = new List<MoneyStateModel>();
            
            foreach (var account in accounts)
            {
                var portfolio = context.PortfolioAsync(account.BrokerAccountId).GetAwaiter().GetResult();
                result.AddRange(portfolio.Positions.Select(v => Money(account.BrokerAccountId + " " + v.Name, (double) (v.Balance * v.AveragePositionPrice.Value), v.AveragePositionPrice.Currency.ToString().ToUpper())));

                var ccys= context.PortfolioCurrenciesAsync(account.BrokerAccountId).GetAwaiter().GetResult();
                result.AddRange(ccys.Currencies.Select(v=> Money(account.BrokerAccountId + " " + v.Currency.ToString().ToUpper(), (double) v.Balance, v.Currency.ToString().ToUpper())));
            }

            return result;
        }
    }
}