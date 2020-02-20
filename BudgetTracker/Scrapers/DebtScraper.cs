using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.JsModel;
using BudgetTracker.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class DebtScraper : GenericScraper
    {
        public DebtScraper(ObjectRepository repository, ILoggerFactory factory) : base(repository, factory)
        {
        }

        public override string ProviderName => "Долги и ссуды";
        
        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome driver)
        {
            var s1 = Calculate(s => s.Amount * (1 + s.Percentage / 100) - s.Returned, "Долги с процентами");
            var s2 = Calculate(s => s.Amount - s.Returned, "Долги");

            return s1.Concat(s2).ToList();
        }

        private IList<MoneyStateModel> Calculate(Func<DebtModel, double> calculator, string name)
        {
            var currentDebt = Repository.Set<DebtModel>().GroupBy(v => v.Ccy).Select(v => new
            {
                Ccy = v.Key,
                Sum = v.Select(calculator).Sum()
            });

            return currentDebt.Select(s => Money(name + "/" + s.Ccy, s.Sum, s.Ccy)).ToList();
        }
    }
}