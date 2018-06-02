using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Payment
{
    public class PaymentViewModel 
    {
        protected const string MiddleDash = "—";

        public PaymentViewModel(PaymentModel paymentModel)
        {
            When = paymentModel.When;
            Amount = paymentModel.SignedAmount;
            Ccy = paymentModel.Ccy;
            What = paymentModel.What;
            Id = paymentModel.Id;
            Provider = paymentModel.Column?.Provider;
            Account = paymentModel.Column?.AccountName;
            Items = new[] {paymentModel};
            Kind = paymentModel.Kind;

            CategoryId = paymentModel.CategoryId;
            Category = paymentModel.Category?.Category;
            ColumnId = paymentModel.Column?.Id;
        }

        public PaymentViewModel(IList<PaymentModel> paymentGroup)
        {
            When = paymentGroup.Max(v => v.When);
            Amount = paymentGroup.Sum(v => v.SignedAmount);
            Ccy = paymentGroup.Select(s => s.Ccy).Distinct().Single();
            Id = paymentGroup.First().Id;

            var list = paymentGroup.Select(v => v.What).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
            What = list.Count == 1 ? list[0] : MiddleDash;
            list = paymentGroup.Select(v => v.Column?.Provider).Where(v=>v!=null).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
            Provider = list.Count == 1 ? list[0] : MiddleDash;
            list = paymentGroup.Select(v => v.Column?.AccountName).Where(v=>v!=null).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
            Account = list.Count == 1 ? list[0] : MiddleDash;
            list = paymentGroup.Select(v => v.Category?.Category).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
            Category = list.Count == 1 ? list[0] : MiddleDash;
            var list2 = paymentGroup.Select(v => v.Kind).Distinct().ToList();
            Kind = list2.Count == 1 ? list2[0] : PaymentKind.Unknown;

            Items = paymentGroup;
        }

        public string What { get; }

        public string Ccy { get; }

        public Guid Id { get; } 

        public double Amount { get; }

        public DateTime When { get; }
        
        public PaymentKind Kind { get; }
        public string Provider { get; }
        public string Account { get; }

        public int Count => Items.Count();
        public IEnumerable<PaymentModel> Items { get; }

        public Guid? CategoryId { get; set; }
        public Guid? ColumnId { get; set; }
        public string Category { get; }
    }
}