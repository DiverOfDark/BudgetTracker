using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Debt;
using BudgetTracker.Model;
using JetBrains.Annotations;

namespace BudgetTracker.Scrapers
{
    [UsedImplicitly]
    internal class DebtScraper : GenericScraper
    {
        public DebtScraper(ObjectRepository repo) : base(repo)
        {
        }

        public override string ProviderName => "Долги и ссуды";
        
        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, Chrome driver)
        {
            var s1 = Calculate(s => new DebtViewModel(s).RemainingWithPercentage, "Долги с процентами");
            var s2 = Calculate(s => new DebtViewModel(s).Remaining, "Долги");

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