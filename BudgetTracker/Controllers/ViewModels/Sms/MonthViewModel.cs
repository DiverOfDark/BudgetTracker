using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Sms
{
    public class MonthViewModel
    {
        public static List<MonthViewModel> FromSms(ObjectRepository objectRepository, bool showHidden)
        {
            var smses = objectRepository.Set<SmsModel>()
                .Where(v => v.AppliedRule == null || showHidden)
                .GroupBy(v => v.When.ToString(Discriminator)).ToDictionary(v => v.Key, v => v);
            
            var payments = objectRepository.Set<PaymentModel>().GroupBy(v => v.When.ToString(Discriminator)).ToDictionary(v => v.Key, v => v);

            var keys = smses.Select(v => v.Key).Concat(payments.Select(v => v.Key)).Distinct().ToList();

            return keys.Select(v =>
            {
                var s = smses.GetValueOrDefault(v) ?? Enumerable.Empty<SmsModel>();
                var p = payments.GetValueOrDefault(v) ?? Enumerable.Empty<PaymentModel>();

                return new MonthViewModel(s, p);
            }).ToList();
        }

        private const string Discriminator = "yyyyMM";
        
        public MonthViewModel(IEnumerable<SmsModel> smsModels, IEnumerable<PaymentModel> payments)
        {
            Sms = smsModels.OrderByDescending(v=>v.When).ToList();
            
            var paymentModels = payments.Select(v=>new PaymentViewModel(v)).ToList();

            When = Sms.Select(v => (DateTime?)v.When).FirstOrDefault() ?? paymentModels.Select(v => v.When).FirstOrDefault();

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
        }

        public IEnumerable<PaymentViewModel> PaymentModels { get; set; }

        public DateTime When { get; }

        public IList<SmsModel> Sms { get; }

        public Dictionary<string, double> Totals => PaymentModels.GroupBy(v => v.Ccy)
            .ToDictionary(v => v.Key, v => v.Sum(s => s.Amount));

        public string Key => When.ToString(Discriminator);
    }
}