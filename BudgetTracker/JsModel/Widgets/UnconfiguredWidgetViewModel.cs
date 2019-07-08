using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class UnconfiguredWidgetViewModel : WidgetViewModel
    {
        public UnconfiguredWidgetViewModel(WidgetModel model) : base(model, null)
        {
        }

        public override int Columns => 3;
    }
}