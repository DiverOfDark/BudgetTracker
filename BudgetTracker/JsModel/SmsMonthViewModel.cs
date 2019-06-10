using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;

namespace BudgetTracker.JsModel
{
    [ExportJsModel]
    public class SmsMonthViewModel
    {
        public static List<SmsMonthViewModel> FromSms(ObjectRepository objectRepository)
        {
            var smses = objectRepository.Set<SmsModel>()
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
            Sms = smsModels.Select(v=>new SmsJsModel(v)).OrderByDescending(v=>v.When).ToList();
            
            When = Sms.Select(v => (DateTime?)v.When).FirstOrDefault() ?? DateTime.Now;
        }

        public DateTime When { get; }

        public IList<SmsJsModel> Sms { get; }

        public string Key => When.ToString(Discriminator);
    }
}