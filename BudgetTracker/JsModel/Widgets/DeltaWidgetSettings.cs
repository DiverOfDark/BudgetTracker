using System.Collections.Generic;
using System.ComponentModel;
using BudgetTracker.JsModel.Attributes;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public enum DeltaInterval
    {
        [JsDisplayName("24ч/48ч/Неделя/Месяц")]
        Daily,
        [JsDisplayName("Месяц/Квартал/Полгода/Год")]
        Monthly
    }


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

        public DeltaInterval DeltaInterval
        {
            get => GetEnumPropertyFromModel<DeltaInterval>();
            set => SetEnumPropertyFromModel(value);
        }

        public DeltaWidgetSettings(Dictionary<string, string> model) : base(model)
        {
        }
    }
}