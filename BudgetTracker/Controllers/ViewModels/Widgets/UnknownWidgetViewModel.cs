using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class UnknownWidgetViewModel : WidgetViewModel
    {
        public UnknownWidgetViewModel(WidgetModel model) : base(model, null)
        {
        }

        public override string TemplateName => WidgetExtensions.AsPath("~/Views/Widget/Widgets/UnknownWidget.cshtml");

        public override int Columns => 3;
    }
}