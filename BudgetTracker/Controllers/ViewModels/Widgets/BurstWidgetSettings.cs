using System.Collections.Generic;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class BurstWidgetSettings : WidgetSettings
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

        public BurstWidgetSettings(Dictionary<string, string> model) : base(model)
        {
        }
    }
}