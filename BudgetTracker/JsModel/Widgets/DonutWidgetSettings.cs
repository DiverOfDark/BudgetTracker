using System.Collections.Generic;
using System.ComponentModel;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class DonutWidgetSettings : WidgetSettings
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
        
        public DonutWidgetSettings(Dictionary<string,string> model) : base(model)
        {
        }
    }
}