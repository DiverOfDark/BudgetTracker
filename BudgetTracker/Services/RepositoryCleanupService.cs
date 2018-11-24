using System;
using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Services
{
    public class RepositoryCleanupService
    {
        private readonly ObjectRepository _objectRepository;

        public RepositoryCleanupService(ObjectRepository objectRepository)
        {
            _objectRepository = objectRepository;
        }
        
        public void Run()
        {
            var yesterday = _objectRepository.Set<MoneyStateModel>()
                .Where(v => v.When < DateTime.UtcNow.AddDays(-2)).ToList();

            var groupedWeek = yesterday.GroupBy(v => v.Provider + v.AccountName + v.When.ToUniversalTime().ToShortDateString());
            var badWeek = groupedWeek.SelectMany(v => v.OrderByDescending(s => s.When).Skip(1)).ToList();
            _objectRepository.RemoveRange(badWeek);
        }
    }
}