using System;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class ExceptionWidgetViewModel : WidgetViewModel
    {
        private readonly Exception _exception;

        public ExceptionWidgetViewModel(WidgetModel model, Exception ex) : base(model, null)
        {
            _exception = ex;
        }

        public string Message => _exception?.Message;

        public string Detailed => _exception?.ToString();

        public override int Columns => 4;
    }
}