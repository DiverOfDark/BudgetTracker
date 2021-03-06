﻿using System;
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

            var groupedWeek = yesterday.GroupBy(v => v.Column.Provider + v.Column.AccountName + v.When.ToShortDateString());
            var badWeek = groupedWeek.SelectMany(v =>
            {
                var models = v.OrderByDescending(s => s.When).ToList();

                var count = models.Count / 2;
                var middle = models.Skip(count - 1).First();

                models.Remove(middle);
                
                return models;
            }).ToList();
            _objectRepository.RemoveRange(badWeek);
            
            _objectRepository.Remove<PaymentModel>(v=> v.Autogenerated && Math.Abs(v.Amount) <= 0.01);
            
            var dups = _objectRepository.Set<MoneyColumnMetadataModel>().Where(v => !v.IsComputed)
                .GroupBy(v => v.Provider + v.AccountName).Where(v => v.Count() > 1);
            foreach (var group in dups)
            {
                var ok = group.First();

                var toDelete = group.Skip(1).ToList();

                foreach (var msm in _objectRepository.Set<MoneyStateModel>().Where(v => toDelete.Contains(v.Column)))
                {
                    msm.Column = ok;
                }
                foreach (var pm in _objectRepository.Set<PaymentModel>().Where(v => toDelete.Contains(v.Column)))
                {
                    pm.Column = ok;
                }

                _objectRepository.RemoveRange(toDelete);
            }
        }
    }
}