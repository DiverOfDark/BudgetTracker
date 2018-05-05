using System;
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
            Items = new[] {paymentModel};
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
            }
            else
            {
                var list = paymentGroup.Select(v => v.Category?.Category ?? v.What).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
                What = list.Count == 1 ? list[0] : "Остальное";
            }

            Items = paymentGroup;
        }

        public string What { get; set; }

        public string Ccy { get; set; }

        public Guid Id { get; } 

        public double Amount { get; set; }

        public DateTime When { get; set; }

        public int Count => Items.Count();
        public IEnumerable<PaymentModel> Items { get; }
    }
}