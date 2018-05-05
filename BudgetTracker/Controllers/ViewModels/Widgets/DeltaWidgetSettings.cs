using System.Collections.Generic;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class DeltaWidgetSettings : WidgetSettings
    {
        public string ProviderName
        {
            get => GetPropertyFromModel();
            set => SetPropertyFromModel(value);
        }

        public string AccountName
        {
            get => GetPropertyFromModel();
            set => SetPropertyFromModel(value);
        }

        public DeltaWidgetSettings(Dictionary<string, string> model) : base(model)
        {
        }
    }
}