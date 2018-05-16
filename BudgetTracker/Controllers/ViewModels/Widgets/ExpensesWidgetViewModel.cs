using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Payment;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class ExpensesWidgetViewModel : WidgetViewModel
    {
        public ExpensesWidgetViewModel(WidgetModel model, ObjectRepository repository) : base(model, new ExpensesWidgetSettings(model.Properties.ToDictionary(v=>v.Key, v=>v.Value)))
        {
            ExpenseSettings = (ExpensesWidgetSettings) Settings;
            var payments = repository.Set<PaymentModel>().Where(v => v.When.AddDays(30) > DateTime.UtcNow && v.Ccy == ExpenseSettings.Currency && v.Kind == PaymentKind.Expense);
            var month = new PaymentMonthViewModel(payments, true);
            Payments = month.PaymentModels.ToList();
        }

        public ExpensesWidgetSettings ExpenseSettings { get; set; }

        public IEnumerable<PaymentViewModel> Payments { get; }

        public override string TemplateName => WidgetExtensions.AsPath("~/Views/Widget/Widgets/ExpensesWidget.cshtml");
        public override int Columns => 8;
        public override int Rows => 2;
    }
}