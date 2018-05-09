using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Sms
{
    public class SmsMonthViewModel
    {
        public static List<SmsMonthViewModel> FromSms(ObjectRepository objectRepository, bool showHidden)
        {
            var smses = objectRepository.Set<SmsModel>()
                .Where(v => v.AppliedRule == null || showHidden)
                .GroupBy(v => v.When.ToString(Discriminator)).ToDictionary(v => v.Key, v => v);
            
            var keys = smses.Select(v => v.Key).Distinct().ToList();

            return keys.Select(v =>
            {
                var s = smses.GetValueOrDefault(v) ?? Enumerable.Empty<SmsModel>();

                return new SmsMonthViewModel(s);
            }).ToList();
        }

        private const string Discriminator = "yyyyMM";

        public SmsMonthViewModel(IEnumerable<SmsModel> smsModels)
        {
            Sms = smsModels.OrderByDescending(v=>v.When).ToList();
            
            When = Sms.Select(v => (DateTime?)v.When).FirstOrDefault() ?? DateTime.Now;
        }

        public DateTime When { get; }

        public IList<SmsModel> Sms { get; }

        public string Key => When.ToString(Discriminator);
    }
}