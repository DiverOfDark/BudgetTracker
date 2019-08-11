using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class UnknownWidgetViewModel : WidgetViewModel
    {
        public UnknownWidgetViewModel(WidgetModel model) : base(model, null)
        {
        }

        public override int Columns => 3;
    }
}