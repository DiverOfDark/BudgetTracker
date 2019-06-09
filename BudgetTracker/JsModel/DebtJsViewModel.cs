using System;
using System.Linq;
using BudgetTracker.JsModel.Attributes;
using BudgetTracker.Model;

namespace BudgetTracker.JsModel
{
    [ExportJsModel]
    public class DebtJsViewModel
    {
        private DebtModel _model;

        public DebtJsViewModel(DebtModel model)
        {
            _model = model;
        }

        public double Amount => _model.Amount;
        public double Percentage => _model.Percentage;
        public string Description => _model.Description;
        public string RegexForTransfer => _model.RegexForTransfer;

        public string Ccy => _model.Ccy;

        public string LastPaymentDate => _model.Payments.OrderByDescending(v => v.When).FirstOrDefault()?.When.ToLongDateString();
        
        public int DaysCount => _model.DaysCount;

        public string When => _model.When.ToShortDateString();

        public Guid Id => _model.Id;

        public double Returned => _model.Payments.Where(v=> v.Kind == (_model.Amount < 0 ? PaymentKind.Expense : PaymentKind.Income)).Select(s => s.Amount).Sum();
    }
}