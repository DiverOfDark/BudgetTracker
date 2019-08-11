using System;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Payment
{
    [ExportJsModel]
    public class PaymentViewModel 
    {
        public PaymentViewModel(PaymentModel paymentModel)
        {
            When = paymentModel.When;
            Amount = paymentModel.Amount;
            Ccy = paymentModel.Ccy;
            What = paymentModel.What;
            Id = paymentModel.Id;
            Provider = paymentModel.Column?.Provider;
            Account = paymentModel.Column?.AccountName;
            Kind = paymentModel.Kind.GetDisplayName();
            KindId = (int) paymentModel.Kind;

            CategoryId = paymentModel.CategoryId;
            Category = paymentModel.Category?.Category;
            DebtId = paymentModel.Debt?.Id;
            Debt = paymentModel.Debt?.Description;
            ColumnId = paymentModel.Column?.Id;
            StatementReference = paymentModel.StatementReference;
            Sms = paymentModel.Sms?.Message;
        }

        public string What { get; }

        public string Ccy { get; }

        public Guid Id { get; } 

        public double Amount { get; }

        public DateTime When { get; }
        
        public int KindId { get; }
        public string Kind { get; }
        public string Provider { get; }
        public string Account { get; }

        public string StatementReference { get; }
        
        public string Sms { get; }
        
        public Guid? CategoryId { get; set; }
        public Guid? DebtId { get; set; }
        public Guid? ColumnId { get; set; }
        public string Category { get; }
        public string Debt { get; }
    }
}