using System;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class ExceptionWidgetViewModel : WidgetViewModel
    {
        public ExceptionWidgetViewModel(WidgetModel model, Exception ex) : base(model, null)
        {
            Exception = ex;
        }
        
        public Exception Exception { get; }

        public override string TemplateName => WidgetExtensions.AsPath("~/Views/Widget/Widgets/ExceptionWidget.cshtml");

        public override int Columns => 4;
    }
}