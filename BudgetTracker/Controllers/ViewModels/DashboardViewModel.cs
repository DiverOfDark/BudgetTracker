using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Widgets;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels
{
    public class DashboardViewModel
    {
        public DashboardViewModel(ObjectRepository objectRepository, bool showButtons)
        {
            ShowButtons = showButtons;
            Widgets = new List<WidgetViewModel>();
            
            var widgets = objectRepository.Set<WidgetModel>().OrderBy(v => v.Order).ToList();
            foreach (var w in widgets)
            {
                switch (w)
                {
                    case var wi when w.Properties.Count(s=>!string.IsNullOrWhiteSpace(s.Value)) == 0:
                        Widgets.Add(new UnconfiguredWidgetViewModel(wi));
                        break;
                    case var wi when w.Kind == WidgetKind.LastValue:
                        Widgets.Add(new LastValueWidgetViewModel(wi, objectRepository));
                        break;
                    case var wi when w.Kind == WidgetKind.Expenses:
                        Widgets.Add(new ExpensesWidgetViewModel(wi, objectRepository));
                        break;
                    case var wi when w.Kind == WidgetKind.Chart:
                        Widgets.Add(new ChartWidgetViewModel(wi, objectRepository));
                        break;
                    case var wi when w.Kind == WidgetKind.Delta:
                        Widgets.Add(new DeltaWidgetViewModel(wi, objectRepository));
                        break;
                    default:
                        Widgets.Add(new UnknownWidgetViewModel(w));
                        break;
                }
            }
        }

        public List<WidgetViewModel> Widgets { get; }
        public bool ShowButtons { get; set; }
    }
}