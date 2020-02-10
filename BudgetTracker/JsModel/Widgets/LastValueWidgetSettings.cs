using System.Collections.Generic;
using BudgetTracker.JsModel.Attributes;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public enum GraphKind
    {
        [JsDisplayName("Разница с предыдущим днём")]
        Differential,
        [JsDisplayName("Накопительный с начала графика")]
        CumulativeFromTimePeriod,
        [JsDisplayName("Накопительный от нуля")]
        CumulativeFromBeginning
    }
    
    public class LastValueWidgetSettings : WidgetSettings
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

        public bool Compact
        {
            get
            {
                var value = GetPropertyFromModel()?.ToLower();
                return value == "on" || value == "true";
            }
            set => SetPropertyFromModel(value.ToString());
        }
        
        public bool NotifyStaleData
        {
            get
            {
                var value = GetPropertyFromModel()?.ToLower();
                return value == "on" || value == "true";
            }
            set => SetPropertyFromModel(value.ToString());
        }
        
        public bool ExemptTransfers
        {
            get
            {
                var value = GetPropertyFromModel()?.ToLower();
                return value == "on" || value == "true";
            }
            set => SetPropertyFromModel(value.ToString());
        }

        public GraphKind GraphKind
        {
            get => GetEnumPropertyFromModel<GraphKind>();
            set => SetEnumPropertyFromModel(value);
        }
        
        public string StringFormat
        {
            get => GetPropertyFromModel();
            set => SetPropertyFromModel(value);
        }

        public LastValueWidgetSettings(Dictionary<string,string> model) : base(model)
        {
        }
    }
}