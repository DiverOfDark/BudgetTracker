﻿using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Sms
{
    public class PaymentViewModel 
    {
        public PaymentViewModel(PaymentModel paymentModel)
        {
            When = paymentModel.When;
            Amount = paymentModel.Amount;
            Ccy = paymentModel.Ccy;
            What = paymentModel.Category?.Category ?? paymentModel.What;
            Id = paymentModel.Id;
            Provider = paymentModel.Provider;
            Account = paymentModel.Account;
            Items = new[] {paymentModel};
            Kind = paymentModel.Kind;
        }

        public string ConvertKind(PaymentKind paymentModelKind)
        {
            switch (paymentModelKind)
            {
                case PaymentKind.Expense:
                    return "Расход";
                case PaymentKind.Income:
                    return "Доход";
                case PaymentKind.Transfer:
                    return "Перевод";
                default:
                    return "Другое";
            }
        }

        public PaymentViewModel(IList<PaymentModel> paymentGroup, bool requireSameWhat = true)
        {
            When = paymentGroup.Max(v => v.When);
            Amount = paymentGroup.Sum(v => v.Amount);
            Ccy = paymentGroup.Select(s => s.Ccy).Distinct().Single();
            Id = paymentGroup.First().Id;
            if (requireSameWhat)
            {
                What = paymentGroup.Select(v => v.Category?.Category ?? v.What).Distinct(StringComparer.CurrentCultureIgnoreCase).Single();
                Provider = paymentGroup.Select(v => v.Provider).Distinct(StringComparer.CurrentCultureIgnoreCase).Single();
                Account = paymentGroup.Select(v => v.Account).Distinct(StringComparer.CurrentCultureIgnoreCase).Single();
                Kind = paymentGroup.Select(v => v.Kind).Distinct().Single();
            }
            else
            {
                var list = paymentGroup.Select(v => v.Category?.Category ?? v.What).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
                What = list.Count == 1 ? list[0] : "Остальное";
                list = paymentGroup.Select(v => v.Provider).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
                Provider = list.Count == 1 ? list[0] : "Остальное";
                list = paymentGroup.Select(v => v.Account).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
                Account = list.Count == 1 ? list[0] : "Остальное";
                var list2 = paymentGroup.Select(v => v.Kind).Distinct().ToList();
                Kind = list2.Count == 1 ? list2[0] : PaymentKind.Unknown;
            }

            Items = paymentGroup;
        }

        public string What { get; set; }

        public string Ccy { get; set; }

        public Guid Id { get; } 

        public double Amount { get; set; }

        public DateTime When { get; set; }
        
        public PaymentKind Kind { get; set; }
        public string Provider { get; }
        public string Account { get; }

        public int Count => Items.Count();
        public IEnumerable<PaymentModel> Items { get; }
    }
}