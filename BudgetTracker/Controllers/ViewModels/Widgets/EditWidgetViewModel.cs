using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class EditWidgetViewModel
    {
        public EditWidgetViewModel()
        {
                
        }

        public EditWidgetViewModel(WidgetModel model)
        {
            Id = model.Id;
            Title = model.Title;
            Kind = model.Kind;
            Properties = model.Properties.ToDictionary(v => v.Key, v => v.Value);
        }
            
        public Guid Id { get; set; }
            
        public string Title { get; set; } 
        public WidgetKind Kind { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();


        public WidgetSettings Settings
        {
            get
            {
                switch (Kind)
                {
                    case WidgetKind.LastValue:
                        return new LastValueWidgetSettings(Properties);
                    case WidgetKind.Chart:
                        return new ChartWidgetSettings(Properties);
                    case WidgetKind.Expenses:
                        return new ExpensesWidgetSettings(Properties);
                    case WidgetKind.Delta:
                        return new DeltaWidgetSettings(Properties);
                    default:
                        return null;
                }
            }
        }
    }
}