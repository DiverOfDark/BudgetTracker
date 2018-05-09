using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Payment
{
    public class PaymentMonthViewModel
    {
        public static List<PaymentMonthViewModel> FromPayments(ObjectRepository objectRepository)
        {
            var payments = objectRepository.Set<PaymentModel>().GroupBy(v => v.When.ToString(Discriminator)).ToDictionary(v => v.Key, v => v);

            var keys = payments.Select(v => v.Key).Distinct().ToList();

            return keys.Select(v =>
            {
                var p = payments.GetValueOrDefault(v) ?? Enumerable.Empty<PaymentModel>();

                return new PaymentMonthViewModel(p);
            }).ToList();
        }

        private const string Discriminator = "yyyyMM";

        public PaymentMonthViewModel(IEnumerable<PaymentModel> payments)
        {
            var paymentModels = payments.Select(v=>new PaymentViewModel(v)).ToList();
            
            var groups = paymentModels
                .GroupBy(v => v.What.ToLower() + v.Ccy + v.Kind)
                .Where(v => v.Count() > 1)
                .ToList();

            foreach (var item in groups)
            {
                paymentModels.RemoveAll(item.Contains);
                paymentModels.Add(new PaymentViewModel(item.SelectMany(v=>v.Items).ToList()));
            }
            
            PaymentModels = paymentModels;
            When = paymentModels.Select(v => v.When).FirstOrDefault();
        }

        public IEnumerable<PaymentViewModel> PaymentModels { get; set; }

        public DateTime When { get; }

        public Dictionary<string, double> Totals => PaymentModels.GroupBy(v => v.Ccy)
            .ToDictionary(v => v.Key, v => v.Sum(s => s.Amount));
        
        public string Key => When.ToString(Discriminator);
    }
}