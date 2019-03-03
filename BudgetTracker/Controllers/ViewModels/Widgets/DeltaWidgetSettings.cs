using System.Collections.Generic;
using System.ComponentModel;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public enum DeltaInterval
    {
        [DisplayName("24ч/48ч/Неделя/Месяц")]
        Daily,
        [DisplayName("Месяц/Кватал/Полгода/Год")]
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