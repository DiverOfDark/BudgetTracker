using System.Collections.Generic;
using System.ComponentModel;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public enum ChartKind
    {
        [DisplayName("Линия")]
        Linear,
        [DisplayName("Пончик")]
        Donut
    }

    public class ChartWidgetSettings : WidgetSettings
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
        
        public ChartKind ChartKind
        {
            get => GetEnumPropertyFromModel<ChartKind>();
            set => SetEnumPropertyFromModel(value);
        }
        
        public ChartWidgetSettings(Dictionary<string,string> model) : base(model)
        {
        }
    }
}