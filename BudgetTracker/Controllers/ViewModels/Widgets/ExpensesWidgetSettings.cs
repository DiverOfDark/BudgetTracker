using System.Collections.Generic;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class ExpensesWidgetSettings : WidgetSettings
    {
        public ExpensesWidgetSettings(Dictionary<string, string> model) : base(model)
        {
        }

        public string Currency
        {
            get => GetPropertyFromModel();
            set => SetPropertyFromModel(value);
        }
    }
}